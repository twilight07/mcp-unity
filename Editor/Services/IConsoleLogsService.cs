using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace McpUnity.Services
{
    /// <summary>
    /// Interface for the console logs service
    /// </summary>
    public interface IConsoleLogsService
    {
        /// <summary>
        /// Get all logs as a JSON array
        /// </summary>
        /// <returns>JArray containing all logs</returns>
        JArray GetAllLogsAsJson();
        
        /// <summary>
        /// Start listening for logs
        /// </summary>
        void StartListening();
        
        /// <summary>
        /// Stop listening for logs
        /// </summary>
        void StopListening();
    }
}
