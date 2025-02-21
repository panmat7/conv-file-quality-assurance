using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using ClosedXML.Excel;
using SkiaSharp;

namespace AvaloniaDraft.ComparingMethods;

public static class SpreadsheetComparison
{
    /// <summary>
    /// Checks if an excel document is wide enough to cause a page break or contains an image.
    /// </summary>
    /// <param name="path">Absolute path to the file</param>
    /// <returns>True/False is a break is probable</returns>
    public static bool PossibleSpreadsheetBreakExcel(string path)
    {
        const double breakLength = 15.5; //Depends on file's margin, on normal it is around 15.92 cm, so a bit bellow here.
        const double ppi = 96.0; //Default for excel, documents do not store
        var widthSum = 0.0;

        using var wb = new XLWorkbook(path);
        foreach (var worksheet in wb.Worksheets)
        {
            if (worksheet.Pictures.Count > 0) return true; //For now automatically flag if contains image
            
            var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            
            if (lastColumn == 0 || lastRow == 0) break;

            var f = worksheet.Style.Font.FontName ?? "";
            var fSize = worksheet.Style.Font.FontSize;

            var charWidth = GetFontWidth(f, (float)fSize);
            
            for (var i = 1; i <= lastColumn; i++)
            {
                var pixels = worksheet.Column(i).Width * charWidth; //Get pixel length 
                widthSum += PixelToCm(pixels, ppi);
                
                if (breakLength < widthSum) return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Approximates character size for a font in pixels.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    static double GetFontWidth(string name, float size)
    {
        using var paint = new SKPaint();
        
        paint.Typeface = SKTypeface.FromFamilyName(name); //Warning - if a not installed font is used will default to another
        paint.TextSize = size;
        return paint.MeasureText("0123456789") / 10;
    }

    static double PixelToCm(double pixels, double ppi)
    {
        return pixels * (2.54 / ppi);
    }

    /// <summary>
    /// Checks if a ODS document is wide enough to cause a page break or contains an image.
    /// </summary>
    /// <param name="path">Absolute path to the file</param>
    /// <returns>True/False is a break is probable. Null if an error occurred reading the file.</returns>
    public static bool? PossibleSpreadsheetBreakOpenDoc(string path)
    {
        const double breakLength = 15.5;
        
        using var arch = ZipFile.OpenRead(path);
        var content = arch.GetEntry("content.xml");
        if (content is null) return null;
            
        using var stream = content.Open();
        var doc = XDocument.Load(stream);

        if (doc.Root is null) return null;
        
        if (GetOdsImageCount(doc) > 0) return true;
        
        return GetOdsTableWidths(doc).Any(n => n > breakLength);
    }
    
    /// <summary>
    /// Returns the number of images embedded in a ods document
    /// </summary>
    /// <param name="doc">Document to be analyzed</param>
    /// <returns>Number of images</returns>
    private static int GetOdsImageCount(XDocument doc)
    {
        //Checking if an image is embedded
        var drawNs = doc.Root.GetNamespaceOfPrefix("draw") ?? //Draw namespace
                     XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
        var xlinkNs = XNamespace.Get("http://www.w3.org/1999/xlink"); //Get xlink namespace
    
        //Getting images
        var images = doc.Descendants(drawNs + "image") //All images
            .Select(img => img.Attribute(xlinkNs + "href")?.Value) //Which contain href attribute
            .Where(href => href != null && href.StartsWith("Pictures/")) //referencing a picture
            .ToList();
        
        return images.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    private static double[] GetOdsTableWidths(XDocument doc)
    {
        
        
        
        return [];
    }
}