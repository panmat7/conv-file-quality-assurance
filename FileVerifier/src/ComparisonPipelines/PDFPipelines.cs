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
            List<Error> e = [];
            
            if (GlobalVariables.Options.GetMethod(Methods.Size.Name))
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
                    e.Add(new Error(
                        "Error comparing color profiles in pdf contained images",
                        "There occurred an error while extracting and comparing " +
                        "color profiles of the images contained in the pdf.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }

                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "Difference in images contained in the pdf's color profile",
                        "The images contained in the two pdf files did not pass Color Profile comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    ));
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
                    e.Add(new Error(
                        "Error comparing transparency in pdf contained images",
                        "There occurred an error while comparing transparency" +
                        " of the images contained in the pdf.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }

                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "Difference in images contained in the pdf's transparency",
                        "The images contained in the two pdfs did not pass Transparency comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Visual
                    ));
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
            
            
            UiControlService.Instance.AppendToConsole(
                $"Result for {Path.GetFileName(pair.OriginalFilePath)}-{Path.GetFileName(pair.NewFilePath)} Comparison: \n" +
                e.GenerateErrorString() + "\n\n");
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}