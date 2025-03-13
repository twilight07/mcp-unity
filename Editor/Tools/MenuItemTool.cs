using System;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for executing Unity Editor menu items
    /// </summary>
    public class MenuItemTool : McpToolBase
    {
        public MenuItemTool()
        {
            Name = "execute_menu_item";
            Description = "Executes functions tagged with the MenuItem attribute";
        }
        
        /// <summary>
        /// Get the parameter schema for this tool
        /// </summary>
        /// <returns>A JObject describing the parameter schema</returns>
        protected override JObject GetParameterSchema()
        {
            return new JObject
            {
                ["menuPath"] = new JObject
                {
                    ["type"] = "string",
                    ["description"] = "Path to the menu item to execute",
                    ["required"] = true
                },
                ["requireConfirmation"] = new JObject
                {
                    ["type"] = "boolean",
                    ["description"] = "Whether to require confirmation before execution",
                    ["required"] = false,
                    ["default"] = true
                }
            };
        }
        
        /// <summary>
        /// Execute the MenuItem tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            try
            {
                // Extract parameters with defaults
                var menuPath = GetParameterValue<string>(parameters?["menuPath"]);
                if (string.IsNullOrEmpty(menuPath))
                {
                    return CreateErrorResponse(
                        "Required parameter 'menuPath' not provided", 
                        "validation_error"
                    );
                }
                
                // Log the execution
                Debug.Log($"[MCP Unity] Executing menu item: {menuPath}");
                
                // Execute the menu item
                var success = EditorApplication.ExecuteMenuItem(menuPath);
                
                // Create the response
                return new JObject
                {
                    ["success"] = success,
                    ["message"] = success 
                        ? $"Successfully executed menu item: {menuPath}" 
                        : $"Failed to execute menu item: {menuPath}"
                };
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Error executing menu item: {ex.Message}");
                
                return CreateErrorResponse(
                    $"Error executing menu item: {ex.Message}",
                    "tool_execution_error",
                    new JObject
                    {
                        ["exception"] = ex.GetType().Name,
                        ["stackTrace"] = ex.StackTrace
                    }
                );
            }
        }
    }
}
