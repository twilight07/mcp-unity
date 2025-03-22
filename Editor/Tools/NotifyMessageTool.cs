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
        /// Execute the NotifyMessage tool with the provided parameters asynchronously
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override Task<JObject> ExecuteAsync(JObject parameters)
        {
            // Extract parameters
            string message = parameters["message"]?.ToObject<string>();
            if (string.IsNullOrEmpty(message))
            {
                return Task.FromResult(McpUnitySocketHandler.CreateErrorResponse(
                    "Required parameter 'message' not provided", 
                    "validation_error"
                ));
            }
            
            string messageType = parameters["type"]?.ToObject<string>()?.ToLowerInvariant() ?? "info";
            
            // Display the message in the Unity console based on the type
            switch (messageType)
            {
                case "warning":
                    Debug.LogWarning($"[MCP Unity Notification] {message}");
                    break;
                case "error":
                    Debug.LogError($"[MCP Unity Notification] {message}");
                    break;
                case "info":
                default:
                    Debug.Log($"[MCP Unity Notification] {message}");
                    break;
            }
            
            return Task.FromResult(new JObject
            {
                ["success"] = true,
                ["message"] = $"Message displayed: {message}",
                ["type"] = "text"
            });
        }
    }
}
