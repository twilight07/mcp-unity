import { McpUnityError, ErrorType } from '../utils/errors.js';
export function createGetHierarchyResource(mcpUnity, logger) {
    const resourceName = 'get_hierarchy';
    const resourceUri = `unity://${resourceName}`;
    const resourceMimeType = 'application/json';
    return {
        name: resourceName,
        uri: resourceUri,
        metadata: {
            description: 'Retrieve all game objects in the Unity loaded scenes with their active state',
            mimeType: resourceMimeType
        },
        handler: async (params) => {
            logger.info(`Fetching Unity hierarchy`, params);
            if (!mcpUnity.isConnected) {
                throw new McpUnityError(ErrorType.CONNECTION, 'Not connected to Unity. Please ensure Unity is running with the MCP Unity plugin enabled.');
            }
            const response = await mcpUnity.sendRequest({
                method: resourceName,
                params: {}
            });
            if (!response.success) {
                throw new McpUnityError(ErrorType.RESOURCE_FETCH, response.message || 'Failed to fetch hierarchy from Unity');
            }
            return {
                contents: [{
                        uri: resourceUri,
                        mimeType: resourceMimeType,
                        text: response.hierarchy
                    }]
            };
        }
    };
}
