import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ToolDefinition } from './toolRegistry.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import { CallToolResult } from '@modelcontextprotocol/sdk/types.js';

export function createUpdateComponentTool(mcpUnity: McpUnity, logger: Logger): ToolDefinition {
  const toolName = 'update_component';
  
  // Create the schema as a ZodObject to match the interface requirements
  const ParamsSchema = z.object({
    instanceId: z.number().optional().describe('The instance ID of the GameObject to update'),
    objectPath: z.string().optional().describe('The path of the GameObject in the hierarchy to update (alternative to instanceId)'),
    componentName: z.string().describe('The name of the component to update or add'),
    componentData: z.record(z.any()).describe('An object containing the fields to update on the component')
  });
  
  return {
    name: toolName,
    description: 'Updates component fields on a GameObject or adds it to the GameObject if it does not contain the component',
    paramsSchema: ParamsSchema,
    handler: async (params): Promise<CallToolResult> => {
      // Validate parameters - require either instanceId or objectPath
      if ((params.instanceId === undefined || params.instanceId === null) && 
          (!params.objectPath || params.objectPath.trim() === '')) {
        throw new McpUnityError(
          ErrorType.VALIDATION,
          "Either 'instanceId' or 'objectPath' must be provided"
        );
      }
      
      if (!params.componentName) {
        throw new McpUnityError(
          ErrorType.VALIDATION,
          "Required parameter 'componentName' must be provided"
        );
      }
      
      // Send request to Unity
      const response = await mcpUnity.sendRequest({
        method: toolName,
        params: {
          instanceId: params.instanceId,
          objectPath: params.objectPath,
          componentName: params.componentName,
          componentData: params.componentData
        }
      });
      
      if (!response.success) {
        throw new McpUnityError(
          ErrorType.TOOL_EXECUTION,
          response.message || `Failed to update component on GameObject`
        );
      }
      
      // Create a description of which GameObject was targeted
      const targetDescription = params.objectPath 
        ? `path '${params.objectPath}'` 
        : `ID ${params.instanceId}`;
      
      return {
        success: true,
        message: response.message,
        content: [{
          type: response.type,
          text: response.message || `Successfully updated component on GameObject with ${targetDescription}`
        }]
      };
    }
  };
}
