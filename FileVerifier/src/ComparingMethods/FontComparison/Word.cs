using Avalonia.Animation.Easings;
using ColorMine.ColorSpaces;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SystemDrawing = System.Drawing;

namespace AvaloniaDraft.ComparingMethods;

public static class WordFontExtraction
{
    public static TextInfo? GetTextInfoWord(string src)
    {
        var textInfo = new TextInfo();

        WordprocessingDocument doc = WordprocessingDocument.Open(src, false);
        var mainDocPart = doc.MainDocumentPart;
        if (mainDocPart == null) return null;

        var themePart = mainDocPart.ThemePart;
        if (themePart == null) return null;

        // Get default fonts
        var fontScheme = themePart.Theme?.ThemeElements?.GetFirstChild<DocumentFormat.OpenXml.Drawing.FontScheme>();
        if (fontScheme == null) return null;

        var fontSchemeXml = XElement.Parse(fontScheme.OuterXml ?? "");
        var defFonts = MSOffice.GetDefaultFontsFromScheme(fontSchemeXml);
        if (defFonts == null) return null;

        var majFonts = defFonts.Value.major;
        var minFonts = defFonts.Value.minor;
        var themeFontLang = mainDocPart.DocumentSettingsPart?.Settings?.GetFirstChild<ThemeFontLanguages>();
        var fontTable = mainDocPart.FontTablePart?.Fonts;
        if (minFonts == null || majFonts == null || themeFontLang == null || fontTable == null) return null;

        // Look through each paragraph
        var paragraphs = mainDocPart.Document?.Body?.Elements<Paragraph>() ?? [];
        foreach (var p in paragraphs)
        {
            CheckParagraph(p, themePart, themeFontLang, fontTable, majFonts, minFonts, textInfo);
        }

        var styles = mainDocPart.StyleDefinitionsPart?.Styles?.Descendants<Style>();
        if (styles == null) return null;

        // Hyperlink colors are only stored in styles, so check there
        foreach (var style in styles)
        {
            CheckStyles(style, themePart, textInfo);
        }

        return textInfo;
    }


    /// <summary>
    /// Check styles for hyperlink color
    /// </summary>
    /// <param name="style"></param>
    /// <param name="themePart"></param>
    /// <param name="textInfo"></param>
    private static void CheckStyles(Style style, ThemePart? themePart, TextInfo textInfo)
    {
        var name = style.StyleName;
        if (name == null || name.Val?.Value != "Hyperlink") return;

        var hex = GetColor(style.StyleRunProperties?.Color, themePart);
        if (hex != null)
        {
            textInfo.TextColors.Add(hex);
        }
    }

    /// <summary>
    /// Check a paragraph
    /// </summary>
    /// <param name="p"></param>
    /// <param name="themePart"></param>
    /// <param name="themeFontLang"></param>
    /// <param name="fontTable"></param>
    /// <param name="majFonts"></param>
    /// <param name="minFonts"></param>
    /// <param name="textInfo"></param>
    private static void CheckParagraph(Paragraph p, ThemePart themePart, ThemeFontLanguages themeFontLang, Fonts fontTable, 
        Dictionary<string, string> majFonts, Dictionary<string, string> minFonts, TextInfo textInfo)
    {
        // Check if paragraph is filled
        string? pColor = GetShadingColor(p.ParagraphProperties?.Shading, themePart);
        if (pColor != null)
        {
            textInfo.BgColors.Add(pColor);
        }

        // Go through each run
        foreach (var run in p.Elements<Run>())
        {
            CheckRun(run, themePart, themeFontLang, fontTable, majFonts, minFonts, textInfo);
        }
    }


    /// <summary>
    /// Get the hex of a shading color
    /// </summary>
    /// <param name="shading"></param>
    /// <param name="themePart"></param>
    /// <returns></returns>
    private static string? GetShadingColor(Shading? shading, ThemePart? themePart)
    {
        if (shading == null) return null;

        // Convert to Color class
        var themeFill = new Color
        {
            Val = new StringValue
            {
                Value = shading.Fill?.Value,
            },
            ThemeColor = shading.ThemeFill,
            ThemeTint = shading.ThemeFillTint,
            ThemeShade = shading.ThemeFillShade,
        };

        return GetColor(themeFill, themePart);
    }


