import { z } from 'zod';
import { McpUnityError, ErrorType } from '../utils/errors.js';
export function createMenuItemTool(mcpUnity, logger) {
    const toolName = 'execute_menu_item';
    return {
        name: toolName,
        description: 'Executes a Unity menu item by path',
        parameters: z.object({
            menuPath: z.string().describe('The path to the menu item to execute (e.g. "GameObject/Create Empty")')
        }),
        handler: async ({ menuPath }) => {
            logger.info(`Executing menu item: ${menuPath}`);
            if (!mcpUnity.isConnected) {
                throw new McpUnityError(ErrorType.CONNECTION, 'Not connected to Unity. Please ensure Unity is running with the MCP Unity plugin enabled.');
            }
            const response = await mcpUnity.sendRequest({
                method: toolName,
                params: { menuPath }
            });
            if (!response.success) {
                throw new McpUnityError(ErrorType.TOOL_EXECUTION, response.message || `Failed to execute menu item: ${menuPath}`);
            }
            return {
                success: true,
                content: [{
                        type: 'text',
                        text: response.message || `Successfully executed menu item: ${menuPath}`
                    }]
            };
        }
    };
}
