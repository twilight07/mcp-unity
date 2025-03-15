import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpServer } from '@modelcontextprotocol/sdk/server/mcp.js';
export interface ToolResponse {
    content: Array<{
        type: "text";
        text: string;
    } | {
        type: "image";
        data: string;
        mimeType: string;
    } | {
        type: "resource";
        resource: {
            text: string;
            uri: string;
            mimeType?: string;
        } | {
            uri: string;
            blob: string;
            mimeType?: string;
        };
    }>;
    [key: string]: unknown;
}
export interface ToolDefinition {
    name: string;
    description: string;
    parameters: z.ZodObject<any>;
    handler: (params: any) => Promise<ToolResponse>;
}
export declare class ToolRegistry {
    private tools;
    private logger;
    constructor(logger: Logger);
    add(tool: ToolDefinition): void;
    getAll(): ToolDefinition[];
    registerWithServer(server: McpServer): void;
}
