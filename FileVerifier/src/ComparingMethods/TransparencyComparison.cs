using System.Collections.Generic;
using System.Linq;
using ImageMagick;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace AvaloniaDraft.ComparingMethods;

public static class TransparencyComparison
{
    /// <summary>
    /// Compares transparency between images in docx and pdf
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool DocxToPdfTransparencyComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        return CompareNonPdfImagesWithPdfImages(oImages, nImages);
    }

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
    /// Compares transparency between xml based PowerPoint and pdf images
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool XmlBasedPowerPointToPdfTransparencyComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
    {
        return CompareNonPdfImagesWithPdfImages(oImages, nImages);
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
    /// Compares transparency between OpenDocuments (excluding sheets) and pdf images
    /// </summary>
    /// <param name="oImages"></param>
    /// <param name="nImages"></param>
    /// <returns></returns>
    public static bool OpenDocumentToPdfTransparencyComparison(List<MagickImage> oImages, List<IPdfImage> nImages)
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
        return image.HasAlpha && image.GetPixels().Any(pixel => pixel.ToColor()!.A < 255);
    }
}