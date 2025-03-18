import { z } from 'zod';
import { McpUnity } from '../unity/mcpUnity.js';
import { Logger } from '../utils/logger.js';
import { ToolDefinition, ToolResponse } from './toolRegistry.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';

export function createMenuItemTool(mcpUnity: McpUnity, logger: Logger): ToolDefinition {
  const toolName = 'execute_menu_item';
  return {
    name: toolName,
    description: 'Executes a Unity menu item by path',
    parameters: z.object({
      menuPath: z.string().describe('The path to the menu item to execute (e.g. "GameObject/Create Empty")')
    }),
    handler: async ({ menuPath }): Promise<ToolResponse> => {
      logger.info(`Executing menu item: ${menuPath}`);
      
      if (!mcpUnity.isConnected) {
        throw new McpUnityError(
          ErrorType.CONNECTION, 
          'Not connected to Unity. Please ensure Unity is running with the MCP Unity plugin enabled.'
        );
      }
      
      const response = await mcpUnity.sendRequest({
        method: toolName,
        params: { menuPath }
      });
      
      if (!response.success) {
        throw new McpUnityError(
          ErrorType.TOOL_EXECUTION,
          response.message || `Failed to execute menu item: ${menuPath}`
        );
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
