using System.IO.Compression;
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
        const double breakLength = 15.5; //From my tests this seems the be width value where to column breaks
        const double ppi = 96.0; //Default for excel, documents do not store
        var widthSum = 0.0;

        using var wb = new XLWorkbook(path);
        foreach (var worksheet in wb.Worksheets)
        {
            var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
            var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            
            if (lastColumn == 0 || lastRow == 0) break;

            var f = worksheet.Style.Font.FontName ?? "";
            var fSize = worksheet.Style.Font.FontSize;

            var charWidth = GetFontWidth(f, (float)fSize);
            
            for (var i = 1; i <= lastColumn; i++)
            {
                var pixels = worksheet.Column(i).Width * charWidth;
                widthSum += PixelToCm(pixels, ppi);
                
                if (breakLength < widthSum) return true;
            }

            if (worksheet.Pictures.Count > 0) return true; //For now automatically flag if contains image
        }

        return false;
    }
    
    /// <summary>
    /// Approximates character size for a font
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
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool PossibleSpreadsheetBreakOpenDoc(string path)
    {
        using var arch = ZipFile.OpenRead(path);
        var content = arch.GetEntry("content.xml");
        if (content is null) return false;
            
        using var stream = content.Open();

        return false;
    }
}