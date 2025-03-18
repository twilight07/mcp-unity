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
    }
}
