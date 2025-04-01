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

        // Go through each cell
        foreach (var cell in sheets.Select(sheet => sheet.Cells))
        {
            ReadCellProperties(cell, fonts, textColors, bgColors, ref foreignWriting);
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }


    /// <summary>
    /// Read the properties of a cell
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="fonts"></param>
    /// <param name="textColors"></param>
    /// <param name="bgColors"></param>
    private static void ReadCellProperties(ExcelRange cell, HashSet<string> fonts, HashSet<string> textColors, HashSet<string> bgColors, ref bool foreignWriting)
    {
        // Check for foreign writing
        var containsForeignText = (!foreignWriting && FontComparison.IsForeign(cell.Text));
        if (containsForeignText) foreignWriting = true;

        // Get the cell's fill color
        var fillCol = cell.Style.Fill.BackgroundColor.LookupColor();
        if (!string.IsNullOrEmpty(fillCol))
        {
            var hex = fillCol.Substring(3); // Remove the '#' and the alpha
            bgColors.Add(hex);
        }

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
            if (!string.IsNullOrEmpty(txtCol)) textColors.Add(txtCol.Substring(3));

            // Font
            var font = FontComparison.NormalizeFontName(fStyle.Name);
            if (!string.IsNullOrEmpty(font)) fonts.Add(font);
        }
    }
}
