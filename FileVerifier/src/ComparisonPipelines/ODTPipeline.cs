using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class OdtPipeline
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for ODT files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetOdtPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return OdtToPdfPipeline;

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing Odt to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void OdtToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            if (GlobalVariables.Options.GetMethod(Methods.Pages.Name))
            {
                var diff = ComperingMethods.GetPageCountDifferenceExif(pair);
                switch (diff)
                {
                    case null:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, false,
                            err: new Error(
                                "Could not get page count",
                                "There was an error trying to get the page count from at least one of the files.",
                                ErrorSeverity.High,
                                ErrorType.FileError
                            )
                        );
                        break;
                    case > 0:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, false,
                            err: new Error(
                                "Difference in page count",
                                "The original and new document have a different page count.",
                                ErrorSeverity.High,
                                ErrorType.FileError,
                                $"{diff}"
                            )
                        );
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
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false,
                        err: new Error(
                        "Could not get file size difference",
                        "The tool was unable to get the file size difference for at least one file.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    ));
                } else if ((bool)res)
                {
                    //For now only printing to console
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false,
                        err: new Error(
                        "File Size Difference",
                        "The difference in size for the two files exceeds expected values.",
                        ErrorSeverity.Medium,
                        ErrorType.FileError
                    ));
                }
                else
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, true);
                }
            }

            if (true)
            {
                //Visual comparison here ?
            }

            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.OdtAndOdpToPdfColorProfileComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false,
                        err: new Error(
                            "Error comparing color profiles in odt contained images",
                            "There occurred an error while extracting and comparing " +
                            "color profiles of the images contained in the odt.",
                            ErrorSeverity.High,
                            ErrorType.Metadata
                        )
                    );
                }

                switch (exceptionOccurred)
                {
                    case false when !res:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false,
                            err: new Error(
                                "Mismatching color profile",
                                "The color profile in the new file does not match the original on at least one image.",
                                ErrorSeverity.Medium,
                                ErrorType.Metadata
                            )
                        );
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
                    res = TransparencyComparison.OdtAndOdpToPdfTransparencyComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false,
                        err: new Error(
                            "Error comparing transparency in odt contained images",
                            "There occurred an error while comparing transparency" +
                            " of the images contained in the odt.",
                            ErrorSeverity.Medium,
                            ErrorType.Metadata
                        )
                    );
                }

                switch (exceptionOccurred)
                {
                    case false when !res:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false,
                            err: new Error(
                                "Difference of transparency detected in images contained in the odt",
                                "The images contained in the docx and pdf files did not pass Transparency comparison.",
                                ErrorSeverity.Medium,
                                ErrorType.Visual
                            )
                        );
                        break;
                    case false when res:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, true);
                        break;
                }
            }
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}