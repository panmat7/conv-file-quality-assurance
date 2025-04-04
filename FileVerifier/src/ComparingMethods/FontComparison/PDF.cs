using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Graphics.Colors;

namespace AvaloniaDraft.ComparingMethods;

public static class PdfFontExtraction
{
    /// <summary>
    /// Get the font information of a PDF file
    /// </summary>
    /// <param name="src">The file path</param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoPdf(string src)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        var doc = UglyToad.PdfPig.PdfDocument.Open(src);
        var pages = doc.GetPages();

        
        // Go through each character
        foreach (var letter in pages.SelectMany(p => p.Letters))
        {
            // Skip if it is null or white space
            if (String.IsNullOrWhiteSpace(letter.Value)) continue;

            // Get the font
            string font = letter.Font.Name;
            fonts.Add(FontComparison.NormalizeFontName(font));

            // Get the color of the letter
            var hex = GetColor(letter.Color);
            if (hex != null) textColors.Add(hex);

            // Check for foreign writing
            if (!foreignWriting && FontComparison.IsForeign(letter.Value)) foreignWriting = true;
        }


        // Get marking and paragraph colors
        foreach (var path in pages.SelectMany(p => p.Paths))
        {
            var hex = GetColor(path.FillColor);
            if (hex != null) bgColors.Add(hex);
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }


    /// <summary>
    /// Get the hex from a PDF color
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    private static string? GetColor(IColor? col)
    {
        if (col == null) return null;

        try
        {
            var color = col.ToRGBValues();
            if (color is (double, double, double) rgb)
            {
                return FontComparison.GetHex(rgb);
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
