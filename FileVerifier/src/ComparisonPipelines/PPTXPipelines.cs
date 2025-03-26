using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class PptxPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for PPTX files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetPptxPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return PptxToPdfPipeline;

        return null;
    }
    
    /// <summary>
    /// Pipeline responsible for comparing Pptx to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void PptxToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
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

            if (GlobalVariables.Options.GetMethod(Methods.Animations.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = AnimationComparison.CheckXmlBasedFormatForAnimation(pair.OriginalFilePath);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Animations.Name, false,
                        err: new Error(
                            "Error while checking for animation usage in original pptx file.",
                            "There occurred an error while trying to find animation usage of the pptx file",
                            ErrorSeverity.High,
                            ErrorType.Metadata
                        ));
                }
                if (!exceptionOccurred && res)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Animations.Name, false,
                        err: new Error(
                            "The original pptx file contains animations",
                            "Context from original pptx file may be lost due to use of animations.",
                            ErrorSeverity.Medium,
                            ErrorType.Visual
                        ));
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
                    res = ColorProfileComparison.XmlBasedPowerPointToPdfColorProfileComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false,
                        err: new Error(
                            "Error comparing color profiles in pptx contained images",
                            "There occurred an error while extracting and comparing " +
                            "color profiles of the images contained in the pptx.",
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
                    res = TransparencyComparison.XmlBasedPowerPointToPdfTransparencyComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false,
                        err: new Error(
                            "Error comparing transparency in pptx contained images",
                            "There occurred an error while comparing transparency" +
                            " of the images contained in the pptx.",
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
                                "Difference of transparency detected in images contained in the pptx",
                                "The images contained in the pptx and pdf files did not pass Transparency comparison.",
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