    /// <summary>
    /// Get the hex of a color
    /// </summary>
    /// <param name="col"></param>
    /// <param name="themePart"></param>
    /// <returns></returns>
    private static string? GetColor(Color? col, ThemePart? themePart, bool shading = true)
    {
        if (col == null) return null;

        var valHex = col.Val?.Value;
        if (valHex != null && valHex != "auto") return valHex;

        var themeCol = col.ThemeColor?.Value;

        var themColString = (themeCol is ThemeColorValues c) ?
        ((IEnumValue)c).Value : null;
        if (themColString == null) return null;

        var scheme = themePart?.Theme?.ThemeElements?.ColorScheme;
        if (scheme == null) return null;

        var themeTint = col.ThemeTint?.Value;
        var themeShade = col.ThemeShade?.Value;

        var themeColHex = themColString.ToLower() switch
        {
            "accent1" => scheme.Accent1Color?.RgbColorModelHex?.Val?.Value,
            "accent2" => scheme.Accent2Color?.RgbColorModelHex?.Val?.Value,
            "accent3" => scheme.Accent3Color?.RgbColorModelHex?.Val?.Value,
            "accent4" => scheme.Accent4Color?.RgbColorModelHex?.Val?.Value,
            "accent5" => scheme.Accent5Color?.RgbColorModelHex?.Val?.Value,
            "accent6" => scheme.Accent6Color?.RgbColorModelHex?.Val?.Value,
            
            "dark1" => scheme.Dark1Color?.RgbColorModelHex?.Val?.Value,
            "dark2" => scheme.Dark2Color?.RgbColorModelHex?.Val?.Value,
            "light1" => scheme.Light1Color?.RgbColorModelHex?.Val?.Value,
            "light2" => scheme.Light1Color?.RgbColorModelHex?.Val?.Value,

            "hyperlink" => scheme.Hyperlink?.RgbColorModelHex?.Val?.Value,
            "followedhyperlink" => scheme.FollowedHyperlinkColor?.RgbColorModelHex?.Val?.Value,

            _ => null,
        };

        

        return GetThemeColor(themeColHex, themeShade, themeTint, shading);
    }


    /// <summary>
    /// Get the theme color with the theme tint and theme shade applied
    /// </summary>
    /// <param name="themeHex"></param>
    /// <param name="themeShade"></param>
    /// <param name="themeTint"></param>
    /// <returns></returns>
    private static string? GetThemeColor(string? themeHex, string? themeShade, string? themeTint, bool shading)
    {
        if (themeHex == null) return null;
        var r = Convert.ToInt32(themeHex.Substring(0, 2), 16);
        var g = Convert.ToInt32(themeHex.Substring(2, 2), 16);
        var b = Convert.ToInt32(themeHex.Substring(4, 2), 16);

        try
        {
            if (themeTint != null)
            {
                int nr, ng, nb;
                var themeTintFactor = int.Parse(themeTint, NumberStyles.HexNumber) / 255.0;
                if (!shading) {
                    nr = ApplyThemeTint(r, themeTintFactor);
                    ng = ApplyThemeTint(g, themeTintFactor);
                    nb = ApplyThemeTint(b, themeTintFactor);
                } 
                else
                {
                    var rgbCol = new Rgb { R = r, G = g, B = b };
                    var hslCol = rgbCol.To<Hsl>();
                    hslCol.L = hslCol.L * themeTintFactor + (100.0 - themeTintFactor * 100.0);

                    var newRgbCol = hslCol.To<Rgb>();
                    nr = (int)newRgbCol.R;
                    ng = (int)newRgbCol.G;
                    nb = (int)newRgbCol.B;
                }

                return FontComparison.GetHex((nr, ng, nb));
            }
        }
        catch
        {
            return null;
        }

        try
        {
            if (themeShade != null)
            {
                int nr, ng, nb;
                var themeShadeFactor = int.Parse(themeShade, NumberStyles.HexNumber) / 255.0;

                if (!shading)
                {
                    nr = ApplyThemeShade(r, themeShadeFactor);
                    ng = ApplyThemeShade(g, themeShadeFactor);
                    nb = ApplyThemeShade(b, themeShadeFactor);
                } 
                else
                {
                    var rgbCol = new Rgb { R = r, G = g, B = b };
                    var hslCol = rgbCol.To<Hsl>();
                    hslCol.L *= themeShadeFactor;

                    var newRgbCol = hslCol.To<Rgb>();
                    nr = (int)newRgbCol.R;
                    ng = (int)newRgbCol.G;
                    nb = (int)newRgbCol.B;
                }

                return FontComparison.GetHex((nr, ng, nb));
            }
        }
        catch
        {
            return null;
        }

        return themeHex;
    }



    /// <summary>
    /// Apply a theme tint
    /// </summary>
    /// <param name="c"></param>
    /// <param name="tint"></param>
    /// <returns></returns>
    private static int ApplyThemeTint(int c, double tint)
    {
        return (int)((1.0 - tint) * (255 - c) + c);
    }


