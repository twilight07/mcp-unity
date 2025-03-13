# MCP Unity Server Restructuring Plan

This document outlines a step-by-step plan for restructuring the UnityMCP system to work with a new architecture to create an MCP Unity Server that uses a Node.js/TypeScript SDK and establishes Unity as a local service to execute logic from that server.

## Goals

- Delete all models, tools, server, and transport scripts
- Change all code references in the project from Unity MCP or Unity MCP Server to MCP Unity
- Create a new MCP server using Node.js and TypeScript
- Support clients like Windsurf, Cursor, or Claude Desktop
- Use Unity engine as a local service to execute logic from the new Node server
- Create an Unity API bridge script that will the connection point between the node server and Unity
- Create a communication protocol that will best fit this use case so the node server send requests to Unity API bridge script and then reply back with the requested data or confirmation message
- Implement only one tool for now, the Menu Item tool functionality

## Phase 1: Clean Up Current Codebase

### Step 1: Back Up Current Code
1. Create a backup branch of the current codebase
2. Document current functionality for reference during migration

### Step 2: Remove Unnecessary Components
1. Delete all model scripts (keep a record of schemas for reference)
2. Delete all transport scripts
3. Delete all server scripts
4. Delete all tool scripts except MenuItemTool.cs

### Step 3: Update Project References
1. Rename all occurrences of "Unity MCP" or "Unity MCP Server" to "MCP Unity" in:
   - Class and interface names
   - Namespaces
   - File names
   - Comments and documentation
   - UI text and labels
2. Update folder structure to reflect new naming convention
3. Update any remaining references to match the new naming convention

### Step 4: Clean Up Remaining Scripts
1. Remove all server-related code from MCPServer.cs
2. Remove all transport-related code from MCPServer.cs
3. Keep only the essential functionality needed for the new architecture
4. Update any remaining references to match the new naming convention

### Step 5: Modify UnityMCPEditorWindow.cs
1. Remove the Settings tab and move the Auto-start option to the Server tab
2. Remove the MCP Config tab and move the Generate MCP Config JSON functionality with its indentation toggle to the Server tab
3. Keep Server and Help tabs
4. Update the UI to reflect the new architecture
5. Update any references to deleted components

## Phase 2: Create Unity API Bridge

### Step 1: Design the API Bridge
1. Create a new `McpUnityBridge.cs` script to handle communication with the Node server
2. Implement core functionality:
   - WebSocket communication for real-time bidirectional messaging
   - Message serialization/deserialization
   - Request handling and response generation
   - Error handling and logging

### Step 2: Implement MenuItemTool Integration
1. Refactor MenuItemTool to work with the new architecture
2. Implement method to execute menu items based on path strings
3. Create response handlers for successful/failed executions
4. Ensure proper error handling and logging

### Step 3: Create Communication Protocol
1. Design a simple JSON-based protocol for communication:
   ```json
   {
     "id": "unique-request-id",
     "type": "request|response|error",
     "method": "executeMenuItem",
     "params": {
       "menuPath": "path/to/menu/item",
       "requireConfirmation": true
     },
     "result": { /* Response data */ },
     "error": { /* Error data if applicable */ }
   }
   ```
2. Implement request/response handling in McpUnityBridge
3. Create helper methods for common operations

## Phase 3: Develop Node.js Server

### Step 1: Set Up TypeScript Project
1. Initialize a new Node.js project with TypeScript
   ```bash
   mkdir mcp-unity-server
   cd mcp-unity-server
   npm init -y
   npm install typescript @types/node ts-node --save-dev
   npx tsc --init
   ```
2. Configure TypeScript settings in tsconfig.json
3. Set up project structure (src, dist, tests)

### Step 2: Implement MCP Server
1. Install required dependencies:
   ```bash
   npm install @modelcontextprotocol/sdk ws express cors
   ```
2. Create server entry point with MCP setup:
   ```typescript
   import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
   import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
   import { z } from "zod";
   ```
