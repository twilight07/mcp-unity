import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ToolDefinition } from './toolRegistry.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import { CallToolResult } from '@modelcontextprotocol/sdk/types.js';

// Function to wait for a specified time
const wait = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

// Ping interval in milliseconds
const PING_INTERVAL = 5000;

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

      // Setup ping interval to check Unity connection during test run
      let pingIntervalId: NodeJS.Timeout | null = null;
      
      try {
        // Set up a ping interval that runs every 5 seconds
        pingIntervalId = setInterval(async () => {
          try {
            // Ping Unity using the MCP ping method
            const connected = await mcpUnity.ping();
            if (!connected) {
              logger.error('Lost connection to Unity during test run');
              if (pingIntervalId) {
                clearInterval(pingIntervalId);
                pingIntervalId = null;
              }
            }
          } catch (error) {
            // If ping fails, log error
            logger.error(`Error pinging Unity: ${error instanceof Error ? error.message : String(error)}`);
          }
        }, PING_INTERVAL);
        
        // Create and wait for the test run
        const response = await mcpUnity.sendRequest({
          method: toolName,
          params: { 
            testMode,
            testFilter
          }
        });

        // Tests completed successfully
        if (pingIntervalId) {
          clearInterval(pingIntervalId);
        }
        
        // Process the test results
        if (!response.success) {
          throw new McpUnityError(
            ErrorType.TOOL_EXECUTION,
            response.message || `Failed to run tests: Mode=${testMode}, Filter=${testFilter || 'none'}`
          );
        }
        
        // Extract test results
        const testResults = response.results || [];
        const testCount = response.testCount || 0;
        const passCount = response.passCount || 0;
        
        // Format the result message
        let resultMessage = `${passCount}/${testCount} tests passed`;
        if (testCount > 0 && passCount < testCount) {
          resultMessage += `. Failed tests: ${testResults
            .filter((r: any) => r.result !== 'Passed')
            .map((r: any) => r.name)
            .join(', ')}`;
        }
        
        return {
          success: response.success,
          message: resultMessage,
          content: [
            {
              type: 'text',
              text: response.message || resultMessage
            },
            {
              type: 'text',
              text: JSON.stringify({
                duration: response.duration,
                testCount,
                passCount,
                tests: testResults
              }, null, 2)
            }
          ]
        };
      } catch (error) {
        // Clean up ping interval if it exists
        if (pingIntervalId) {
          clearInterval(pingIntervalId);
        }
        
        // Handle any errors that occurred
        const errorMessage = error instanceof Error ? error.message : String(error);
        logger.error(`Error running tests: ${errorMessage}`);
        
        throw new McpUnityError(
          ErrorType.TOOL_EXECUTION,
          `Failed to run tests: ${errorMessage}`
        );
      }
    }
  };
}
