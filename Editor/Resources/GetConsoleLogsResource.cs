using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for retrieving all logs from the Unity console
    /// </summary>
    public class GetConsoleLogsResource : McpResourceBase
    {
        // Structure to store log information
        private class LogEntry
        {
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public LogType Type { get; set; }
            public DateTime Timestamp { get; set; }
        }
        
        // Collection to store all log messages
        private readonly List<LogEntry> _logEntries = new List<LogEntry>();

        public GetConsoleLogsResource()
        {
            Name = "get_console_logs";
            Description = "Retrieves all logs from the Unity console";
            Uri = "unity://logs";
            
            // Register for log messages
            Application.logMessageReceived += OnLogMessageReceived;

#if UNITY_6000_0_OR_NEWER
            // Unity 6 specific implementation
            ConsoleWindowUtility.consoleLogsChanged += OnConsoleCountChanged;
#else
            // Unity 2022.3 implementation using reflection
            EditorApplication.update += CheckConsoleClearViaReflection;
#endif
            
            Debug.Log("[MCP Unity] Console logs resource initialized");
        }

        /// <summary>
        /// Fetch all logs from the Unity console
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject (not used)</param>
        /// <returns>A JObject containing the list of logs</returns>
        public override JObject Fetch(JObject parameters)
        {
            // Convert log entries to a JSON array
            JArray logsArray = new JArray();
            
            lock (_logEntries)
            {
                foreach (var entry in _logEntries)
                {
                    logsArray.Add(new JObject
                    {
                        ["message"] = entry.Message,
                        ["stackTrace"] = entry.StackTrace,
                        ["type"] = entry.Type.ToString(),
                        ["timestamp"] = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")
                    });
                }
            }
            
            // Create the response
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Retrieved {logsArray.Count} log entries",
                ["logs"] = logsArray
            };
        }

        /// <summary>
        /// Check if console was cleared using reflection (for Unity 2022.3)
        /// </summary>
        private void CheckConsoleClearViaReflection()
        {
            try
            {
                // Get current log counts using LogEntries (internal Unity API)
                var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
                if (logEntriesType == null) return;
                
                var getCountMethod = logEntriesType.GetMethod("GetCount",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (getCountMethod == null) return;
                
                int currentTotalCount = (int)getCountMethod.Invoke(null, null);
                        
                // If we had logs before, but now we don't, console was likely cleared
                if (currentTotalCount == 0 && _logEntries.Count > 0)
                {
                    ClearLogs();
                }
            }
            catch (Exception ex)
            {
                // Just log the error but don't break functionality
                Debug.LogError($"[MCP Unity] Error checking console clear: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Callback for when a log message is received
        /// </summary>
        /// <param name="logString">The log message</param>
        /// <param name="stackTrace">The stack trace</param>
        /// <param name="type">The log type</param>
        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            // Add the log entry to our collection
            _logEntries.Add(new LogEntry
            {
                Message = logString,
                StackTrace = stackTrace,
                Type = type,
                Timestamp = DateTime.Now
            });
        }
        
#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Called when the console logs count changes
        /// </summary>
        private void OnConsoleCountChanged()
        {
            ConsoleWindowUtility.GetConsoleLogCounts(out int error, out int warning, out int log);
            if (error == 0 && warning == 0 && log == 0 && _logEntries.Count > 0)
            {
                ClearLogs();
            }
        }
#endif
        
        /// <summary>
        /// Clear all stored log entries
        /// </summary>
        private void ClearLogs()
        {
            lock (_logEntries)
            {
                _logEntries.Clear();
            }
            
            Debug.Log("[MCP Unity] Console logs cleared");
        }
    }
}
