using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using McpUnity.Tools;
using McpUnity.Resources;
using McpUnity.Services;

namespace McpUnity.Unity
{
    /// <summary>
    /// Bridge between Unity and Node.js MCP server.
    /// Now uses TcpListener to receive requests from Node.js.
    /// </summary>
    [InitializeOnLoad]
    public class McpUnityBridge
    {
        private static McpUnityBridge _instance;
        private TcpListener _tcpListener;
        private CancellationTokenSource _cts;
        private Task _listenerTask;
        private Dictionary<string, McpToolBase> _tools = new Dictionary<string, McpToolBase>();
        private Dictionary<string, McpResourceBase> _resources = new Dictionary<string, McpResourceBase>();
        private TestRunnerService _testRunnerService;
        
        /// <summary>
        /// Static constructor that gets called when Unity loads due to InitializeOnLoad attribute
        /// </summary>
        static McpUnityBridge()
        {
            // Initialize the singleton instance when Unity loads
            // This ensures the bridge is available as soon as Unity starts
            // Subscribe to editor quitting event to clean up
            EditorApplication.quitting += () => Instance.StopListener().ConfigureAwait(false);
        }
        
        /// <summary>
        /// Singleton instance accessor
        /// </summary>
        public static McpUnityBridge Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new McpUnityBridge();
                    Debug.Log("[MCP Unity] McpUnityBridge initialized");
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// Current Listening state
        /// </summary>
        public bool IsListening { get; private set; }
        
        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private McpUnityBridge()
        {
            InitializeServices();
            RegisterResources();
            RegisterTools();
            StartListener();
        }
        
        /// <summary>
        /// Create a standardized error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorType">Type of error</param>
        /// <param name="details">Additional error details (optional)</param>
        /// <returns>A JObject containing the error information</returns>
        public static JObject CreateErrorResponse(string message, string errorType = "resource_fetch_error", JObject details = null)
        {
            var error = new JObject
            {
                ["type"] = errorType,
                ["message"] = message
            };
            
            if (details != null)
            {
                error["details"] = details;
            }
            
            return new JObject
            {
                ["error"] = error
            };
        }
        
