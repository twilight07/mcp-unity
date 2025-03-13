// Import MCP SDK components
import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';

import { McpUnity } from './unity/mcpUnity.js';
import { Logger, LogLevel } from './utils/logger.js';
import { ToolRegistry } from './tools/toolRegistry.js';
import { createMenuItemTool } from './tools/menuItemTool.js';

// Initialize loggers
const serverLogger = new Logger('Server', LogLevel.INFO);
const unityLogger = new Logger('Unity', LogLevel.INFO);
const toolLogger = new Logger('Tools', LogLevel.INFO);

// Initialize the MCP server
const server = new Server(
  {
    name: "MCP Unity Server",
    version: "1.0.0"
  },
  {
    capabilities: {
      tools: {},
    },
  }
);

// Initialize Unity bridge
const mcpUnity = new McpUnity(8080, unityLogger);

// Initialize tool registry
const toolRegistry = new ToolRegistry(toolLogger);

// Register the menu item tool
toolRegistry.register(createMenuItemTool(mcpUnity, toolLogger));

// Register all tools with the MCP server
toolRegistry.registerWithServer(server);

// Start the server
async function startServer() {
  try {
    // Initialize STDIO transport for MCP client communication
    const stdioTransport = new StdioServerTransport();
    
    // Connect the server to the transport
    await server.connect(stdioTransport);
    
    // Start Unity WebSocket connection
    await mcpUnity.start();
    
    serverLogger.info('MCP Server started and ready');
  } catch (error) {
    serverLogger.error('Failed to start server', error);
    process.exit(1);
  }
}

// Actually start the server
startServer();

// Handle shutdown
process.on('SIGINT', async () => {
  serverLogger.info('Shutting down...');
  await mcpUnity.stop();
  process.exit(0);
});

// Handle uncaught exceptions
process.on('uncaughtException', (error) => {
  serverLogger.error('Uncaught exception', error);
});

// Handle unhandled promise rejections
process.on('unhandledRejection', (reason) => {
  serverLogger.error('Unhandled rejection', reason);
});
