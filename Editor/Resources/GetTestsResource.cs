using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;
using McpUnity.Unity;
using McpUnity.Services;

namespace McpUnity.Resources
{
    /// <summary>
    /// Resource for getting available tests from Unity Test Runner
    /// </summary>
    public class GetTestsResource : McpResourceBase
    {
        private readonly ITestRunnerService _testRunnerService;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public GetTestsResource(ITestRunnerService testRunnerService)
        {
            Name = "get_tests";
            Description = "Gets available tests from Unity Test Runner";
            Uri = "unity://tests/{testMode}";
            
            _testRunnerService = testRunnerService;
        }
        
        /// <summary>
        /// Fetch tests based on provided parameters
        /// </summary>
        /// <param name="parameters">Resource parameters as a JObject</param>
        public override JObject Fetch(JObject parameters)
        {
            // Get filter parameters
            string testModeFilter = parameters["testMode"]?.ToObject<string>();
            
            // Get all tests from the service
            var allTests = _testRunnerService.GetAllTests(testModeFilter);
            
            // Create the results array
            var results = new JArray();
            foreach (var test in allTests)
            {
                results.Add(new JObject
                {
                    ["name"] = test.Name,
                    ["fullName"] = test.FullName,
                    ["path"] = test.Path,
                    ["testMode"] = test.TestMode,
                    ["runState"] = test.RunState
                });
            }
            
            // Return the results
            return new JObject
            {
                ["success"] = true,
                ["message"] = $"Retrieved {allTests.Count} tests",
                ["tests"] = results
            };
        }
    }
}
