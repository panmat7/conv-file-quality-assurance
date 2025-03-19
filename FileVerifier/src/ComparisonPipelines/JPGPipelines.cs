using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class JpgPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for JPEG files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetJPEGPipelines(string outputFormat)
    {
        if (!FormatCodes.PronomCodesJPEG.Contains(outputFormat) && FormatCodes.PronomCodesImages.Contains(outputFormat))
            return JPEGToImagePipeline;
        
        if(FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return JPEGToPDFPipieline;

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing JPEG to other image format conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void JPEGToImagePipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount, Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            List<Error> e = [];
            
            if (true) //Check options for file size check later
            {
                var res = ComperingMethods.CheckFileSizeDifference(pair, 0.5); //Use settings later

                if (res == null)
                {
                    e.Add(new Error(
                        "Could not get file size difference",
                        "The tool was unable to get the file size difference for at least one file.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    ));
                } else if ((bool)res)
                {
                    //For now only printing to console
                    e.Add(new Error(
                        "File Size Difference",
                        "The difference in size for the two files exceeds expected values.",
                        ErrorSeverity.Medium,
                        ErrorType.FileError
                    ));
                }
            }

            if (true)
            {
                var res = ComperingMethods.GetImageResolutionDifference(pair);

                if (res is null)
                {
                    //For now only printing to console
                    e.Add(new Error(
                        "Error getting image resolution difference",
                        "There occured an error while trying to get the difference in image resolution.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    ));
                } else if (res.Item1 > 0 || res.Item2 > 0)
                {
                    //For now only printing to console
                    e.Add(new Error(
                        "Image resolution difference",
                        "Mismatched resolution between images.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    ));
                }
            }

            if (true) //Check options for metadata check later
            {
                var res = ComperingMethods.GetMissingOrWrongImageMetadataExif(pair);

                if (res is null)
                {
                    e.Add(new Error(
                        "Error getting image metadata",
                        "There occured an error while trying to get metadata from one of the files.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                } else if (res.Count > 0)
                {
                    e.AddRange(res);
                }
            }

            if (true) //Check for point by point later
            {
                var acceptance = 85; //Read from options later ?

                var res = PbpComparison.CalculateImageSimilarity(pair, additionalThreads);

                if (res < 0)
                {
                    e.Add(new Error(
                        "Error calculating image similarity",
                        "There occured an error while calculating the image similarity during Pixel by Pixel comparison.",
                        ErrorSeverity.High,
                        ErrorType.Visual
                    ));
                } else if (res < acceptance)
                {
                    e.Add(new Error(
                        "Difference in image's visual appearance",
                        "The images did not pass Pixel by Pixel comparison.",
                        ErrorSeverity.High,
                        ErrorType.Visual,
                        res.ToString("0.##")
                    ));
                }
            }
            
            UiControlService.Instance.AppendToConsole(
                $"Result for {Path.GetFileName(pair.OriginalFilePath)}-{Path.GetFileName(pair.NewFilePath)} Comparison: \n" +
                e.GenerateErrorString() + "\n\n");
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }

    private static void JPEGToPDFPipieline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}