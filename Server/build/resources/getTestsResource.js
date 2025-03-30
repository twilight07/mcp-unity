import { McpUnityError, ErrorType } from '../utils/errors.js';
export function createGetTestsResource(mcpUnity, logger) {
    const resourceName = 'get_tests';
    const resourceUri = `unity://tests`;
    const resourceMimeType = 'application/json';
    return {
        name: resourceName,
        uri: resourceUri,
        metadata: {
            description: "Gets the list of available tests from Unity's Test Runner",
            mimeType: resourceMimeType
        },
        handler: async (params) => {
            const { testMode, nameFilter } = params;
            const response = await mcpUnity.sendRequest({
                method: resourceName,
                params: {
                    testMode,
                    nameFilter
                }
            });
            if (!response.success && response.error) {
                throw new McpUnityError(ErrorType.RESOURCE_FETCH, response.error.message || 'Failed to fetch tests from Unity');
            }
            const testsResponse = {
                tests: response.tests || [],
                count: response.count || 0,
                editModeCount: response.editModeCount || 0,
                playModeCount: response.playModeCount || 0
            };
            // Format the test data as JSON
            const testDataJson = JSON.stringify(testsResponse, null, 2);
            return {
                _meta: {
                    test_count: testsResponse.count,
                    edit_mode_count: testsResponse.editModeCount,
                    play_mode_count: testsResponse.playModeCount
                },
                contents: [
                    {
                        uri: resourceUri,
                        text: testDataJson,
                        mimeType: resourceMimeType
                    }
                ]
            };
        }
    };
}
