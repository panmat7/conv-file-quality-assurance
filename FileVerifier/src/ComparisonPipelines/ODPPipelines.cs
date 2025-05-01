using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;

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
        if (FormatCodes.PronomCodesAllPDF.Contains(outputFormat))
            return OdpToPdfPipeline;
        if (FormatCodes.PronomCodesPresentationDocuments.Contains(outputFormat))
            return OdpToPresentationPipeline;

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
            Error error;

            var compResult = new ComparisonResult(pair);

            var failedToExtract = false;
            var equalNumberOfImages = false;

            var tempFoldersForImages = BasePipeline.CreateTempFoldersForImages();
            try
            {
                ImageExtraction.ExtractImagesFromOpenDocumentsToDisk(pair.OriginalFilePath, tempFoldersForImages.Item1);
                ImageExtraction.ExtractImagesFromPdfToDisk(pair.NewFilePath, tempFoldersForImages.Item2);
                // Some checks will be skipped if the number of images is not equal
                equalNumberOfImages = ImageExtraction.CheckIfEqualNumberOfImages(tempFoldersForImages.Item1,
                    tempFoldersForImages.Item2);
            }
            catch (Exception)
            {
                failedToExtract = true;
            }

            ComperingMethods.CompareFonts(pair, ref compResult);
            
            if (GlobalVariables.Options.GetMethod(Methods.Pages))
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
                        compResult.AddTestResult(Methods.Pages, false, errors: [error]);
                        break;
                    case > 0:
                        error = new Error(
                            "Difference in page count",
                            "The original and new document have a different page count.",
                            ErrorSeverity.High,
                            ErrorType.FileError,
                            $"{diff}"
                        );
                        compResult.AddTestResult(Methods.Pages, false, errors: [error]);
                        break;
                    default:
                        compResult.AddTestResult(Methods.Pages, true);
                        break;
                }
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.Size))
            {
                var res = ComperingMethods.CheckFileSizeDifference(pair);

                if (res == null)
                {
                    error = new Error(
                            "Could not get file size difference",
                            "The tool was unable to get the file size difference for at least one file.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        );
                    compResult.AddTestResult(Methods.Size, false, errors: [error]);
                } else if (res.Value)
                {
                    error = new Error(
                            "File Size Difference",
                            "The difference in size for the two files exceeds expected values.",
                            ErrorSeverity.Medium,
                            ErrorType.FileError
                        );
                    compResult.AddTestResult(Methods.Size, false, errors: [error]);
                }
                else
                {
                    compResult.AddTestResult(Methods.Size, true);
                }
            }

            if (GlobalVariables.Options.GetMethod(Methods.Animations))
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
                    compResult.AddTestResult(Methods.Animations, false, errors: [error]);
                }
                if (!exceptionOccurred && res)
                {
                    error = new Error(
                        "The original odp file contains animations",
                        "Context from original odp file may be lost due to use of animations.",
                        ErrorSeverity.Medium,
                        ErrorType.Visual
                    );
                    compResult.AddTestResult(Methods.Animations, false, errors: [error]);
                }
                else
                {
                    compResult.AddTestResult(Methods.Animations, true);
                }
            }

            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile))
            {
                if (!failedToExtract)
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

            GlobalVariables.Logger.AddComparisonResult(compResult);

        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
    
    /// <summary>
    /// Pipeline responsible for comparing ODP to other presentations
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void OdpToPresentationPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            Error error;

            var failedToExtract = false;
            var equalNumberOfImages = false;

            var tempFoldersForImages = BasePipeline.CreateTempFoldersForImages();
            try
            {
                ImageExtraction.ExtractImagesToDisk(pair.OriginalFilePath, pair.OriginalFileFormat, tempFoldersForImages.Item1);
                ImageExtraction.ExtractImagesToDisk(pair.NewFilePath, pair.NewFileFormat, tempFoldersForImages.Item2);
                // Some checks will be skipped if the number of images is not equal
                equalNumberOfImages = ImageExtraction.CheckIfEqualNumberOfImages(tempFoldersForImages.Item1,
                    tempFoldersForImages.Item2);
            }
            catch (Exception)
            {
                failedToExtract = true;
            }

            ComperingMethods.CompareFonts(pair);
            
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
                        break;
                    default:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, true);
                        break;
                }
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.Size.Name))
            {
                var res = ComperingMethods.CheckFileSizeDifference(pair);

                if (res == null)
                {
                    error = new Error(
                            "Could not get file size difference",
                            "The tool was unable to get the file size difference for at least one file.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                } else if (res.Value)
                {
                    error = new Error(
                            "File Size Difference",
                            "The difference in size for the two files exceeds expected values.",
                            ErrorSeverity.Medium,
                            ErrorType.FileError
                        );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                }
                else
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, true);
                }
            }

            if (!failedToExtract)
            {
                if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name))
                {
                    if (equalNumberOfImages)
                    {
                        BasePipeline.CheckColorProfiles(tempFoldersForImages.Item1,
                            tempFoldersForImages.Item2, pair);
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
                        GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false, errors: [error]);
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
                GlobalVariables.Logger.AddTestResult(pair, "Image Extraction", false, errors: [error]);
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.Metadata.Name))
            {
                if (equalNumberOfImages)
                {
                    ExtractedImageMetadata.CompareExtractedImages(pair, tempFoldersForImages.Item1,
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
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false, errors: [error]);
                }
            }
            
            BasePipeline.DeleteTempFolders(tempFoldersForImages.Item1, tempFoldersForImages.Item2);
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}