3. Implement WebSocket server for Unity communication
4. Create McpUnity bridge connection management
5. Configure STDIO transport protocol for MCP client communication:
   ```typescript
   // Initialize STDIO transport
   const stdioTransport = new StdioServerTransport();
   
   // Connect the server to the transport
   await server.connect(stdioTransport);
   ```

### Step 3: Implement Menu Item Tool
1. Create a tool that communicates with Unity's MenuItemTool:
   ```typescript
   server.tool("execute_menu_item",
     { 
       menuPath: z.string(),
       requireConfirmation: z.boolean().optional().default(true)
     },
     async ({ menuPath, requireConfirmation }) => {
       // Send request to Unity and wait for response
       const response = await mcpUnity.sendRequest({
         method: "executeMenuItem",
         params: { menuPath, requireConfirmation }
       });
       
       return {
         content: [{ type: "text", text: response.message }]
       };
     }
   );
   ```
2. Implement proper error handling
3. Add logging for debugging

### Step 4: Implement Robust Error Handling and Logging
1. Create a centralized logging system:
   ```typescript
   // Logging levels
   enum LogLevel {
     DEBUG = 0,
     INFO = 1,
     WARN = 2,
     ERROR = 3
   }
   
   // Logger implementation
   class Logger {
     private level: LogLevel;
     private prefix: string;
     
     constructor(prefix: string, level: LogLevel = LogLevel.INFO) {
       this.prefix = prefix;
       this.level = level;
     }
     
     debug(message: string, data?: any) {
       this.log(LogLevel.DEBUG, message, data);
     }
     
     info(message: string, data?: any) {
       this.log(LogLevel.INFO, message, data);
     }
     
     warn(message: string, data?: any) {
       this.log(LogLevel.WARN, message, data);
     }
     
     error(message: string, error?: any) {
       this.log(LogLevel.ERROR, message, error);
     }
     
     private log(level: LogLevel, message: string, data?: any) {
       if (level < this.level) return;
       
       const timestamp = new Date().toISOString();
       const levelStr = LogLevel[level];
       const logMessage = `[${timestamp}] [${levelStr}] [${this.prefix}] ${message}`;
       
       if (data) {
         console.log(logMessage, data);
       } else {
         console.log(logMessage);
       }
     }
   }
   
   // Create loggers
   const serverLogger = new Logger('Server');
   const unityLogger = new Logger('Unity');
   const toolsLogger = new Logger('Tools');
   ```

2. Implement standardized error handling:
   ```typescript
   // Error types
   enum ErrorType {
     CONNECTION = 'connection_error',
     TIMEOUT = 'timeout_error',
     TOOL_EXECUTION = 'tool_execution_error',
     VALIDATION = 'validation_error',
     INTERNAL = 'internal_error'
   }
   
   // Error class
   class McpUnityError extends Error {
     type: ErrorType;
     details?: any;
     
     constructor(type: ErrorType, message: string, details?: any) {
       super(message);
       this.type = type;
       this.details = details;
       this.name = 'McpUnityError';
     }
     
     toJSON() {
       return {
         type: this.type,
         message: this.message,
         details: this.details
       };
     }
   }
   
   // Error handler
   function handleError(error: any, context: string): McpUnityError {
     if (error instanceof McpUnityError) {
       return error;
     }
     
     // Log the error
     toolsLogger.error(`${context}: ${error.message}`, error);
     
     // Convert to McpUnityError
     return new McpUnityError(
       ErrorType.INTERNAL,
       `Error in ${context}: ${error.message}`,
       { originalError: error.toString() }
     );
   }
   ```