        /// <summary>
        /// Start the TCP Listener to receive requests from Node.js
        /// </summary>
        public void StartListener(int port = -1)
        {
            if (IsListening) return;
            
            try
            {
                // Use the port from settings or the provided port
                int listenerPort = port > 0 ? port : McpUnitySettings.Instance.Port;
                
                Debug.Log($"[MCP Unity] Starting TCP listener on port {listenerPort}");
                
                _tcpListener = new TcpListener(IPAddress.Loopback, listenerPort);
                _tcpListener.Start();
                
                _cts = new CancellationTokenSource();
                IsListening = true;
                
                // Start the listener loop in a background task
                _listenerTask = ListenerLoop(_cts.Token);
                
                Debug.Log("[MCP Unity] TCP listener started");
            }
            catch (Exception ex)
            {
                IsListening = false;
                Debug.LogError($"[MCP Unity] Failed to start TCP listener: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Stop the TCP listener
        /// </summary>
        public async Task StopListener()
        {
            if (!IsListening) return;
            
            try
            {
                // Signal cancellation
                _cts?.Cancel();
                
                // Close the listener to interrupt any pending GetContextAsync calls
                _tcpListener?.Stop();
                IsListening = false;
                
                // Wait for the listener task to complete
                if (_listenerTask != null)
                {
                    await _listenerTask;
                }
                
                Debug.Log("[MCP Unity] TCP listener stopped");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error stopping TCP listener: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Main listener loop for TCP requests
        /// </summary>
        private async Task ListenerLoop(CancellationToken cancellationToken)
        {
            while (IsListening && !cancellationToken.IsCancellationRequested)
            {
                TcpClient client = null;
                
                try
                {
                    // Use a timeout to check cancellation more frequently
                    Task<TcpClient> acceptTask = _tcpListener.AcceptTcpClientAsync();
                    Task completedTask = await Task.WhenAny(
                        acceptTask,
                        Task.Delay(200, cancellationToken)
                    );
                    
                    // Check if the AcceptTcpClientAsync completed or the delay/cancellation occurred
                    if (completedTask != acceptTask)
                    {
                        // Check if cancellation was requested
                        cancellationToken.ThrowIfCancellationRequested();
                        continue;
                    }
                    
                    // Get the client from the completed task
                    client = await acceptTask;
                    
                    // Handle health check and other requests in a separate task
                    _ = ProcessClientAsync(client, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation was requested
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MCP Unity] Error accepting TCP client: {ex.Message}");
                    client?.Close();
                    continue;
                }
            }
            
            Debug.Log("[MCP Unity] ListenerLoop exited");
        }
        
        /// <summary>
        /// Process a TCP client connection
        /// </summary>
        private async Task ProcessClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {
                // Enable basic socket keepalive
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                
                // Set timeouts
                client.ReceiveTimeout = 10000; // 10 seconds
                client.SendTimeout = 10000;    // 10 seconds
                
                // Read the incoming request
                string requestBody = await ReadMessageAsync(stream);
                
                Debug.Log($"[MCP Unity] TCP message received: {requestBody}");
                
                // Process the regular request
                await ProcessRequestAsync(stream, requestBody);
            }
        }
        
        /// <summary>
        /// Read a message from a TCP stream
        /// </summary>
        private async Task<string> ReadMessageAsync(NetworkStream stream)
        {
            // Using a memory stream to accumulate data
            using (MemoryStream memoryStream = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int bytesRead;
                
                // Read until client closes the connection or we have a complete message
                // This assumes the client properly closes the connection after sending the full message
                // or that we can process JSON as it arrives
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                    
                    // For simpler implementation, we assume each message is sent in a single TCP packet
                    // A more robust implementation would need to implement a message framing protocol
                    break;
                }
                
                // Convert the accumulated data to a string
                memoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
        
        /// <summary>
        /// Process a request from the Node.js server
        /// </summary>
        private async Task ProcessRequestAsync(NetworkStream stream, string requestBody)
        {
            var requestJson = JObject.Parse(requestBody);
            var method = requestJson["method"]?.ToString();
            var parameters = requestJson["params"] as JObject ?? new JObject();
            
            JObject responseJson;
            
            if (string.IsNullOrEmpty(method))
            {
                responseJson = CreateErrorResponse("Missing method in request", "invalid_request");
            }
            else if (method == "ping")
            {
                // Handle MCP Protocol ping request - respond with an empty result object
                responseJson = new JObject();
            }
            else if (_tools.TryGetValue(method, out var tool))
            {
                // Execute the tool
                responseJson = await ExecuteToolAsync(tool, parameters);
            }
            else if (_resources.TryGetValue(method, out var resource))
            {
                // Fetch the resource
                responseJson = FetchResource(resource, parameters);
            }
            else
            {
                responseJson = CreateErrorResponse($"Unknown method: {method}", "unknown_method");
            }
                
            // Format as JSON-RPC 2.0 response
            JObject jsonRpcResponse = new JObject
            {
                ["jsonrpc"] = "2.0",
                ["id"] = requestJson["id"]?.ToString()
            };
            
            // Add result or error
            if (responseJson.ContainsKey("error"))
            {
                var errorObj = responseJson["error"];
                jsonRpcResponse["error"] = errorObj;
            }
            else
            {
                jsonRpcResponse["result"] = responseJson;
            }
            
            await SendTcpResponseAsync(stream, jsonRpcResponse);
        }
        
        /// <summary>
        /// Send a TCP response to the Node.js server
        /// </summary>
        private async Task SendTcpResponseAsync(NetworkStream stream, JObject responseData)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(responseData.ToString(Formatting.None));
                
                // Send the response
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error sending TCP response: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Execute a tool with the provided parameters
        /// </summary>
        private async Task<JObject> ExecuteToolAsync(McpToolBase tool, JObject parameters)
        {
            // We need to dispatch to Unity's main thread
            var tcs = new TaskCompletionSource<JObject>();
                
            EditorApplication.delayCall += () =>
            {
                try
                {
                    _ = DelayTaskCall(tool, parameters, tcs);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MCP Unity] Error executing tool {tool.Name}: {ex.Message}");
                    tcs.SetResult(CreateErrorResponse(
                        $"Failed to execute tool {tool.Name}: {ex.Message}",
                        "tool_execution_error"
                    ));
                }
            };
                
            // Wait for the task to complete
            return await tcs.Task;
        }
        
        // TODO: Add summary
        private async Task DelayTaskCall(McpToolBase tool, JObject parameters, TaskCompletionSource<JObject> tcs)
        {
            var result = await tool.ExecuteAsync(parameters);
            tcs.SetResult(result);
        }
        
        /// <summary>
        /// Fetch a resource with the provided parameters
        /// </summary>
        private JObject FetchResource(McpResourceBase resource, JObject parameters)
        {
            // We need to dispatch to Unity's main thread and wait for completion
            var tcs = new TaskCompletionSource<JObject>();
                
            EditorApplication.delayCall += () =>
            {
                try
                {
                    var result = resource.Fetch(parameters);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[MCP Unity] Error fetching resource {resource.Name}: {ex.Message}");
                    tcs.SetResult(CreateErrorResponse(
                        $"Failed to fetch resource {resource.Name}: {ex.Message}",
                        "resource_fetch_error"
                    ));
                }
            };
                
            // Wait for the task to complete
            return tcs.Task.GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Register all available tools
        /// </summary>
        private void RegisterTools()
        {
            // Register MenuItemTool
            MenuItemTool menuItemTool = new MenuItemTool();
            _tools.Add(menuItemTool.Name, menuItemTool);
            
            // Register SelectObjectTool
            SelectObjectTool selectObjectTool = new SelectObjectTool();
            _tools.Add(selectObjectTool.Name, selectObjectTool);
            
            // Register PackageManagerTool
            PackageManagerTool packageManagerTool = new PackageManagerTool();
            _tools.Add(packageManagerTool.Name, packageManagerTool);
            
            // Register RunTestsTool
            RunTestsTool runTestsTool = new RunTestsTool(_testRunnerService);
            _tools.Add(runTestsTool.Name, runTestsTool);
            
            // Register NotifyMessageTool
            NotifyMessageTool notifyMessageTool = new NotifyMessageTool();
            _tools.Add(notifyMessageTool.Name, notifyMessageTool);
            
            // Register additional tools here as needed
        }
        
        /// <summary>
        /// Register all available resources
        /// </summary>
        private void RegisterResources()
        {
            // Register GetMenuItemsResource
            GetMenuItemsResource getMenuItemsResource = new GetMenuItemsResource();
            _resources.Add(getMenuItemsResource.Name, getMenuItemsResource);
            
            // Register GetConsoleLogsResource
            GetConsoleLogsResource getConsoleLogsResource = new GetConsoleLogsResource();
            _resources.Add(getConsoleLogsResource.Name, getConsoleLogsResource);
            
            // Register GetHierarchyResource
            GetHierarchyResource getHierarchyResource = new GetHierarchyResource();
            _resources.Add(getHierarchyResource.Name, getHierarchyResource);
            
            // Register GetPackagesResource
            GetPackagesResource getPackagesResource = new GetPackagesResource();
            _resources.Add(getPackagesResource.Name, getPackagesResource);
            
            // Register GetAssetsResource
            GetAssetsResource getAssetsResource = new GetAssetsResource();
            _resources.Add(getAssetsResource.Name, getAssetsResource);
            
            // Register GetTestsResource
            GetTestsResource getTestsResource = new GetTestsResource(_testRunnerService);
            _resources.Add(getTestsResource.Name, getTestsResource);
            
            // Register additional resources here as needed
        }
        
        /// <summary>
        /// Initialize services used by tools and resources
        /// </summary>
        private void InitializeServices()
        {
            // Create TestRunnerService
            _testRunnerService = new TestRunnerService();
        }
    }
}
