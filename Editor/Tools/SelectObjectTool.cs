using System;
using System.Threading.Tasks;
using McpUnity.Unity;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for selecting objects in the Unity Editor
    /// </summary>
    public class SelectObjectTool : McpToolBase
    {
        public SelectObjectTool()
        {
            Name = "select_object";
            Description = "Sets the selected object in the Unity editor by path or ID";
        }
        
        /// <summary>
        /// Execute the SelectObject tool with the provided parameters asynchronously
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override Task<JObject> ExecuteAsync(JObject parameters)
        {
            // Extract parameters
            string objectPath = parameters["objectPath"]?.ToObject<string>();
            if (string.IsNullOrEmpty(objectPath))
            {
                return Task.FromResult(McpUnitySocketHandler.CreateErrorResponse(
                    "Required parameter 'objectPath' not provided", 
                    "validation_error"
                ));
            }
                
            // Log the execution
            Debug.Log($"[MCP Unity] Selecting object: {objectPath}");
            
            // Try to find the object by path in the hierarchy
            GameObject gameObject = GameObject.Find(objectPath);
            
            // If not found by path, try to find by name
            if (!gameObject)
            {
                // Find all game objects with the given name
                GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name == objectPath)
                    {
                        gameObject = obj;
                        break;
                    }
                }
            }
            
            // If still not found, check if it's a numeric ID
            if (!gameObject && int.TryParse(objectPath, out int instanceID))
            {
                UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
                if (obj is GameObject)
                {
                    gameObject = obj as GameObject;
                }
            }
            
            // Select the object if found
            bool success = false;
            if (gameObject)
            {
                Selection.activeGameObject = gameObject;
                EditorGUIUtility.PingObject(gameObject);
                success = true;
            }
                
            // Create the response
            return Task.FromResult(new JObject
            {
                ["success"] = success,
                ["message"] = success 
                    ? $"Successfully selected object: {objectPath}" 
                    : $"Failed to find object: {objectPath}",
                ["type"] = "text"
            });
        }
    }
}
