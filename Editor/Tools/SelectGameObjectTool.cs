using System;
using System.Threading.Tasks;
using McpUnity.Unity;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for selecting GameObjects in the Unity Editor
    /// </summary>
    public class SelectGameObjectTool : McpToolBase
    {
        public SelectGameObjectTool()
        {
            Name = "select_gameobject";
            Description = "Sets the selected GameObject in the Unity editor by path or instance ID";
        }
        
        /// <summary>
        /// Execute the SelectGameObject tool with the provided parameters asynchronously
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override Task<JObject> ExecuteAsync(JObject parameters)
        {
            // Extract parameters
            string objectPath = parameters["objectPath"]?.ToObject<string>();
            int? instanceId = parameters["instanceId"]?.ToObject<int?>();
            
            // Validate parameters - require either objectPath or instanceId
            if (string.IsNullOrEmpty(objectPath) && !instanceId.HasValue)
            {
                return Task.FromResult(McpUnitySocketHandler.CreateErrorResponse(
                    "Required parameter 'objectPath' or 'instanceId' not provided", 
                    "validation_error"
                ));
            }
            
            // First try to find by instance ID if provided
            if (instanceId.HasValue)
            {
                Debug.Log($"[MCP Unity] Selecting GameObject by instance ID: {instanceId.Value}");

                Selection.activeGameObject = EditorUtility.InstanceIDToObject(instanceId.Value) as GameObject;
            }
            // Otherwise, try to find by object path/name if provided
            else
            {
                Debug.Log($"[MCP Unity] Selecting GameObject by path: {objectPath}");
                
                // Try to find the object by path in the hierarchy
                Selection.activeGameObject = GameObject.Find(objectPath);
            }

            // Ping the selected object
            EditorGUIUtility.PingObject(Selection.activeGameObject);
                
            // Create the response
            return Task.FromResult(new JObject
            {
                ["success"] = true,
                ["message"] = $"Successfully selected GameObject" + 
                    (instanceId.HasValue ? $" with instance ID: {instanceId.Value}" : $": {objectPath}")
            });
        }
    }
}
