using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using ImageMagick;
using UglyToad.PdfPig;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace AvaloniaDraft.ComparingMethods;

// TODO: Several cases need to be included
// TODO: What if the same image is used multiple times. Only one image is then stored in XML
// TODO: What if images are not in same order in original and new?

public static class ColorProfileComparison
{
    /// <summary>
    /// Checks if color profile in original and new file are the same
    /// </summary>
    /// <param name="files"> Takes in the two files used during comparison </param>
    /// <returns> Returns whether it passed the test </returns>
    public static bool FileColorProfileComparison(FilePair files)
    {
        var oFormat = files.OriginalFileFormat;
        var nFormat = files.NewFileFormat;

        try
        {
            return oFormat switch
            {
                _ when FormatCodes.PronomCodesImages.Contains(oFormat) && FormatCodes.PronomCodesImages.Contains(nFormat) 
                    => ImageToImageColorProfileComparison(files),
                _ when FormatCodes.PronomCodesImages.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat)
                    => ImageToPdfColorProfileComparison(files),
                _ when FormatCodes.PronomCodesPDFA.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat) =>
                    PdfToPdfColorProfileComparison(files),
                _ when FormatCodes.PronomCodesXMLBasedPowerPoint.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat)
                    => XmlBasedPowerPointToPdfColorProfileComparison(files),
                _ when FormatCodes.PronomCodesDOCX.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat)
                    => DocxToPdfColorProfileComparison(files),
                _ when FormatCodes.PronomCodesXLSX.Contains(oFormat) && FormatCodes.PronomCodesPDFA.Contains(nFormat)
                    => XlsxToPdfColorProfileComparison(files),
                _ => throw new NotSupportedException("Unsupported comparison format.")
            };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error while comparing color profiles: {e.Message}");
            return false; // If checking for color profile fails it automatically fails the test
        }
    }
    
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
        var oImages = ExtractImagesFromPdf(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);

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
        var nImages = ExtractImagesFromPdf(files.NewFilePath);
        
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
        var oImages = ExtractImagesFromXmlBasedPowerPoint(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);

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
        var oImages = ExtractImagesFromDocx(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        return oImages.Count == nImages.Count && !oImages.Where((t, i) => !CompareColorProfiles(t, nImages[i])).Any();
    }

    public static bool XlsxToPdfColorProfileComparison(FilePair files)
    {
        var imagesOverCells = GetNonAnchoredImagesFromXlsx(files.OriginalFilePath);
        
        // Get the array position of images
        var imageNumbersOverCells = imagesOverCells.Select(image => int.Parse(new string(image
            .Where(char.IsDigit).ToArray())) - 1).ToList();
        
        var oImages = ExtractImagesFromXlsx(files.OriginalFilePath);
        var nImages = ExtractImagesFromPdf(files.NewFilePath);
        
        // If there are no images no test is done and we return true
        if (oImages.Count < 1) return true;
        
        // Do comparison only on images that are not drawn over cell
        return !oImages.Where((t, i) => imageNumbersOverCells.Count != 0 && 
                                        imageNumbersOverCells.Contains(i) && 
                                        !CompareColorProfiles(t, nImages[i])).Any();
    }

    /// <summary>
    /// Extracts images from a PDF file
    /// </summary>
    /// <param name="filePath"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <returns></returns>
    private static List<MagickImage> ExtractImagesFromPdf(string filePath)
    {
        var extractedImages = new List<MagickImage>();

        using var pdfDocument = PdfDocument.Open(filePath);
        foreach (var page in pdfDocument.GetPages())
        {
            var images = page.GetImages();

            foreach (var image in images)
            {
                // Convert the raw image bytes to a MagickImage
                using var magickImage = new MagickImage(image.RawBytes);
                // Clone the image to avoid disposing it when the using block ends
                extractedImages.Add((MagickImage)magickImage.Clone());
            }
        }
        return extractedImages;
    }

    /// <summary>
    /// Extracts all images from a xml based PowerPoint format
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static List<MagickImage> ExtractImagesFromXmlBasedPowerPoint(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);
    
        var images = zip.Entries
            .Where(e => e.FullName.StartsWith("ppt/media/", StringComparison.OrdinalIgnoreCase))
            .Select(e =>
            {
                using var stream = e.Open();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return new MagickImage(memoryStream.ToArray());
            })
            .ToList();
    
        return images;
    }
    
    /// <summary>
    /// Extracts all images from a .docx file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static List<MagickImage> ExtractImagesFromDocx(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);

        var images = zip.Entries
            .Where(e => e.FullName.StartsWith("word/media/", StringComparison.OrdinalIgnoreCase))
            .Select(e =>
            {
                using var stream = e.Open();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return new MagickImage(memoryStream.ToArray());
            })
            .ToList();

        return images;
    }
    
    /// <summary>
    /// Extracts all images from a .xlsx file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static List<MagickImage> ExtractImagesFromXlsx(string filePath)
    {
        using var zip = ZipFile.OpenRead(filePath);

        var images = zip.Entries
            .Where(e => e.FullName.StartsWith("xl/media/", StringComparison.OrdinalIgnoreCase))
            .Select(e =>
            {
                using var stream = e.Open();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                return new MagickImage(memoryStream.ToArray());
            })
            .ToList();

        return images;
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
    
    /// <summary>
    /// This function returns a list of all image names used in a xlsx spreadsheet. This is mostly used to determine which
    /// images are drawn inside of cells. Images inside of cells do not store data on color profile and so a way to ignore
    /// these images is necessary. Getting the names of the images inside of cells is not possible so instead names of all
    /// other images are found to filter them out.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static List<string> GetNonAnchoredImagesFromXlsx(string filePath)
    {
        var imageNames = new List<string>();

        using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Read))
        {
            var drawingEntries = archive.Entries
                .Where(entry => entry.FullName.StartsWith("xl/drawings/drawing") && entry.FullName.EndsWith(".xml"));

            imageNames.AddRange(from entry in drawingEntries let doc = LoadXDocument(entry) 
                let blipElements = GetBlipElements(doc) from blip in blipElements 
                let embedId = blip
                    .Attribute(XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/relationships") 
                               + "embed")?.Value where !string.IsNullOrEmpty(embedId) 
                let relsEntry = GetRelsEntry(archive, entry) select GetRelationship(relsEntry, embedId) 
                into relationship select (string)relationship?.Attribute("Target")! into target 
                select Path.GetFileName(target));
        }
        return imageNames.Distinct().ToList(); // Ensure unique names
    }

    /// <summary>
    /// Load an XDocument from a ZipArchiveEntry
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    private static XDocument LoadXDocument(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream);
        return XDocument.Load(reader);
    }

    /// <summary>
    /// Get all blip elements from a drawing XML
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    private static IEnumerable<XElement> GetBlipElements(XDocument doc)
    {
        XNamespace a = "http://schemas.openxmlformats.org/drawingml/2006/main";
        return doc.Descendants(a + "blip");
    }

    /// <summary>
    /// Get the _rels entry for a drawing entry
    /// </summary>
    /// <param name="archive"></param>
    /// <param name="entry"></param>
    /// <returns></returns>
    private static ZipArchiveEntry? GetRelsEntry(ZipArchive archive, ZipArchiveEntry entry)
    {
        var drawingDir = Path.GetDirectoryName(entry.FullName)?.Replace('\\', '/');
        var relsPath = $"{drawingDir}/_rels/{Path.GetFileName(entry.FullName)}.rels";
        return archive.GetEntry(relsPath);
    }

    /// <summary>
    /// Get the relationship for an embedId
    /// </summary>
    /// <param name="relsEntry"></param>
    /// <param name="embedId"></param>
    /// <returns></returns>
    private static XElement? GetRelationship(ZipArchiveEntry? relsEntry, string embedId)
    {
        using var relsStream = relsEntry?.Open();
        if (relsStream == null) return null;
        using var relsReader = new StreamReader(relsStream);
        var relsDoc = XDocument.Load(relsReader);
        XNamespace relsNs = "http://schemas.openxmlformats.org/package/2006/relationships";
        return relsDoc.Descendants(relsNs + "Relationship")
            .FirstOrDefault(rel =>
                (string)rel.Attribute("Id")! == embedId &&
                (string)rel.Attribute("Type")! == "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
    }
}