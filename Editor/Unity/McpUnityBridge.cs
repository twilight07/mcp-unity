using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using McpUnity.Tools;
using McpUnity.Resources;

namespace McpUnity.Unity
{
    /// <summary>
    /// Connection state for the MCP Unity Bridge
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }

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
        
        // Dictionary to store tool instances
        private Dictionary<string, McpToolBase> _tools = new Dictionary<string, McpToolBase>();
        
        // Dictionary to store resource instances
        private Dictionary<string, McpResourceBase> _resources = new Dictionary<string, McpResourceBase>();
        
        // Events
        public static event Action OnConnected;
        public static event Action OnDisconnected;
        public static event Action OnConnecting;
        public static event Action<string> OnError;
        
        /// <summary>
        /// Static constructor that gets called when Unity loads due to InitializeOnLoad attribute
        /// </summary>
        static McpUnityBridge()
        {
            // Initialize the singleton instance when Unity loads
            // This ensures the bridge is available as soon as Unity starts
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
        /// Current connection state mapped from WebSocket state
        /// </summary>
        public ConnectionState ConnectionState
        {
            get
            {
                if (_webSocket == null)
                    return ConnectionState.Disconnected;
                    
                switch (_webSocket.State)
                {
                    case WebSocketState.Open:
                        return ConnectionState.Connected;
                    case WebSocketState.Connecting:
                        return ConnectionState.Connecting;
                    case WebSocketState.CloseSent:  // Still finalizing connection
                    case WebSocketState.CloseReceived:  // Still finalizing connection
                    case WebSocketState.None:
                    case WebSocketState.Closed:
                    case WebSocketState.Aborted:
                        return ConnectionState.Disconnected;
                    default:
                        return ConnectionState.Disconnected;
                }
            }
        }
        
        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private McpUnityBridge()
        {
            // Initialize tools
            RegisterTools();
            RegisterResources();
            
            // Initialize the bridge
            // Auto-connect if configured to do so
            if (McpUnitySettings.Instance.AutoStartServer)
            {
                Connect().ConfigureAwait(false);
            }
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
        /// Connect to the Node.js server
        /// </summary>
        public async Task Connect(int port = -1)
        {
            // Don't try to connect if already connected or connecting
            if (ConnectionState != ConnectionState.Disconnected) return;
            
            string url = $"ws://localhost:{McpUnitySettings.Instance.Port}";
            
            try
            {
                // Notify that we're connecting
                OnConnecting?.Invoke();
                Debug.Log($"[MCP Unity] Connecting to server at {url}...");
                
                _cts = new CancellationTokenSource();
                _webSocket = new ClientWebSocket();
                
                await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
                
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
            // Only disconnect if connected
            if (ConnectionState != ConnectionState.Connected) return;
            
            try
            {
                _cts = null;
                
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                
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
        /// Reconnect to the Node.js server
        /// If disconnected, it will connect.
        /// If connected or connecting, it will disconnect first and then connect again.
        /// </summary>
        public async Task Reconnect()
        {
            Debug.Log("[MCP Unity] Reconnecting to server...");
            
            // If already connected or connecting, disconnect first
            if (ConnectionState != ConnectionState.Disconnected)
            {
                await Disconnect();
                
                // Small delay to ensure clean disconnect
                await Task.Delay(500);
            }
            
            // Now connect
            await Connect();
        }
        
        /// <summary>
        /// Register all available tools
        /// </summary>
        private void RegisterTools()
        {
            // Register MenuItemTool
            MenuItemTool menuItemTool = new MenuItemTool();
            _tools.Add(menuItemTool.Name, menuItemTool);
            
            Debug.Log($"[MCP Unity] Registered tool: {menuItemTool.Name}");
            
            // Register additional tools here as needed
        }
        
        /// <summary>
        /// Register all available resources
        /// </summary>
        private void RegisterResources()
        {
            // Register MenuItemResource
            MenuItemResource menuItemResource = new MenuItemResource();
            _resources.Add(menuItemResource.Name, menuItemResource);
            
            Debug.Log($"[MCP Unity] Registered resource: {menuItemResource.Name}");
            
            // Register additional resources here as needed
        }
        
        /// <summary>
        /// Send a message to the Node.js server
        /// </summary>
        private async Task SendMessage(string message)
        {
            if (ConnectionState != ConnectionState.Connected)
            {
                throw new InvalidOperationException("Not connected to server");
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
            byte[] buffer = new byte[4096];
            StringBuilder messageBuilder = new StringBuilder();
            
            while (ConnectionState == ConnectionState.Connected && !_cts.Token.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                    
                do
                {
                    result = await ReceiveAsync(buffer);
                        
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await Disconnect();
                        return;
                    }
                        
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);
                
                ProcessMessage(messageBuilder.ToString());
                messageBuilder.Clear();
            }
        }

        /// <summary>
        /// Get received a message from the Node.js server
        /// </summary>
        private async Task<WebSocketReceiveResult> ReceiveAsync(byte[] buffer)
        {
            try
            {
                return await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] WebSocket Receive error: {ex.Message}");
                OnError?.Invoke(ex.Message);
            }
            
            // Return a web socket result to close the connection in the ReceiveLoop method
            return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);
        }
        
        /// <summary>
        /// Process incoming messages
        /// </summary>
        private void ProcessMessage(string message)
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
                Debug.Log($"[MCP Unity] Request message: {message}");
                
                // Handle request from Node.js server
                ProcessRequest(json);
            }
            else if (type == "response")
            {
                Debug.Log($"[MCP Unity] Response message: {message}");
            }
            else if (type == "error")
            {
                Debug.LogError($"[MCP Unity] Error message: {message}");
            }
            else
            {
                Debug.LogWarning($"[MCP Unity] Unknown type {type} message: {message}");
            }
        }
        
        /// <summary>
        /// Process a request from the Node.js server
        /// </summary>
        private async void ProcessRequest(JObject request)
        {
            JObject result = null;
            var id = request["id"].ToString();
            var method = request["method"].ToString();
            var parameters = (JObject)request["params"];
            var response = new JObject
            {
                ["id"] = id,
                ["type"] = "response"
            };
            
            // Find the tool by method name
            if (_tools.TryGetValue(method, out var tool))
            {
                // Execute the tool with the provided parameters
                result = ExecuteTool(tool, parameters);
            }
            // Find the resource by method name
            else if (_resources.TryGetValue(method, out var resource))
            {
                // Fetch the resource with the provided parameters
                result = FetchResource(resource, parameters);
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
                    
            // Check if the result contains an error
            if (result != null && result.TryGetValue("error", out var error))
            {
                response["type"] = "error";
                response["error"] = error;
            }
            else
            {
                response["result"] = result;
            }
            
            await SendMessage(response.ToString());
        }

        private JObject ExecuteTool(McpToolBase tool, JObject parameters)
        {
            try
            {
                return tool.Execute(parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error executing tool {tool.Name} with the following error: {ex.Message}");
                return new JObject
                {
                    ["error"] = new JObject
                    {
                        ["type"] = "internal_error",
                        ["message"] = ex.Message,
                        ["stack"] = ex.StackTrace
                    }
                };
            }
        }
        
        private JObject FetchResource(McpResourceBase resource, JObject parameters)
        {
            try
            {
                return resource.Fetch(parameters);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error fetching resource {resource.Name} with the following error: {ex.Message}");
                return new JObject
                {
                    ["error"] = new JObject
                    {
                        ["type"] = "internal_error",
                        ["message"] = ex.Message,
                        ["stack"] = ex.StackTrace
                    }
                };
            }
        }
    }
}
