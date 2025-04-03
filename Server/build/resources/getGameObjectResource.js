import { z } from 'zod';
import { ResourceTemplate } from '@modelcontextprotocol/sdk/server/mcp.js';
// Define the parameter schema using zod
export const GameObjectParamsSchema = z.object({
    instanceId: z.number().optional().describe("The instance ID of the GameObject to retrieve"),
    objectPath: z.string().optional().describe("The path of the GameObject in the hierarchy (alternative to instanceId)")
}).refine(data => data.instanceId !== undefined || data.objectPath !== undefined, {
    message: "Either instanceId or objectPath must be provided"
});
export function createGetGameObjectResource(mcpUnity, logger) {
    const resourceName = 'get_gameobject';
    const resourceUri = 'unity://gameobject/{id}';
    const resourceMimeType = 'application/json';
    // Create a resource template with the MCP SDK
    const template = new ResourceTemplate(resourceUri, {
        list: () => listGameObjects(mcpUnity, logger, resourceMimeType)
    });
    return {
        name: resourceName,
        uri: template,
        metadata: {
            description: 'Retrieve a GameObject by ID or path',
            mimeType: resourceMimeType
        },
        // Handler with params from the template
        handler: async (params) => {
            let validatedParams;
            // Extract and convert the parameter from path or query params
            if (params.id) {
                // Try to parse as number for instanceId, otherwise use as path
                const id = params.id;
                if (!isNaN(Number(id))) {
                    validatedParams = { instanceId: Number(id) };
                }
                else {
                    validatedParams = { objectPath: id };
                }
            }
            else {
                // Try to use directly provided instanceId or objectPath
                validatedParams = GameObjectParamsSchema.parse({
                    instanceId: params.instanceId,
                    objectPath: params.objectPath
                });
            }
            // Send request to Unity
            const response = await mcpUnity.sendRequest({
                method: resourceName,
                params: {
                    instanceId: validatedParams.instanceId,
                    objectPath: validatedParams.objectPath
                }
            });
            return {
                contents: [{
                        uri: `unity://gameobject/${validatedParams.instanceId || validatedParams.objectPath}`,
                        text: JSON.stringify(response, null, 2)
                    }]
            };
        }
    };
}
/**
 * Get a list of all GameObjects in the scene
 * @param mcpUnity The McpUnity instance to communicate with Unity
 * @param logger The logger instance
 * @param resourceMimeType The MIME type for the resource
 * @returns A promise that resolves to a list of GameObject resources
 */
async function listGameObjects(mcpUnity, logger, resourceMimeType) {
    const hierarchyResponse = await mcpUnity.sendRequest({
        method: 'get_hierarchy',
        params: {}
    });
    if (!hierarchyResponse.success) {
        logger.error(`Failed to fetch hierarchy: ${hierarchyResponse.message}`);
        throw new Error(hierarchyResponse.message || 'Failed to fetch hierarchy');
    }
    // Process the hierarchy to create a list of GameObject references
    const gameObjects = processHierarchyToGameObjectList(hierarchyResponse.hierarchy || []);
    // Create resources array with both instance ID and path URIs
    const resources = [];
    // Add resources for each GameObject
    gameObjects.forEach(obj => {
        // Add resource with instance ID URI
        resources.push({
            uri: `unity://gameobject/${obj.instanceId}`,
            name: obj.name,
            description: `GameObject with instance ID ${obj.instanceId} at path: ${obj.path}`,
            mimeType: resourceMimeType
        });
        // Add resource with path URI if path exists
        if (obj.path) {
            resources.push({
                uri: `unity://gameobject/${encodeURIComponent(obj.path)}`,
                name: obj.name,
                description: `GameObject with instance ID ${obj.instanceId} at path: ${obj.path}`,
                mimeType: resourceMimeType
            });
        }
    });
    return { resources };
}
/**
 * Process the hierarchy data to create a list of GameObject references
 * @param hierarchyData The hierarchy data from Unity
 * @returns An array of GameObject references with their instance IDs and paths
 */
function processHierarchyToGameObjectList(hierarchyData) {
    const gameObjects = [];
    // Helper function to traverse the hierarchy recursively
    function traverseHierarchy(node, path = '') {
        if (!node)
            return;
        // Current path is parent path + node name
        const currentPath = path ? `${path}/${node.name}` : node.name;
        // Add this GameObject to the list
        gameObjects.push({
            instanceId: node.instanceId,
            name: node.name,
            path: currentPath,
            active: node.active,
            uri: `unity://gameobject/${node.instanceId}`
        });
        // Process children recursively
        if (node.children && Array.isArray(node.children)) {
            for (const child of node.children) {
                traverseHierarchy(child, currentPath);
            }
        }
    }
    // Start traversal with each root GameObject
    if (Array.isArray(hierarchyData)) {
        for (const rootNode of hierarchyData) {
            traverseHierarchy(rootNode);
        }
    }
    return gameObjects;
}
