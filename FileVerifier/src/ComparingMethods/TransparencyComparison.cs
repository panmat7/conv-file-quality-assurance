using System.Linq;
using AvaloniaDraft.FileManager;
using ImageMagick;
using SixLabors.ImageSharp;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Tokens;

namespace AvaloniaDraft.ComparingMethods;

public static class TransparencyComparison
{
    public static bool ImageToImageTransparencyComparison(FilePair files)
    {
        return true;
    }
    
    public static bool ImageToPdfTransparencyComparison(FilePair files)
    {
        var oImage = new MagickImage(files.OriginalFilePath);
        var nImage = ImageExtraction.GetNonDuplicatePdfImages(files.NewFilePath)[0];

        return CheckNonPdfImageTransparency(oImage) == CheckPdfImageTransparency(nImage);
    }
    
    public static bool PdfToPdfTransparencyComparison(FilePair files)
    {
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(files.OriginalFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(files.NewFilePath);

        if (oImages.Count != nImages.Count)
        {
            return false;
        }

        for (var i = 0; i < oImages.Count; i++)
        {
            var oImage = oImages[i];
            var nImage = nImages[i];

            var oImageHasTransparency = CheckPdfImageTransparency(oImage);
            var nImageHasTransparency = CheckPdfImageTransparency(nImage);

            if (oImageHasTransparency != nImageHasTransparency)
            {
                return false;
            }
        }
        return true;
    }
    
    public static bool OdtAndOdpToPdfTransparencyComparison(FilePair files)
    {
        return true;
    }

    private static bool CheckPdfImageTransparency(IPdfImage image)
    {
        // Check for soft mask (SMask)
        return image.ImageDictionary.ContainsKey(NameToken.Smask) ||
               // Check for explicit mask (Mask)
               image.ImageDictionary.ContainsKey(NameToken.Mask);
    }

    private static bool CheckNonPdfImageTransparency(MagickImage image)
    {
        return image.HasAlpha && image.GetPixels().Any(pixel => pixel.ToColor()!.A < 255);
    }
}