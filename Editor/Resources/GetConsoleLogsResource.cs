using Newtonsoft.Json.Linq;
using McpUnity.Services;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for retrieving all logs from the Unity console
    /// </summary>
    public class GetConsoleLogsResource : McpResourceBase
    {
        private readonly IConsoleLogsService _consoleLogsService;

        public GetConsoleLogsResource(IConsoleLogsService consoleLogsService)
        {
            Name = "get_console_logs";
            Description = "Retrieves logs from the Unity console, optionally filtered by type (error, warning, info)";
            Uri = "unity://logs/{logType}";
            
            _consoleLogsService = consoleLogsService;
        }

        /// <summary>
        /// Fetch logs from the Unity console, optionally filtered by type
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject (may include 'logType')</param>
        /// <returns>A JObject containing the list of logs</returns>
        public override JObject Fetch(JObject parameters)
        {
            string logType = null;
            if (parameters != null && parameters.ContainsKey("logType") && parameters["logType"] != null)
            {
                logType = parameters["logType"].ToString()?.ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(logType))
                {
                    logType = null;
                }
            }

            JArray logsArray = _consoleLogsService.GetAllLogsAsJson(logType);

            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Retrieved {logsArray.Count} log entries" + (logType != null ? $" of type '{logType}'" : ""),
                ["logs"] = logsArray
            };
        }


    }
}
