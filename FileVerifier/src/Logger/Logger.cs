using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using AvaloniaDraft.FileManager;
using System.Linq;

namespace Avalonia.Logger;

class Logger
{
    /// <summary>
    /// A result of a single comparison test
    /// </summary>
    public struct TestResult
    {
        public bool pass { get; set; }
        public double? percentage { get; set; }

        public TestResult(bool pass, double? percentage)
        {
            this.pass = pass;
            this.percentage = percentage;
        }
    }

    /// <summary>
    /// A result of comparing two files
    /// </summary>
    public struct ComparisonResult
    {
        public FilePair filePair { get; set; }
        public Dictionary<string, TestResult> tests { get; set; }

        public bool pass { get; set; }

        public ComparisonResult(FilePair filePair)
        {
            tests = new Dictionary<string, TestResult>();

            pass = true;
            this.filePair = filePair;
        }

        /// <summary>
        /// Add a test result
        /// </summary>
        /// <param name="testResult">The test result</param>
        /// <param name="testName">Name of the test/method</param>
        public void AddTestResult(TestResult testResult, string testName)
        {
            tests[testName] = testResult;
            if (!testResult.pass)
            {
                pass = false;
            }
        }
    }


    public bool active { get; set; }
    public Stopwatch stopwatch { get; set; }
    public List<ComparisonResult> results { get; set; }


    /// <summary>
    /// Initialize the logger. This must be called before any other function
    /// </summary>
    public void Initialize()
    {
        active = true;

        stopwatch = new Stopwatch();
        stopwatch.Start();

        results = new List<ComparisonResult>();
    }


    /// <summary>
    /// Add a result from a test
    /// </summary>
    /// <param name="filePair">The filepair</param>
    /// <param name="testName">The name of the test</param>
    /// <param name="pass">If the test passed or not</param>
    /// <param name="percentage">The percentage of which the test was successful. Leave out if not relecant</param>
    public void AddTestResult(FilePair filePair, string testName, bool pass, double? percentage = null)
    {
        var testResult = new TestResult(pass, percentage);

        var index = results.FindIndex(r => r.filePair == filePair);
        if (index == -1)
        {
            var cr = new ComparisonResult(filePair);
            cr.AddTestResult(testResult, testName);
            results.Add(cr);
        } else
        {
            results[index].AddTestResult(testResult, testName);
        }
    }


    /// <summary>
    /// Finish logging. Must be called before ExportJSON can be called
    /// </summary>
    public void Finish()
    {
        if (!active) return;

        stopwatch.Stop();
        active = false;
    }


    /// <summary>
    /// Export the current log to a JSON file
    /// </summary>
    /// <param name="dir">The directory where the JSON file is to be exported</param>
    public void ExportJSON(string path)
    {
        if (active) return;

        try
        {
            string jsonString = JsonSerializer.Serialize(this);

            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to save log", ex);
        }
    }


    /// <summary>
    /// Import log values from a JSON file
    /// </summary>
    /// <param name="src">The JSON file to import</param>
    public void ImportJSON(string src)
    {
        try
        {
            var seralizerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            var jsonString = File.ReadAllText(src);

            var l = JsonSerializer.Deserialize<Logger>(jsonString, seralizerOptions);
            if (l is Logger logger)
            {
                this.results = logger.results;
                this.stopwatch = logger.stopwatch;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load log", ex);
        }
    }
}
