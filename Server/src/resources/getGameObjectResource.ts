import { McpUnity } from '../unity/mcpUnity.js';
import { Logger } from '../utils/logger.js';
import { ResourceDefinition } from './resourceRegistry.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import { ReadResourceResult } from '@modelcontextprotocol/sdk/types.js';

export function createGetGameObjectResource(mcpUnity: McpUnity, logger: Logger): ResourceDefinition {
  const resourceName = 'get_gameobject';
  const resourceUri = `unity://gameobject`;
  const resourceMimeType = 'application/json';
  
  return {
    name: resourceName,
    uri: resourceUri,
    metadata: {
      description: 'Retrieve detailed information about a specific GameObject by instance ID',
      mimeType: resourceMimeType
    },
    handler: async (params: any): Promise<ReadResourceResult> => {
      // Validate parameters
      if (!params || !params.instanceId) {
        throw new McpUnityError(
          ErrorType.VALIDATION,
          'Missing required parameter: instanceId'
        );
      }

      // Prepare parameters to send to Unity
      const requestParams = {
        instanceId: params.instanceId
      };

      // Send request to Unity
      const response = await mcpUnity.sendRequest({
        method: resourceName,
        params: requestParams
      });
      
      if (!response.success) {
        throw new McpUnityError(
          ErrorType.RESOURCE_FETCH,
          response.message || 'Failed to fetch GameObject data from Unity'
        );
      }
      
      return {
        contents: [{ 
          uri: resourceUri,
          mimeType: resourceMimeType,
          text: JSON.stringify(response.gameObject)
        }]
      };
    }
  };
}
