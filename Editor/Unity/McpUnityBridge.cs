using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using McpUnity.Tools;

namespace McpUnity.Unity
{
    /// <summary>
    /// Bridge between Unity and Node.js MCP server.
    /// Handles WebSocket communication and request processing.
    /// </summary>
    [InitializeOnLoad]
    public class McpUnityBridge
    {
        private static McpUnityBridge _instance;
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        private bool _isConnected = false;
        private Dictionary<string, TaskCompletionSource<JObject>> _pendingRequests = new Dictionary<string, TaskCompletionSource<JObject>>();
        
        // Dictionary to store tool instances
        private Dictionary<string, McpToolBase> _tools = new Dictionary<string, McpToolBase>();
        
        // Events
        public static event Action OnConnected;
        public static event Action OnDisconnected;
        public static event Action<string> OnError;
        
        /// <summary>
        /// Static constructor that gets called when Unity loads due to InitializeOnLoad attribute
        /// </summary>
        static McpUnityBridge()
        {
            // Initialize the singleton instance when Unity loads
            // This ensures the bridge is available as soon as Unity starts
            _ = Instance;
            
            // Subscribe to editor quitting event to clean up
            EditorApplication.quitting += () => Instance.Disconnect().ConfigureAwait(false);
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
        /// Whether the bridge is currently connected
        /// </summary>
        public bool IsConnected => _isConnected;
        
        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private McpUnityBridge()
        {
            // Initialize tools
            RegisterTools();
            
            // Initialize the bridge
            // Auto-connect if configured to do so
            if (McpUnitySettings.Instance.AutoStartServer)
            {
                Connect(McpUnitySettings.Instance.WebSocketUrl).ConfigureAwait(false);
            }
        }
        
        /// <summary>
        /// Register all available tools
        /// </summary>
        private void RegisterTools()
        {
            // Register MenuItemTool
            var menuItemTool = new MenuItemTool();
            _tools.Add(menuItemTool.Name, menuItemTool);
            
            Debug.Log($"[MCP Unity] Registered tool: {menuItemTool.Name}");
            
            // Register additional tools here as needed
        }
        
        /// <summary>
        /// Connect to the Node.js server
        /// </summary>
        public async Task Connect(string url)
        {
            if (_isConnected) return;
            
            try
            {
                _cts = new CancellationTokenSource();
                _webSocket = new ClientWebSocket();
                
                await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
                _isConnected = true;
                
                Debug.Log($"[MCP Unity] Connected to server at {url}");
                OnConnected?.Invoke();
                
                // Start listening for messages
                _ = ReceiveLoop();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Connection error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }
        
        /// <summary>
        /// Disconnect from the Node.js server
        /// </summary>
        public async Task Disconnect()
        {
            if (!_isConnected) return;
            
            try
            {
                _cts.Cancel();
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                _isConnected = false;
                
                Debug.Log("[MCP Unity] Disconnected from server");
                OnDisconnected?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Disconnection error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
        }
        
        /// <summary>
        /// Send a request to the Node.js server and wait for response
        /// </summary>
        public async Task<JObject> SendRequest(string method, JObject parameters)
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("Not connected to server");
            }
            
            string requestId = Guid.NewGuid().ToString();
            var request = new JObject
            {
                ["id"] = requestId,
                ["type"] = "request",
                ["method"] = method,
                ["params"] = parameters
            };
            
            var tcs = new TaskCompletionSource<JObject>();
            _pendingRequests[requestId] = tcs;
            
            await SendMessage(request.ToString());
            
            // Wait for response with timeout
            var timeoutTask = Task.Delay(5000);
            var responseTask = tcs.Task;
            
            if (await Task.WhenAny(responseTask, timeoutTask) == timeoutTask)
            {
                _pendingRequests.Remove(requestId);
                throw new TimeoutException("Request timed out");
            }
            
            return await responseTask;
        }
        
        /// <summary>
        /// Get metadata about all registered tools
        /// </summary>
        /// <returns>A JArray containing tool metadata</returns>
        public JArray GetToolsMetadata()
        {
            var metadata = new JArray();
            
            foreach (var tool in _tools.Values)
            {
                metadata.Add(tool.GetMetadata());
            }
            
            return metadata;
        }
        
        /// <summary>
        /// Send a message to the Node.js server
        /// </summary>
        private async Task SendMessage(string message)
        {
            if (!_isConnected || _webSocket.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("WebSocket is not connected");
            }
            
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cts.Token);
            Debug.Log($"[MCP Unity] Sent message: {message}");
        }
        
