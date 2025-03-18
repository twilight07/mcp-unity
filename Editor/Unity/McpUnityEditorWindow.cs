using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace McpUnity.Unity
{
    /// <summary>
    /// Editor window for controlling the MCP Unity Server.
    /// Provides UI for starting/stopping the server and configuring settings.
    /// </summary>
    public class McpUnityEditorWindow : EditorWindow
    {
        private string _statusMessage = "";
        private GUIStyle _headerStyle;
        private GUIStyle _subHeaderStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _wrappedLabelStyle;
        private int _selectedTab = 0;
        private string[] _tabNames = new string[] { "Server", "Help" };
        private bool _isInitialized = false;
        private string _mcpConfigJson = "";
        private bool _tabsIndentationJson = false;

        [MenuItem("Tools/MCP Unity/Server Window", false, 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<McpUnityEditorWindow>("MCP Unity");
            window.minSize = new Vector2(600, 400);
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("MCP Unity");
            _statusMessage = "Ready";
            
            // Subscribe to McpUnityBridge events
            McpUnityBridge.OnConnected += HandleConnected;
            McpUnityBridge.OnDisconnected += HandleDisconnected;
            McpUnityBridge.OnConnecting += HandleConnecting;
            McpUnityBridge.OnError += HandleError;
            
            InitializeStyles();
        }

        private void OnDisable()
        {
            // Unsubscribe from McpUnityBridge events
            McpUnityBridge.OnConnected -= HandleConnected;
            McpUnityBridge.OnDisconnected -= HandleDisconnected;
            McpUnityBridge.OnConnecting -= HandleConnecting;
            McpUnityBridge.OnError -= HandleError;
        }

        private void HandleConnected()
        {
            _statusMessage = "Connected";
            
            Repaint();
        }

        private void HandleDisconnected()
        {
            _statusMessage = "Disconnected";
            
            Repaint();
        }

        private void HandleConnecting()
        {
            _statusMessage = "Connecting...";
            
            Repaint();
        }

        private void HandleError(string errorMessage)
        {
            _statusMessage = $"Error: {errorMessage}";
            
            Repaint();
        }

        private void OnGUI()
        {
            InitializeStyles();

            EditorGUILayout.BeginVertical();
            
            // Header
            EditorGUILayout.Space();
            WrappedLabel("MCP Unity Server", _headerStyle);
            EditorGUILayout.Space();
            
            // Tabs
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space();
            
            switch (_selectedTab)
            {
                case 0: // Server tab
                    DrawServerTab();
                    break;
                case 1: // Help tab
                    DrawHelpTab();
                    break;
            }
            
            // Bottom Status bar
            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.Info);
            }
            
            // Version info at the bottom
            GUILayout.FlexibleSpace();
            WrappedLabel($"MCP Unity Server v{McpUnitySettings.ServerVersion}", EditorStyles.miniLabel, GUILayout.Width(150));
            
            EditorGUILayout.EndVertical();
        }

        #region Tab Drawing Methods

        private void DrawServerTab()
        {
            EditorGUILayout.BeginVertical("box");
            
            // Server status
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Server Status:", GUILayout.Width(120));
            
            McpUnitySettings settings = McpUnitySettings.Instance;
            McpUnityBridge mcpUnityBridge = McpUnityBridge.Instance;
            ConnectionState connectionState = mcpUnityBridge.ConnectionState;
            string statusText = connectionState == ConnectionState.Connected ? "Connected" : "Disconnected";
            Color statusColor = connectionState == ConnectionState.Connected  ? Color.green : Color.red;
            statusColor = connectionState == ConnectionState.Connecting  ? Color.yellow : statusColor;
            statusText = connectionState == ConnectionState.Connecting ? "Connecting..." : statusText;
            
            GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
            statusStyle.normal.textColor = statusColor;
            
            EditorGUILayout.LabelField(statusText, statusStyle);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            // Auto-start server option
            EditorGUILayout.BeginHorizontal();
            bool newAutoStart = EditorGUILayout.Toggle("Auto-start Server", settings.AutoStartServer);
            if (newAutoStart != settings.AutoStartServer)
            {
                settings.AutoStartServer = newAutoStart;
                settings.SaveSettings();
            }
            EditorGUILayout.EndHorizontal();
            
            // Port configuration
            EditorGUILayout.BeginHorizontal();
            int newPort = EditorGUILayout.IntField("WebSocket Port", settings.Port);
            if (newPort < 1 || newPort > 65536)
            {
                newPort = settings.Port;
                Debug.LogError($"{newPort} is an invalid port number. Please enter a number between 1 and 65535.");
            }
            
            if (newPort != settings.Port)
            {
                settings.Port = newPort;
                settings.SaveSettings();
                _ = mcpUnityBridge.Disconnect();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Server control buttons
            EditorGUILayout.BeginHorizontal();
            
            // Determine button states based on connection state
            ConnectionState currentState = mcpUnityBridge.ConnectionState;
            
            // Connect button - enabled only when disconnected
            GUI.enabled = currentState == ConnectionState.Disconnected;
            if (GUILayout.Button("Start Server", GUILayout.Height(30)))
            {
                _ = mcpUnityBridge.Connect();
            }
            
            // Disconnect button - enabled only when connected
            GUI.enabled = currentState == ConnectionState.Connected;
            if (GUILayout.Button("Stop Server", GUILayout.Height(30)))
            {
                _ = mcpUnityBridge.Disconnect();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // MCP Config generation section
            EditorGUILayout.LabelField("MCP Configuration", EditorStyles.boldLabel);

            var before = _tabsIndentationJson;
            _tabsIndentationJson = EditorGUILayout.Toggle("Use Tabs indentation", _tabsIndentationJson);
            
            if (string.IsNullOrEmpty(_mcpConfigJson) || before != _tabsIndentationJson)
            {
                GenerateMcpConfigJson();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.TextArea(_mcpConfigJson, GUILayout.Height(200));
                
            if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(30)))
            {
                EditorGUIUtility.systemCopyBuffer = _mcpConfigJson;
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawHelpTab()
        {
            WrappedLabel("About MCP Unity", _subHeaderStyle);
            EditorGUILayout.BeginVertical(_boxStyle);
            WrappedLabel("MCP Unity is a Unity Editor integration of the Model Context Protocol (MCP), which enables standardized communication between AI models and applications.");
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Open MCP Protocol Documentation"))
            {
                Application.OpenURL("https://modelcontextprotocol.io");
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            WrappedLabel("Available Tools", _subHeaderStyle);
            
            EditorGUILayout.BeginVertical(_boxStyle);
            
            WrappedLabel("execute_menu_item", EditorStyles.boldLabel);
            WrappedLabel("Executes a function that is currently tagged with MenuItem attribute in the project or in the Unity Editor's menu path");
            
            EditorGUILayout.EndVertical();
            
            // Author information
            EditorGUILayout.Space();
            WrappedLabel("Author", _subHeaderStyle);
            
            EditorGUILayout.BeginVertical(_boxStyle);
            
            WrappedLabel("Created by CoderGamester", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            WrappedLabel("For issues, feedback, or contributions, please visit:");
            
            // Begin horizontal layout for buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("GitHub: https://github.com/CoderGamester", GUILayout.Height(30)))
            {
                Application.OpenURL("https://github.com/CoderGamester");
            }
            
            if (GUILayout.Button("LinkedIn: Miguel Tomás", GUILayout.Height(30)))
            {
                Application.OpenURL("https://www.linkedin.com/in/miguel-tomas/");
            }
            
            // End horizontal layout
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void GenerateMcpConfigJson()
        {
            var config = new Dictionary<string, object>
            {
                { "mcpServers", new Dictionary<string, object>
                    {
                        { "mcp-unity", new Dictionary<string, object>
                            {
                                { "command", "node" },
                                { "args", new[] { Path.Combine(GetServerPath(), "build", "index.js") } },
                                { "env", new Dictionary<string, string>
                                    {
                                        { "PORT", McpUnitySettings.Instance.Port.ToString() }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            
            // Initialize string writer with proper indentation
            var stringWriter = new StringWriter();
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                jsonWriter.Formatting = Formatting.Indented;
                
                // Set indentation character and count
                if (_tabsIndentationJson)
                {
                    jsonWriter.IndentChar = '\t';
                    jsonWriter.Indentation = 1;
                }
                else
                {
                    jsonWriter.IndentChar = ' ';
                    jsonWriter.Indentation = 2;
                }
                
                // Serialize directly to the JsonTextWriter
                var serializer = new JsonSerializer();
                serializer.Serialize(jsonWriter, config);
            }
            
            _mcpConfigJson = stringWriter.ToString().Replace("\\", "/").Replace("//", "/");
        }

        /// <summary>
        /// Gets the absolute path to the Server directory containing package.json
        /// Works whether MCP Unity is installed via Package Manager or directly in the Assets folder
        /// </summary>
        private string GetServerPath()
        {
            // First, try to find the package info via Package Manager
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{McpUnitySettings.PackageName}");
                
            if (packageInfo != null && !string.IsNullOrEmpty(packageInfo.resolvedPath))
            {
                return Path.Combine(packageInfo.resolvedPath, "Server");
            }
            
            var assets = AssetDatabase.FindAssets("tsconfig");

            if(assets.Length == 1)
            {
                // Convert relative path to absolute path
                var relativePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
            }
            if (assets.Length > 0)
            {
                foreach (var assetJson in assets)
                {
                    string relativePath = AssetDatabase.GUIDToAssetPath(assetJson);
                    string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
                    
                    if(Path.GetFileName(Path.GetDirectoryName(fullPath)) == "Server")
                    {
                        return Path.GetDirectoryName(fullPath);
                    }
                }
            }
            
            // If we get here, we couldn't find the server path
            var errorString = "[MCP Unity] Could not locate Server directory. Please check the installation of the MCP Unity package.";

            Debug.LogError(errorString);

            return errorString;
        }

        #endregion

        #region Helper Methods

        private void InitializeStyles()
        {
            if (_isInitialized) return;
            
            // Header style
            _headerStyle = new GUIStyle(EditorStyles.largeLabel)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(0, 0, 10, 10)
            };
            
            // Sub-header style
            _subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 10, 5)
            };
            
            // Box style
            _boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 10, 10)
            };
            
            // Wrapped label style for text that needs to wrap
            _wrappedLabelStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true,
                richText = true
            };
            
            _isInitialized = true;
        }
        
        /// <summary>
        /// Creates a label with text that properly wraps based on available width
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="style">Optional style override (wordWrap will be forced true)</param>
        /// <param name="options">Layout options</param>
        private void WrappedLabel(string text, GUIStyle style = null, params GUILayoutOption[] options)
        {
            if (style == null)
            {
                // Use our predefined wrapped label style
                EditorGUILayout.LabelField(text, _wrappedLabelStyle, options);
                return;
            }
            
            // Create a temporary style with wordWrap enabled based on the provided style
            GUIStyle wrappedStyle = new GUIStyle(style)
            {
                wordWrap = true
            };
            
            EditorGUILayout.LabelField(text, wrappedStyle, options);
        }
        
        #endregion
    }
}
