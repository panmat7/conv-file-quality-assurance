using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class PdfPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for PDF files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetPdfPipelines(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return PdfToPdfPipeline;
        
        return null;
    }
    
    /// <summary>
    /// Pipeline responsible for comparing PDF to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void PdfToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
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
                    GlobalVariables.Logger.AddTestResult(pair, "Size", true);
                }
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.PdfToPdfColorProfileComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false,
                        err: new Error(
                            "Error comparing color profiles in pdf contained images",
                            "There occurred an error while extracting and comparing " +
                            "color profiles of the images contained in the pdf.",
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
                    res = TransparencyComparison.PdfToPdfTransparencyComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false,
                        err: new Error(
                            "Error comparing transparency in pdf contained images",
                            "There occurred an error while comparing transparency" +
                            " of the images contained in the pdf.",
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
                                "Difference of transparency detected in images contained in the pdf",
                                "The images contained in the pdf and pdf files did not pass Transparency comparison.",
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
            
            if (GlobalVariables.Options.GetMethod(Methods.VisualDocComp.Name))
            {
                var res = ComperingMethods.VisualDocumentComparison(pair);
                
                if (res == null)
                   GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, false,
                       err: new Error(
                           "Error while preforming the visual comparison",
                           "Could not preform the visual comparison due to an error while getting the page " +
                           "images or while segmenting the image.",
                           ErrorSeverity.Medium,
                           ErrorType.Visual
                       ));
                else if (res.Count > 0)
                {
                    Console.WriteLine("ERRORS DURING VISUAL COMPARISON");
                    //TODO: Log errors
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, true);
            }
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}