import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ReadResourceResult } from '@modelcontextprotocol/sdk/types.js';
import { ResourceDefinition } from './resourceRegistry.js';
import { ResourceTemplate } from '@modelcontextprotocol/sdk/server/mcp.js';

export interface TestItem {
  name: string;
  fullName: string;
  path: string;
  testMode: string;
  runState: string;
}

// Define the parameter schema using zod
export const TestsParamsSchema = z.object({
  testMode: z.string().optional().describe("Test mode to filter (EditMode or PlayMode)").default('EditMode'),
  nameFilter: z.string().optional().describe("Filter tests by name")
});

export function createGetTestsResource(mcpUnity: McpUnity, logger: Logger): ResourceDefinition {
  const resourceName = 'get_tests';
  const resourceMimeType = 'application/json';
  
  // Create the resource definition
  return {
    name: resourceName,
    uri: new ResourceTemplate('tests://{testMode}/{nameFilter}', { list: undefined }),
    metadata: {
      description: "Gets the list of available tests from Unity's Test Runner",
      mimeType: resourceMimeType
    },
    
    // Handler for the resource
    handler: async (params: any): Promise<ReadResourceResult> => {
      // Extract query parameters from the URL if available
      const testMode = params?.testMode || params?.mode || 'EditMode';
      const nameFilter = params?.nameFilter || params?.filter || '';
      
      // Validate parameters using Zod schema
      const validatedParams = TestsParamsSchema.parse({
        testMode,
        nameFilter
      });
      
      const response = await mcpUnity.sendRequest({
        method: resourceName,
        params: {
          testMode: validatedParams.testMode,
          nameFilter: validatedParams.nameFilter
        }
      });
      
      return {
        contents: [{
          uri: `tests://${validatedParams.testMode}/${validatedParams.nameFilter}`,
          mimeType: resourceMimeType,
          text: JSON.stringify(response, null, 2)
        }]
      };
    }
  };
}
