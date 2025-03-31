using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Generic;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class EmlPipelines
{
    /// <summary>
    /// Function resposible for assigning the correct pipeline for DOCX files
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
    /// Pipeline responsible for comparing DOCX to PDF conversions
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
            List<Error> e = [];
            Error error;
            
            var oImages = ImageExtraction.ExtractImagesFromEml(pair.OriginalFilePath);
            var nImages = ImageExtraction.GetNonDuplicatePdfImages(pair.NewFilePath);
            
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
                        "Error comparing color profiles in eml contained images",
                        "There occurred an error while extracting and comparing " +
                        "color profiles of the images contained in the eml.",
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
            
            UiControlService.Instance.AppendToConsole(
                $"Result for {Path.GetFileName(pair.OriginalFilePath)}-{Path.GetFileName(pair.NewFilePath)} Comparison: \n" +
                e.GenerateErrorString() + "\n\n");
            
            ImageExtraction.DisposeMagickImages(oImages);
            

            e.AddRange(BasePipeline.CompareFonts(pair));

        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}