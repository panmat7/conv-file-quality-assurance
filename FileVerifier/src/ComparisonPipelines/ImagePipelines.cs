using System;
using System.Collections.Generic;
using System.IO;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ClosedXML;
using DocumentFormat.OpenXml.Wordprocessing;
using ImageMagick;

namespace AvaloniaDraft.ComparisonPipelines;

public static class ImagePipelines
{
    /// <summary>
    /// Function responsible for assigning the correct pipeline for PNG files
    /// </summary>
    /// <param name="outputFormat">Format of the converted file</param>
    /// <returns>Function with the correct pipeline, null if there were no suitable function.</returns>
    public static Action<FilePair, int, Action<int>, Action>? GetImagePipelines(string outputFormat)
    {
        if (FormatCodes.PronomCodesImages.Contains(outputFormat))
            return ImageToImagePipeline;
        
        if(FormatCodes.PronomCodesPDF.Contains(outputFormat) || FormatCodes.PronomCodesPDFA.Contains(outputFormat))
            return ImageToPDFPipeline;

        return null;
    }

    /// <summary>
    /// Pipeline responsible for comparing images to other image formats conversions
    /// </summary>
    /// <param name="pair">The pair of files to compare</param>
    /// <param name="additionalThreads">Number of threads available for usage</param>
    /// <param name="updateThreadCount">Callback function used to update current thread count</param>
    /// <param name="markDone">Function marking the FilePair as done</param>
    private static void ImageToImagePipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount, Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            Error error;

            var oFormatInfo = MagickFormatInfo.Create(pair.OriginalFilePath);
            var nFormatInfo = MagickFormatInfo.Create(pair.NewFilePath);
            
            var oSettings = ColorProfileComparison.CreateFormatSpecificSettings(oFormatInfo?.Format);
            var nSettings = ColorProfileComparison.CreateFormatSpecificSettings(nFormatInfo?.Format);
            
            using var oImage = new MagickImage(pair.OriginalFilePath, oSettings);
            using var nImage = new MagickImage(pair.NewFilePath, nSettings);
            