### Step 5: Design for Future Extensibility
1. Create an extensible architecture for adding more tools:
   ```typescript
   // Tool registry interface
   interface ToolDefinition {
     name: string;
     description: string;
     parameters: z.ZodType<any>;
     handler: (params: any) => Promise<any>;
   }
   
   // Tool registry
   class ToolRegistry {
     private tools: Map<string, ToolDefinition> = new Map();
     
     register(tool: ToolDefinition) {
       this.tools.set(tool.name, tool);
       toolsLogger.info(`Registered tool: ${tool.name}`);
     }
     
     getAll(): ToolDefinition[] {
       return Array.from(this.tools.values());
     }
     
     registerWithServer(server: McpServer) {
       for (const tool of this.getAll()) {
         server.tool(
           tool.name,
           tool.parameters,
           async (params) => {
             try {
               return await tool.handler(params);
             } catch (error) {
               throw handleError(error, `Tool ${tool.name}`);
             }
           }
         );
       }
     }
   }
   
   // Usage
   const toolRegistry = new ToolRegistry();
   
   // Register the menu item tool
   toolRegistry.register({
     name: 'execute_menu_item',
     description: 'Executes a Unity menu item by path',
     parameters: z.object({
       menuPath: z.string(),
       requireConfirmation: z.boolean().optional().default(true)
     }),
     handler: async ({ menuPath, requireConfirmation }) => {
       const response = await mcpUnity.sendRequest({
         method: "executeMenuItem",
         params: { menuPath, requireConfirmation }
       });
       
       return {
         content: [{ type: "text", text: response.message }]
       };
     }
   });
   
   // Register all tools with the server
   toolRegistry.registerWithServer(server);
   ```

2. Prepare for future implementation of Resources and Prompts:
   ```typescript
   // Placeholder for future resource implementation
   class ResourceRegistry {
     // Will be implemented in the future
     registerWithServer(server: McpServer) {
       // No resources registered yet
     }
   }
   
   // Placeholder for future prompts implementation
   class PromptRegistry {
     // Will be implemented in the future
     registerWithServer(server: McpServer) {
       // No prompts registered yet
     }
   }
   
   // Future extensibility
   const resourceRegistry = new ResourceRegistry();
   const promptRegistry = new PromptRegistry();
   
   // Register with server
   resourceRegistry.registerWithServer(server);
   promptRegistry.registerWithServer(server);
   ```

## Phase 4: Create Client Configuration

### Step 1: Implement Configuration Generator
1. Update the GenerateMCPConfigJson method in UnityMCPEditorWindow.cs to generate configuration compatible with new clients:
   ```csharp
   private void GenerateMCPConfigJson()
   {
      // TODO: Get absolute path on the computer to the directory where package.json is located. Use Editor or C# Tools if necessary
      var pathToServer = "";

       // Create the config object according to MCP specification for Windsurf/Cursor/Claude
       var config = new Dictionary<string, object>
       {
           { "mcpServers", new Dictionary<string, object>
               {
                   { "mcp-unity", new Dictionary<string, object>
                       {
                           { "command", "npx" },
                           { "args", new[] { pathToServer + "/build/index.js" } },
                       }
                   }
               }
           }
       };
       
       // Serialize to JSON with the selected indentation
       // [Serialization code...]
       
       _mcpConfigJson = stringWriter.ToString();
   }
   ```
2. Ensure compatibility with Windsurf, Cursor, and Claude Desktop

### Step 2: Create Documentation
1. Document the new configuration process
2. Provide examples for different client setups
3. Create troubleshooting guide

## Phase 5: Testing and Integration

### Step 1: Create Test Suite
1. Implement unit tests for McpUnityBridge
2. Create integration tests for Node.js server and Unity communication
3. Test with actual clients (Windsurf, Cursor, Claude Desktop)

### Step 2: Debugging and Optimization
1. Add comprehensive logging
2. Optimize message passing and response times

### Step 3: Finalize Documentation
1. Create user guide
2. Document API reference
3. Provide sample projects and use cases

## Phase 6: Documentation

### Step 1: API Documentation
1. Document the McpUnityBridge API:
   - Connection methods
   - Message handling
   - Error handling
   - Event system

