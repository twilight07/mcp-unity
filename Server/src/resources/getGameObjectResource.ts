import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ReadResourceResult } from '@modelcontextprotocol/sdk/types.js';
import { ResourceDefinition } from './resourceRegistry.js';
import { ResourceTemplate } from '@modelcontextprotocol/sdk/server/mcp.js';

// Define the parameter schema using zod
export const GameObjectParamsSchema = z.object({
  instanceId: z.number().optional().describe("The instance ID of the GameObject to retrieve"),
  objectPath: z.string().optional().describe("The path of the GameObject in the hierarchy (alternative to instanceId)")
}).refine(data => data.instanceId !== undefined || data.objectPath !== undefined, {
  message: "Either instanceId or objectPath must be provided"
});

// Infer the type from the schema
export type GameObjectParams = z.infer<typeof GameObjectParamsSchema>;

export function createGetGameObjectResource(mcpUnity: McpUnity, logger: Logger): ResourceDefinition {
  const resourceName = 'get_gameobject';
  const resourceMimeType = 'application/json';
  
  // Create a resource template with the MCP SDK
  const template = new ResourceTemplate('unity://gameobject/{id}', { list: undefined });
  
  return {
    name: resourceName,
    uri: template,
    metadata: {
      description: 'Retrieve a GameObject by ID or path',
      mimeType: resourceMimeType
    },
    
    // Handler with params from the template
    handler: async (params: any): Promise<ReadResourceResult> => {
      let validatedParams: GameObjectParams;
      
      // Extract and convert the parameter from path or query params
      if (params.id) {
        // Try to parse as number for instanceId, otherwise use as path
        const id = params.id;
        if (!isNaN(Number(id))) {
          validatedParams = { instanceId: Number(id) };
        } else {
          validatedParams = { objectPath: id };
        }
      } else {
        // Try to use directly provided instanceId or objectPath
        validatedParams = GameObjectParamsSchema.parse({
          instanceId: params.instanceId,
          objectPath: params.objectPath
        });
      }
      
      // Send request to Unity
      const response = await mcpUnity.sendRequest({
        method: resourceName,
        params: {
          instanceId: validatedParams.instanceId,
          objectPath: validatedParams.objectPath
        }
      });
      
      return {
        contents: [{
          uri: `unity://gameobject/${validatedParams.instanceId || validatedParams.objectPath}`,
          text: JSON.stringify(response, null, 2)
        }]
      };
    }
  };
}
