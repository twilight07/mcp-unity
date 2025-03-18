using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace McpUnity.Resources
{
    /// <summary>
    /// Base class for MCP Unity resources that provide data from the Unity Editor
    /// </summary>
    public abstract class McpResourceBase
    {
        /// <summary>
        /// The name of the resource as used in API calls
        /// </summary>
        public string Name { get; protected set; }
        
        /// <summary>
        /// Description of the resource's functionality
        /// </summary>
        public string Description { get; protected set; }
        
        /// <summary>
        /// Whether this resource is enabled and available for use
        /// </summary>
        public bool IsEnabled { get; protected set; } = true;
        
        /// <summary>
        /// Get metadata about the resource for API documentation
        /// </summary>
        /// <returns>A JObject containing resource metadata</returns>
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
        /// Get the parameter schema for this resource
        /// </summary>
        /// <returns>A JObject describing the parameter schema</returns>
        protected virtual JObject GetParameterSchema()
        {
            // Base implementation returns an empty schema
            // Override in derived classes to provide specific parameter schemas
            return new JObject();
        }
        
        /// <summary>
        /// Fetch the resource data with the provided parameters
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject</param>
        /// <returns>The result of the resource fetch as a JObject</returns>
        public abstract JObject Fetch(JObject parameters);
        
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
    }
}
