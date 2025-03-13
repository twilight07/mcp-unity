using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace McpUnity.Tools
{
    /// <summary>
    /// Base class for MCP Unity tools that interact with the Unity Editor
    /// </summary>
    public abstract class McpToolBase
    {
        /// <summary>
        /// The name of the tool as used in API calls
        /// </summary>
        public string Name { get; protected set; }
        
        /// <summary>
        /// Description of the tool's functionality
        /// </summary>
        public string Description { get; protected set; }
        
        /// <summary>
        /// Whether this tool is enabled and available for use
        /// </summary>
        public bool IsEnabled { get; protected set; } = true;
        
        /// <summary>
        /// Get metadata about the tool for API documentation
        /// </summary>
        /// <returns>A JObject containing tool metadata</returns>
        public virtual JObject GetMetadata()
        {
            return new JObject
            {
                ["name"] = Name,
                ["description"] = Description,
                ["enabled"] = IsEnabled,
                ["parameters"] = GetParameterSchema()
            };
        }
        
        /// <summary>
        /// Get the parameter schema for this tool
        /// </summary>
        /// <returns>A JObject describing the parameter schema</returns>
        protected virtual JObject GetParameterSchema()
        {
            // Base implementation returns an empty schema
            // Override in derived classes to provide specific parameter schemas
            return new JObject();
        }
        
        /// <summary>
        /// Execute the tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        /// <returns>The result of the tool execution as a JObject</returns>
        public abstract JObject Execute(JObject parameters);
        
        /// <summary>
        /// Convert a JToken to a specific type with proper error handling
        /// </summary>
        /// <typeparam name="T">The type to convert to</typeparam>
        /// <param name="token">The JToken to convert</param>
        /// <param name="defaultValue">Default value if conversion fails</param>
        /// <returns>The converted value or default</returns>
        protected T GetParameterValue<T>(JToken token, T defaultValue = default)
        {
            if (token == null)
            {
                return defaultValue;
            }
            
            try
            {
                return token.ToObject<T>();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MCP Unity] Error converting parameter to {typeof(T).Name}: {ex.Message}");
                return defaultValue;
            }
        }
        
        /// <summary>
        /// Create a standardized error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errorType">Type of error</param>
        /// <param name="details">Additional error details (optional)</param>
        /// <returns>A JObject containing the error information</returns>
        protected JObject CreateErrorResponse(string message, string errorType = "tool_execution_error", JObject details = null)
        {
            var error = new JObject
            {
                ["type"] = errorType,
                ["message"] = message
            };
            
            if (details != null)
            {
                error["details"] = details;
            }
            
            return new JObject
            {
                ["error"] = error
            };
        }
        
        /// <summary>
        /// Create a standardized success response
        /// </summary>
        /// <param name="data">Response data</param>
        /// <returns>A JObject containing the response data</returns>
        protected JObject CreateSuccessResponse(JObject data)
        {
            return data;
        }
    }
}