        /// <summary>
        /// Receive messages from the Node.js server
        /// </summary>
        private async Task ReceiveLoop()
        {
            var buffer = new byte[4096];
            
            try
            {
                while (_isConnected && !_cts.Token.IsCancellationRequested)
                {
                    StringBuilder messageBuilder = new StringBuilder();
                    WebSocketReceiveResult result;
                    
                    do
                    {
                        result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
                        
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await Disconnect();
                            return;
                        }
                        
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        messageBuilder.Append(message);
                    }
                    while (!result.EndOfMessage);
                    
                    string fullMessage = messageBuilder.ToString();
                    Debug.Log($"[MCP Unity] Received message: {fullMessage}");
                    ProcessMessage(fullMessage);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] WebSocket error: {ex.Message}");
                OnError?.Invoke(ex.Message);
                
                if (_isConnected)
                {
                    _isConnected = false;
                    OnDisconnected?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Process incoming messages
        /// </summary>
        private void ProcessMessage(string message)
        {
            try
            {
                var json = JObject.Parse(message);
                var id = json["id"]?.ToString();
                var type = json["type"]?.ToString();
                
                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(type))
                {
                    Debug.LogWarning($"[MCP Unity] Invalid message format: {message}");
                    return;
                }
                
                if (type == "request")
                {
                    // Handle request from Node.js server
                    ProcessRequest(json);
                }
                else if (type == "response" || type == "error")
                {
                    // Handle response to a previous request
                    if (_pendingRequests.TryGetValue(id, out var tcs))
                    {
                        _pendingRequests.Remove(id);
                        
                        if (type == "error")
                        {
                            tcs.SetException(new Exception(json["error"]?.ToString() ?? "Unknown error"));
                        }
                        else
                        {
                            tcs.SetResult(json);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error processing message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Process a request from the Node.js server
        /// </summary>
        private async void ProcessRequest(JObject request)
        {
            var id = request["id"].ToString();
            var method = request["method"].ToString();
            var parameters = (JObject)request["params"];
            var response = new JObject
            {
                ["id"] = id,
                ["type"] = "response"
            };
            
            try
            {
                // Find the tool by method name
                if (_tools.TryGetValue(method, out var tool))
                {
                    // Execute the tool with the provided parameters
                    var result = tool.Execute(parameters);
                    
                    // Check if the result contains an error
                    if (result.ContainsKey("error"))
                    {
                        response["type"] = "error";
                        response["error"] = result["error"];
                    }
                    else
                    {
                        response["result"] = result;
                    }
                }
                else
                {
                    response["type"] = "error";
                    response["error"] = new JObject
                    {
                        ["type"] = "method_not_found",
                        ["message"] = $"Method not implemented: {method}"
                    };
                    Debug.LogWarning($"[MCP Unity] Method not implemented: {method}");
                }
            }
            catch (Exception ex)
            {
                response["type"] = "error";
                response["error"] = new JObject
                {
                    ["type"] = "internal_error",
                    ["message"] = ex.Message,
                    ["stack"] = ex.StackTrace
                };
                Debug.LogError($"[MCP Unity] Error processing request: {ex.Message}");
            }
            
            await SendMessage(response.ToString());
        }
    }
}
