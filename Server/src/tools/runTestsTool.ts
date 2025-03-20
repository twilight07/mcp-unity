import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ToolDefinition } from './toolRegistry.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import { CallToolResult } from '@modelcontextprotocol/sdk/types.js';

export function createRunTestsTool(mcpUnity: McpUnity, logger: Logger): ToolDefinition {
  const toolName = 'run_tests';
  return {
    name: toolName,
    description: 'Runs Unity\'s Test Runner tests',
    paramsSchema: z.object({
      testMode: z.string().optional().describe('The test mode to run (EditMode, PlayMode, or All)'),
      testFilter: z.string().optional().describe('Optional test filter (e.g. specific test name or namespace)')
    }),
    handler: async ({ testMode = 'All', testFilter }): Promise<CallToolResult> => {
      logger.info(`Running tests: Mode=${testMode}, Filter=${testFilter || 'none'}`);
      
      if (!mcpUnity.isConnected) {
        throw new McpUnityError(
          ErrorType.CONNECTION, 
          'Not connected to Unity. Please ensure Unity is running with the MCP Unity plugin enabled.'
        );
      }
      
      const response = await mcpUnity.sendRequest({
        method: toolName,
        params: { 
          testMode,
          testFilter
        }
      });
      
      if (!response.success) {
        throw new McpUnityError(
          ErrorType.TOOL_EXECUTION,
          response.message || `Failed to run tests: Mode=${testMode}, Filter=${testFilter || 'none'}`
        );
      }
      
      return {
        success: true,
        message: response.message,
        content: [{
          type: response.type,
          text: response.message || `Test run started: Mode=${testMode}, Filter=${testFilter || 'none'}` 
        }]
      };
    }
  };
}
