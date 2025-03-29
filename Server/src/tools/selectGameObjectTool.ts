import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ToolDefinition } from './toolRegistry.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import { CallToolResult } from '@modelcontextprotocol/sdk/types.js';

export function createSelectGameObjectTool(mcpUnity: McpUnity, logger: Logger): ToolDefinition {
  const toolName = 'select_gameobject';
  
  // Create the schema as a ZodObject to match the interface requirements
  const ParamsSchema = z.object({
    objectPath: z.string().optional().describe('The path or name of the GameObject to select (e.g. "Main Camera")'),
    instanceId: z.number().optional().describe('The instance ID of the GameObject to select')
  });
  
  return {
    name: toolName,
    description: 'Sets the selected GameObject in the Unity editor by path or instance ID',
    paramsSchema: ParamsSchema,
    handler: async (params): Promise<CallToolResult> => {
      // Custom validation since we can't use refine/superRefine while maintaining ZodObject type
      if (params.objectPath === undefined && params.instanceId === undefined) {
        throw new McpUnityError(
          ErrorType.VALIDATION,
          "Either 'objectPath' or 'instanceId' must be provided"
        );
      }
      
      const response = await mcpUnity.sendRequest({
        method: toolName,
        params
      });
      
      if (!response.success) {
        throw new McpUnityError(
          ErrorType.TOOL_EXECUTION,
          response.message || `Failed to select GameObject`
        );
      }
      
      return {
        success: true,
        message: response.message,
        content: [{
          type: response.type,
          text: response.message || `Successfully selected GameObject`
        }]
      };
    }
  };
}
