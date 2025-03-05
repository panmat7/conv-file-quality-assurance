using System.Collections.Generic;
using System.Linq;
using AvaloniaDraft.FileManager;
using ImageMagick;
using SixLabors.ImageSharp;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace AvaloniaDraft.ComparingMethods;

public static class TransparencyComparison
{
    /// <summary>
    /// Compares transparency between two image formats
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool ImageToImageTransparencyComparison(FilePair files)
    {
        var oImage = new MagickImage(files.OriginalFilePath);
        var nImage = new MagickImage(files.NewFilePath);

        return CheckNonPdfImageTransparency(oImage) == CheckNonPdfImageTransparency(nImage);
    }
    
    /// <summary>
    /// Compares transparency between an image and pdf
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool ImageToPdfTransparencyComparison(FilePair files)
    {
        var oImage = new MagickImage(files.OriginalFilePath);
        var nImage = ImageExtraction.GetNonDuplicatePdfImages(files.NewFilePath)[0];

        return CheckNonPdfImageTransparency(oImage) == CheckPdfImageTransparency(nImage);
    }

    /// <summary>
    /// Compares transparency between images in docx and pdf
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool DocxToPdfTransparencyComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromDocx(files.OriginalFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(files.NewFilePath);
        
        return CompareNonPdfImagesWithPdfImages(oImages, nImages);
    }
    
    /// <summary>
    /// Compares transparency between images in pdf files
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool PdfToPdfTransparencyComparison(FilePair files)
    {
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(files.OriginalFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(files.NewFilePath);

        return ComparePdfImagesWithPdfImages(oImages, nImages);
    }

    /// <summary>
    /// Compares transparency between xml based PowerPoint and pdf images
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool XmlBasedPowerPointToPdfTransparencyComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(files.OriginalFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(files.NewFilePath);
        
        return CompareNonPdfImagesWithPdfImages(oImages, nImages);
    }
    
    /// <summary>
    /// Compares transparency between OpenDocuments (excluding sheets) and pdf images
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    public static bool OdtAndOdpToPdfTransparencyComparison(FilePair files)
    {
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(files.OriginalFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(files.NewFilePath);

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