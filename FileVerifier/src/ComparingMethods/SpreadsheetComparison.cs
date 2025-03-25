using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using AvaloniaDraft.Helpers;
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
    /// <returns>List of found errors</returns>
    public static List<Error> PossibleSpreadsheetBreakExcel(string path)
    {
        //NOTE: Considers columns with edited width even if they are empty
        //Used to ensure only 1 message per file
        var manual = false;
        var image = false;
        var table = false;
        
        using var wb = new XLWorkbook(path);
        
        var errors = new List<Error>();
        
        foreach (var worksheet in wb.Worksheets)
        {
            if (worksheet.PageSetup.RowBreaks.Count > 0 || worksheet.PageSetup.ColumnBreaks.Count > 0 && !manual)
            {
                errors.Add(new Error(
                    "Manual page break found",
                    "This spreadsheet contains manually sat page breaks. This might render the results of break-checks unreliable.",
                    ErrorSeverity.Low,
                    ErrorType.Visual
                )); //If contains manually sat page breaks
                manual = true;
            }
            
            if (worksheet.Pictures.Count > 0 && !image)
            {
                errors.Add(new Error(
                    "Images found in spreadsheet",
                    "This spreadsheet contains images. If too wide, they could create a brake.",
                    ErrorSeverity.Medium,
                    ErrorType.Visual
                ));
                image = true;
            }

            if (CheckTableBreakXlsx(worksheet) && !table)
            {
                errors.Add(new Error(
                    "Table break",
                    "The spreadsheet contains one or several tables that could break during conversion.",
                    ErrorSeverity.High,
                    ErrorType.Visual
                )); //If the table is wide enough for a break
                table = true;
            }

            if (table && manual && image) return errors; //Can stop early
        }

        return errors;
    }

    /// <summary>
    /// Checks if a XLSX worksheet contains a table wide enough to cause a break.
    /// </summary>
    /// <param name="worksheet">The worksheet to be checked.</param>
    /// <returns>True/False whether a risk exists</returns>
    private static bool CheckTableBreakXlsx(IXLWorksheet worksheet)
    {
        const double breakLength = 15.6; //Depends on file's margin, on normal it is around 15.92 cm, so a bit bellow here.
        const double ppi = 96.0; //Default for excel, documents do not store
        var widthSum = 0.0;
        
        var lastColumn = worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
            
        if (lastColumn == 0 || lastRow == 0) return false;

        var f = worksheet.Style.Font.FontName ?? "";
        var fSize = worksheet.Style.Font.FontSize;

        var charWidth = GetFontWidth(f, (float)fSize);
            
        for (var i = 1; i <= lastColumn; i++)
        {
            var pixels = worksheet.Column(i).Width * charWidth; //Get pixel length 
            widthSum += PixelToCm(pixels, ppi);

            if (breakLength < widthSum) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if an ODS document is wide enough to cause a page break or contains an image.
    /// </summary>
    /// <param name="path">Absolute path to the file</param>
    /// <returns>List of found errors. Null if an error occurred reading the file.</returns>
    public static List<Error>? PossibleSpreadsheetBreakOpenDoc(string path)
    {
        const double breakPoint = 15.6; //Depends on file's margin, on normal it is around 15.92 cm, so a bit bellow here.
        
        using var arch = ZipFile.OpenRead(path);
        var content = arch.GetEntry("content.xml");
        if (content is null) return null;
            
        using var stream = content.Open();
        var doc = XDocument.Load(stream);

        if (doc.Root is null) return null;

        try
        {
            var errors = new List<Error>();

            if (CheckManualBreaksOds(doc))
                errors.Add(new Error(
                    "Manual page break found",
                    "This spreadsheet contains manually sat page breaks. This might render the results of break-checks unreliable.",
                    ErrorSeverity.Low,
                    ErrorType.Visual
                )); //If contains manually sat page breaks
            
            if (CheckObjectBreaksOds(doc, breakPoint))
                errors.Add(new Error(
                    "Object break",
                    "The spreadsheet contains one or several objects that could break during conversion.",
                    ErrorSeverity.High,
                    ErrorType.Visual
                )); //If any object causes a break

            if (GetOdsTableWidths(doc).Any(n => n > breakPoint))
                errors.Add(new Error(
                    "Table break",
                    "The spreadsheet contains one or several tables that could break during conversion.",
                    ErrorSeverity.High,
                    ErrorType.Visual
                )); //If the table is wide enough for a break
            
            return errors;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if the ODS documents contain any manually set page breaks.
    /// </summary>
    /// <param name="doc">The document to be checked.</param>
    /// <returns>True/False whether a manual break is set.</returns>
    private static bool CheckManualBreaksOds(XDocument doc)
    {
        XNamespace styleNs = "urn:oasis:names:tc:opendocument:xmlns:style:1.0";
        XNamespace foNs = "urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0";

        return doc.Descendants(styleNs + "table-column-properties")
                   .Any(col => col.Attribute(foNs + "break-before")?.Value == "page") ||
               doc.Descendants(styleNs + "table-row-properties")
                   .Any(row => row.Attribute(foNs + "break-before")?.Value == "page");
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

            var convertedX = ConvertToCm(x);
            var convertedWidth = ConvertToCm(width);
            
            if (convertedX + convertedWidth > breakPoint) //Checking if the image poses a table break threat
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

                    return ConvertToCm(widthString);
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
    /// Converts a string containing a width value to numerical centimeters. Used for ODS.
    /// </summary>
    /// <param name="widthString">String containing the value.</param>
    /// <returns>The value converted to cm as a double, or 0 if could not convert.</returns>
    private static double ConvertToCm(string widthString)
    {
        var factor = 1.0;
        
        if (widthString.EndsWith("cm"))
        {
            widthString = widthString.Replace("cm", "").Trim();
        }
        else if (widthString.EndsWith("in"))
        {
            widthString = widthString.Replace("in", "").Trim();
            factor = 2.54; // Convert inches to cm
        }
        else if (widthString.EndsWith("mm"))
        {
            widthString = widthString.Replace("mm", "").Trim();
            factor = 0.1; // Convert mm to cm
        }
        else if (widthString.EndsWith("pt"))
        {
            widthString = widthString.Replace("pt", "").Trim();
            factor = 0.0352778; // Convert points to cm
        }
        
        if (double.TryParse(widthString, NumberStyles.Any, CultureInfo.InvariantCulture, out var width))
        {
            return width * factor;
        }
        
        return 0;
    }
    
    /// <summary>
    /// Checks if a CSV file is wide enough to cause a table break.
    /// </summary>
    /// <param name="path">Path to the file</param>
    /// <returns>True/False is a break is probable. Null if an error occurred reading the file.</returns>
    public static bool? PossibleLineBreakCsv(string path)
    {
        const double breakLength = 15.6; //Depends on file's margin, on normal it is around 15.92 cm, so a bit bellow here.
        
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