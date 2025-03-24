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
                    e.Add(new Error(
                        "Error while checking for animation usage in original odp file.",
                        "There occurred an error while trying to find animation usage of the odp file",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }
                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "The original odp file contains animations",
                        "Context from original odp file may be lost due to use of animations.",
                        ErrorSeverity.Medium,
                        ErrorType.Visual
                    ));
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
                    e.Add(new Error(
                        "Error comparing color profiles in odp and pdf contained images",
                        "There occurred an error while extracting and comparing " +
                        "color profiles of the images contained in the odp and pdf.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }

                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "Difference in images contained in the odp and pdf color profile",
                        "The images contained in the odp and pdf did not pass Color Profile comparison.",
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
                    res = TransparencyComparison.XmlBasedPowerPointToPdfTransparencyComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    e.Add(new Error(
                        "Error comparing transparency in odp contained images",
                        "There occurred an error while comparing transparency" +
                        " of the images contained in the odp and pdf.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    ));
                }

                if (!exceptionOccurred && !res)
                {
                    e.Add(new Error(
                        "Difference in images contained in the odp and pdf in transparency",
                        "The images contained in the odp and pdf did not pass Transparency comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Visual
                    ));
                }
            }
            
            UiControlService.Instance.AppendToConsole(
                $"Result for {Path.GetFileName(pair.OriginalFilePath)}-{Path.GetFileName(pair.NewFilePath)} Comparison: \n" +
                e.GenerateErrorString() + "\n\n");
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}