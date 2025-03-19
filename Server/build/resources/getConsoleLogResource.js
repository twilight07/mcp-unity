import { McpUnityError, ErrorType } from '../utils/errors.js';
export function createGetConsoleLogsResource(mcpUnity, logger) {
    const resourceName = 'get_console_logs';
    const resourceUri = `unity://${resourceName}`;
    const resourceMimeType = 'application/json';
    return {
        name: resourceName,
        uri: resourceUri,
        metadata: {
            description: 'Retrieve all logs from the Unity console',
            mimeType: resourceMimeType
        },
        handler: async (params) => {
            logger.info(`Fetching Unity console logs`, params);
            if (!mcpUnity.isConnected) {
                throw new McpUnityError(ErrorType.CONNECTION, 'Not connected to Unity. Please ensure Unity is running with the MCP Unity plugin enabled.');
            }
            const response = await mcpUnity.sendRequest({
                method: resourceName,
                params: {}
            });
            if (!response.success) {
                throw new McpUnityError(ErrorType.RESOURCE_FETCH, response.message || 'Failed to fetch console logs from Unity');
            }
            return {
                contents: [{
                        uri: resourceUri,
                        mimeType: resourceMimeType,
                        text: response.logs
                    }]
            };
        }
    };
}