            //Check options if this check is enabled.
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
                }
                else if ((bool)res)
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
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.Size.Name, true);
                
            }

            if (GlobalVariables.Options.GetMethod(Methods.Resolution.Name))
            {
                var res = ComperingMethods.GetImageResolutionDifference(pair);

                if (res is null)
                {
                    error = new Error(
                        "Error getting image resolution difference",
                        "There occured an error while trying to get the difference in image resolution.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    );

                    GlobalVariables.Logger.AddTestResult(pair, Methods.Resolution.Name, false, errors: [error]);
                }
                else if (res.Item1 > 0 || res.Item2 > 0)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Resolution.Name, false, errors: [
                        new Error(
                        "Image resolution difference",
                        "Mismatched resolution between images.",
                        ErrorSeverity.High,
                        ErrorType.FileError
                    )]);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.Resolution.Name, true);
            }

            if (GlobalVariables.Options.GetMethod(Methods.Metadata.Name))
            {
                var res = ComperingMethods.GetMissingOrWrongImageMetadataExif(pair);
                
                if (res is null)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false, errors: [
                        new Error(
                            "Could not read metadata",
                            "There occurred an error when trying to read the metadata of the image file.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )
                    ]);
                }
                else if (res.Count > 0)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false, errors: res);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.Metadata.Name, true);
            }

            if(GlobalVariables.Options.GetMethod(Methods.PointByPoint.Name))
            {
                var acceptance = 0.5; //Read from options later ?

                var res = PbpComparisonMagick.CalculateImageSimilarity(pair);

                if (res < 0)
                {
                    error = new Error(
                        "Error calculating image similarity",
                        "There occured an error while calculating the image similarity during Pixel by Pixel comparison.",
                        ErrorSeverity.High,
                        ErrorType.Visual
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.PointByPoint.Name, false, errors: [error]);
                } else if (res < acceptance)
                {
                    error = new Error(
                            "Difference in image's visual appearance",
                            "The images did not pass Pixel by Pixel comparison.",
                            ErrorSeverity.High,
                            ErrorType.Visual,
                            res.ToString("0.##")
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.PointByPoint.Name, false, errors: [error]);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.PointByPoint.Name, true);
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.ImageToImageColorProfileComparison(oImage, nImage);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    error = new Error(
                        "Error comparing color profiles",
                        "There occurred an error while extracting and comparing color profiles.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                }

                if (!exceptionOccurred && !res)
                {
                    error = new Error(
                        "Difference in both images color profile",
                        "The images did not pass Color Profile comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.ColorProfile.Name, true);
            }
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
    
    private static void ImageToPDFPipeline(FilePair pair, int additionalThreads, Action<int> updateThreadCount,
        Action markDone)
    {
        BasePipeline.ExecutePipeline(() =>
        {
            Error error;
            
            var tempFolder = BasePipeline.CreateTempFolderForImages();
            ImageExtraction.ExtractImagesFromPdfToDisk(pair.NewFilePath, tempFolder);
            
            var oFormatInfo = MagickFormatInfo.Create(pair.OriginalFilePath);
            var nFormatInfo = MagickFormatInfo.Create(pair.NewFilePath);
            var oSettings = ColorProfileComparison.CreateFormatSpecificSettings(oFormatInfo?.Format);
            var nSettings = ColorProfileComparison.CreateFormatSpecificSettings(nFormatInfo?.Format);
            
            using var oImage = new MagickImage(pair.OriginalFilePath, oSettings);
    
            var tempFiles = Directory.GetFiles(tempFolder);
            
            //Image converted to PDF should result in a single image embedded in the PDF 
            if (tempFiles.Length != 1)
            {
                GlobalVariables.Logger.AddTestResult(pair, "Image Count in Resulting PDF", false,
                    comments: ["The resulting PDF does not contain exactly one image."]);
                return;
            }
            
            //Converting the image to bytes encoded to correct format
            using var nImage = new MagickImage(pair.NewFilePath, nSettings);
            var pronomCode = ImageExtraction.GetExpectedPronomFromImage(nImage.Format);
            
            FilePair? pairWithTemp = null;
            
            pairWithTemp = new FilePair(
                pair.OriginalFilePath, pair.OriginalFileFormat,
                tempFiles[0], pronomCode
            );
            
            if (GlobalVariables.Options.GetMethod(Methods.Size.Name))
            {
                var res = ComperingMethods.CheckFileSizeDifference(pair, 0.5); //Use settings later
    
                if (res == null)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [
                        new Error(
                            "Could not get file size difference",
                            "The tool was unable to get the file size difference for at least one file.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )
                    ]);
                }
                else if ((bool)res)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [
                        new Error(
                            "File Size Difference",
                            "The difference in size for the two files exceeds expected values.",
                            ErrorSeverity.Medium,
                            ErrorType.FileError
                        )
                    ]);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, true);
            }
    
            if (GlobalVariables.Options.GetMethod(Methods.Resolution.Name) && pairWithTemp != null)
            {
                var res = ComperingMethods.GetImageResolutionDifference(pairWithTemp);
    
                if (res is null)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Resolution.Name, false, errors: [
                        new Error(
                            "Error getting image resolution difference",
                            "There occured an error while trying to get the difference in image resolution.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )
                    ]);
                }
                else if (res.Item1 > 0 || res.Item2 > 0)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Resolution.Name, false, errors: [
                        new Error(
                            "Image resolution difference",
                            "Mismatched resolution between images.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )
                    ]);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Resolution.Name, true);
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.Metadata.Name) && pairWithTemp != null)
            {
                var res = ComperingMethods.GetMissingOrWrongImageMetadataExif(pairWithTemp);
                    
                if (res is null)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false, 
                        comments: ["This test was preformed on an extracted image."],
                        errors: [
                        new Error(
                            "Could not read metadata",
                            "There occurred an error when trying to read the metadata of the image file.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )
                    ]);
                    
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false, errors: [
                        new Error(
                            "Could not read metadata",
                            "There occurred an error when trying to read the metadata of the image file.",
                            ErrorSeverity.High,
                            ErrorType.FileError
                        )
                    ]);
                }
                else if (res.Count > 0)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false, errors: res);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.Metadata.Name, true);
            }
    
            if(GlobalVariables.Options.GetMethod(Methods.PointByPoint.Name) && pairWithTemp != null)
            {
                var acceptance = 85; //Read from options later ?
    
                var res = ImageRegistration.CalculateHistogramSimilarity(pairWithTemp);
    
                if (res < 0)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.PointByPoint.Name, false, 
                        comments: ["This test was preformed on an extracted image."],
                        errors: [
                            new Error(
                                "Error calculating image similarity",
                                "There occured an error while calculating the image similarity during Pixel by Pixel comparison.",
                                ErrorSeverity.High,
                                ErrorType.Visual
                            )
                        ]);
                } else if (res < acceptance)
                {
                    GlobalVariables.Logger.AddTestResult(pair, Methods.PointByPoint.Name, false, 
                        comments: ["This test was preformed on an extracted image."],
                        errors: [
                            new Error(
                                "Difference in image's visual appearance",
                                "The images did not pass Pixel by Pixel comparison.",
                                ErrorSeverity.High,
                                ErrorType.Visual,
                                res.ToString("0.##")
                            )
                        ]);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.PointByPoint.Name, true);
            }
            
            if (GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name))
            {
                var res = false;
                var exceptionOccurred = false;

                try
                {
                    res = ColorProfileComparison.ImageToImageColorProfileComparison(oImage, nImage);
                }
                catch (Exception)
                {
                    exceptionOccurred = true;
                    error = new Error(
                        "Error comparing color profiles",
                        "There occurred an error while extracting and comparing color profiles.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                }

                if (!exceptionOccurred && !res)
                {
                    error = new Error(
                        "Difference in both images color profile",
                        "The images did not pass Color Profile comparison.",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    );
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Size.Name, false, errors: [error]);
                }
                else
                    GlobalVariables.Logger.AddTestResult(pair, Helpers.Methods.ColorProfile.Name, true);
            }
            
        }, [pair.OriginalFilePath, pair.NewFilePath], additionalThreads, updateThreadCount, markDone);
    }
}