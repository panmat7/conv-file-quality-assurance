using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.FileManager;

public static class PngPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for PNG files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if none for were</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetPNGPipelines(string outputFormat)
    {
        if (FormatCodes.PronomCodesJPEG.Contains(outputFormat))
        {
            return PNGToJPEGPipeline;
        }

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing PNG to JPEG conversions
    /// </summary>
    /// <param name="pair">The pair os files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void PNGToJPEGPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount, Action markDone)
    {
        try
        {
            List<Error> e = [];
            
            if (true) //Check options for file size check later
            {
                var res = ComperingMethods.GetFileSizeDifference(pair);
                
                var f = new FileInfo(pair.OriginalFilePath);
                
                if (res > f.Length * 1.5 || res < f.Length * 0.5) //Adjust for accuracy later
                {
                    //For now only printing to console
                    e.Add(new Error(
                        "File Size Difference",
                        "The difference in size for the two files exceeds expected values.",
                        ErrorSeverity.High,
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
                        ErrorType.Visual
                    ));
                }
            }
            
            ConsoleService.Instance.WriteToConsole(e.GenerateErrorString());
        }
        catch
        {
            var e = new Error(
                "Error during file comparison.",
                "There occured an internal error while trying to compare images.",
                ErrorSeverity.Internal
            );
            
            ConsoleService.Instance.WriteToConsole(e.FormatErrorMessage());
        }
        finally
        {
            updateThreadCount(-(1 + additionalThreads)); //Ensuring that this happens even if something fails
            markDone();
        }
    }
}