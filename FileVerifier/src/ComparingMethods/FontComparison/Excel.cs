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
        var textInfo = new TextInfo();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var doc = new ExcelPackage(src);
        var sheets = doc.Workbook.Worksheets;

        // Go through each cell
        foreach (var cell in sheets.SelectMany(sheet => sheet.Cells))
        {
            ReadCellProperties(cell, textInfo);
        }

        return textInfo;
    }


    /// <summary>
    /// Read the properties of a cell
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="fonts"></param>
    /// <param name="textColors"></param>
    /// <param name="bgColors"></param>
    private static void ReadCellProperties(ExcelRangeBase cell, TextInfo textInfo)
    {
        // Check for foreign writing
        var containsForeignText = (!textInfo.ForeignWriting && FontComparison.IsForeign(cell.Text));
        if (containsForeignText) textInfo.ForeignWriting = true;

        // Get the cell's fill color
        var fillCol = cell.Style.Fill.BackgroundColor.LookupColor();
        if (!string.IsNullOrEmpty(fillCol))
        {
            var hex = fillCol.Substring(3); // Remove the '#' and the alpha
            textInfo.BgColors.Add(hex);
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
                textInfo.TextColors.Add(hex);

                // Font
                var font = FontComparison.NormalizeFontName(p.FontName);
                if (font != "") textInfo.Fonts.Add(font);
            }
        }
        else // One font style
        {
            var fStyle = cell.Style.Font;

            // Text color
            var txtCol = fStyle.Color.LookupColor();
            if (!string.IsNullOrEmpty(txtCol)) textInfo.TextColors.Add(txtCol.Substring(3));

            // Font
            var font = FontComparison.NormalizeFontName(fStyle.Name);
            if (!string.IsNullOrEmpty(font)) textInfo.Fonts.Add(font);
        }
    }
}