    /// <summary>
    /// Apply a theme shade
    /// </summary>
    /// <param name="c"></param>
    /// <param name="shade"></param>
    /// <returns></returns>
    private static int ApplyThemeShade(int c, double shade)
    {
        return (int)((1.0 - shade) * c);
    }


    /// <summary>
    /// Check a run
    /// </summary>
    /// <param name="run"></param>
    /// <param name="themePart"></param>
    /// <param name="themeFontLang"></param>
    /// <param name="fontTable"></param>
    /// <param name="majFonts"></param>
    /// <param name="minFonts"></param>
    /// <param name="textInfo"></param>
    private static void CheckRun(Run run, ThemePart themePart, ThemeFontLanguages themeFontLang, Fonts fontTable, 
        Dictionary<string, string> majFonts, Dictionary<string, string> minFonts, TextInfo textInfo)
    {
        var runProperties = run.RunProperties;

        // Check hightlight color
        var highlightCol = runProperties?.Highlight?.Val;
        var hex = MSOffice.GetHighlightColor(highlightCol);
        if (hex != null) textInfo.BgColors.Add(hex);

        if (string.IsNullOrWhiteSpace(run.InnerText)) return;

        // Check text color
        var textCol = GetColor(runProperties?.Color, themePart);
        textInfo.TextColors.Add(textCol ?? "000000");

        // Check shading color
        string? shadingCol = GetShadingColor(runProperties?.Shading, themePart);
        if (shadingCol != null)
        {
            textInfo.BgColors.Add(shadingCol);
        }

        // Font
        (var runFonts, var fw) = GetFontsFromRun(run, themeFontLang, fontTable, majFonts, minFonts);
        if (fw) textInfo.ForeignWriting = true;
        if (runFonts != null) foreach (var font in runFonts)
        {
            if (!string.IsNullOrEmpty(font)) textInfo.Fonts.Add(FontComparison.NormalizeFontName(font));
        }
    }


    /// <summary>
    /// Get the fonts used for a run
    /// </summary>
    /// <param name="r"></param>
    /// <param name="themeFontLang"></param>
    /// <param name="fontTable"></param>
    /// <param name="major"></param>
    /// <param name="minor"></param>
    /// <returns></returns>
    private static (List<string?>? fonts, bool foreignWriting) GetFontsFromRun(Run r, ThemeFontLanguages themeFontLang, Fonts? fontTable, Dictionary<string, string> major, Dictionary<string, string> minor)
    {
        var txt = r.InnerText;
        if (fontTable == null) return (null, FontComparison.IsForeign(txt));

        var rPr = r.RunProperties;
        if (rPr == null) return (null, FontComparison.IsForeign(txt));

        var rFonts = rPr.RunFonts;
        if (rFonts == null) return (new List<string?> { minor["Latin"] }, FontComparison.IsForeign(txt));

        // East Asia
        var eaLang = rPr.Languages?.EastAsia?.Value;
        var eaTheme = GetThemeFontValueString(rFonts.EastAsiaTheme?.Value);
        var eaThemeLang = themeFontLang.EastAsia?.Value ?? "";

        // Complex script
        var csTheme = GetThemeFontValueString(rFonts.ComplexScriptTheme?.Value ?? rFonts.EastAsiaTheme?.Value);
        var csThemeLang = themeFontLang.Bidi?.Value ?? "";

        // Other (ASCII / High ansii)
        var asciiTheme = GetThemeFontValueString(rFonts.AsciiTheme?.Value);
        var hansiTheme = GetThemeFontValueString(rFonts.HighAnsiTheme?.Value);
        var oThemeLang = themeFontLang.Val?.Value ?? "";


        var themeLangs = new Dictionary<string, string>() {
            { "ascii", oThemeLang },
            { "highansi", oThemeLang },
            { "eastasia", eaThemeLang },
            { "cs", csThemeLang }
        };

        // The theme font takes priority over specified font
        var eaFont = GetThemeFont(themeLangs, eaTheme, major, minor) ?? rFonts.EastAsia;

        var lang = rPr.Languages;

        bool fontIsBig5OrGB2312 = FontIsBig5OrGB2312(eaFont, eaTheme, major, minor);
        bool langIsZh = LangIsZh(lang, eaLang);

        var hint = rFonts.Hint?.Value;
        bool eaHint = false;
        if (hint is FontTypeHintValues h) eaHint = (h == FontTypeHintValues.EastAsia);

        var csRef = (rPr.RightToLeftText != null || rPr.ComplexScript != null); // A reference to complex script

        var fonts = new List<string?>();

        var result = MSOffice.GetFontSlotsAndCheckForForeignCharacters(txt, csRef, eaHint, langIsZh, fontIsBig5OrGB2312, false);
        var slots = result.slots;
        var foreignWriting = result.foreignWriting;

        foreach (var slot in slots)
        {
            string? fontName = slot.ToLower() switch
            {
                "ascii" => rFonts.Ascii ?? GetThemeFont(themeLangs, asciiTheme, major, minor),
                "hansi" => rFonts.HighAnsi ?? GetThemeFont(themeLangs, hansiTheme, major, minor),
                "eastasia" => rFonts.EastAsia ?? GetThemeFont(themeLangs, eaTheme, major, minor),
                "cs" => rFonts.ComplexScript ?? GetThemeFont(themeLangs, csTheme, major, minor),
                _ => null
            };
            if (string.IsNullOrEmpty(fontName)) continue;

            // Check that the font exists in the font table, and that it is using its primary and not an alternative name
            bool existsInFontTable = fontTable.Descendants<Font>()
                .Any(f => {
                    if (fontName == f.Name?.Value) return true;

                    var altNames = f?.AltName?.Val?.Value?.Split(',')?.ToList();
                    if (altNames == null) return false;

                    if (altNames.Contains(fontName)) return true;

                    return false;
                });
            if (existsInFontTable) fonts.Add(fontName);
        }
            
        return (fonts, foreignWriting);
    }


