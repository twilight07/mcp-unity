import * as z from "zod";
import { McpUnityError, ErrorType } from "../utils/errors.js";
// Constants for the tool
const toolName = "get_console_logs";
const toolDescription = "Retrieves logs from the Unity console";
const paramsSchema = z.object({
    logType: z
        .enum(["info", "warning", "error"])
        .optional()
        .describe("The type of logs to retrieve (info, warning, error) - defaults to all logs if not specified"),
});
/**
 * Creates and registers the Get Console Logs tool with the MCP server
 * This tool allows retrieving messages from the Unity console
 *
 * @param server The MCP server instance to register with
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance for diagnostic information
 */
export function registerGetConsoleLogsTool(server, mcpUnity, logger) {
    logger.info(`Registering tool: ${toolName}`);
    // Register this tool with the MCP server
    server.tool(toolName, toolDescription, paramsSchema.shape, async (params) => {
        try {
            logger.info(`Executing tool: ${toolName}`, params);
            const result = await toolHandler(mcpUnity, params);
            logger.info(`Tool execution successful: ${toolName}`);
            return result;
        }
        catch (error) {
            logger.error(`Tool execution failed: ${toolName}`, error);
            throw error;
        }
    });
}
/**
 * Handles requests for Unity console logs
 *
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param params The parameters for the tool
 * @returns A promise that resolves to the tool execution result
 * @throws McpUnityError if the request to Unity fails
 */
async function toolHandler(mcpUnity, params) {
    const { logType } = params;
    // Send request to Unity using the same method name as the resource
    // This allows reusing the existing Unity-side implementation
    const response = await mcpUnity.sendRequest({
        method: "get_console_logs",
        params: {
            logType: logType,
        },
    });
    if (!response.success) {
        throw new McpUnityError(ErrorType.TOOL_EXECUTION, response.message || "Failed to fetch logs from Unity");
    }
    return {
        content: [
            {
                type: "text",
                text: JSON.stringify(response.data || response.logs || response, null, 2),
            },
        ],
    };
}
