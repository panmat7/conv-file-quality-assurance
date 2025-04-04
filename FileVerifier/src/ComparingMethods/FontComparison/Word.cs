using Avalonia.Animation.Easings;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AvaloniaDraft.ComparingMethods;

public static class WordFontExtraction
{
    public static TextInfo? GetTextInfoWord(string src)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        WordprocessingDocument doc = WordprocessingDocument.Open(src, false);
        var mainDocPart = doc.MainDocumentPart;

        // Get default fonts
        var fontScheme = mainDocPart?.ThemePart?.Theme?.ThemeElements?.GetFirstChild<DocumentFormat.OpenXml.Drawing.FontScheme>();
        if (fontScheme == null) return null;

        var fontSchemeXml = XElement.Parse(fontScheme?.OuterXml ?? "");
        var defFonts = MSOffice.GetDefaultFontsFromScheme(fontSchemeXml);
        if (defFonts == null) return null;

        var majFonts = defFonts.Value.major;
        var minFonts = defFonts.Value.minor;
        var themeFontLang = mainDocPart?.DocumentSettingsPart?.Settings?.GetFirstChild<ThemeFontLanguages>();
        var fontTable = mainDocPart?.FontTablePart?.Fonts;
        if (minFonts == null || majFonts == null || themeFontLang == null || fontTable == null) return null;

        // Look through each paragraph
        var paragraphs = mainDocPart?.Document?.Body?.Elements<Paragraph>() ?? [];
        foreach (var p in paragraphs)
        {
            // Check if paragraph is filled
            string? pColor = p.ParagraphProperties?.Shading?.Fill;
            if (pColor != null)
            {
                bgColors.Add(pColor);
            }

            // Go through each run
            foreach (var run in p.Elements<Run>())
            {
                CheckRun(run, themeFontLang, fontTable, majFonts, minFonts, fonts, textColors, bgColors, ref foreignWriting);
            }
        }

        var styles = mainDocPart?.StyleDefinitionsPart?.Styles?.Descendants<Style>();
        if (styles == null) return null;

        // Hyperlink colors are only stored in styles, so check there
        foreach (var style in styles)
        {
            var name = style.StyleName;
            if (name == null || name?.Val?.Value != "Hyperlink") continue;

            var hex = style.StyleRunProperties?.Color?.Val?.Value;
            if (hex != null)
            {
                textColors.Add(hex);
            }
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }


