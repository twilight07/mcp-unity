import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ResourceDefinition } from './resourceRegistry.js';
export declare const GameObjectParamsSchema: z.ZodEffects<z.ZodObject<{
    instanceId: z.ZodOptional<z.ZodNumber>;
    objectPath: z.ZodOptional<z.ZodString>;
}, "strip", z.ZodTypeAny, {
    objectPath?: string | undefined;
    instanceId?: number | undefined;
}, {
    objectPath?: string | undefined;
    instanceId?: number | undefined;
}>, {
    objectPath?: string | undefined;
    instanceId?: number | undefined;
}, {
    objectPath?: string | undefined;
    instanceId?: number | undefined;
}>;
export type GameObjectParams = z.infer<typeof GameObjectParamsSchema>;
export declare function createGetGameObjectResource(mcpUnity: McpUnity, logger: Logger): ResourceDefinition;