2. Document the Node.js server API:
   - Server initialization
   - Tool registration
   - WebSocket communication
   - STDIO transport configuration

### Step 2: Integration Guides
1. Create step-by-step guides for integrating with supported clients:
   - Claude Desktop integration guide
   - Cursor integration guide
   - Windsurf integration guide

2. Include configuration examples for each client:
   ```json
   // Claude Desktop configuration example
   {
     "mcpServers": {
       "mcp-unity": {
         "command": "npx",
         "args": ["mcp-unity-server", "start"],
         "url": "http://localhost:3000"
       }
     }
   }
   ```

### Step 3: Usage Examples
1. Document common use cases:
   - Executing Unity menu items from an AI assistant
   - Handling errors and timeouts
   - Configuring the server for different environments

2. Create code examples for each use case:
   ```typescript
   // Example: Executing a menu item from Claude
   // Ask Claude: "Create a new scene in Unity"
   
   // Claude will execute:
   await tools.execute_menu_item({
     menuPath: "File/New Scene",
     requireConfirmation: true
   });
   ```

3. Include troubleshooting guide:
   - Common connection issues
   - Configuration problems
   - Unity integration challenges

## Implementation Details

### McpUnityBridge Structure
```csharp
using System;
using UnityEngine;
using UnityEditor;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UnityMCP
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
            // Initialize the bridge
            // Auto-connect if configured to do so
            if (MCPSettings.Instance.AutoStartServer)
            {
                Connect("ws://localhost:8080").ConfigureAwait(false);
            }
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
        /// Execute a menu item and return the result
        /// </summary>
        public bool ExecuteMenuItem(string menuPath, bool requireConfirmation = true)
        {
            try
            {
                Debug.Log($"[MCP Unity] Executing menu item: {menuPath}");
                return EditorApplication.ExecuteMenuItem(menuPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error executing menu item: {ex.Message}");
                return false;
            }
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
                string id = json["id"]?.ToString();
                string type = json["type"]?.ToString();
                
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
            string id = request["id"].ToString();
            string method = request["method"].ToString();
            JObject parameters = (JObject)request["params"];
            
            JObject response = new JObject
            {
                ["id"] = id,
                ["type"] = "response"
            };
            
            try
            {
                switch (method)
                {
                    case "executeMenuItem":
                        string menuPath = parameters["menuPath"].ToString();
                        bool requireConfirmation = parameters["requireConfirmation"]?.ToObject<bool>() ?? true;
                        
                        bool success = ExecuteMenuItem(menuPath, requireConfirmation);
                        
                        response["result"] = new JObject
                        {
                            ["success"] = success,
                            ["message"] = success ? $"Successfully executed menu item: {menuPath}" : $"Failed to execute menu item: {menuPath}"
                        };
                        break;
                        
                    default:
                        throw new NotImplementedException($"Method not implemented: {method}");
                }
            }
            catch (Exception ex)
            {
                response["type"] = "error";
                response["error"] = ex.Message;
            }
            
            await SendMessage(response.ToString());
        }
    }
}

### Node.js MCP Server Structure
```typescript
import { McpServer, ResourceTemplate } from "@modelcontextprotocol/sdk/server/mcp.js";
import { StdioServerTransport } from "@modelcontextprotocol/sdk/server/stdio.js";
import { HttpServerTransport } from "@modelcontextprotocol/sdk/server/http.js";
import { z } from "zod";
import WebSocket from "ws";
import express from "express";
import cors from "cors";
import { v4 as uuidv4 } from "uuid";

// Unity WebSocket bridge
class McpUnity {
  private ws: WebSocket.Server;
  private connections: Map<string, WebSocket> = new Map();
  private pendingRequests: Map<string, { 
    resolve: (value: any) => void, 
    reject: (reason: any) => void 
  }> = new Map();
  
