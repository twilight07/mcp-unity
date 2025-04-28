using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor.TestTools.TestRunner.Api;

namespace McpUnity.Services
{
    /// <summary>
    /// Interface for the test runner service
    /// </summary>
    public interface ITestRunnerService
    {
        /// <summary>
        /// Get the TestRunnerApi instance
        /// </summary>
        TestRunnerApi TestRunnerApi { get; }
        
        /// <summary>
        /// Get a list of available tests, optionally filtered by test mode
        /// </summary>
        /// <param name="testMode">Optional test mode filter (EditMode, PlayMode, or empty for all)</param>
        /// <returns>List of test items matching the specified test mode, or all tests if no mode specified</returns>
        List<TestItemInfo> GetAllTests(string testMode = "");

        /// <summary>
        /// Execute tests with the provided parameters
        /// </summary>
        /// <param name="testMode">Test mode to run</param>
        /// <param name="testFilter">Optional test filter</param>
        /// <param name="completionSource">TaskCompletionSource to resolve when tests are complete</param>
        /// <returns>Task that resolves with test results when tests are complete</returns>
        void ExecuteTests(
            TestMode testMode,
            string testFilter,
            TaskCompletionSource<JObject> completionSource);
    }
}
