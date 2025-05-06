using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AvaloniaDraft.Helpers;
using DocumentFormat.OpenXml.Wordprocessing;
using Avalonia.Logging;
using System.Diagnostics.CodeAnalysis;
using AvaloniaDraft.ProgramManager;

namespace AvaloniaDraft.Logger;


/// <summary>
/// A result of a single comparison test
/// </summary>
[ExcludeFromCodeCoverage]
public class TestResult
{
    public bool Pass { get; set; }
    public double? Percentage { get; set; }
    public List<string>? Comments { get; set; }
    public List<Error>? Errors { get; set; }

    public TestResult(bool pass, double? percentage, List<string>? comments, List<Error>? errors)
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
[ExcludeFromCodeCoverage]
public class ComparisonResult
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
    public void AddTestResult(Method method, bool pass, double? percentage = null, List<string>? comments = null, List<Error>? errors = null)
    {
        var testResult = new TestResult(pass, percentage, comments, errors);
        Tests[method.Name] = testResult;
        if (!testResult.Pass)
        {
            Pass = false;
        }
    }

    /// <summary>
    /// Add a test result
    /// </summary>
    /// <param name="method">Name of the method results are to be added for.</param>
    public void AddTestResult(string method, bool pass, double? percentage = null, List<string>? comments = null, List<Error>? errors = null)
    {
        var testResult = new TestResult(pass, percentage, comments, errors);
        Tests[method] = testResult;
        if (!testResult.Pass)
        {
            Pass = false;
        }
    }
}


/// <summary>
/// A logger to store test results
/// </summary>
[ExcludeFromCodeCoverage]
public class Logger
{
    private bool Active { get; set; }
    private bool Finished { get; set; }

    public int FileComparisonCount { get; set; }
    public int FileComparisonsFailed { get; set; }
    public DateTime LastRefresh { get; set; }
    public long Elapsed { get; set; }
    public List<IgnoredFile> IgnoredFiles { get; set; } = [];
    public List<FilePair> InternalErrorFilePairs { get; set; } = [];    
    public List<ComparisonResult> Results { get; set; } = [];


    /// <summary>
    /// Initialize/reset the logger. This must be called before any other function
    /// </summary>
    public void Initialize()
    {
        Active = false;
        Finished = false;

        Elapsed = 0;

        FileComparisonCount = 0;
        FileComparisonsFailed = 0;
        Results = new List<ComparisonResult>();
        IgnoredFiles = new List<IgnoredFile>();
    }


    /// <summary>
    /// Check if the logger has finished
    /// </summary>
    /// <returns></returns>
    public bool HasFinished()
    {
        return Finished;
    }


    /// <summary>
    /// Start the logger
    /// </summary>
    public void Start()
    {
        Active = true;
        LastRefresh = DateTime.UtcNow;
    }

    /// <summary>
    /// Add the result of a file pair comparison
    /// </summary>
    /// <param name="result"></param>
    public void AddComparisonResult(ComparisonResult result)
    {
        FileComparisonCount++;
        if (!result.Pass) FileComparisonsFailed++;
        Results.Add(result);
    }


    /// <summary>
    /// Add a file pair that had an internal error when comparing
    /// </summary>
    /// <param name="fp"></param>
    public void AddInternalErrorFilePair(string o, string n)
    {
        InternalErrorFilePairs.Add(new FilePair(o, n));
    }


    /// <summary>
    /// Add an ignored file
    /// </summary>
    /// <param name="file"></param>
    public void AddIgnoredFile(IgnoredFile file)
    {
        if (!IgnoredFiles.Any(f => f.FilePath == file.FilePath)) IgnoredFiles.Add(file);
    }

    /// <summary>
    /// Finish logging
    /// </summary>
    public void Finish()
    {
        if (!Active) return;

        UpdateElapsedTime();

        Active = false;
        Finished = true;
    }


    /// <summary>
    /// Return a list of all file pairs
    /// </summary>
    /// <returns></returns>
    public List<FilePair> GetFilePairs()
    {
        return Results.Select(r => r.FilePair).ToList();
    }


    /// <summary>
    /// Update 'Elapsed'
    /// </summary>
    private void UpdateElapsedTime()
    {
        var timespan = DateTime.UtcNow - LastRefresh;
        Elapsed += timespan.Ticks;
        LastRefresh = DateTime.UtcNow;
    }


    /// <summary>
    /// Return a formatted string of the elapsed time
    /// </summary>
    /// <returns></returns>
    public string FormatElapsedTime()
    {
        var timespan = new TimeSpan(Elapsed);
        return timespan.ToString("hh\\:mm\\:ss");
    }

    /// <summary>
    /// Save the report
    /// </summary>
    public void SaveReport(bool checkpoint = false)
    {
        var reportsFolderName = "reports";
        var reportName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
        var checkpointName = "checkpoint.json";
        var name = checkpoint ? checkpointName : reportName;

        // Get directory
        string? dir = null;
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Path.GetFileName(currentDir) == "FileVerifier")
            {
                dir = checkpoint ? currentDir : Path.Join(currentDir, reportsFolderName);
                break;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        if (dir == null) return;

        var path = Path.Join(dir, name);

        ExportJSON(path);
    }


    /// <summary>
    /// Export the current log to a JSON file
    /// </summary>
    /// <param name="dir">The directory where the JSON file is to be exported</param>
    public void ExportJSON(string path)
    {
        try
        {
            if (!Finished) UpdateElapsedTime();
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trying to save report: {ex}");
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
            var jsonString = File.ReadAllText(src);

            var l = JsonSerializer.Deserialize<Logger>(jsonString);
            if (l is Logger logger)
            {
                FileComparisonCount = l.FileComparisonCount;
                FileComparisonsFailed = l.FileComparisonsFailed;
                Results = logger.Results;
                IgnoredFiles = logger.IgnoredFiles;
                InternalErrorFilePairs = logger.InternalErrorFilePairs;

                LastRefresh = DateTime.UtcNow;
                Elapsed = l.Elapsed;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trying to load log: {ex}");
        }
    }
}
