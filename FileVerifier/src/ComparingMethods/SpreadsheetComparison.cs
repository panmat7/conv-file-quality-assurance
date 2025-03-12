using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using ClosedXML.Excel;
using SkiaSharp;
using Encoding = System.Text.Encoding;

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
        //NOTE: Considers columns with edited width even if they are empty
        
        const double breakLength = 15.5; //Depends on file's margin, on normal it is around 15.92 cm, so a bit bellow here.
        const double ppi = 96.0; //Default for excel, documents do not store

        using var wb = new XLWorkbook(path);
        foreach (var worksheet in wb.Worksheets)
        {
            var widthSum = 0.0;
            
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
    /// Checks if an ODS document is wide enough to cause a page break or contains an image.
    /// </summary>
    /// <param name="path">Absolute path to the file</param>
    /// <returns>True/False is a break is probable. Null if an error occurred reading the file.</returns>
    public static bool? PossibleSpreadsheetBreakOpenDoc(string path)
    {
        const double breakPoint = 15.5; //Depends on file's margin, on normal it is around 15.92 cm, so a bit bellow here.
        
        using var arch = ZipFile.OpenRead(path);
        var content = arch.GetEntry("content.xml");
        if (content is null) return null;
            
        using var stream = content.Open();
        var doc = XDocument.Load(stream);

        if (doc.Root is null) return null;

        try
        {
            if (CheckObjectBreaksOds(doc, breakPoint)) return true; //If any object causes a break

            return GetOdsTableWidths(doc).Any(n => n > breakPoint);
        }
        catch
        {
            return null;
        }
    }
    
    /// <summary>
    /// Checks whether any object (image, shape, etc.) is wide enough, and position in a place where it could cause a break.
    /// </summary>
    /// <param name="doc">The document to be checked.</param>
    /// <param name="breakPoint">The width at which to flagg a potential break.</param>
    /// <returns>True/False whether a break can occur.</returns>
    private static bool CheckObjectBreaksOds(XDocument doc, double breakPoint)
    {
        var drawNs = doc.Root.GetNamespaceOfPrefix("draw") ?? //Draw namespace
                     XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:drawing:1.0");
        var svgNs = XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0");
        
        var frameCoords = doc.Descendants(drawNs + "frame") //All frames
            .Select(f => new
            {
                X = f.Attribute(svgNs + "x")?.Value ?? "0cm", //X position
                Width = f.Attribute(svgNs + "width")?.Value ?? "0cm" //Actual width
            })
            .ToList();
        
        foreach(var frame in frameCoords)
        {
            var x = frame.X.Replace("cm", "").Trim(); //Trimming the cm ending
            var width = frame.Width.Replace("cm", "").Trim();
            if (double.TryParse(x, NumberStyles.Any, CultureInfo.InvariantCulture, out var xNum) //Converting to nums
                && double.TryParse(width, NumberStyles.Any, CultureInfo.InvariantCulture, out var widthNum)
                && xNum + widthNum > breakPoint) //Checking if the image poses a table break threat
            {
                return true;
            }
        }

        return false;
    }
    
    /// <summary>
    /// Returns a list containing widths of all tables inside an ods document in cm
    /// </summary>
    /// <param name="doc">Document to be analyzed</param>
    /// <returns>List containing width of all tables in cm</returns>
    private static double[] GetOdsTableWidths(XDocument doc)
    {
        //NOTE: This functions considers columns with edited width as used, even if they are empty
        
        var styleNs = XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:style:1.0"); //Style namespace
        var tableNs = XNamespace.Get("urn:oasis:names:tc:opendocument:xmlns:table:1.0"); //Table namespace

        var colStyles = doc.Descendants(styleNs + "style")
            .Where(s => s.Attribute(styleNs + "family")?.Value == "table-column" //Getting only column styles
                        && s.Attribute(styleNs + "name") != null)
            .ToDictionary(
                s => s.Attribute(styleNs + "name")!.Value,
                s =>
                {
                    var props = s.Element(styleNs + "table-column-properties"); //Getting column properties
                    var widthAttr = props?.Attribute(styleNs + "column-width"); //Getting width
                    var widthString = widthAttr?.Value; //Extracting string
                    
                    if (string.IsNullOrWhiteSpace(widthString)) return 0;
                    
                    widthString = widthString.Replace("cm", "").Trim(); //Removing cm ending and parsing the number
                    if (double.TryParse(widthString, NumberStyles.Any, CultureInfo.InvariantCulture, out var width))
                    {
                        return width;
                    }
                    
                    return 0;
                }
            );


        
        //Checking each column and getting its width
        var tableWidth = new Dictionary<string, double>();

        foreach (var table in doc.Descendants(tableNs + "table"))
        {
            var tableName = table.Attribute(tableNs + "name")?.Value ?? "Unnamed Table"; //Getting table name
    
            var colWidths = table.Descendants(tableNs + "table-column")
                .Select(col => {
                    // Get the style name using the table namespace
                    var styleName = col.Attribute(tableNs + "style-name")?.Value;
                    // Get the repeat number, if none present default to 1
                    var columnsRepeated = (int?)col.Attribute(tableNs + "number-columns-repeated") ?? 1;
                    // Getting width from styles and multiply by repeats
                    var width = colStyles!.GetValueOrDefault(styleName, 0);
                    return width * columnsRepeated;
                })
                .ToList();
    
            tableWidth.Add(tableName, colWidths.Sum());
        }
        
        return tableWidth.Select(pair => pair.Value).ToArray();
    }
    
    /// <summary>
    /// Checks if a CSV file is wide enough to cause a table break.
    /// </summary>
    /// <param name="path">Path to the file</param>
    /// <returns>True/False is a break is probable. Null if an error occurred reading the file.</returns>
    public static bool? PossibleLineBreakCsv(string path)
    {
        const double breakLength = 15.5; //Depends on file's margin, on normal it is around 15.92 cm, so a bit bellow here.
        
        var content = File.ReadAllText(path, Encoding.UTF8);

        var delimiter = FindDelimiter(content); //CSV are not standardized and could vary
        if (delimiter == '\0') return null;
        
        var rows = content.Split('\n').ToList();
        var fontSize = GetFontWidth("LiberationSans", 10); //Libre office default
        
        var lengths = new Dictionary<int, double>();
        
        foreach(var row in rows) 
        {
            var column = row.Split(delimiter);
            for (var i = 0; i < column.Length; i++)
            {
                //Number of characters + two whitespaces added during conversion to pdf * character font size
                var length = PixelToCm((column[i].Length + 2) * fontSize, 96); 

                if (lengths.TryAdd(i, length)) continue;
                
                if(lengths[i] < length) lengths[i] = length;
            }
        }

        return lengths.Sum(n => n.Value) > breakLength;
    }
    
    /// <summary>
    /// Returns the most probable csv delimiter, assumed based on the number of its appearance.
    /// </summary>
    /// <param name="content">Content of the CSV file</param>
    /// <returns>The assumed delimiter</returns>
    private static char FindDelimiter(string content)
    {
        var delimiters = new Dictionary<char, int>
        {
            { ',', content.Count(c => c == ',') },
            { '\t', content.Count(c => c == '\t') },
            { ';', content.Count(c => c == ';') },
            { ':', content.Count(c => c == ':') },
            { '|', content.Count(c => c == '|') },
        };

        return delimiters.OrderByDescending(p => p.Value).FirstOrDefault(p => p.Value > 0).Key;
    }
    
    /// <summary>
    /// Approximates character size for a font in pixels.
    /// </summary>
    /// <param name="name">Name of the font</param>
    /// <param name="size">Font size</param>
    /// <returns>Approximate pixel size for the font</returns>
    private static double GetFontWidth(string name, float size)
    {
        using var paint = new SKPaint();
        
        paint.Typeface = SKTypeface.FromFamilyName(name) ?? SKTypeface.Default; //Warning - if a not installed font is used will default to another
        paint.TextSize = size;
        return paint.MeasureText("0123456789") / 10;
    }
    
    /// <summary>
    /// Converts pixels to cm
    /// </summary>
    /// <param name="pixels">Pixel value</param>
    /// <param name="ppi">Pixels per inch</param>
    /// <returns>Value in cm</returns>
    private static double PixelToCm(double pixels, double ppi)
    {
        return pixels * (2.54 / ppi);
    }
}