// Import MCP SDK components
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
import { StdioServerTransport } from '@modelcontextprotocol/sdk/server/stdio.js';
import { readFileSync, existsSync } from 'fs';
import { join, dirname } from 'path';
import { McpUnity } from './unity/mcpUnity.js';
import { Logger, LogLevel } from './utils/logger.js';
import { ToolRegistry } from './tools/toolRegistry.js';
import { createMenuItemTool } from './tools/menuItemTool.js';
// Initialize loggers
const serverLogger = new Logger('Server', LogLevel.INFO);
const unityLogger = new Logger('Unity', LogLevel.INFO);
const toolLogger = new Logger('Tools', LogLevel.INFO);
// Read port from port.txt file or use default
function getPort() {
    try {
        // Get the directory above the current working directory
        const parentDir = dirname(process.cwd());
        const portFilePath = join(parentDir, 'port.txt');
        if (existsSync(portFilePath)) {
            const portStr = readFileSync(portFilePath, 'utf-8').trim();
            const port = parseInt(portStr, 10);
            if (!isNaN(port) && port > 0 && port < 65536) {
                serverLogger.info(`Using port from port.txt: ${port}`);
                return port;
            }
        }
    }
    catch (error) {
        serverLogger.warn(`Error reading port.txt: ${error}`);
    }
    serverLogger.info('Using default port: 8080');
    return 8080;
}
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
const mcpUnity = new McpUnity(getPort(), unityLogger);
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
        // Start Unity WebSocket connection with better error handling
        try {
            await mcpUnity.start();
            serverLogger.info(`MCP Unity WebSocket server started on port ${getPort()}`);
        }
        catch (wsError) {
            serverLogger.error('Failed to start WebSocket server', wsError);
            // Don't exit process here, just log the error
        }
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
