using AvaloniaDraft.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDraft.ComparingMethods;


public struct TextInfo
{
    public bool foreignWriting;
    public HashSet<string> fonts;
    public HashSet<HashSet<string>> altFonts;
    public HashSet<string> textColors;
    public HashSet<string> bgColors;

    public TextInfo(HashSet<string> fonts, HashSet<string> textColors, HashSet<string> bgColors, HashSet<HashSet<string>> altFonts, bool foreignWriting = false)
    {
        this.fonts = fonts;
        this.textColors = textColors;
        this.bgColors = bgColors;
        this.altFonts = altFonts;
        this.foreignWriting = foreignWriting;
    }


    public void Print()
    {
        Console.WriteLine("Fonts:");
        foreach (var font in fonts)
        {
            Console.WriteLine(font);
        }

        var listedAltFonts = new List<string>();
        foreach (var fontChoices in altFonts)
        {
            foreach (var f in fontChoices)
            {
                if (!listedAltFonts.Contains(f))
                {
                    //Console.WriteLine(f);
                    listedAltFonts.Add(f);
                }
            }

            //PrintList(fontChoices);
        }

        Console.WriteLine("\nText colors:");
        foreach (var c in textColors)
        {
            Console.WriteLine(c);
        }

        Console.WriteLine("\nText background colors:");
        foreach (var c in bgColors)
        {
            Console.WriteLine(c);
        }
    }
}


public struct FontComparisonResult
{
    public bool pass;
    public bool containsForeignCharacters;
    public Error? err;

    public FontComparisonResult(bool pass, bool containsForeignCharacters, Error? err = null)
    {
        this.pass = pass;
        this.containsForeignCharacters = containsForeignCharacters;
        this.err = err;
    }
}

public static class FontComparison
{
    /// <summary>
    /// Compare a file pair and check if the fonts, text colors and text background colors are the same
    /// </summary>
    /// <param name="src1"></param>
    /// <param name="src2"></param>
    public static bool? CompareFiles(string src1, string src2)
    {
        if (GetTextInfo(src1) is not TextInfo t1) return null;
        if (GetTextInfo(src2) is not TextInfo t2) return null;

        // Alternate fonts
        foreach (var altFonts in t1.altFonts)
        {
            var containsAtLeastOneAltFont = false;

            foreach (var font in altFonts)
            {
                if (t2.fonts.Contains(font))
                {
                    containsAtLeastOneAltFont = true;
                    break;
                }
            }

            if (!containsAtLeastOneAltFont) return false;
        }

        var differentFonts = t1.fonts.Except(t2.fonts);
        if (differentFonts.Any()) return false;

        if (!CompareColors(t1.textColors, t2.textColors)) return false;
        if (!CompareColors(t1.bgColors, t2.bgColors)) return false;

        return true;
    }


    /// <summary>
    /// Get text information such as used fonts, text colors and background colors from a document
    /// </summary>
    /// <param name="src">The file</param>
    /// <returns>The text information</returns>
    private static TextInfo? GetTextInfo(string src)
    {
        /// TODO: Change from file extensions to PRONOM codes
        var ext = System.IO.Path.GetExtension(src);
        return ext switch
        {
            ".pdf" => PdfFontExtraction.GetTextInfoPdf(src),
            
            ".docx" => WordFontExtraction.GetTextInfoWord(src),
            ".pptx" => PPFontExtraction.GetTextInfoPP(src),
            ".xlsx" => ExcelFontExtraction.GetTextInfoExcel(src),

            ".odt" => ODFontExtraction.GetTextInfoODT(src),
            ".odp" => ODFontExtraction.GetTextInfoODP(src),
            ".ods" => ODFontExtraction.GetTextInfoODS(src),

            ".rtf" => RtfFontExtraction.GetTextInfoRTF(src),

            ".eml" => HtmlBasedFontExtraction.GetTextInfoEml(src),
            ".html" => HtmlBasedFontExtraction.GetTextInfoHtml(src),

            _ => null
        };
    }



