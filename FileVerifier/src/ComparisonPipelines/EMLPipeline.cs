using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class EmlPipeline
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for EML files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetEmlPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return EmlToPdfPipeline;

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing EML to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void EmlToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.EmlToPdfColorProfileComparison(pair);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false,
                        err: new Error(
                            "Error comparing color profiles in eml contained images",
                            "There occurred an error while extracting and comparing " +
                            "color profiles of the images contained in the eml.",
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
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}