using System.Threading.Tasks;
using McpUnity.Unity;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for sending notification messages to the Unity console
    /// </summary>
    public class NotifyMessageTool : McpToolBase
    {
        public NotifyMessageTool()
        {
            Name = "notify_message";
            Description = "Sends a message to the Unity console";
        }
        
        /// <summary>
        /// Execute the NotifyMessage tool with the provided parameters synchronously
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            // Extract parameters
            string message = parameters["message"]?.ToObject<string>();
            string type = parameters["type"]?.ToObject<string>()?.ToLower() ?? "info";
 
            if (string.IsNullOrEmpty(message))
            {
                return McpUnitySocketHandler.CreateErrorResponse(
                    "Required parameter 'message' not provided", 
                    "validation_error"
                );
            }
 
            // Log the message based on type
            switch (type)
            {
                case "error":
                    Debug.LogError($"[MCP]: {message}");
                    break;
                case "warning":
                    Debug.LogWarning($"[MCP]: {message}");
                    break;
                default:
                    Debug.Log($"[MCP]: {message}");
                    break;
            }
 
            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Message displayed: {message}",
                ["type"] = "text"
            };
        }
    }
}
