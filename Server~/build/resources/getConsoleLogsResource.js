import { ResourceTemplate } from '@modelcontextprotocol/sdk/server/mcp.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
// Constants for the resource
const resourceName = 'get_console_logs';
const resourceMimeType = 'application/json';
const resourceUri = 'unity://logs/{logType}';
const resourceTemplate = new ResourceTemplate(resourceUri, {
    list: () => listLogTypes(resourceMimeType)
});
function listLogTypes(resourceMimeType) {
    return {
        resources: [
            {
                uri: `unity://logs/`,
                name: "All logs",
                description: "Retrieve all Unity console logs",
                mimeType: resourceMimeType
            },
            {
                uri: `unity://logs/error`,
                name: "Error logs",
                description: "Retrieve only error logs from the Unity console",
                mimeType: resourceMimeType
            },
            {
                uri: `unity://logs/warning`,
                name: "Warning logs",
                description: "Retrieve only warning logs from the Unity console",
                mimeType: resourceMimeType
            },
            {
                uri: `unity://logs/info`,
                name: "Info logs",
                description: "Retrieve only info logs from the Unity console",
                mimeType: resourceMimeType
            }
        ]
    };
}
/**
 * Registers the get_console_logs resource with the MCP server
 */
export function registerGetConsoleLogsResource(server, mcpUnity, logger) {
    logger.info(`Registering resource: ${resourceName}`);
    server.resource(resourceName, resourceTemplate, {
        description: 'Retrieve Unity console logs by type',
        mimeType: resourceMimeType
    }, async (uri, variables) => {
        try {
            return await resourceHandler(mcpUnity, uri, variables, logger);
        }
        catch (error) {
            logger.error(`Error handling resource ${resourceName}: ${error}`);
            throw error;
        }
    });
}
/**
 * Handles requests for Unity console logs by log type
 */
async function resourceHandler(mcpUnity, uri, variables, logger) {
    // Extract and convert the parameter from the template variables
    let logType = variables["logType"] ? decodeURIComponent(variables["logType"]) : undefined;
    if (logType === '')
        logType = undefined;
    // Send request to Unity
    const response = await mcpUnity.sendRequest({
        method: resourceName,
        params: {
            logType: logType
        }
    });
    if (!response.success) {
        throw new McpUnityError(ErrorType.RESOURCE_FETCH, response.message || 'Failed to fetch logs from Unity');
    }
    return {
        contents: [{
                uri: `unity://logs/${logType ?? ''}`,
                mimeType: resourceMimeType,
                text: JSON.stringify(response, null, 2)
            }]
    };
}
