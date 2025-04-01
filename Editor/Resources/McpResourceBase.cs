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
        /// The URI pattern of the resource
        /// </summary>
        public string Uri { get; protected set; }
        
        /// <summary>
        /// Whether this resource is enabled and available for use
        /// </summary>
        public bool IsEnabled { get; protected set; } = true;
        
        /// <summary>
        /// Fetch the resource data with the provided parameters
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject</param>
        /// <returns>The result of the resource fetch as a JObject</returns>
        public abstract JObject Fetch(JObject parameters);
    }
}
