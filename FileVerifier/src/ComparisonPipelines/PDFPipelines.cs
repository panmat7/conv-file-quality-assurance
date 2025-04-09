using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Error error;

            var failedToExtract = false;
            var equalNumberOfImages = false;

            var tempFoldersForImages = BasePipeline.CreateTempFoldersForImages();
            try
            {
                ImageExtraction.ExtractImagesFromPdfToDisk(pair.OriginalFilePath, tempFoldersForImages.Item1);
                ImageExtraction.ExtractImagesFromPdfToDisk(pair.NewFilePath, tempFoldersForImages.Item2);
                // Some checks will be skipped if the number of images is not equal
                equalNumberOfImages = ImageExtraction.CheckIfEqualNumberOfImages(tempFoldersForImages.Item1,
                    tempFoldersForImages.Item2);
            }
            catch (Exception)
            {
                failedToExtract = true;
            }
          
            e.AddRange(ComperingMethods.CompareFonts(pair));
            int? pageDiff = null;
            
            if (GlobalVariables.Options.GetMethod(Methods.Pages.Name))
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
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, false, errors: [error]);
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
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Pages.Name, false, errors: [error]);
                        e.Add(error);
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
                    e.Add(error);
                } else if ((bool)res)
                {
                    error = new Error(
                            "File Size Difference",
                            "The difference in size for the two files exceeds expected values.",
                            ErrorSeverity.Medium,
                            ErrorType.FileError
                        );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                    e.Add(error);
                }
                else
                {
                    GlobalVariables.Logger.AddTestResult(pair, "Size", true);
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
                        e.Add(error);
                    }
                }
            
                if (GlobalVariables.Options.GetMethod(Methods.Transparency.Name))
                {
                    if (equalNumberOfImages)
                    {
                        BasePipeline.CheckTransparency(tempFoldersForImages.Item1,
                            tempFoldersForImages.Item2, pair);
                    }
                    else
                    {
                        error = new Error(
                            "Unequal number of images",
                            "The comparison of transparency could not be performed " +
                            "because the number of images in the original and new file is different.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        );
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false, errors: [error]);
                        e.Add(error);
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
            
            if (GlobalVariables.Options.GetMethod(Methods.VisualDocComp.Name))
            {
                //No point preformed if mismatched pages
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
                                GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, false,
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
                            GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, false, errors: res);
                        }
                        
                        if(!errors) GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, true);
                        
                        GC.WaitForPendingFinalizers();
                    }
                    else
                    {
                        var res = ComperingMethods.VisualDocumentComparison(pair);
                
                        if (res == null)
                            GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, false,
                                errors: [new Error(
                                    "Error while preforming the visual comparison",
                                    "Could not preform the visual comparison due to an error while getting the page " +
                                    "images or while segmenting the image.",
                                    ErrorSeverity.Medium,
                                    ErrorType.Visual
                                )]);
                        else if (res.Count > 0)
                            GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, false,
                                errors: res);
                        
                        else
                            GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, true);
                    }
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Methods.VisualDocComp.Name, false,
                        comments: ["Comparison not preformed due to page count differences."]);
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