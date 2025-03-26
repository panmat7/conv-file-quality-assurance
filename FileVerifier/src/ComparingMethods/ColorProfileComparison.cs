using System;
using System.Linq;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ImageMagick;

// NOTE: COLOR PROFILE COMPARISON WILL NOT WORK CORRECTLY WHEN IMAGES ARE IN DIFFERENT ORDER BETWEEN ORIGINAL AND NEW
// THERE IS STILL A CHANCE IT ENDS UP GIVING THE CORRECT RESULT IF PROFILES HAPPEN TO MATCH, BUT THERE IS NO GUARANTEE

namespace AvaloniaDraft.ComparingMethods;

public static class ColorProfileComparison
{
    /// <summary>
    /// Compares the color profiles of two images
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool ImageToImageColorProfileComparison(FilePair files)
    {
        using var oImage = new MagickImage(files.OriginalFilePath);
        using var nImage = new MagickImage(files.NewFilePath);
        return CompareColorProfiles(oImage, nImage);
    }
    
    /// <summary>
    /// Compares the color profiles of two PDF files
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool PdfToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromPdf(files.OriginalFilePath);
        var nImages = ImageExtraction.ExtractImagesFromPdf(files.NewFilePath);

        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        // If there are different number of images in the PDF files, it means there is a loss of data and we fail the test
        if (oImages.Count != nImages.Count)
        {
            return false;
        }
        // If there is only one image in each PDF file, we compare the color profiles of the images
        if (oImages.Count == 1 && nImages.Count == 1)
        {
            return CompareColorProfiles(oImages[0], nImages[0]);
        }
        // If there are multiple images in the PDF files, we compare the color profiles of each image
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
    }
    
    /// <summary>
    /// Compares the color profile of an image and a PDF file
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool ImageToPdfColorProfileComparison(FilePair files)
    {
        using var oImage = new MagickImage(files.OriginalFilePath);
        var nImages = ImageExtraction.ExtractImagesFromPdf(files.NewFilePath);
        
        // Check if more than one image is extracted from the PDF file
        return nImages.Count <= 1 && CompareColorProfiles(oImage, nImages[0]);
    }
    
    /// <summary>
    /// Compares the color profile of a xml based PowerPoint and a PDF file
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool XmlBasedPowerPointToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(files.OriginalFilePath);
        var nImages = ImageExtraction.ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
    }

    /// <summary>
    /// Compares the color profile of a docx file and a PDF file
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool DocxToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromDocx(files.OriginalFilePath);
        var nImages = ImageExtraction.ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
    }

    public static bool XlsxToPdfColorProfileComparison(FilePair files)
    {
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(files.OriginalFilePath);
        
        // Get the array position of images
        var imageNumbersOverCells = imagesOverCells.Select(image => int.Parse(new string(image
            .Where(char.IsDigit).ToArray())) - 1).ToList();
        
        var oImages = ImageExtraction.ExtractImagesFromXlsx(files.OriginalFilePath);
        var nImages = ImageExtraction.ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        // Do comparison only on images that are not drawn over cell
        return !oImages.Where((t, i) => imageNumbersOverCells.Count != 0 && 
                                        imageNumbersOverCells.Contains(i) && 
                                        !CompareColorProfiles(t, nImages[i])).Any();
    }

    public static bool EmlToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromEml(files.OriginalFilePath);
        var nImages = ImageExtraction.ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
    }

    public static bool OdtAndOdpToPdfColorProfileComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(files.OriginalFilePath);
        var nImages = ImageExtraction.ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
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
            _ => nProfile != null && // If only one image has a color profile it means loss of data
                 oProfile.Equals(nProfile)
        };
    }
}