    /// <summary>
    /// Normalize a font name
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static string NormalizeFontName(string f)
    {
        // Remove font subset prefix if present
        if (f.Length >= 8 && f[6] == '+')
        {
            f = f.Substring(7);
        }

        // Remove substrings, such as style variations, that only appear in some formats
        string[] str = ["Semibold", "Demibold", "Bold", "Italic", "Condensed", "Medium", "Regular", "Reg", "PS", "MT", "MS", "Unicode", "-", ",", " "];
        foreach (string s in str)
        {
            f = f.Replace(s, "");
        }
        if (f == "BaskOldFace") f = "BaskervilleOldFace";

        f = f.ToLower();

        return f;
    }


    /// <summary>
    /// Gets the hex of a color
    /// </summary>
    /// <param name="rgb">The RGB values of the color</param>
    /// <returns></returns>
    public static string GetHex((double r, double g, double b) rgb)
    {
        int r = (int)Math.Round(rgb.r * 255);
        int g = (int)Math.Round(rgb.g * 255);
        int b = (int)Math.Round(rgb.b * 255);

        return GetHex((r, g, b));
    }


    /// <summary>
    /// Gets the hex of a color
    /// </summary>
    /// <param name="rgb">The RGB values of the color</param>
    /// <returns></returns>
    public static string GetHex((int r, int g, int b) rgb)
    {
        var hex = rgb.r.ToString("X2") + rgb.g.ToString("X2") + rgb.b.ToString("X2");

        return hex;
    }


    /// <summary>
    /// Get the hex of a color
    /// </summary>
    /// <param name="col">The color</param>
    /// <returns></returns>
    public static string GetHex(Color col)
    {
        return GetHex((col.R, col.G, col.B));
    }


    /// <summary>
    /// Check whether or not a text contains any foreign characters
    /// </summary>
    /// <param name="txt">The string to check</param>
    /// <returns></returns>
    public static bool IsForeign(string txt)
    {
        return txt.Any(c => IsForeign(c));
    }

    /// <summary>
    /// Check whether or not a character is foreign
    /// </summary>
    /// <param name="c">The character to check</param>
    /// <returns></returns>
    public static bool IsForeign(char c)
    {
        var nonForeignRanges = new List<(int, int)>
        {
            (0x0, 0x7F),    // Basic Latin
            (0x0A0, 0xFF),  // Latin supplement
            (0x2000, 0x206F) // General punctuation
        };

        return !InRange(c, nonForeignRanges);
    }


    /// <summary>
    /// Check whether or not a value exists within any of a list of ranges
    /// </summary>
    /// <param name="val"></param>
    /// <param name="ranges"></param>
    /// <returns></returns>
    public static bool InRange(int val, List<(int lowest, int highest)> ranges)
    {
        return ranges.Any(range => val >= range.lowest && val <= range.highest);
    }



    /// <summary>
    /// Compare two lists of colors to see if they are the same
    /// </summary>
    /// <param name="cols1">The first list of colors</param>
    /// <param name="cols2">The second list of colors</param>
    /// <returns></returns>
    private static bool CompareColors(HashSet<string> cols1, HashSet<string> cols2)
    {
        foreach (var col1 in cols1)
        {
            var match = false;

            foreach (var col2 in cols2)
            {
                if (ColorsEqual(col1, col2))
                {
                    match = true;
                    break;
                }
            }

            if (!match) return false;
        }

        return true;
    }


    /// <summary>
    /// Compare two colors, while allowing for a slight difference
    /// </summary>
    /// <param name="hex1">The first color</param>
    /// <param name="hex2">The second color</param>
    /// <param name="lumDiffAllowed">The allowed difference in luminance (0.0 - 1.0)</param>
    /// <returns></returns>
    private static bool ColorsEqual(string hex1, string hex2, float lumDiffAllowed = 0.03f)
    {
        var col1 = ColorTranslator.FromHtml($"#{hex1}");
        var col2 = ColorTranslator.FromHtml($"#{hex2}");

        var h1 = col1.GetHue();
        var s1 = col1.GetSaturation();
        var l1 = col1.GetBrightness();

        var h2 = col2.GetHue();
        var s2 = col2.GetSaturation();
        var l2 = col2.GetBrightness();

        if (Math.Abs(h1 - h2) > 2 || Math.Abs(s1 * 100 - s2 * 100) > 2) return false;

        var lDif = Math.Abs(l1 - l2);

        if (lDif > lumDiffAllowed) return false;

        return true;
    }
}
