using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using McpUnity.Tools;
using McpUnity.Resources;

namespace McpUnity.Unity
{
    /// <summary>
    /// WebSocket handler for MCP Unity communications
    /// </summary>
    public class McpUnitySocketHandler : WebSocketBehavior
    {
        private readonly McpUnityServer _server;
        
        /// <summary>
        /// Default constructor required by WebSocketSharp
        /// </summary>
        public McpUnitySocketHandler(McpUnityServer server)
        {
            _server = server;
        }
        
        /// <summary>
        /// Create a standardized error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorType">Type of error</param>
        /// <returns>A JObject containing the error information</returns>
        public static JObject CreateErrorResponse(string message, string errorType)
        {
            return new JObject
            {
                ["error"] = new JObject
                {
                    ["type"] = errorType,
                    ["message"] = message
                }
            };
        }
        
        /// <summary>
        /// Handle incoming messages from WebSocket clients
        /// </summary>
        protected override async void OnMessage(MessageEventArgs e)
        {
            try
            {
                Debug.Log($"[MCP Unity] WebSocket message received: {e.Data}");
                
                var requestJson = JObject.Parse(e.Data);
                var method = requestJson["method"]?.ToString();
                var parameters = requestJson["params"] as JObject ?? new JObject();
                var requestId = requestJson["id"]?.ToString();
                
                JObject responseJson;
                
                if (string.IsNullOrEmpty(method))
                {
                    responseJson = CreateErrorResponse("Missing method in request", "invalid_request");
                }
                else if (_server.TryGetTool(method, out var tool))
                {
                    // Execute the tool
                    responseJson = await ExecuteToolAsync(tool, parameters);
                }
                else if (_server.TryGetResource(method, out var resource))
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
                    ["id"] = requestId
                };
                
                // Add result or error
                if (responseJson.TryGetValue("error", out var errorObj))
                {
                    jsonRpcResponse["error"] = errorObj;
                }
                else
                {
                    jsonRpcResponse["result"] = responseJson;
                }
                
                // Send the response back to the client
                Send(jsonRpcResponse.ToString(Formatting.None));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error processing message: {ex.Message}");
                
                Send(CreateErrorResponse($"Internal server error: {ex.Message}", "internal_error").ToString(Formatting.None));
            }
        }
        
        /// <summary>
        /// Handle WebSocket connection open
        /// </summary>
        protected override void OnOpen()
        {
            Debug.Log("[MCP Unity] WebSocket client connected");
        }
        
        /// <summary>
        /// Handle WebSocket connection close
        /// </summary>
        protected override void OnClose(CloseEventArgs e)
        {
            Debug.Log($"[MCP Unity] WebSocket client disconnected: {e.Reason}");
        }
        
        /// <summary>
        /// Handle WebSocket errors
        /// </summary>
        protected override void OnError(ErrorEventArgs e)
        {
            Debug.LogError($"[MCP Unity] WebSocket error: {e.Message}");
        }
        
        /// <summary>
        /// Execute a tool with the provided parameters
        /// </summary>
        private async Task<JObject> ExecuteToolAsync(McpToolBase tool, JObject parameters)
        {
            // We need to dispatch to Unity's main thread
            var tcs = new TaskCompletionSource<JObject>();
            
            UnityEditor.EditorApplication.delayCall += () =>
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
        
        // Helper method to delegate tool execution to the main thread
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
            
            UnityEditor.EditorApplication.delayCall += () =>
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
    }
}
