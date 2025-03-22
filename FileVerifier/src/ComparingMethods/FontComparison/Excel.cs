using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using  OfficeOpenXml; // EPPlus

namespace AvaloniaDraft.ComparingMethods;

public static class ExcelFontExtraction
{

    /// <summary>
    /// Get font information from an Excel file
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoExcel(string src)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        var doc = new ExcelPackage(src);
        var sheets = doc.Workbook.Worksheets;

        // Go through each sheet
        foreach (var sheet in sheets)
        {

            // Go through each cell
            var cells = sheet.Cells;
            foreach (var cell in cells)
            {
                // Get the cell's fill color
                var fillCol = cell.Style.Fill.BackgroundColor.LookupColor();
                if (fillCol != null && fillCol != "")
                {
                    var hex = fillCol.Substring(3); // Remove the '#' and the alpha
                    bgColors.Add(hex);
                }

                if (!foreignWriting && FontComparison.IsForeign(cell.Text)) foreignWriting = true;

                // If there are multiple styles in cell
                if (cell.IsRichText)
                {
                    var richTxt = cell.RichText;

                    // Go through each part of the rich text
                    foreach (var p in richTxt)
                    {
                        // Text color
                        var txtCol = p.Color;
                        var hex = FontComparison.GetHex((txtCol.R, txtCol.G, txtCol.B));
                        textColors.Add(hex);

                        // Font
                        var font = FontComparison.NormalizeFontName(p.FontName);
                        if (font != "") fonts.Add(font);
                    }
                }
                else // One font style
                {
                    var fStyle = cell.Style.Font;

                    // Text color
                    var txtCol = fStyle.Color.LookupColor();
                    if (txtCol != null && txtCol != "")
                    {
                        textColors.Add(txtCol.Substring(3));
                    }

                    // Font
                    var font = FontComparison.NormalizeFontName(fStyle.Name);
                    if (font != "") fonts.Add(font);
                }
            }
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }
}
