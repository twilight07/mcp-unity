using System;
using McpUnity.Unity;
using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for running Unity Test Runner tests
    /// </summary>
    public class RunTestsTool : McpToolBase, ICallbacks
    {
        private readonly TestRunnerApi _testRunnerApi;
        private bool _isRunning = false;
        private string _requestId = null;
        private List<TestResult> _testResults = new List<TestResult>();
        
        // Structure to store test results
        private class TestResult
        {
            public string Name { get; set; }
            public string FullName { get; set; }
            public string ResultState { get; set; }
            public string Message { get; set; }
            public float Duration { get; set; }
            public bool Passed => ResultState == "Passed";
        }
        
        public RunTestsTool()
        {
            Name = "run_tests";
            Description = "Runs tests using Unity's Test Runner";
            _testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        }
        
        /// <summary>
        /// Execute the RunTests tool with the provided parameters
        /// </summary>
        /// <param name="parameters">Tool parameters as a JObject</param>
        public override JObject Execute(JObject parameters)
        {
            // Check if tests are already running
            if (_isRunning)
            {
                return McpUnityBridge.CreateErrorResponse(
                    "Tests are already running. Please wait for them to complete.",
                    "test_runner_busy"
                );
            }
            
            // Extract parameters
            string testModeStr = parameters["testMode"]?.ToObject<string>() ?? "editmode";
            string testFilter = parameters["testFilter"]?.ToObject<string>() ?? "";
            
            // Parse test mode
            TestMode testMode;
            switch (testModeStr.ToLowerInvariant())
            {
                case "playmode":
                    testMode = TestMode.PlayMode;
                    break;
                case "editmode":
                    testMode = TestMode.EditMode;
                    break;
                default:
                    testMode = TestMode.EditMode;
                    break;
            }
            
            // Log the execution
            Debug.Log($"[MCP Unity] Running tests: Mode={testMode}, Filter={testFilter}");
            
            // Reset state
            _isRunning = true;
            _testResults.Clear();
            _requestId = Guid.NewGuid().ToString();
            
            // Set up filter
            var filter = new Filter
            {
                testMode = testMode
            };
            
            // Apply name filter if provided
            if (!string.IsNullOrEmpty(testFilter))
            {
                filter.testNames = new[] { testFilter };
                filter.categoryNames = new[] { testFilter };
            }
            
            try
            {
                // Run tests with provided filter
                _testRunnerApi.RegisterCallbacks(this);
                _testRunnerApi.Execute(new ExecutionSettings(filter));
                
                // Return immediate response
                return new JObject
                {
                    ["success"] = true,
                    ["message"] = $"Test run started: Mode={testMode}, Filter={testFilter}",
                    ["type"] = "text",
                    ["requestId"] = _requestId,
                    ["status"] = "running"
                };
            }
            catch (Exception ex)
            {
                _isRunning = false;
                _testRunnerApi.UnregisterCallbacks(this);
                
                return McpUnityBridge.CreateErrorResponse(
                    $"Failed to start test run: {ex.Message}",
                    "test_runner_error"
                );
            }
        }
        
        #region ICallbacks Implementation
        
        // Called when a test run starts
        public void RunStarted(ITestAdaptor testsToRun)
        {
            Debug.Log($"[MCP Unity] Test run started: {testsToRun.Name}");
        }
        
        // Called when a test runs
        public void TestStarted(ITestAdaptor test)
        {
            // Nothing to do here
        }
        
        // Called when a test finishes
        public void TestFinished(ITestResultAdaptor result)
        {
            _testResults.Add(new TestResult
            {
                Name = result.Test.Name,
                FullName = result.FullName,
                ResultState = result.ResultState,
                Message = result.Message,
                Duration = result.Duration
            });
            
            Debug.Log($"[MCP Unity] Test finished: {result.Test.Name} - {result.ResultState}");
        }
        
        // Called when a test run completes
        public void RunFinished(ITestResultAdaptor result)
        {
            Debug.Log($"[MCP Unity] Test run completed: {result.Test.Name} - {result.ResultState}");
            
            _isRunning = false;
            _testRunnerApi.UnregisterCallbacks(this);
            
            // Create test results summary
            var summary = new JObject
            {
                ["testCount"] = _testResults.Count,
                ["passCount"] = _testResults.FindAll(r => r.Passed).Count,
                ["duration"] = result.Duration,
                ["success"] = result.ResultState == "Passed",
                ["requestId"] = _requestId,
                ["status"] = "completed",
                ["message"] = $"Test run completed: {result.Test.Name} - {result.ResultState}"
            };
            
            // Add test results array
            var resultArray = new JArray();
            foreach (var testResult in _testResults)
            {
                resultArray.Add(new JObject
                {
                    ["name"] = testResult.Name,
                    ["fullName"] = testResult.FullName,
                    ["result"] = testResult.ResultState,
                    ["message"] = testResult.Message,
                    ["duration"] = testResult.Duration
                });
            }
            summary["results"] = resultArray;
            
            // Send results through WebSocket
            try
            {
                var message = new JObject
                {
                    ["id"] = _requestId,
                    ["type"] = "notification",
                    ["method"] = "test_run_completed",
                    ["params"] = summary
                };
                
                // Would normally send this through the WebSocket, but we'll log it for now
                Debug.Log($"[MCP Unity] Test results: {message}");
                
                // In a real implementation, we would send this to the Node.js server
                // McpUnityBridge.Instance.SendNotification("test_run_completed", summary);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Failed to send test results: {ex.Message}");
            }
        }
        
        #endregion
    }
}
