using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class DocxPipelines
{
    /// <summary>
    /// Function resposible for assigning the correct pipeline for DOCX files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetDocxPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return DOCXToPDFPipeline;
        

        return null;
    }
    
    /// <summary>
    /// Pipeline responsible for comparing DOCX to PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void DOCXToPDFPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            var diff = ComperingMethods.GetPageCountDifferenceExif(pair);
            switch (diff)
            {
                case null:
                    GlobalVariables.Logger.AddTestResult(pair, "Page Count", false,
                        errors: [new Error(
                            "Could not get page count",
                            "There was an error trying to get the page count from at least one of the files.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )]
                    );
                    break;
                case > 0:
                    GlobalVariables.Logger.AddTestResult(pair, "Page Count", false,
                        errors: [new Error(
                            "Difference in page count",
                            "The original and new document have a different page count.",
                            ErrorSeverity.High,
                            ErrorType.FileError,
                            $"{diff}"
                        )]
                    );
                    break;
                default:
                    GlobalVariables.Logger.AddTestResult(pair, "Page Count", true);
                    break;
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
                    res = ColorProfileComparison.DocxToPdfColorProfileComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false,
                        errors: [new Error(
                            "Error comparing color profiles in docx contained images",
                            "There occurred an error while extracting and comparing " +
                            "color profiles of the images contained in the docx.",
                            ErrorSeverity.High,
                            ErrorType.Metadata
                        )]
                    );
                }

                if (!exceptionOccurred && !res)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false,
                        errors: [new Error(
                            "Mismatching color profile",
                            "The color profile in the new file does not match the original on at least one image.",
                            ErrorSeverity.Medium,
                            ErrorType.Metadata
                        )]
                    );
                else if (!exceptionOccurred && res)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, true);
                
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
                        errors: [new Error(
                            "Error comparing transparency in docx contained images",
                            "There occurred an error while comparing transparency" +
                            " of the images contained in the docx.",
                            ErrorSeverity.Medium,
                            ErrorType.Metadata
                        )]
                    );
                }

                if (!exceptionOccurred && !res)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false,
                        errors: [new Error(
                            "Difference in images contained in the docx's transparency",
                            "The images contained in the docx and pdf files did not pass Transparency comparison.",
                            ErrorSeverity.Medium,
                            ErrorType.Visual
                        )]
                    );
                else if(!exceptionOccurred && res)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, true);
            }
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}