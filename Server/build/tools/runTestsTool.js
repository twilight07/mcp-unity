import { z } from 'zod';
import { McpUnityError, ErrorType } from '../utils/errors.js';
// Function to wait for a specified time
const wait = (ms) => new Promise((resolve) => setTimeout(resolve, ms));
// Ping interval in milliseconds
const PING_INTERVAL = 5000;
export function createRunTestsTool(mcpUnity, logger) {
    const toolName = 'run_tests';
    return {
        name: toolName,
        description: 'Runs Unity\'s Test Runner tests',
        paramsSchema: z.object({
            testMode: z.string().optional().describe('The test mode to run (EditMode, PlayMode, or All)'),
            testFilter: z.string().optional().describe('Optional test filter (e.g. specific test name or namespace)')
        }),
        handler: async ({ testMode = 'All', testFilter }) => {
            logger.info(`Running tests: Mode=${testMode}, Filter=${testFilter || 'none'}`);
            if (!mcpUnity.isConnected) {
                throw new McpUnityError(ErrorType.CONNECTION, 'Not connected to Unity. Please ensure Unity is running with the MCP Unity plugin enabled.');
            }
            // Create a promise for the test run
            const testRunPromise = mcpUnity.sendRequest({
                method: toolName,
                params: {
                    testMode,
                    testFilter
                }
            });
            // Setup ping interval to check Unity connection during test run
            let pingIntervalId = null;
            let isConnected = true;
            // Set up a ping interval that runs every 5 seconds
            pingIntervalId = setInterval(async () => {
                try {
                    // Ping Unity using the health endpoint
                    await mcpUnity.checkUnityConnection();
                }
                catch (error) {
                    // If ping fails, mark as disconnected
                    isConnected = false;
                    if (pingIntervalId) {
                        clearInterval(pingIntervalId);
                        pingIntervalId = null;
                    }
                    logger.error('Lost connection to Unity during test run');
                }
            }, PING_INTERVAL);
            try {
                // Wait for the test run to complete or for a connection error
                const response = await Promise.race([
                    testRunPromise,
                    // Check connection status every second
                    (async () => {
                        while (isConnected) {
                            await wait(1000);
                        }
                        throw new McpUnityError(ErrorType.CONNECTION, 'Lost connection to Unity during test run');
                    })()
                ]);
                // Clean up the ping interval
                if (pingIntervalId) {
                    clearInterval(pingIntervalId);
                }
                // Process the test results
                if (!response.success) {
                    throw new McpUnityError(ErrorType.TOOL_EXECUTION, response.message || `Failed to run tests: Mode=${testMode}, Filter=${testFilter || 'none'}`);
                }
                // Extract test results
                const testResults = response.results || [];
                const testCount = response.testCount || 0;
                const passCount = response.passCount || 0;
                // Format the result message
                let resultMessage = `${passCount}/${testCount} tests passed`;
                if (testCount > 0 && passCount < testCount) {
                    resultMessage += `. Failed tests: ${testResults
                        .filter((r) => r.result !== 'Passed')
                        .map((r) => r.name)
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
            }
            catch (error) {
                // Clean up the ping interval
                if (pingIntervalId) {
                    clearInterval(pingIntervalId);
                }
                // Handle and rethrow errors
                if (error instanceof Error) {
                    logger.error(`Test run failed: ${error.message}`);
                }
                else {
                    logger.error('Test run failed with an unknown error');
                }
                throw error;
            }
        }
    };
}
