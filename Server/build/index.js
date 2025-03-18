// Import MCP SDK components
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
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
const server = new McpServer({
    name: "MCP Unity Server",
    version: "1.0.0"
}, {
    capabilities: {
        tools: {},
    },
});
// Initialize Unity WebSocket bridge with port from port.txt
const mcpUnity = new McpUnity(unityLogger);
// Initialize tool registry
const toolRegistry = new ToolRegistry(toolLogger);
// Add the menu item tool
toolRegistry.add(createMenuItemTool(mcpUnity, toolLogger));
// Register all tools with the MCP server
toolRegistry.registerWithServer(server);
// Server startup function
async function startServer() {
    try {
        // Initialize STDIO transport for MCP client communication
        const stdioTransport = new StdioServerTransport();
        // Connect the server to the transport
        await server.connect(stdioTransport);
        // Start Unity WebSocket connection
        await mcpUnity.start();
        serverLogger.info('MCP Server started and ready');
    }
    catch (error) {
        serverLogger.error('Failed to start server', error);
        process.exit(1);
    }
}
// Start the server
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