    /// <summary>
    /// Determine if the font is "Big5" or "GB2312"
    /// </summary>
    /// <param name="font"></param>
    /// <param name="fontTheme"></param>
    /// <param name="majorFonts"></param>
    /// <param name="minorFonts"></param>
    /// <returns></returns>
    private static bool FontIsBig5OrGB2312(string? font, string? fontTheme, Dictionary<string, string> majorFonts, Dictionary<string, string> minorFonts)
    {
        if (font == "Big5" || font == "GB2312")
        {
            return true;
        }

        if (fontTheme == null) return false;

        if (fontTheme == "majorEastAsia" && majorFonts.TryGetValue("eastAsia", out string? majEA))
        {
            if (majEA == "Big5" || majEA == "GB2312") return true;
        }
        else if (fontTheme == "minorEastAsia" && minorFonts.TryGetValue("eastAsia", out string? minEA))
        {
            if (minEA == "Big5" || minEA == "GB2312") return true;
        }

        return false;
    }


    /// <summary>
    /// Determine if the language is "zh"
    /// </summary>
    /// <param name="lang"></param>
    /// <param name="eaLang"></param>
    /// <returns></returns>
    private static bool LangIsZh (Languages? languages, string? eaLang)
    {
        var ic = StringComparison.OrdinalIgnoreCase;
        bool langIsZh = ((languages?.Val?.Value != null && languages.Val.Value.Contains("zh", ic)) || 
            (eaLang != null && eaLang.Contains("zh", ic)));
        return langIsZh;
    }


    /// <summary>
    /// Get the theme font
    /// </summary>
    /// <param name="themeLangs"></param>
    /// <param name="theme"></param>
    /// <param name="major"></param>
    /// <param name="minor"></param>
    /// <returns></returns>
    private static string? GetThemeFont(Dictionary<string, string> themeLangs, string? theme, Dictionary<string, string> major, Dictionary<string, string> minor)
    {
        string? font;

        var classification = theme?.Substring("m**or".Length);
        var dic = (theme?.Contains("major") ?? false) ? major : minor;
        themeLangs.TryGetValue(classification?.ToLower() ?? "", out var lang);

        // Get the language's corresponding script
        var script = ScriptCodes.GetScript(lang);

        // Get the correct font for the script
        if (!string.IsNullOrEmpty(script))
        {
            dic.TryGetValue(script == "Latn" ? "Latin" : script, out font);
        }
        else
        {
            font = classification?.ToLower() switch
            {
                "ascii" or "hansi" => dic["Latin"] ?? null,
                "eastasia" => dic["EastAsia"] ?? null,
                "cs" => dic["ComplexScript"] ?? null,
                _ => null,
            };
        }

        return font;
    }



    /// <summary>
    /// Get the string value of a ThemeFontValues value
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    private static string? GetThemeFontValueString(ThemeFontValues? v)
    {
        if (v == null) return null;

        if (v == ThemeFontValues.MinorAscii) return "minorAscii";
        if (v == ThemeFontValues.MajorAscii) return "majorAscii";
        if (v == ThemeFontValues.MinorHighAnsi) return "minorHighAnsi";
        if (v == ThemeFontValues.MajorHighAnsi) return "majorHighAnsi";
        if (v == ThemeFontValues.MinorBidi) return "minorBidi";
        if (v == ThemeFontValues.MajorBidi) return "majorBidi";
        if (v == ThemeFontValues.MinorEastAsia) return "minorEastAsia";
        if (v == ThemeFontValues.MajorEastAsia) return "majorEastAsia";

        return null;
    }
}