  constructor(port: number = 8080) {
    this.ws = new WebSocket.Server({ port });
    
    this.ws.on("connection", (socket: WebSocket) => {
      const id = uuidv4();
      this.connections.set(id, socket);
      console.log(`Unity connected: ${id}`);
      
      socket.on("message", (data: WebSocket.Data) => {
        try {
          const message = JSON.parse(data.toString());
          this.handleMessage(message);
        } catch (error) {
          console.error("Error parsing message:", error);
        }
      });
      
      socket.on("close", () => {
        this.connections.delete(id);
        console.log(`Unity disconnected: ${id}`);
      });
    });
    
    console.log(`Unity bridge listening on port ${port}`);
  }
  
  async sendRequest(method: string, params: any): Promise<any> {
    if (this.connections.size === 0) {
      throw new Error("No Unity connections available");
    }
    
    // Use the first available connection
    const [id, socket] = this.connections.entries().next().value;
    const requestId = uuidv4();
    
    const request = {
      id: requestId,
      type: "request",
      method,
      params
    };
    
    return new Promise((resolve, reject) => {
      this.pendingRequests.set(requestId, { resolve, reject });
      socket.send(JSON.stringify(request));
      
      // Add timeout
      setTimeout(() => {
        if (this.pendingRequests.has(requestId)) {
          this.pendingRequests.delete(requestId);
          reject(new Error("Request timed out"));
        }
      }, 5000);
    });
  }
  
  private handleMessage(message: any) {
    const { id, type, result, error } = message;
    
    if (!id || !type) {
      console.warn("Invalid message format:", message);
      return;
    }
    
    if (type === "response" || type === "error") {
      const pendingRequest = this.pendingRequests.get(id);
      if (pendingRequest) {
        this.pendingRequests.delete(id);
        
        if (type === "error") {
          pendingRequest.reject(new Error(error || "Unknown error"));
        } else {
          pendingRequest.resolve(result);
        }
      }
    }
  }
}

// Main application
async function main() {
  // Create Unity bridge
  const mcpUnity = new McpUnity(8080);
  
  // Create MCP server
  const server = new McpServer({
    name: "MCP Unity Server",
    version: "1.0.0"
  });
  
  // Add menu item tool
  server.tool("execute_menu_item",
    { 
      menuPath: z.string(),
      requireConfirmation: z.boolean().optional().default(true)
    },
    async ({ menuPath, requireConfirmation }) => {
      try {
        const result = await mcpUnity.sendRequest("executeMenuItem", {
          menuPath,
          requireConfirmation
        });
        
        return {
          content: [{ 
            type: "text", 
            text: result.message || "Command executed successfully" 
          }]
        };
      } catch (error) {
        console.error("Error executing menu item:", error);
        throw error;
      }
    }
  );
  
  // Set up HTTP server with Express
  const app = express();
  app.use(cors());
  app.use(express.json());
  
  // Health check endpoint
  app.get("/health", (req, res) => {
    res.json({ status: "ok" });
  });
  
  const port = process.env.PORT || 3000;
  const httpServer = app.listen(port, () => {
    console.log(`HTTP server running on port ${port}`);
  });
  
  // Start MCP transports
  const stdioTransport = new StdioServerTransport();
  const httpTransport = new HttpServerTransport({ port: parseInt(port.toString()) });
  
  await Promise.all([
    server.connect(stdioTransport),
    server.connect(httpTransport)
  ]);
  
  console.log("MCP Unity Server started and ready");
}

main().catch(console.error);
```

## Conclusion

This restructuring plan provides a comprehensive approach to transforming the UnityMCP system into a modern architecture that uses a Node.js/TypeScript server with Unity as a local service. The implementation will maintain the core functionality of executing menu items while significantly simplifying the architecture and improving maintainability.

By following this plan, the new system will be compatible with clients like Windsurf, Cursor, and Claude Desktop, providing a seamless integration experience for users of these platforms.
