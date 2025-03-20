using System;
using McpUnity.Unity;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for managing packages in the Unity Package Manager
    /// </summary>
    public class PackageManagerTool : McpToolBase
    {
        private AddRequest _addRequest;
        
        public PackageManagerTool()
        {
            Name = "package_manager";
            Description = "Manages packages in the Unity Package Manager";
        }
        
        /// <summary>
        /// Execute the PackageManager tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            // Extract method parameter
            string method = parameters["methodSource"]?.ToObject<string>();
            if (string.IsNullOrEmpty(method))
            {
                return McpUnityBridge.CreateErrorResponse(
                    "Required parameter 'methodSource' not provided", 
                    "validation_error"
                );
            }
            
            // Process based on method
            switch (method.ToLowerInvariant())
            {
                case "registry":
                    return AddFromRegistry(parameters);
                case "github":
                    return AddFromGitHub(parameters);
                case "disk":
                    return AddFromDisk(parameters);
                default:
                    return McpUnityBridge.CreateErrorResponse(
                        $"Unknown method '{method}'. Valid methods are: registry, github, disk",
                        "validation_error"
                    );
            }
        }
        
        /// <summary>
        /// Add a package from the Unity registry
        /// </summary>
        private JObject AddFromRegistry(JObject parameters)
        {
            // Extract parameters
            string packageName = parameters["packageName"]?.ToObject<string>();
            if (string.IsNullOrEmpty(packageName))
            {
                return McpUnityBridge.CreateErrorResponse(
                    "Required parameter 'packageName' not provided for registry method", 
                    "validation_error"
                );
            }
            
            string version = parameters["version"]?.ToObject<string>();
            string packageIdentifier = packageName;
            
            // Add version if specified
            if (!string.IsNullOrEmpty(version))
            {
                packageIdentifier = $"{packageName}@{version}";
            }
            
            Debug.Log($"[MCP Unity] Adding package from registry: {packageIdentifier}");
            
            try
            {
                // Add the package
                _addRequest = Client.Add(packageIdentifier);
                
                // Wait for the request to complete
                while (!_addRequest.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                return ProcessAddRequestResult();
            }
            catch (Exception ex)
            {
                return McpUnityBridge.CreateErrorResponse(
                    $"Exception adding package: {ex.Message}",
                    "package_manager_error"
                );
            }
        }
        
        /// <summary>
        /// Add a package from GitHub
        /// </summary>
        private JObject AddFromGitHub(JObject parameters)
        {
            // Extract parameters
            string repositoryUrl = parameters["repositoryUrl"]?.ToObject<string>();
            if (string.IsNullOrEmpty(repositoryUrl))
            {
                return McpUnityBridge.CreateErrorResponse(
                    "Required parameter 'repositoryUrl' not provided for github method", 
                    "validation_error"
                );
            }
            
            string branch = parameters["branch"]?.ToObject<string>();
            string path = parameters["path"]?.ToObject<string>();
            
            // Format the package URL
            string packageUrl = repositoryUrl;
            
            // Remove any .git suffix if present
            if (packageUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                packageUrl = packageUrl.Substring(0, packageUrl.Length - 4);
            }
            
            // Add branch if specified
            if (!string.IsNullOrEmpty(branch))
            {
                packageUrl += "#" + branch;
            }
            
            // Add path if specified
            if (!string.IsNullOrEmpty(path))
            {
                if (!string.IsNullOrEmpty(branch))
                {
                    // Branch is already added, append path with slash
                    packageUrl += "/" + path;
                }
                else
                {
                    // No branch, use hash followed by path
                    packageUrl += "#" + path;
                }
            }
            
            Debug.Log($"[MCP Unity] Adding package from GitHub: {packageUrl}");
            
            try
            {
                // Add the package
                _addRequest = Client.Add(packageUrl);
                
                // Wait for the request to complete
                while (!_addRequest.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                return ProcessAddRequestResult();
            }
            catch (Exception ex)
            {
                return McpUnityBridge.CreateErrorResponse(
                    $"Exception adding package: {ex.Message}",
                    "package_manager_error"
                );
            }
        }
        
        /// <summary>
        /// Add a package from disk
        /// </summary>
        private JObject AddFromDisk(JObject parameters)
        {
            // Extract parameters
            string path = parameters["path"]?.ToObject<string>();
            if (string.IsNullOrEmpty(path))
            {
                return McpUnityBridge.CreateErrorResponse(
                    "Required parameter 'path' not provided for disk method", 
                    "validation_error"
                );
            }
            
            // Format as file URL
            string packageUrl = $"file:{path}";
            
            Debug.Log($"[MCP Unity] Adding package from disk: {packageUrl}");
            
            try
            {
                // Add the package
                _addRequest = Client.Add(packageUrl);
                
                // Wait for the request to complete
                while (!_addRequest.IsCompleted)
                {
                    System.Threading.Thread.Sleep(100);
                }
                
                return ProcessAddRequestResult();
            }
            catch (Exception ex)
            {
                return McpUnityBridge.CreateErrorResponse(
                    $"Exception adding package: {ex.Message}",
                    "package_manager_error"
                );
            }
        }
        
        /// <summary>
        /// Process the result of a package add request
        /// </summary>
        private JObject ProcessAddRequestResult()
        {
            // Check request status
            if (_addRequest.Status == StatusCode.Success)
            {
                return new JObject
                {
                    ["success"] = true,
                    ["message"] = $"Successfully added package: {_addRequest.Result.displayName} ({_addRequest.Result.name}) version {_addRequest.Result.version}",
                    ["type"] = "text",
                    ["packageInfo"] = JObject.FromObject(new
                    {
                        name = _addRequest.Result.name,
                        displayName = _addRequest.Result.displayName,
                        version = _addRequest.Result.version
                    })
                };
            }
            else if (_addRequest.Status == StatusCode.Failure)
            {
                return McpUnityBridge.CreateErrorResponse(
                    $"Failed to add package: {_addRequest.Error.message}",
                    "package_manager_error"
                );
            }
            else
            {
                return McpUnityBridge.CreateErrorResponse(
                    $"Unknown package manager status: {_addRequest.Status}",
                    "package_manager_error"
                );
            }
        }
    }
}
