using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ComparisonPipelines;

public static class ODSPipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for ODS files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetOdsPipeline(string outputFormat)
    {
        if (FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return OdsToPdfPipeline;

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing Ods to other PDF conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void OdsToPdfPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            Error error;

            var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(pair.OriginalFilePath);
            var nImages = ImageExtraction.GetNonDuplicatePdfImages(pair.NewFilePath);
            
            ComperingMethods.CompareFonts(pair);
            
            if (GlobalVariables.Options.GetMethod(Methods.Size.Name))
            {
                var res = ComperingMethods.CheckFileSizeDifference(pair, 0.5); //Use settings later

                if (res == null)
                {
                    error = new Error(
                            "Could not get file size difference",
                            "The tool was unable to get the file size difference for at least one file.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                } else if ((bool)res)
                {
                    //For now only printing to console
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
                        "Error comparing color profiles in ods contained images",
                        "There occurred an error while extracting and comparing " +
                        "color profiles of the images contained in the ods.",
                        ErrorSeverity.High,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.ColorProfile.Name, false, errors: [error]);
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
                    res = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    error = new Error(
                        "Error comparing transparency in ods contained images",
                        "There occurred an error while comparing transparency" +
                        " of the images contained in the ods.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false, errors: [error]);
                }

                switch (exceptionOccurred)
                {
                    case false when !res:
                        error = new Error(
                            "Difference of transparency detected in images contained in the ods",
                            "The images contained in the ods and pdf files did not pass Transparency comparison.",
                            ErrorSeverity.Medium,
                            ErrorType.Visual
                        );
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, false, errors: [error]);
                        break;
                    case false when res:
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, true);
                        break;
                }
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.TableBreakCheck.Name))
            {
                var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(pair.OriginalFilePath);
                if (res == null)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.TableBreakCheck.Name, false, errors: [
                        new Error(
                            "Could not preform check for table breaks",
                            "There occured an error when trying to preform check for table breaks.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )
                    ]);
                else if (res.Count > 0)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.TableBreakCheck.Name, false, errors: res);
                else
                    GlobalVariables.Logger.AddTestResult(pair, Methods.TableBreakCheck.Name, true);
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.Metadata.Name))
            {
                if(oImages.Count != nImages.Count)
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                        comments: ["Could not preform the metadata check due to the two files having different number of images.",
                            "This test was preformed on an extracted image."]);
                else
                {
                    var res = ComperingMethods.ComparExtractedImageMetadata(oImages, nImages);

                    if (res == null)
                        GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                            comments: ["Error while checking the metadata of extracted images.",
                                "This test was preformed on an extracted image."]);
                    else
                    {
                        var failedCount = res.Value.Item1;
                        var errCount = res.Value.Item2;
                        var errorFound = res.Value.Item3;
                    
                        //Nothing wrong
                        if(failedCount == 0 && errCount == 0 && !errorFound.Any())
                            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, true,
                                comments: ["This test was preformed on an extracted image."]);
                    
                        //No failures
                        else if(failedCount == 0)
                            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                                errors: errorFound.ToList(),
                                comments: [$"One or more of the following errors are present in {errCount} of {nImages.Count} images.",
                                    "This test was preformed on an extracted image."]);
                        //No errors
                        else if (errCount == 0)
                            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                                comments: [$"Could not check {failedCount} of {nImages.Count} images.",
                                    "This test was preformed on an extracted image."]);
                        //Failures and errors (very bad)
                        else
                            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                                errors: errorFound.ToList(),
                                comments: [$"Could not check {failedCount} of {nImages.Count} images.",
                                    $"One or more of the following errors are present in {errCount} of {nImages.Count} images.",
                                    "This test was preformed on an extracted image."]);
                    }
                }
            }
            
            ImageExtraction.DisposeMagickImages(oImages);
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}