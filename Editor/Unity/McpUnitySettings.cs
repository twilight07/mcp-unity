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
        private static readonly string PortFileName = "port.txt";

        // Server settings
        public bool AutoStartServer = true;
        public int Port = 8080;
        
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
        /// Get the absolute path to the port.txt file
        /// </summary>
        private string GetPortFilePath()
        {
            // Try to find the script's location
            var scriptPath = AssetDatabase.FindAssets($"t:Script {nameof(McpUnitySettings)}");
            if (scriptPath != null && scriptPath.Length > 0)
            {
                // Get the path to the script
                var assetPath = AssetDatabase.GUIDToAssetPath(scriptPath[0]);
                
                // Get the full path to the script
                var editorDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath));
                
                // Navigate up to the Editor parent directory
                do
                {
                    editorDir = Path.GetDirectoryName(editorDir);
                } while (!editorDir.EndsWith("Editor"));
                editorDir = Path.GetDirectoryName(editorDir);
                
                return Path.Combine(editorDir, PortFileName);
            }
            
            // Fallback to project root
            return Path.Combine(Path.GetDirectoryName(Application.dataPath), PortFileName);
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
                
                // Load port from port.txt if it exists
                string portFilePath = GetPortFilePath();
                
                if (File.Exists(portFilePath))
                {
                    string portStr = File.ReadAllText(portFilePath).Trim();
                    if (int.TryParse(portStr, out int port))
                    {
                        Port = port;
                    }
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
                // Save settings to McpUnitySettings.json
                string json = JsonUtility.ToJson(this, true);
                File.WriteAllText(SettingsPath, json);
                
                // Save port to port.txt
                File.WriteAllText(GetPortFilePath(), Port.ToString());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Failed to save settings: {ex.Message}");
            }
        }
    }
}
