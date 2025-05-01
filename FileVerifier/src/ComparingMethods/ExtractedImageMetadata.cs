using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;
using DocumentFormat.OpenXml.Drawing.Charts;
using ImageMagick;
using UglyToad.PdfPig.Content;

namespace AvaloniaDraft.ComparingMethods;

[ExcludeFromCodeCoverage]
public static class ExtractedImageMetadata
{
    /// <summary>
    /// Preforms the comparison between the images and logs all errors
    /// </summary>
    /// <param name="pair">The pair of files, used for correct logging.</param>
    /// <param name="oPath">Images from the original file.</param>
    /// <param name="nPath">Images from the new file.</param>
    public static void CompareExtractedImages(FilePair pair, ref ComparisonResult compResult, string oPath, string nPath)
    {
        var oFiles = Directory.GetFiles(oPath).OrderBy(File.GetCreationTime).ToList();
        var nFiles = Directory.GetFiles(nPath).OrderBy(File.GetCreationTime).ToList();
        
        //If no images, we just pass the test
        if (oFiles.Count == 0 && nFiles.Count == 0)
        {
            compResult.AddTestResult(Methods.Metadata, true, 
                comments: ["No images present in the files."]);
            return;
        }

        var errCount = 0;
        var failedCount = 0;
        var imgCount = oFiles.Count;
        var distinctErrors = new HashSet<Error>();
        var transparency = false;
        for (var i = 0; i < oFiles.Count; i++)
        {
            var oExt = Path.GetExtension(oFiles[i]).TrimStart('.');
            var nExt = Path.GetExtension(nFiles[i]).TrimStart('.');
            
            var tempPair = new FilePair(
                oFiles[i], ExtensionToPronom(oExt),
                nFiles[i], ExtensionToPronom(nExt));
            
            var e = ComperingMethods.GetMissingOrWrongImageMetadataExif(tempPair);

            if (e == null)
                failedCount++;
            else if (e.Count > 0)
            {
                errCount++;
                e.ForEach(err => distinctErrors.Add(err));
                
                //Specifying transparency differences.
                if(!transparency && e.Any(err => err.Description.Contains("Transparency loss")))
                    GlobalVariables.Logger.AddTestResult(pair, Methods.Transparency.Name, true,
                        errors: [ new Error("Transparency difference detected",
                                "The images contained in the documents have different transparencies.",
                                ErrorSeverity.Medium,
                                ErrorType.Visual
                        )]
                    );
            }
        }

        //Nothing wrong
        if(failedCount == 0 && errCount == 0 && distinctErrors.Count == 0)
            compResult.AddTestResult(Methods.Metadata, true,
                comments: ["This test was performed on an extracted image."]);
        
        //No failures
        else if(failedCount == 0)
            compResult.AddTestResult(Methods.Metadata, false,
                errors: distinctErrors.ToList(),
                comments: [$"One or more of the following errors are present in {errCount} of {imgCount} image pairs.",
                    "This test was performed on an extracted image."]);
        //No errors
        else if (errCount == 0)
            compResult.AddTestResult(Methods.Metadata, false,
                comments: [$"Could not check {failedCount} of {imgCount} images.",
                    "This test was performed on an extracted image."]);
        
        //Failures and errors (very bad)
        else
            compResult.AddTestResult(Methods.Metadata, false,
                errors: distinctErrors.ToList(),
                comments: [$"Could not check {failedCount} of {imgCount} images.",
                    $"One or more of the following errors are present in {errCount} of {imgCount} image pairs.",
                    "This test was performed on an extracted image."]);
    }

    /// <summary>
    /// Gets the expected pronom code based on the extension string.
    /// </summary>
    /// <param name="extension">The string extention (without the dot).</param>
    /// <returns>The first PRONOM code of the list.</returns>
    private static string ExtensionToPronom(string extension)
    {
        return extension switch
        {
            "jpg" or "jpeg" => FormatCodes.PronomCodesJPEG.PronomCodes[0],
            "png" => FormatCodes.PronomCodesPNG.PronomCodes[0],
            "gif" => FormatCodes.PronomCodesGIF.PronomCodes[0],
            "bmp" => FormatCodes.PronomCodesBMP.PronomCodes[0],
            "tiff" => FormatCodes.PronomCodesTIFF.PronomCodes[0],
            _ => ""
        };
    }
}