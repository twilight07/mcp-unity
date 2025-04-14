using System;
using System.IO;
using McpUnity.Utils;
using UnityEngine;
using UnityEditor;

namespace McpUnity.Unity
{
    /// <summary>
    /// Handles persistence of MCP Unity settings
    /// </summary>
    [Serializable]
    public class McpUnitySettings
    {
        // Constants
        public const string ServerVersion = "1.0.0";
        public const string PackageName = "com.gamelovers.mcp-unity";
        
        private static McpUnitySettings _instance;
        private static readonly string SettingsPath = "ProjectSettings/McpUnitySettings.json";

        // Server settings
        public int Port { get; set; } = 8090;
        
        [Tooltip("Whether to automatically start the MCP server when Unity opens")]
        public bool AutoStartServer = true;
        
        [Tooltip("Whether to show info logs in the Unity console")]
        public bool EnableInfoLogs = true;

        /// <summary>
        /// Singleton instance of settings
        /// </summary>
        public static McpUnitySettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new McpUnitySettings();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Private constructor for singleton
        /// </summary>
        private McpUnitySettings() 
        { 
            LoadSettings();
            VsCodeWorkspaceUtils.AddPackageCacheToWorkspace();
        }

        /// <summary>
        /// Load settings from disk
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                // Load settings from McpUnitySettings.json
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    JsonUtility.FromJsonOverwrite(json, this);
                }
                
                // Check for environment variable PORT
                string envPort = System.Environment.GetEnvironmentVariable("UNITY_PORT");
                if (!string.IsNullOrEmpty(envPort) && int.TryParse(envPort, out int port))
                {
                    Port = port;
                }
            }
            catch (Exception ex)
            {
                // Can't use LoggerService here as it depends on settings
                Debug.LogError($"[MCP Unity] Failed to load settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Save settings to disk
        /// </summary>
        public void SaveSettings()
        {
            try
            {
                // Save settings to McpUnitySettings.json
                string json = JsonUtility.ToJson(this, true);
                File.WriteAllText(SettingsPath, json);
                
                // Set environment variable PORT for the Node.js process
                // Note: This will only affect processes started after this point
                System.Environment.SetEnvironmentVariable("UNITY_PORT", Port.ToString(), System.EnvironmentVariableTarget.User);
            }
            catch (Exception ex)
            {
                // Can't use LoggerService here as it might create circular dependency
                Debug.LogError($"[MCP Unity] Failed to save settings: {ex.Message}");
            }
        }
    }
}
