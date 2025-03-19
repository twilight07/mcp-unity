import { McpUnityError, ErrorType } from '../utils/errors.js';
export function createGetMenuItemResource(mcpUnity, logger) {
    const resourceName = 'get_menu_items';
    const resourceUri = `unity://${resourceName}`;
    const resourceMimeType = 'application/json';
    return {
        name: resourceName,
        uri: resourceUri,
        metadata: {
            description: 'List of available menu items in Unity to execute',
            mimeType: resourceMimeType
        },
        handler: async (params) => {
            logger.info(`Fetching menu items list`, params);
            if (!mcpUnity.isConnected) {
                throw new McpUnityError(ErrorType.CONNECTION, 'Not connected to Unity. Please ensure Unity is running with the MCP Unity plugin enabled.');
            }
            const response = await mcpUnity.sendRequest({
                method: resourceName,
                params: {}
            });
            if (!response.success) {
                throw new McpUnityError(ErrorType.RESOURCE_FETCH, response.message || 'Failed to fetch menu items from Unity');
            }
            // Ensure we have menu items data
            const menuItems = response.menuItems || [];
            // Create a JSON string representation of the menu items
            const menuItemsText = JSON.stringify(menuItems, null, 2);
            return {
                contents: [{
                        uri: resourceUri,
                        mimeType: resourceMimeType,
                        text: menuItemsText
                    }]
            };
        }
    };
}
