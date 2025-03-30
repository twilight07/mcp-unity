import { z } from 'zod';
import { Logger } from '../utils/logger.js';
import { McpUnity } from '../unity/mcpUnity.js';
import { ToolDefinition } from './toolRegistry.js';
import { McpUnityError, ErrorType } from '../utils/errors.js';
import { CallToolResult } from '@modelcontextprotocol/sdk/types.js';

export function createAddPackageTool(mcpUnity: McpUnity, logger: Logger): ToolDefinition {
  const toolName = 'add_package';
  return {
    name: toolName,
    description: 'Adds packages into the Unity Package Manager',
    paramsSchema: z.object({
      source: z.string().describe('The source to use (registry, github, or disk) to add the package'),
      packageName: z.string().optional().describe('The package name to add from Unity registry (e.g. com.unity.textmeshpro)'),
      version: z.string().optional().describe('The version to use for registry packages (optional)'),
      repositoryUrl: z.string().optional().describe('The GitHub repository URL (e.g. https://github.com/username/repo.git)'),
      branch: z.string().optional().describe('The branch to use for GitHub packages (optional)'),
      path: z.string().optional().describe('The path to use (folder path for disk method or subfolder for GitHub)')
    }),
    handler: async (params): Promise<CallToolResult> => {
      const { source, packageName, version, repositoryUrl, branch, path } = params;
            
      // Validate required parameters based on method
      if (source === 'registry' && !packageName) {
        throw new McpUnityError(
          ErrorType.VALIDATION,
          'Required parameter "packageName" not provided for registry source'
        );
      } else if (source === 'github' && !repositoryUrl) {
        throw new McpUnityError(
          ErrorType.VALIDATION,
          'Required parameter "repositoryUrl" not provided for github source'
        );
      } else if (source === 'disk' && !path) {
        throw new McpUnityError(
          ErrorType.VALIDATION,
          'Required parameter "path" not provided for disk source'
        );
      }
      
      // Send to Unity
      const response = await mcpUnity.sendRequest({
        method: toolName,
        params
      });
      
      if (!response.success) {
        throw new McpUnityError(
          ErrorType.TOOL_EXECUTION,
          response.message || `Failed to manage package with source: ${source}`
        );
      }
      
      return {
        success: true,
        message: response.message,
        content: [{
          type: response.type,
          text: response.message
        }]
      };
    }
  };
}
