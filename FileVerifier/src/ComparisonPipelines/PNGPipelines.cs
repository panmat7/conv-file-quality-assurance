using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ClosedXML;

namespace AvaloniaDraft.ComparisonPipelines;

public static class PngPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for PNG files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetPNGPipelines(string outputFormat)
    {
        if (!FormatCodes.PronomCodesPNG.Contains(outputFormat) && FormatCodes.PronomCodesImages.Contains(outputFormat))
            return PNGToImagePipeline;
        
        if(FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return PNGToPDFPipeline;

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing PNG to other image formats conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void PNGToImagePipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount, Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            List<Error> e = [];
            Error error;
            
            //Check options if this check is enabled.
            if (GlobalVariables.Options.GetMethod(Methods.Size.Name))
            {
                var res = ComperingMethods.CheckFileSizeDifference(pair, 0.5); //Use settings later

                if (res == null)
                {
                    error = new Error(
                        "Could not get file size difference",
                        "The tool was unable to get the file size difference for at least one file.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, err: error);
                    e.Add(error);
                }
                else if ((bool)res)
                {
                    error = new Error(
                        "File Size Difference",
                        "The difference in size for the two files exceeds expected values.",
                        ErrorSeverity.Medium,
                        ErrorType.FileError
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, err: error);
                    e.Add(error);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.Size.Name, true);
                
            }

            if (GlobalVariables.Options.GetMethod(Methods.Resolution.Name))
            {
                var res = ComperingMethods.GetImageResolutionDifference(pair);

                if (res is null)
                {
                    error = new Error(
                        "Error getting image resolution difference",
                        "There occured an error while trying to get the difference in image resolution.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Resolution.Name, false, err: error);
                    e.Add(error);
                }
                else if (res.Item1 > 0 || res.Item2 > 0)
                {
                    error = new Error(
                        "Image resolution difference",
                        "Mismatched resolution between images.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Resolution.Name, false, err: error);
                    e.Add(error);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.Resolution.Name, true);
            }

            if (GlobalVariables.Options.GetMethod(Methods.Metadata.Name))
            {
                var res = ComperingMethods.GetMissingOrWrongImageMetadataExif(pair);
                
                if (res is null)
                {
                    error = new Error(
                        "Image resolution difference",
                        "Mismatched resolution between images.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false, err: error);
                    e.Add(error);
                }
                else if (res.Count > 0)
                {
                    //TODO: Log list of errors
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.Metadata.Name, true);
            }

            if(GlobalVariables.Options.GetMethod(Methods.PointByPoint.Name))
            {
                var acceptance = 85; //Read from options later ?

                var res = ImageRegistration.CalculateHistogramSimilarity(pair);

                if (res < 0)
                {
                    error = new Error(
                        "Error calculating image similarity",
                        "There occured an error while calculating the image similarity during Pixel by Pixel comparison.",
                        ErrorSeverity.High,
                        ErrorType.Visual
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.PointByPoint.Name, false, err: error);
                    e.Add(error);
                } else if (res < acceptance)
                {
                    error = new Error(
                        "Difference in image's visual appearance",
                        "The images did not pass Pixel by Pixel comparison.",
                        ErrorSeverity.High,
                        ErrorType.Visual,
                        res.ToString("0.##")
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.PointByPoint.Name, false, err: error);
                    e.Add(error);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.PointByPoint.Name, true);
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name)) // Check for color profile later
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.ImageToImageColorProfileComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    error = new Error(
                        "Error comparing color profiles",
                        "There occurred an error while extracting and comparing color profiles.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, err: error);
                    e.Add(error);
                }

                if (!exceptionOccurred && !res)
                {
                    error = new Error(
                        "Difference in both images color profile",
                        "The images did not pass Color Profile comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, err: error);
                    e.Add(error);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.ColorProfile.Name, true);
            }
            
            UiControlService.Instance.AppendToConsole(
                $"Result for {Path.GetFileName(pair.OriginalFilePath)}-{Path.GetFileName(pair.NewFilePath)} Comparison: \n" +
                e.GenerateErrorString() + "\n\n");
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
    
    private static void PNGToPDFPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        
    }
}