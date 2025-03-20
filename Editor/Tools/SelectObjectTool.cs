using System;
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
        /// Get the parameter schema for this tool
        /// </summary>
        /// <returns>A JObject describing the parameter schema</returns>
        protected override JObject GetParameterSchema()
        {
            return new JObject
            {
                ["objectPath"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "The path or ID of the object to select (e.g. \"Main Camera\" or a Unity object ID)",
                    ["required"] = true
                }
            };
        }
        
        /// <summary>
        /// Execute the SelectObject tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            // Extract parameters
            string objectPath = parameters["objectPath"].ToObject<string>();
            if (string.IsNullOrEmpty(objectPath))
            {
                return McpUnityBridge.CreateErrorResponse(
                    "Required parameter 'objectPath' not provided", 
                    "validation_error"
                );
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
            return new JObject
            {
                ["success"] = success,
                ["message"] = success 
                    ? $"Successfully selected object: {objectPath}" 
                    : $"Failed to find object: {objectPath}",
                ["type"] = "text"
            };
        }
    }
}
