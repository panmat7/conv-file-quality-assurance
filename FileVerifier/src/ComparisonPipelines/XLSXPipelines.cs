using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;
using AvaloniaDraft.ProgramManager;

namespace AvaloniaDraft.ComparisonPipelines;

public static class XLSXPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for XLSX files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetXlsxPipeline(string? outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return XlsxToPdfPipeline;
        if (!FormatCodes.PronomCodesCSV.Contains(outputFormat) && FormatCodes.PronomCodesSpreadsheets.Contains(outputFormat))
            return XlsxToSpreadsheetPipeline;

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing XLSX to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void XlsxToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
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
                ImageExtractionToDisk.ExtractImagesFromXlsxToDisk(pair.OriginalFilePath, tempFoldersForImages.Item1);
                ImageExtractionToDisk.ExtractImagesFromPdfToDisk(pair.NewFilePath, tempFoldersForImages.Item2);
                // Some checks will be skipped if the number of images is not equal
                equalNumberOfImages = ImageExtractionToDisk.CheckIfEqualNumberOfImages(tempFoldersForImages.Item1,
                    tempFoldersForImages.Item2);
            }
            catch (Exception)
            {
                failedToExtract = true;
            }
        
            ComparingMethods.ComparingMethods.CompareFonts(pair, ref compResult);
            
            if (GlobalVariables.Options.GetMethod(Methods.Size))
            {
                var res = ComparingMethods.ComparingMethods.CheckFileSizeDifference(pair);

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
                    error =  new Error(
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

            if (GlobalVariables.Options.GetMethod(Methods.TableBreakCheck))
            {
                var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(pair.OriginalFilePath);
                if (res.Count > 0)
                    compResult.AddTestResult(Methods.TableBreakCheck, false, errors: res);
                else
                    compResult.AddTestResult(Methods.TableBreakCheck, true);
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
            //Check for empty pages
            if (GlobalVariables.Options.GetMethod(Methods.CheckForEmptyPages))
            {
                
                if (FindEmptyPagesPdf.EmptyPagePdf(pair.NewFilePath) > 0)
                {
                    error = new Error(
                        "Empty pages",
                        "The new file contains empty pages.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    );
                    compResult.AddTestResult(Methods.CheckForEmptyPages, false, errors: [error]);
                } 
            }
            
            BasePipeline.DeleteTempFolders(tempFoldersForImages.Item1, tempFoldersForImages.Item2);

            GlobalVariables.Logger.AddComparisonResult(compResult);

        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
    
    /// <summary>
    /// Pipeline responsible for comparing XLSX to other spreadsheets
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void XlsxToSpreadsheetPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            Error error;

            var failedToExtract = false;
            var equalNumberOfImages = false;

            var compResult = new ComparisonResult(pair);

            var tempFoldersForImages = BasePipeline.CreateTempFoldersForImages();
            try
            {
                ImageExtractionToDisk.ExtractImagesToDisk(pair.OriginalFilePath, pair.OriginalFileFormat, tempFoldersForImages.Item1);
                ImageExtractionToDisk.ExtractImagesToDisk(pair.NewFilePath, pair.NewFileFormat, tempFoldersForImages.Item2);
                // Some checks will be skipped if the number of images is not equal
                equalNumberOfImages = ImageExtractionToDisk.CheckIfEqualNumberOfImages(tempFoldersForImages.Item1,
                    tempFoldersForImages.Item2);
            }
            catch (Exception)
            {
                failedToExtract = true;
            }
            
            ComparingMethods.ComparingMethods.CompareFonts(pair, ref compResult);
            
            if (GlobalVariables.Options.GetMethod(Methods.Size))
            {
                var res = ComparingMethods.ComparingMethods.CheckFileSizeDifference(pair);

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
                    //For now only printing to console
                    error = new Error(
                            "File Size Difference",
                            "The difference in size for the two files exceeds expected values.",
                            ErrorSeverity.Medium,
                            ErrorType.FileError
                        );
                    compResult.AddTestResult(Methods.Size.Name, false, errors: [error]);
                }
                else
                {
                    compResult.AddTestResult(Methods.Size.Name, true);
                }
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
                compResult.AddTestResult("Image Extraction", false, errors: [error]);
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
            
            GlobalVariables.Logger.AddComparisonResult(compResult);
            
            BasePipeline.DeleteTempFolders(tempFoldersForImages.Item1, tempFoldersForImages.Item2);
             
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}