using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using AvaloniaDraft.FileManager;
using System.Linq;
using System.Runtime.CompilerServices;
using AvaloniaDraft.Helpers;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AvaloniaDraft.Logger;

public class Logger
{
    /// <summary>
    /// A result of a single comparison test
    /// </summary>
    public struct TestResult
    {
        public bool Pass { get; set; }
        public double? Percentage { get; set; }
        public List<string>? Comments { get; set; }
        public List<Error> Errors { get; set; }

        public TestResult(bool pass, double? percentage, List<string>? comments, List<Error> errors)
        {
            this.Pass = pass;
            this.Percentage = percentage;
            this.Comments = comments;
            this.Errors = errors;
        }
    }

    /// <summary>
    /// A result of comparing two files
    /// </summary>
    public struct ComparisonResult
    {
        public FilePair FilePair { get; set; }
        public Dictionary<string, TestResult> Tests { get; set; }

        public bool Pass { get; set; }

        public ComparisonResult(FilePair filePair)
        {
            Tests = new Dictionary<string, TestResult>();

            Pass = true;
            this.FilePair = filePair;
        }

        /// <summary>
        /// Add a test result
        /// </summary>
        /// <param name="testResult">The test result</param>
        /// <param name="testName">Name of the test/method</param>
        public void AddTestResult(TestResult testResult, string testName)
        {
            Tests[testName] = testResult;
            if (!testResult.Pass)
            {
                Pass = false;
            }
        }
    }


    public bool Active { get; set; }
    public bool Finished { get; set; }
    public Stopwatch Stopwatch { get; set; }
    public List<ComparisonResult> Results { get; set; }


    /// <summary>
    /// Initialize the logger. This must be called before any other function
    /// </summary>
    public void Initialize()
    {
        Active = false;
        Finished = false;

        Stopwatch = new Stopwatch();

        Results = new List<ComparisonResult>();
    }


    /// <summary>
    /// Start the logger
    /// </summary>
    public void Start()
    {
        if (Active) return;

        Active = true;
        Stopwatch.Restart();
        Stopwatch.Start();
    }


    /// <summary>
    /// Add a result from a test
    /// </summary>
    /// <param name="filePair">The filepair</param>
    /// <param name="testName">The name of the test</param>
    /// <param name="pass">If the test passed or not</param>
    /// <param name="percentage">The percentage of which the test was successful. Leave out if not relevant</param>
    /// <param name="comments">A list of comments on the result</param>
    /// <param name="errors">Error</param>
    public void AddTestResult(FilePair filePair, string testName, bool pass, double? percentage = null, List<string>? comments = null, List<Error>? errors = null)
    {
        var testResult = new TestResult(pass, percentage, comments, errors);

        var index = Results.FindIndex(r => r.FilePair.OriginalFilePath == filePair.OriginalFilePath && r.FilePair.NewFilePath == filePair.NewFilePath);
        if (index == -1)
        {
            var cr = new ComparisonResult(filePair);
            cr.AddTestResult(testResult, testName);
            Results.Add(cr);
        }
        else
        {
            Results[index].AddTestResult(testResult, testName);
        }
    }



    public string? FormatTestResult(FilePair filePair)
    {
        var index = Results.FindIndex(r => r.FilePair.OriginalFilePath == filePair.OriginalFilePath && r.FilePair.NewFilePath == filePair.NewFilePath);
        if (index == -1) return null;
        var result = Results[index];

        var errors = new List<Error>();
        foreach (var t in result.Tests.Values)
        {
            foreach (var e in t.Errors)
            {
                errors.Add(e);
            }
        }

        return $"Result for {Path.GetFileName(filePair.OriginalFilePath)}-{Path.GetFileName(filePair.NewFilePath)} " +
            $"Comparison: \n{errors.GenerateErrorString()}\n\n";
    }

    /// <summary>
    /// Finish logging. Must be called before ExportJSON can be called
    /// </summary>
    public void Finish()
    {
        if (!Active) return;

        Stopwatch.Stop();
        Active = false;
        Finished = true;
    }


    /// <summary>
    /// Export the current log to a JSON file
    /// </summary>
    /// <param name="dir">The directory where the JSON file is to be exported</param>
    public void ExportJSON(string path)
    {
        if (Active) return;

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
                this.Results = logger.Results;
                this.Stopwatch = logger.Stopwatch;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load log", ex);
        }
    }
}
