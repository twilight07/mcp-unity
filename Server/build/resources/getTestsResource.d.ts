import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ResourceDefinition } from './resourceRegistry.js';
export interface TestItem {
    name: string;
    fullName: string;
    path: string;
    testMode: string;
    runState: string;
}
export declare function createGetTestsResource(mcpUnity: McpUnity, logger: Logger): ResourceDefinition;
