using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class OdpPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for ODP files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetOdpPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return OdpToPdfPipeline;

        return null;
    }
    
    /// <summary>
    /// Pipeline responsible for comparing Odp to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void OdpToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            List<Error> e = [];
            Error error;

            var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(pair.OriginalFilePath);
            var nImages = ImageExtraction.GetNonDuplicatePdfImages(pair.NewFilePath);

            e.AddRange(ComperingMethods.CompareFonts(pair));
            
            if (GlobalVariables.Options.GetMethod(Methods.Pages.Name))
            {
                var diff = ComperingMethods.GetPageCountDifferenceExif(pair);
                switch (diff)
                {
                    case null:
                        error = new Error(
                            "Could not get page count",
                            "There was an error trying to get the page count from at least one of the files.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        );
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, false, errors: [error]);
                        e.Add(error);
                        break;
                    case > 0:
                        error = new Error(
                            "Difference in page count",
                            "The original and new document have a different page count.",
                            ErrorSeverity.High,
                            ErrorType.FileError,
                            $"{diff}"
                        );
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, false, errors: [error]);
                        e.Add(error);
                        break;
                    default:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, true);
                        break;
                }
            }
            
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
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                    e.Add(error);
                } else if ((bool)res)
                {
                    error = new Error(
                            "File Size Difference",
                            "The difference in size for the two files exceeds expected values.",
                            ErrorSeverity.Medium,
                            ErrorType.FileError
                        );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                    e.Add(error);
                }
                else
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, true);
                }
            }

            if (GlobalVariables.Options.GetMethod(Methods.Animations.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = AnimationComparison.CheckOdpForAnimation(pair.OriginalFilePath);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    error = new Error(
                        "Error while checking for animation usage in original odp file.",
                        "There occurred an error while trying to find animation usage of the odp file",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Animations.Name, false, errors: [error]);
                    e.Add(error);
                }
                if (!exceptionOccurred && res)
                {
                    error = new Error(
                        "The original odp file contains animations",
                        "Context from original odp file may be lost due to use of animations.",
                        ErrorSeverity.Medium,
                        ErrorType.Visual
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Animations.Name, false, errors: [error]);
                    e.Add(error);
                }
                else
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Animations.Name, true);
                }
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.GeneralDocsToPdfColorProfileComparison(oImages, nImages);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    error = new Error(
                        "Error comparing color profiles in odp contained images",
                        "There occurred an error while extracting and comparing " +
                        "color profiles of the images contained in the odp.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false, errors: [error]);
                    e.Add(error);
                }

                switch (exceptionOccurred)
                {
                    case false when !res:
                        error = new Error(
                            "Mismatching color profile",
                            "The color profile in the new file does not match the original on at least one image.",
                            ErrorSeverity.Medium,
                            ErrorType.Metadata
                        );
                        GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false, errors: [error]);
                        e.Add(error);
                        break;
                    case false when res:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, true);
                        break;
                }
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.Transparency.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    error = new Error(
                        "Error comparing transparency in odp contained images",
                        "There occurred an error while comparing transparency" +
                        " of the images contained in the odp.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false, errors: [error]);
                    e.Add(error);
                }

                switch (exceptionOccurred)
                {
                    case false when !res:
                        error = new Error(
                            "Difference of transparency detected in images contained in the odp",
                            "The images contained in the odp and pdf files did not pass Transparency comparison.",
                            ErrorSeverity.Medium,
                            ErrorType.Visual
                        );
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false, errors: [error]);
                        e.Add(error);
                        break;
                    case false when res:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, true);
                        break;
                }
            }
            
            UiControlService.Instance.AppendToConsole(
                $"Result for {Path.GetFileName(pair.OriginalFilePath)}-{Path.GetFileName(pair.NewFilePath)} Comparison: \n" +
                e.GenerateErrorString() + "\n\n");
            
            ImageExtraction.DisposeMagickImages(oImages);
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}