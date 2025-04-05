using System.Collections.Generic;
using System.Linq;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ImageMagick;
using UglyToad.PdfPig.Content;

namespace AvaloniaDraft.ComparingMethods;

public static class ExtractedImageMetadata
{
    /// <summary>
    /// Preforms the comparison between the images and logs all errors
    /// </summary>
    /// <param name="pair">The pair of files, used for correct logging.</param>
    /// <param name="oImages">Images from the original file.</param>
    /// <param name="nImages">Images from the new file.</param>
    public static void CompareExtractedImages(FilePair pair, List<MagickImage> oImages, List<MagickImage> nImages)
    {
        //If no images, we just pass the test
        if (oImages.Count == 0 && nImages.Count == 0)
        {
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, true, 
                comments: ["No images present in the files."]);
            return;
        }
        
        //Auto fail is mismatch count something wrong + we cannot know which images correspond
        if(oImages.Count != nImages.Count)
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                comments: ["Could not preform the metadata check due to the two files having different number of images.",
                    "This test was preformed on an extracted image."]);
        else
        {
            //Getting results
            var res = CompareExtractedImageMetadata(oImages, nImages);

            if (res == null)
                GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                    comments: ["Error while checking the metadata of extracted images.",
                        "This test was preformed on an extracted image."]);
            else
            {
                LogErrors(pair, res, oImages.Count);
            }
        }
    }
    
    /// <summary>
    /// Preforms the comparison between the images and logs all errors
    /// </summary>
    /// <param name="pair">The pair of files, used for correct logging.</param>
    /// <param name="oImages">Images from the original file.</param>
    /// <param name="nImages">Images from the new file.</param>
    public static void CompareExtractedImages(FilePair pair, List<IPdfImage> oImages, List<IPdfImage> nImages)
    {
        //If no images, we just pass the test
        if (oImages.Count == 0 && nImages.Count == 0)
        {
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, true, 
                comments: ["No images present in the files."]);
            return;
        }
        
        //Auto fail is mismatch count something wrong + we cannot know which images correspond
        if(oImages.Count != nImages.Count)
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                comments: ["Could not preform the metadata check due to the two files having different number of images.",
                    "This test was preformed on an extracted image."]);
        else
        {
            //Getting results
            var res = CompareExtractedImageMetadata(oImages, nImages);

            if (res == null)
                GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                    comments: ["Error while checking the metadata of extracted images.",
                        "This test was preformed on an extracted image."]);
            else
            {
                LogErrors(pair, res, oImages.Count);
            }
        }
    }
    
    /// <summary>
    /// Preforms the comparison between the images and logs all errors
    /// </summary>
    /// <param name="pair">The pair of files, used for correct logging.</param>
    /// <param name="oImages">Images from the original file.</param>
    /// <param name="nImages">Images from the new file.</param>
    public static void CompareExtractedImages(FilePair pair, List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        //If no images, we just pass the test
        if (oImages.Count == 0 && nImages.Count == 0)
        {
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, true, 
                comments: ["No images present in the files."]);
            return;
        }
        
        //Auto fail is mismatch count something wrong + we cannot know which images correspond
        if(oImages.Count != nImages.Count)
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                comments: ["Could not preform the metadata check due to the two files having different number of images.",
                    "This test was preformed on an extracted image."]);
        else
        {
            //Getting results
            var res = CompareExtractedImageMetadata(oImages, nImages);

            if (res == null)
                GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                    comments: ["Error while checking the metadata of extracted images.",
                        "This test was preformed on an extracted image."]);
            else
            {
                LogErrors(pair, res, oImages.Count);
            }
        }
    }
    
    /// <summary>
    /// Logs all the errors, or logs a pass, based on the results.
    /// </summary>
    /// <param name="pair"></param>
    /// <param name="res"></param>
    /// <param name="imgCount"></param>
    private static void LogErrors(FilePair pair, (int, int, List<Error>)? res, int imgCount)
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
                comments: [$"One or more of the following errors are present in {errCount} of {imgCount} images.",
                    "This test was preformed on an extracted image."]);
        //No errors
        else if (errCount == 0)
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                comments: [$"Could not check {failedCount} of {imgCount} images.",
                    "This test was preformed on an extracted image."]);
        
        //Failures and errors (very bad)
        else
            GlobalVariables.Logger.AddTestResult(pair, Methods.Metadata.Name, false,
                errors: errorFound.ToList(),
                comments: [$"Could not check {failedCount} of {imgCount} images.",
                    $"One or more of the following errors are present in {errCount} of {imgCount} images.",
                    "This test was preformed on an extracted image."]);
    }
    
    
    /// <summary>
    /// Compares metadata of images extracted in MagickImage format.
    /// </summary>
    /// <param name="oImages">List of images in the original file.</param>
    /// <param name="nImages">List of images in the new file.</param>
    /// <returns>
    /// The count of how many times metadata extraction failed, the count of how many images had errors and a list of 
    /// each distinct error encountered. 
    /// </returns>
    private static (int, int, List<Error>)? CompareExtractedImageMetadata(List<MagickImage> oImages, List<MagickImage> nImages)
    {
        var failedCount = 0;
        var errCount = 0;
        HashSet<Error> errorFound = new();
        List<string> paths = new();

        try
        {
            for (int i = 0; i < nImages.Count; i++)
            {
                var oImg = ImageExtraction.SaveExtractedMagickImageToDisk(oImages[i], oImages[i].Format);
                var nImg = ImageExtraction.SaveExtractedMagickImageToDisk(nImages[i], nImages[i].Format);

                if (oImg == null || nImg == null)
                {
                    failedCount++;
                    continue;
                }

                paths.Add(oImg.Value.Item1);
                paths.Add(nImg.Value.Item1);

                var tempPair = new FilePair(
                    oImg.Value.Item1, oImg.Value.Item2,
                    nImg.Value.Item1, nImg.Value.Item2
                );

                var errors = ComperingMethods.GetMissingOrWrongImageMetadataExif(tempPair);

                if (errors == null)
                {
                    failedCount++;
                    continue;
                }

                if (errors.Count > 0)
                {
                    errors.ForEach(err => errorFound.Add(err));
                    errCount++;
                }
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            foreach (var file in paths)
            {
                TempFiles.DeleteTemporaryFile(file);
            }
        }
        
        return (failedCount, errCount, errorFound.ToList());
    }
    
    /// <summary>
    /// Compares metadata of images extracted in MagickImage and IPdfImage formats.
    /// </summary>
    /// <param name="oImages">List of images in the original file.</param>
    /// <param name="nImages">List of images in the new file.</param>
    /// <returns>
    /// The count of how many times metadata extraction failed, the count of how many images had errors and a list of 
    /// each distinct error encountered. 
    /// </returns>
    private static (int, int, List<Error>)? CompareExtractedImageMetadata(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        var failedCount = 0;
        var errCount = 0;
        HashSet<Error> errorFound = new();
        List<string> paths = new();

        try
        {
            for (int i = 0; i < nImages.Count; i++)
            {
                var oImg = ImageExtraction.SaveExtractedMagickImageToDisk(oImages[i], oImages[i].Format);
                var nImg = ImageExtraction.SaveExtractedIPdfImageToDisk(nImages[i]);

                if (oImg == null || nImg == null)
                {
                    failedCount++;
                    continue;
                }
                
                paths.Add(oImg.Value.Item1);
                paths.Add(nImg.Value.Item1);

                var tempPair = new FilePair(
                    oImg.Value.Item1, oImg.Value.Item2,
                    nImg.Value.Item1, nImg.Value.Item2
                );

                var errors = ComperingMethods.GetMissingOrWrongImageMetadataExif(tempPair);
                if (errors == null)
                {
                    failedCount++;
                    continue;
                }

                if (errors.Count > 0)
                {
                    errors.ForEach(err => errorFound.Add(err));
                    errCount++;
                }
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            foreach (var file in paths)
            {
                TempFiles.DeleteTemporaryFile(file);
            }
        }
        
        return (failedCount, errCount, errorFound.ToList());
    }
    
    /// <summary>
    /// Compares metadata of images extracted in IPdfImage format.
    /// </summary>
    /// <param name="oImages">List of images in the original file.</param>
    /// <param name="nImages">List of images in the new file.</param>
    /// <returns>
    /// The count of how many times metadata extraction failed, the count of how many images had errors and a list of 
    /// each distinct error encountered. 
    /// </returns>
    private static (int, int, List<Error>)? CompareExtractedImageMetadata(List<IPdfImage> oImages, List<IPdfImage> nImages)
    {
        var failedCount = 0;
        var errCount = 0;
        HashSet<Error> errorFound = new();
        List<string> paths = new();

        try
        {
            for (int i = 0; i < nImages.Count; i++)
            {
                var oImg = ImageExtraction.SaveExtractedIPdfImageToDisk(oImages[i]);
                var nImg = ImageExtraction.SaveExtractedIPdfImageToDisk(nImages[i]);

                if (oImg == null || nImg == null)
                {
                    failedCount++;
                    continue;
                }
                
                paths.Add(oImg.Value.Item1);
                paths.Add(nImg.Value.Item1);

                var tempPair = new FilePair(
                    oImg.Value.Item1, oImg.Value.Item2,
                    nImg.Value.Item1, nImg.Value.Item2
                );

                var errors = ComperingMethods.GetMissingOrWrongImageMetadataExif(tempPair);
                if (errors == null)
                {
                    failedCount++;
                    continue;
                }

                if (errors.Count > 0)
                {
                    errors.ForEach(err => errorFound.Add(err));
                    errCount++;
                }
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            foreach (var file in paths)
            {
                TempFiles.DeleteTemporaryFile(file);
            }
        }
        
        return (failedCount, errCount, errorFound.ToList());
    }
}