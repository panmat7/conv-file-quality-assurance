using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDraft.ComparingMethods;

public static class PdfFontExtraction
{
    /// <summary>
    /// Get the font information of a ODT file
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
        foreach (var page in pages)
        {
            var letters = page.Letters;

            // Get marking and paragraph colors
            var paths = page.Paths;
            foreach (var path in paths)
            {
                if (path.IsFilled)
                {
                    try
                    {
                        var color = path.FillColor?.ToRGBValues();
                        if (color is (double r, double g, double b) rgb)
                        {
                            var hex = FontComparison.GetHex(rgb);
                            bgColors.Add(hex);
                        }
                    }
                    catch
                    {
                        // Nothing
                    }
                }
            }

            // Go through each character
            foreach (var letter in letters)
            {
                // Skip if it is null or white space
                if (String.IsNullOrWhiteSpace(letter.Value)) continue;

                // Get the font
                string font = letter.Font.Name;
                fonts.Add(FontComparison.NormalizeFontName(font));

                // Get the color of the letter
                var col = letter.Color.ToRGBValues();
                var hex = FontComparison.GetHex(col);

                // Check for foreign writing
                if (!foreignWriting && FontComparison.IsForeign(letter.Value)) foreignWriting = true;

                if (hex != null) textColors.Add(hex);
            }
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }
}
