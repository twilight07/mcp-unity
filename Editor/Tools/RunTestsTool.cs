using System;
using System.Threading;
using System.Threading.Tasks;
using McpUnity.Unity;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEditor.TestTools.TestRunner.Api;
using McpUnity.Services;

namespace McpUnity.Tools
{
    /// <summary>
    /// Tool for running Unity Test Runner tests
    /// </summary>
    public class RunTestsTool : McpToolBase, ICallbacks
    {
        private readonly TestRunnerService _testRunnerService;
        private bool _isRunning = false;
        private TaskCompletionSource<JObject> _testRunCompletionSource;
        private List<TestResult> _testResults = new List<TestResult>();
        
        // Structure to store test results
        private class TestResult
        {
            public string Name { get; set; }
            public string FullName { get; set; }
            public string ResultState { get; set; }
            public string Message { get; set; }
            public double Duration { get; set; }
            public bool Passed => ResultState == "Passed";
        }
        
        public RunTestsTool(TestRunnerService testRunnerService)
        {
            Name = "run_tests";
            Description = "Runs tests using Unity's Test Runner";
            
            _testRunnerService = testRunnerService;
            
            // Register callbacks with the TestRunnerApi
            _testRunnerService.TestRunnerApi.RegisterCallbacks(this);
        }
        
        /// <summary>
        /// Executes the RunTests tool asynchronously on the main thread.
        /// </summary>
        /// <param name="parameters">Tool parameters, including optional 'testMode' and 'testFilter'.</param>
        /// <param name="tcs">TaskCompletionSource to set the result or exception.</param>
        public override void ExecuteAsync(JObject parameters, TaskCompletionSource<JObject> tcs)
        {
            // Check if tests are already running
            if (_isRunning)
            {
                tcs.SetResult(McpUnitySocketHandler.CreateErrorResponse(
                    "Tests are already running. Please wait for them to complete.",
                    "test_runner_busy"
                ));
                return;
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
            _testRunCompletionSource = tcs;
            
            // Execute tests using the TestRunnerService
            _testRunnerService.ExecuteTests(
                testMode, 
                testFilter, 
                tcs
            );
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
            
            // Create test results summary
            var summary = new JObject
            {
                ["testCount"] = _testResults.Count,
                ["passCount"] = _testResults.FindAll(r => r.Passed).Count,
                ["duration"] = result.Duration,
                ["success"] = result.ResultState == "Passed",
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
            
            // Set the test run completion result
            try
            {
                _testRunCompletionSource.SetResult(summary);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MCP Unity] Failed to set test results: {ex.Message}");
                _testRunCompletionSource.TrySetException(ex);
            }
            finally
            {
                _testRunCompletionSource = null;
            }
        }
        
        #endregion
    }
}
