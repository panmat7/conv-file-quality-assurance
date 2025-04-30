using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;

namespace AvaloniaDraft.ComparisonPipelines;

public static class PdfPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for PDF files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetPdfPipelines(string? outputFormat)
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
            Error error;

            var compResult = new ComparisonResult(pair);

            var failedToExtract = false;
            var equalNumberOfImages = false;

            var tempFoldersForImages = BasePipeline.CreateTempFoldersForImages();
            try
            {
                ImageExtractionToDisk.ExtractImagesFromPdfToDisk(pair.OriginalFilePath, tempFoldersForImages.Item1);
                ImageExtractionToDisk.ExtractImagesFromPdfToDisk(pair.NewFilePath, tempFoldersForImages.Item2);
                // Some checks will be skipped if the number of images is not equal
                equalNumberOfImages = ImageExtractionToDisk.CheckIfEqualNumberOfImages(tempFoldersForImages.Item1,
                    tempFoldersForImages.Item2);
            }
            catch (Exception)
            {
                failedToExtract = true;
            }
          
            ComperingMethods.CompareFonts(pair, ref compResult);
            int? pageDiff = null;
            
            if (GlobalVariables.Options.GetMethod(Methods.Pages))
            {
                pageDiff = ComperingMethods.GetPageCountDifferenceExif(pair);
                switch (pageDiff)
                {
                    case null:
                        error = new Error(
                            "Could not get page count",
                            "There was an error trying to get the page count from at least one of the files.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        );
                        compResult.AddTestResult(Methods.Pages, false, errors: [error]);
                        e.Add(error);
                        break;
                    case > 0:
                        error = new Error(
                            "Difference in page count",
                            "The original and new document have a different page count.",
                            ErrorSeverity.High,
                            ErrorType.FileError,
                            $"{pageDiff}"
                        );
                        compResult.AddTestResult(Methods.Pages, false, errors: [error]);
                        e.Add(error);
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
                    e.Add(error);
                } else if (res.Value)
                {
                    error = new Error(
                            "File Size Difference",
                            "The difference in size for the two files exceeds expected values.",
                            ErrorSeverity.Medium,
                            ErrorType.FileError
                        );
                    compResult.AddTestResult(Methods.Size, false, errors: [error]);
                    e.Add(error);
                }
                else
                {
                    compResult.AddTestResult(Methods.Size, true);
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
                        e.Add(error);
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
            
            if (GlobalVariables.Options.GetMethod(Methods.VisualDocComp))
            {
                //No point performed if mismatched pages
                if (pageDiff == 0)
                {
                    //Getting the page count and using it to determine if to do everything in one go, or split
                    var pageCount = ComperingMethods.GetPageCountExif(pair.OriginalFilePath, pair.OriginalFileFormat);

                    if (pageCount > 75)
                    {
                        var checkIntervals = DocumentVisualOperations.GetPageCheckIndexes(pageCount.Value, 25);
                        var errors = false;
                        
                        foreach (var interval in checkIntervals)
                        {
                            var res = ComperingMethods.VisualDocumentComparison(pair, interval.Item1, interval.Item2);

                            if (res == null)
                            {
                                compResult.AddTestResult(Methods.VisualDocComp, false,
                                    errors: [new Error(
                                        "Error while preforming the visual comparison",
                                        "Could not preform the visual comparison due to an error while getting the page " +
                                        "images or while segmenting the image.",
                                        ErrorSeverity.Medium,
                                        ErrorType.Visual
                                    )]);
                                errors = true;
                                break;
                            }

                            if (res.Count <= 0) continue;
                            
                            errors = true;
                            compResult.AddTestResult(Methods.VisualDocComp, false, errors: res);
                        }
                        
                        if(!errors) compResult.AddTestResult(Methods.VisualDocComp, true);
                        
                        GC.WaitForPendingFinalizers();
                    }
                    else
                    {
                        var res = ComperingMethods.VisualDocumentComparison(pair);
                
                        if (res == null)
                            compResult.AddTestResult(Methods.VisualDocComp, false,
                                errors: [new Error(
                                    "Error while preforming the visual comparison",
                                    "Could not preform the visual comparison due to an error while getting the page " +
                                    "images or while segmenting the image.",
                                    ErrorSeverity.Medium,
                                    ErrorType.Visual
                                )]);
                        else if (res.Count > 0)
                            compResult.AddTestResult(Methods.VisualDocComp, false,
                                errors: res);
                        
                        else
                            compResult.AddTestResult(Methods.VisualDocComp, true);
                    }
                }
                else
                    compResult.AddTestResult(Methods.VisualDocComp, false,
                        comments: ["Comparison not performed due to page count differences."]);
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
}