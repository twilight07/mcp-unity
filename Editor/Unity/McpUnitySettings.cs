using System;
using System.IO;
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
        public bool AutoStartServer = true;
        public string WebSocketUrl = "ws://localhost:8080";
        
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
                    _instance.LoadSettings();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Private constructor for singleton
        /// </summary>
        private McpUnitySettings() { }

        /// <summary>
        /// Load settings from disk
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    JsonUtility.FromJsonOverwrite(json, this);
                }
            }
            catch (Exception ex)
            {
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
                string json = JsonUtility.ToJson(this, true);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Failed to save settings: {ex.Message}");
            }
        }
    }
}
