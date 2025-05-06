using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Generic;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;
using AvaloniaDraft.ProgramManager;

namespace AvaloniaDraft.ComparisonPipelines;

public static class EmlPipelines
{
    /// <summary>
    /// Function resposible for assigning the correct pipeline for DOCX files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetEmlPipeline(string? outputFormat)
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
            Error error;

            var compResult = new ComparisonResult(pair);

            var failedToExtract = false;
            var equalNumberOfImages = false;

            var tempFoldersForImages = BasePipeline.CreateTempFoldersForImages();
            try
            {
                ImageExtractionToDisk.ExtractImagesFromEmlToDisk(pair.OriginalFilePath, tempFoldersForImages.Item1);
                ImageExtractionToDisk.ExtractImagesFromPdfToDisk(pair.NewFilePath, tempFoldersForImages.Item2);
                // Some checks will be skipped if the number of images is not equal
                equalNumberOfImages = ImageExtractionToDisk.CheckIfEqualNumberOfImages(tempFoldersForImages.Item1,
                    tempFoldersForImages.Item2);
            }
            catch (Exception)
            {
                failedToExtract = true;
            }

            if (!failedToExtract)
            {
                if (GlobalVariables.Options.GetMethod(Methods.ColorProfile))
                {
                    if (equalNumberOfImages)
                    {
                        BasePipeline.CheckColorProfiles(tempFoldersForImages.Item1,
                            tempFoldersForImages.Item2, pair, ref compResult);
                    }
                    else
                    {
                        error = new Error(
                            "Unequal number of images",
                            "The comparison of color profiles could not be performed " +
                            "because the number of images in the original and new file is different.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        );
                        compResult.AddTestResult(Methods.ColorProfile, false, errors: [error]);
                    }
                }
            }
            else
            {
                error = new Error(
                    "Failed to extract images from files",
                    "Comparisons involving extracted images can not be performed " +
                    "because the tool was unable to extract images from at least one of the files.",
                    ErrorSeverity.High,
                    ErrorType.FileError
                );
                compResult.AddTestResult(Methods.ColorProfile, false, errors: [error]);
            }

            if (GlobalVariables.Options.GetMethod(Methods.Metadata))
            {
                if (equalNumberOfImages)
                {
                    ExtractedImageMetadata.CompareExtractedImages(pair, ref compResult, tempFoldersForImages.Item1,
                        tempFoldersForImages.Item2);
                }
                else
                {
                    error = new Error(
                        "Unequal number of images",
                        "The comparison of extracted image metadata could not be performed " +
                        "because the number of images in the original and new file is different.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    );
                    compResult.AddTestResult(Methods.Transparency, false, errors: [error]);
                }
            }
            
            BasePipeline.DeleteTempFolders(tempFoldersForImages.Item1, tempFoldersForImages.Item2);

            ComparingMethods.ComparingMethods.CompareFonts(pair, ref compResult);

            GlobalVariables.Logger.AddComparisonResult(compResult);

        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}