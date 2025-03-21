using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// Execute the tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        /// <returns>The result of the tool execution as a JObject</returns>
        public abstract Task<JObject> ExecuteAsync(JObject parameters);
        
        /// <summary>
        /// Synchronous execution method for backward compatibility
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        /// <returns>The result of the tool execution as a JObject</returns>
        public virtual JObject Execute(JObject parameters)
        {
            // Call the async method and wait for it to complete
            return ExecuteAsync(parameters).GetAwaiter().GetResult();
        }
    }
}
