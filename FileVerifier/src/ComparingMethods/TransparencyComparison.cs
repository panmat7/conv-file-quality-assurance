using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageMagick;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace AvaloniaDraft.ComparingMethods;

public static class TransparencyComparison
{
    /// <summary>
    /// Compares transparency between images in pdf files
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool PdfToPdfTransparencyComparison(List<IPdfImage> oImages, List<IPdfImage> nImages)
    {
        return ComparePdfImagesWithPdfImages(oImages, nImages);
    }
    
    /// <summary>
    /// Compares transparency between xlsx file and pdf images
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <param name="imagesOverCells"></param>
    /// <returns></returns>
    public static bool XlsxToPdfColorProfileComparison(List<MagickImage> oImages, List<IPdfImage> nImages, 
        List<string> imagesOverCells)
    {
        // Get the array position of images
        var imageNumbersOverCells = imagesOverCells.Select(image => int.Parse(new string(image
            .Where(char.IsDigit).ToArray())) - 1).ToList();
        
        // Do comparison only on images that are not drawn over cell
        return !oImages.Where((t, i) => imageNumbersOverCells.Count != 0 && 
                                        !imageNumbersOverCells.Contains(i) && 
                                        !CompareNonPdfImagesWithPdfImages([t], [nImages[i]])).Any();
    }

    /// <summary>
    /// General function to compare the transparency of images between docx, pptx, pdf with a pdf file
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool GeneralDocsToPdfTransparencyComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        return CompareNonPdfImagesWithPdfImages(oImages, nImages);
    }

    /// <summary>
    /// Compares transparency between two lists of images where both lists of images are from pdf
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    private static bool ComparePdfImagesWithPdfImages(List<IPdfImage> oImages, List<IPdfImage> nImages)
    {
        if (oImages.Count < 1)
        {
            return true;
        }
        
        if (oImages.Count != nImages.Count)
        {
            return false;
        }
        
        return !oImages.Where((t, i) => CheckPdfImageTransparency(t) != CheckPdfImageTransparency(nImages[i])).Any();
    }
    
    /// <summary>
    /// Compares transparency between two lists of images where both images from non pdf are compared with pdf images
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    private static bool CompareNonPdfImagesWithPdfImages(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        if (oImages.Count < 1)
        {
            return true;
        }
        
        if (oImages.Count != nImages.Count)
        {
            return false;
        }
        
        return !oImages.Where((t, i) => CheckNonPdfImageTransparency(t) != CheckPdfImageTransparency(nImages[i])).Any();
    }
    
    public static bool CompareTransparencyInImagesOnDisk(string oFolderPath, string nFolderPath)
    {
        var oFiles = Directory.GetFiles(oFolderPath).OrderBy(File.GetCreationTime).ToArray();
        var nFiles = Directory.GetFiles(nFolderPath).OrderBy(File.GetCreationTime).ToArray();
    
        // If both folders are empty, return true
        if (oFiles.Length == 0 && nFiles.Length == 0) return true;
    
        // If the number of files in the folders differ, return false
        if (oFiles.Length != nFiles.Length) throw new InvalidOperationException("The number of files in the folders differ.");
    
        for (var i = 0; i < oFiles.Length; i++)
        {
            using var oImage = new MagickImage(oFiles[i]);
            using var nImage = new MagickImage(nFiles[i]);
            
            var oImageHasTransparency = CheckNonPdfImageTransparency(oImage);
            
            var nImageHasTransparency = CheckNonPdfImageTransparency(nImage);
            
            if (oImageHasTransparency == nImageHasTransparency) continue;
            return false;
        }
    
        return true;
    }

    /// <summary>
    /// Checks the transparency of an image from a pdf
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    private static bool CheckPdfImageTransparency(IPdfImage image)
    {
        // Check for soft mask (SMask)
        return image.ImageDictionary.ContainsKey(NameToken.Smask) ||
               // Check for explicit mask (Mask)
               image.ImageDictionary.ContainsKey(NameToken.Mask);
    }

    /// <summary>
    /// Checks the transparency of an image
    /// </summary>
    /// <param name="image"></param>
    /// <returns></returns>
    private static bool CheckNonPdfImageTransparency(MagickImage image)
    {
        using var pixels = image.GetPixels();
        if (!image.HasAlpha)
            return false;

        var values = pixels.GetValues();
        var channels = (int)image.ChannelCount;
        var compValue = image.Depth == 16 ? 65535 : 255;
        
        
        if (values == null) return false;
        for (var i = 3; i < values.Length; i += channels)
        {
            if (values[i] < compValue)
                return true;
        }

        return false;
    }
}