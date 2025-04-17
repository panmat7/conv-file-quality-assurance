using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ExtractionPipelines;

public static class BaseExtraction
{
    public static Dictionary<string, string>? ExecutePipeline(Func<Dictionary<string, string>> pipeline, string file, Action updateThreadCount, Action markDone)
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
    
    public static Func<SingleFile, Action, Action, Dictionary<string, string>?>? SelectPipeline(string format)
    {
        if (FormatCodes.PronomCodesImages.Contains(format))
            return ImageExtractionPipeline;

        return null;
    }

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