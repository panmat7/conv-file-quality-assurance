using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using Emgu.CV.Face;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvaloniaDraft.ComparingMethods;


public struct TextInfo
{
    public bool ForeignWriting { get; set; }
    public HashSet<string> Fonts { get; set; }
    public HashSet<HashSet<string>> AltFonts { get; set; }
    public HashSet<string> TextColors { get; set; }
    public HashSet<string> BgColors { get; set; }

    public TextInfo(HashSet<string> fonts, HashSet<string> textColors, HashSet<string> bgColors, HashSet<HashSet<string>> altFonts, bool foreignWriting = false)
    {
        this.Fonts = fonts;
        this.TextColors = textColors;
        this.BgColors = bgColors;
        this.AltFonts = altFonts;
        this.ForeignWriting = foreignWriting;
    }
}


public struct FontComparisonResult
{
    public bool Pass { get; set; }
    public List<Error> Errors { get; set; }

    public bool ContainsForeignCharacters { get; set; }
    
    public bool BgColorsNotConverted { get; set; }
    public bool TextColorsNotConverted { get; set; }

    public List<string> FontsOnlyInOriginal { get; set; }
    public List<string> FontsOnlyInConverted { get; set; }

    public List<string> TextColorsOnlyInOriginal { get; set; }
    public List<string> BgColorsOnlyInOriginal { get; set; }

    public FontComparisonResult()
    {
        Pass = false;
        ContainsForeignCharacters = false;

        Errors = new List<Error>();

        FontsOnlyInOriginal = [];
        FontsOnlyInConverted = [];

        TextColorsOnlyInOriginal = [];
        BgColorsOnlyInOriginal = [];
    }
}

public static partial class FontComparison
{
    [GeneratedRegex(@"rgb\(([0-9]+), ([0-9]+), ([0-9]+)\)")]
    public static partial Regex RgbRegex();

    /// <summary>
    /// Compare a file pair and check if the fonts, text colors and text background colors are the same
    /// </summary>
    /// <param name="fp">The file pair to compare</param>
    public static FontComparisonResult CompareFiles(FilePair fp)
    {
        var result = new FontComparisonResult();
        result.Pass = true;

        // Make sure there were no issues reading the original file
        var oti = GetTextInfo(fp.OriginalFilePath, fp.OriginalFileFormat);
        if (oti is null)
        {
            result.Errors.Add(new Error(
                "Error reading/opening file", 
                $"Failed to read original file: {Path.GetFileName(fp.OriginalFilePath)}", 
                ErrorSeverity.High, 
                ErrorType.FileError
            ));
        }

        // Make sure there were no issues reading the converted file
        var nti = GetTextInfo(fp.NewFilePath, fp.NewFileFormat);
        if (nti is null)
        {
            result.Errors.Add(new Error(
                "Error reading/opening file", 
                $"Failed to read converted file: {Path.GetFileName(fp.NewFilePath)}", 
                ErrorSeverity.High, 
                ErrorType.FileError
            ));
        }

        // Return if there were errors reading either
        if (oti is not TextInfo ot || nti is not TextInfo nt)
        {
            result.Pass = false;
            return result;
        }

        // Check for fonts present in the original, but not the converted
        var fontsOnlyInOriginal = ot.Fonts.Except(nt.Fonts);
        if (fontsOnlyInOriginal.Any())
        {
            foreach (var font in fontsOnlyInOriginal) result.FontsOnlyInOriginal.Add(font);
            result.Pass = false;
        }

        // Check for fonts present in the converted, but not the original
        var fontsOnlyInConverted = nt.Fonts.Except(ot.Fonts);
        if (fontsOnlyInOriginal.Any())
        {
            foreach (var font in fontsOnlyInConverted) result.FontsOnlyInConverted.Add(font);
            result.Pass = false;
        }

        // Foreign characters
        result.ContainsForeignCharacters = (ot.ForeignWriting || nt.ForeignWriting);

        // Check if text colors are the same
        var textColorsOnlyInOriginal = CompareColors(ot.TextColors, nt.TextColors);
        if (textColorsOnlyInOriginal.Any())
        {
            result.TextColorsNotConverted = true;
            result.TextColorsOnlyInOriginal = textColorsOnlyInOriginal;
            result.Pass = false;
        }

        // Check if background colors are the same
        var bgColorsOnlyInOriginal = CompareColors(ot.BgColors, nt.BgColors);
        if (bgColorsOnlyInOriginal.Any())
        {
            result.BgColorsNotConverted = true;
            result.BgColorsOnlyInOriginal = bgColorsOnlyInOriginal;
            result.Pass = false;
        }

        return result;
    }


    /// <summary>
    /// Get text information such as used fonts, text colors and background colors from a document
    /// </summary>
    /// <param name="src">The file</param>
    /// <returns>The text information</returns>
    private static TextInfo? GetTextInfo(string src, string formatCode)
    {
        if (FormatCodes.PronomCodesAllPDF.Contains(formatCode))
            return PdfFontExtraction.GetTextInfoPdf(src);

        if (FormatCodes.PronomCodesDOCX.Contains(formatCode))
            return WordFontExtraction.GetTextInfoWord(src);

        if (FormatCodes.PronomCodesPPTX.Contains(formatCode))
            return PPFontExtraction.GetTextInfoPP(src);

        if (FormatCodes.PronomCodesXLSX.Contains(formatCode))
            return ExcelFontExtraction.GetTextInfoExcel(src);

        if (FormatCodes.PronomCodesODF.Contains(formatCode))
            return ODFontExtraction.GetTextInfoODF(src);

        if (FormatCodes.PronomCodesRTF.Contains(formatCode))
            return RtfFontExtraction.GetTextInfoRTF(src);

        if (FormatCodes.PronomCodesEML.Contains(formatCode))
            return HtmlBasedFontExtraction.GetTextInfoEml(src);

        if (FormatCodes.PronomCodesHTML.Contains(formatCode))
            return HtmlBasedFontExtraction.GetTextInfoHtml(src);

        return null;
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
        string[] str = ["Semibold", "Demibold", "Bold", "Italic", "Condensed", "Medium", "Regular", "Reg", "PS", "MT", "MS", "Unicode", "-", ",", "'", " "];
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
            (0x2000, 0x206F), // General punctuation
            (0x25A0, 0x25FF), // Geometric shapes
            (0x2B00, 0x2BFF), // Miscellaneous Symbols and Arrows
            (0xE000, 0xF8FF), // Private use area
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
    /// <returns>A list of colors from cols1 that did not match any colors form cols2</returns>
    private static List<string> CompareColors(HashSet<string> cols1, HashSet<string> cols2)
    {
        var nonMatchingColors = new List<string>();
        foreach (var col1 in cols1)
        {
            if (!cols2.Any(col2 => ColorsEqual(col1, col2))) nonMatchingColors.Add(col1);
        }
        return nonMatchingColors;
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