    /// <summary>
    /// Check a run
    /// </summary>
    /// <param name="run"></param>
    /// <param name="themeFontLang"></param>
    /// <param name="fontTable"></param>
    /// <param name="majFonts"></param>
    /// <param name="minFonts"></param>
    /// <param name="fonts"></param>
    /// <param name="textColors"></param>
    /// <param name="bgColors"></param>
    /// <param name="foreignWriting"></param>
    private static void CheckRun(Run run, ThemeFontLanguages themeFontLang, Fonts fontTable, Dictionary<string, string> majFonts, Dictionary<string, string> minFonts,
        HashSet<string> fonts, HashSet<string> textColors, HashSet<string> bgColors, ref bool foreignWriting)
    {
        var runProperties = run.RunProperties;


        // Check hightlight color
        var highlightCol = runProperties?.Highlight?.Val;
        var hex = MSOffice.GetHighlightColor(highlightCol);
        if (hex != null) bgColors.Add(hex);

        if (string.IsNullOrWhiteSpace(run.InnerText)) return;

        // Check text color
        string? textCol = runProperties?.Color?.Val?.Value;
        textColors.Add(textCol ?? "000000");

        // Check shading color
        string? shadingCol = runProperties?.Shading?.Fill;
        if (shadingCol != null)
        {
            bgColors.Add(shadingCol);
        }

        // Font
        (var runFonts, var fw) = GetFontsFromRun(run, themeFontLang, fontTable, majFonts, minFonts);
        if (fw) foreignWriting = true;
        if (runFonts != null) foreach (var font in runFonts)
        {
            if (!string.IsNullOrEmpty(font)) fonts.Add(FontComparison.NormalizeFontName(font));
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
        var eaTheme = GetThemeFontValueString(rFonts?.EastAsiaTheme?.Value);
        var eaThemeLang = themeFontLang.EastAsia?.Value ?? "";

        // Complex script
        var csTheme = GetThemeFontValueString(rFonts?.ComplexScriptTheme?.Value ?? rFonts?.EastAsiaTheme?.Value);
        var csThemeLang = themeFontLang.Bidi?.Value ?? "";

        // Other (ASCII / High ansii)
        var asciiTheme = GetThemeFontValueString(rFonts?.AsciiTheme?.Value);
        var hansiTheme = GetThemeFontValueString(rFonts?.HighAnsiTheme?.Value);
        var oThemeLang = themeFontLang.Val?.Value ?? "";


        var themeLangs = new Dictionary<string, string>() {
            { "ascii", oThemeLang },
            { "highansi", oThemeLang },
            { "eastasia", eaThemeLang },
            { "cs", csThemeLang }
        };

        // The theme font takes priority over specified font
        var eaFont = GetThemeFont(themeLangs, eaTheme, major, minor) ?? rFonts?.EastAsia;

        var lang = rPr.Languages;

        bool fontIsBig5OrGB2312 = FontIsBig5OrGB2312(eaFont, eaTheme, major, minor);
        bool langIsZh = LangIsZh(lang, eaLang);

        var hint = rFonts?.Hint?.Value;
        bool eaHint = false;
        if (hint is FontTypeHintValues h) eaHint = (h == FontTypeHintValues.EastAsia);

        var csRef = (rPr.RightToLeftText != null || rPr.ComplexScript != null); // A reference to complex script

        var fonts = new List<string?>();

        var result = GetFontClassifications(txt, csRef, eaHint, langIsZh, fontIsBig5OrGB2312);
        var classifications = result.classifications;
        var foreignWriting = result.foreignWriting;

        foreach (var classification in classifications)
        {
            var font = classification.ToLower() switch
            {
                "ascii" => rFonts?.Ascii ?? GetThemeFont(themeLangs, asciiTheme, major, minor),
                "hansi" => rFonts?.HighAnsi ?? GetThemeFont(themeLangs, hansiTheme, major, minor),
                "eastasia" => rFonts?.EastAsia ?? GetThemeFont(themeLangs, eaTheme, major, minor),
                "cs" => rFonts?.ComplexScript ?? GetThemeFont(themeLangs, csTheme, major, minor),
                _ => null
            };
            if (string.IsNullOrEmpty(font)) continue;



            // Check that the font exists in the font table, and that it is using its primary and not alternative name.
            bool existsInFontTable = fontTable.Descendants<Font>()
                .Any(f => font == f.Name?.Value || font == f.AltName?.Val?.Value);
            if (existsInFontTable) fonts.Add(font);
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
        bool fontIsBig5OrGB2312 = false;
        if (font == "Big5" || font == "GB2312")
        {
            fontIsBig5OrGB2312 = true;
        }
        else if (fontTheme != null)
        {
            if (fontTheme == "majorEastAsia" && majorFonts.ContainsKey("eastAsia"))
            {
                var majEA = majorFonts["eastAsia"];
                if (majEA == "Big5" || majEA == "GB2312") fontIsBig5OrGB2312 = true;
            }
            else if (fontTheme == "minorEastAsia" && minorFonts.ContainsKey("eastAsia"))
            {
                var minEA = minorFonts["eastAsia"];
                if (minEA == "Big5" || minEA == "GB2312") fontIsBig5OrGB2312 = true;
            }
        }

        return fontIsBig5OrGB2312;
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

        var classification = theme?.Substring("m**or".Count());
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



    /// <summary>
    /// Get the correct font classifications for a run. Will also return whether or not the text contains any foreign characers
    /// </summary>
    /// <param name="txt">The run text</param>
    /// <param name="csRef">Whether or not there is a reference to complex scrip (<w:cs/> or <w:rtl/>)</param>
    /// <param name="eaHint">Whether or not the hint is set to 'eastAsia'</param>
    /// <param name="langIsZH">Whether or not the language is 'zh'</param>
    /// <param name="fontIsBig5orGB2312">Whether or not the font is 'Big5' or 'GB2312'</param>
    /// <returns></returns>
    private static (HashSet<string> classifications, bool foreignWriting) GetFontClassifications(string txt, bool csRef, bool eaHint, bool langIsZH, bool fontIsBig5orGB2312)
    {
        if (string.IsNullOrWhiteSpace(txt)) return ([], false);

        bool foreignChars = false;

        const string ascii = "ascii";
        const string hansi = "hAnsi";
        const string ea = "eastAsia";
        const string cs = "cs";

        var asciiRanges = new List<(int, int)>()
        {
            (0x0, 0x7F),
            (0x590, 0x7BF),
            (0xFB1D, 0xFB4F),
            (0xFB50, 0xFDFF),
            (0xFE70, 0xFEFE)
        };

        var eaRanges = new List<(int, int)>()
        {
            (0x1100, 0x11FF),
            (0x2E80, 0xDFFF),
            (0xF900, 0xFAFF),
            (0xFB00, 0xFB1C),
            (0xFE30, 0xFE6F),
            (0xFF00, 0xFFEF),
        };

        var hansiRanges = new List<(int, int)>() {
            (0x1F00, 0x1FFF),
            (0xA0, 0xFF)
        };

        var hansiOrEaIfHintRanges = new List<(int, int)>()
        {
            (0xA1, 0xA1),
            (0xA4, 0xA4),
            (0xA7, 0xA8),
            (0xAA, 0xAA),
            (0xAD, 0xAD),
            (0xAF, 0xAF),
            (0xB0, 0xB4),
            (0xB6, 0xBA),
            (0xBC, 0xBF),
            (0xD7, 0xD7),
            (0xF7, 0xF7),
            (0x02B0, 0x04FF),
            (0x1100, 0x11FF),
            (0x1E00, 0x1EFF),
            (0x2000, 0x27BF),
            (0xE000, 0xF8FF)
        };

        var eaIfZHRanges = new List<(int, int)>()
        {
            (0xE0, 0xE1),
            (0xE8, 0xEA),
            (0xEC, 0xED),
            (0xF2, 0xF3),
            (0xF9, 0xFA),
            (0xFC, 0xFC)
        };

        var eaIfZHOrBig5orGB2312Ranges = new List<(int, int)>()
        {
            (0x0100, 0x02AF)
        };


        var classifications = new HashSet<string>();
        foreach (var c in txt)
        {
            if (FontComparison.IsForeign(c)) foreignChars = true;

            // East Asian if language is zh or font is Big5 or GB2312, otherwise High Ansi
            if (FontComparison.InRange(c, eaIfZHOrBig5orGB2312Ranges))
            {
                classifications.Add((eaHint && (langIsZH || fontIsBig5orGB2312)) ? ea : hansi);
                continue;
            }

            // East Asian if language is zh, otherwise High Ansi
            if (FontComparison.InRange(c, eaIfZHRanges))
            {
                classifications.Add((eaHint && langIsZH) ? ea : hansi);
                continue;
            }

            // East Asian if hint, otherwise High Ansi
            if (FontComparison.InRange(c, hansiOrEaIfHintRanges))
            {
                classifications.Add((eaHint) ? ea : hansi);
                continue;
            }

            // East Asian
            if (FontComparison.InRange(c, eaRanges))
            {
                classifications.Add((csRef) ? cs : ea);
                continue;
            }

            // ASCII
            if (FontComparison.InRange(c, asciiRanges))
            {
                classifications.Add((csRef) ? cs : ascii);
                continue;
            }

            // High Ansi
            if (FontComparison.InRange(c, hansiRanges))
            {
                classifications.Add((csRef) ? cs : hansi);
            }
        }

        return (classifications, foreignChars);
    }



}
