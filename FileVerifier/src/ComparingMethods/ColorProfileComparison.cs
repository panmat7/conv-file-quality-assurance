using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ImageMagick;
using UglyToad.PdfPig.Content;

// NOTE: COLOR PROFILE COMPARISON WILL NOT WORK CORRECTLY WHEN IMAGES ARE IN DIFFERENT ORDER BETWEEN ORIGINAL AND NEW
// THERE IS STILL A CHANCE IT ENDS UP GIVING THE CORRECT RESULT IF PROFILES HAPPEN TO MATCH, BUT THERE IS NO GUARANTEE

namespace AvaloniaDraft.ComparingMethods;

public static class ColorProfileComparison
{
    /// <summary>
    /// Compares the color profiles of two images
    /// </summary>
    /// <param name="oImage"></param>
    /// <param name="nImage"></param>
    /// <returns></returns>
    public static bool ImageToImageColorProfileComparison(MagickImage oImage, MagickImage nImage)
    {
        return CompareColorProfiles(oImage, nImage);
    }

    /// <summary>
    /// Compares the color profiles of two PDF files
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool PdfToPdfColorProfileComparison(List<IPdfImage> oImages, List<IPdfImage> nImages)
    {
        var convertedOImages = ImageExtraction.ConvertPdfImagesToMagickImages(oImages);
        var convertedNImages = ImageExtraction.ConvertPdfImagesToMagickImages(nImages);
        
        // If there are no images no test is done and we return true
        if (convertedOImages.Count < 1) return true;
        
        // If there are different number of images in the PDF files, it means there is a loss of data and we fail the test
        if (convertedOImages.Count != convertedNImages.Count)
        {
            return false;
        }
        // If there is only one image in each PDF file, we compare the color profiles of the images
        if (convertedOImages.Count == 1 && convertedNImages.Count == 1)
        {
            return CompareColorProfiles(convertedOImages[0], convertedNImages[0]);
        }
        // If there are multiple images in the PDF files, we compare the color profiles of each image
        return convertedOImages.Count == convertedNImages.Count && !convertedOImages.Where((t, i) => 
            !CompareColorProfiles(t, convertedNImages[i])).Any();
    }

    /// <summary>
    /// Compares the color profile of an image and a PDF file
    /// </summary>
    /// <param name="oImage"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool ImageToPdfColorProfileComparison(MagickImage oImage, List<IPdfImage> nImages)
    {
        // Convert from IPdfImage to MagickImage
        var convertedNImages = ImageExtraction.ConvertPdfImagesToMagickImages(nImages);
        
        // Check if more than one image is extracted from the PDF file
        return nImages.Count <= 1 && CompareColorProfiles(oImage, convertedNImages[0]);
    }

    /// <summary>
    /// Compares the color profile of a xml based PowerPoint and a PDF file
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool XmlBasedPowerPointToPdfColorProfileComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        // Convert from IPdfImage to MagickImage
        var convertedNImages = ImageExtraction.ConvertPdfImagesToMagickImages(nImages);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, 
            convertedNImages[i])).Any();
    }

    /// <summary>
    /// Compares the color profile of a docx file and a PDF file
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool DocxToPdfColorProfileComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        // Convert from IPdfImage to MagickImage
        var convertedNImages = ImageExtraction.ConvertPdfImagesToMagickImages(nImages);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, convertedNImages[i])).Any();
    }

    /// <summary>
    /// Compares the color profiles of a xlsx file and a pdf file
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <param name="imagesOverCells"></param>
    /// <returns></returns>
    public static bool XlsxToPdfColorProfileComparison(List<MagickImage> oImages, List<IPdfImage> nImages, 
        List<string> imagesOverCells)
    {
        // Convert from IPdfImage to MagickImage
        var convertedNImages = ImageExtraction.ConvertPdfImagesToMagickImages(nImages);
        
        // Get the array position of images
        var imageNumbersOverCells = imagesOverCells.Select(image => int.Parse(new string(image
            .Where(char.IsDigit).ToArray())) - 1).ToList();
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        // Do comparison only on images that are not drawn over cell
        return !oImages.Where((t, i) => imageNumbersOverCells.Count != 0 && 
                                        imageNumbersOverCells.Contains(i) && 
                                        !CompareColorProfiles(t, convertedNImages[i])).Any();
    }

    /// <summary>
    /// Compares the color profiles of a eml file and a pdf file
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool EmlToPdfColorProfileComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        // Convert from IPdfImage to MagickImage
        var convertedNImages = ImageExtraction.ConvertPdfImagesToMagickImages(nImages);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, convertedNImages[i])).Any();
    }

    /// <summary>
    /// Compares the color profiles of odt and odp with a pdf file
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool OpenDocumentToPdfColorProfileComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        // Convert from IPdfImage to MagickImage
        var convertedNImages = ImageExtraction.ConvertPdfImagesToMagickImages(nImages);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, convertedNImages[i])).Any();
    }

    /// <summary>
    /// Function checks that embedded color profile for two images are the same
    /// </summary>
    /// <param name="oImage"></param>
    /// <param name="nImage"></param>
    /// <returns></returns>
    public static bool CompareColorProfiles(MagickImage oImage, MagickImage nImage)
    {
        
        var oProfile = oImage.GetColorProfile();
        var nProfile = nImage.GetColorProfile();

        return oProfile switch
        {
            null when nProfile == null => true // If both images do not have color profiles it means no loss of data
            ,
            null => false // If only one image has a color profile it means loss of data
            ,
            _ => nProfile != null && // If the profiles are different it means loss of data
                 oProfile.Equals(nProfile)
        };
    }
}