using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ProgramManager;

namespace AvaloniaDraft.ExtractionPipelines;

public static class BaseExtraction
{
    /// <summary>
    /// Executes a data extraction pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline function to be executed.</param>
    /// <param name="file">The name of the files that is operated on.</param>
    /// <param name="updateThreadCount">Function adding the thread back to the budget.</param>
    /// <param name="markDone">Function marking the file as done,</param>
    /// <returns>All data in a name-to-value dictionary, or null if an error occured.</returns>
    private static Dictionary<string, string>? ExecutePipeline(Func<Dictionary<string, string>> pipeline, string file, Action updateThreadCount, Action markDone)
    {
        try
        {
            return pipeline();
        }
        catch(Exception er)
        {
            Console.WriteLine(er);
            var e = new Error(
                "Error during file processing.",
                "An internal error occurred while processing the files.",
                ErrorSeverity.Internal
            );

            UiControlService.Instance.AppendToConsole(
                $"Extraction for {file} failed:\n" +
                e.FormatErrorMessage() +
                "\n\n"
            );
            return null;
        }
        finally
        {
            UiControlService.Instance.MarkProgress();
            updateThreadCount();
            markDone();
        }
    }
    
    /// <summary>
    /// Function selecting the pipeline for a file based on its format.
    /// </summary>
    /// <param name="format">Format of the file.</param>
    /// <returns>The pipeline function.</returns>
    public static Func<SingleFile, Action, Action, Dictionary<string, string>?>? SelectPipeline(string? format)
    {
        if (FormatCodes.PronomCodesImages.Contains(format))
            return ImageExtractionPipeline;
        
        if(FormatCodes.PronomCodesSpreadsheets.Contains(format))
            return SpreadsheetExtractionPipeline;

        return null;
    }

    /// <summary>
    /// Data extraction pipeline for image files.
    /// </summary>
    /// <param name="file">File that is to be worked on.</param>
    /// <param name="updateThreads">Function adding the thread back to the budget.</param>
    /// <param name="markDone">Function marking the file as done.</param>
    /// <returns>All data in a name-to-value dictionary, or null if an error occured.</returns>
    private static Dictionary<string, string>? ImageExtractionPipeline(SingleFile file, Action updateThreads, Action markDone)
    {
        var res = ExecutePipeline(() =>
        {
            Dictionary<string, string> result = new();

            var size = new FileInfo(file.FilePath).Length;
            result["FileSize"] = $"{size / 1024}KB";
            
            var metaRes = ExtractionMethods.GetImageMetadataInfo(file);
            if (metaRes is not null)
                foreach (var data in metaRes)
                    result.Add(data.Key, data.Value);
            else
                result["ImageMetadataExtraction"] = "Failed";

            return result;
        }, file.FilePath, updateThreads, markDone);

        return res;
    }

    /// <summary>
    /// Data extraction pipeline for spreadsheet files.
    /// </summary>
    /// <param name="file">The file that is to be worked on.</param>
    /// <param name="updateThreads">Function adding the thread back to the budget.</param>
    /// <param name="markDone">Function marking the file as done.</param>
    /// <returns>All data in a name-to-value dictionary, or null if an error occured.</returns>
    private static Dictionary<string, string>? SpreadsheetExtractionPipeline(SingleFile file, Action updateThreads,
        Action markDone)
    {
        var res = ExecutePipeline(() =>
        {
            Dictionary<string, string> result = new();

            var size = new FileInfo(file.FilePath).Length;
            result["FileSize"] = $"{size / 1024}KB";

            var breaks = ExtractionMethods.CheckSpreadsheetBreak(file);
            
            foreach (var data in breaks)
                result.Add(data.Key, data.Value);

            return result;
        }, file.FilePath, updateThreads, markDone);

        return res;
    }
}