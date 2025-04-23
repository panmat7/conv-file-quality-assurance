using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AvaloniaDraft.ComparingMethods;

public static class MSOffice
{
    public static string? GetHighlightColor(EnumValue<HighlightColorValues>? color)
    {
        var colorString = (color?.Value is HighlightColorValues c) ? ((IEnumValue)c).Value : null;
        if (string.IsNullOrEmpty(colorString)) return null;


        return colorString switch
        {
            "yellow" => "FFFF00",
            "green" => "00FF00",
            "cyan" => "00FFFF",
            "magenta" => "800080",
            "blue" => "0000FF",
            "red" => "FF0000",
            "darkBlue" => "00008B",
            "darkCyan" => "008B8B",
            "darkGreen" => "006400",
            "darkMagenta" => "800080",
            "darkRed" => "8B0000",
            "darkYellow" => "808000",
            "darkGray" => "A9A9A9",
            "ligthGray" => "D3D3D3",
            "black" => "000000",
            _ => null,
        };
    }


    public static (Dictionary<string, string> major, Dictionary<string, string> minor)? GetDefaultFontsFromScheme(XElement fontScheme)
    {
        var majorFontsXml = XmlHelpers.GetFirsElementByLocalName(fontScheme, "majorFont");
        var minorFontsXml = XmlHelpers.GetFirsElementByLocalName(fontScheme, "minorFont");

        if (majorFontsXml == null || minorFontsXml == null) return null;

        return (GetDefaultFonts(majorFontsXml), (GetDefaultFonts(minorFontsXml)));
    }


    public static Dictionary<string, string> GetDefaultFonts(XElement fontList)
    {
        var dic = new Dictionary<string, string>();

        foreach (var font in fontList.Descendants())
        {
            (string? key, string? value) = font.Name.LocalName switch
            {
                "latin" => ("Latin", XmlHelpers.GetAttributeByLocalName(font, "typeface")),
                "ea" => ("EastAsia", XmlHelpers. GetAttributeByLocalName(font, "typeface")),
                "cs" => ("ComplexScript", XmlHelpers.GetAttributeByLocalName(font, "typeface")),

                "font" => (XmlHelpers.GetAttributeByLocalName(font, "script"), XmlHelpers.GetAttributeByLocalName(font, "typeface")),

                _ => (null, null),
            };

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value)) dic[key] = value;
        }

        // Make sure to contain defaults
        dic.TryAdd("Latin", "");
        dic.TryAdd("EastAsia", "");
        dic.TryAdd("ComplexScript", "");

        dic.TryAdd("Latn", dic["Latin"]);

        return dic;
    }




    /// <summary>
    /// Get the correct font slots for a run. Will also return whether or not the text contains any foreign characers
    /// </summary>
    /// <param name="txt">The run text</param>
    /// <param name="csRef">Whether or not there is a reference to complex scrip (<w:cs/> or <w:rtl/>)</param>
    /// <param name="eaHint">Whether or not the hint is set to 'eastAsia'</param>
    /// <param name="langIsZH">Whether or not the language is 'zh'</param>
    /// <param name="fontIsBig5orGB2312">Whether or not the font is 'Big5' or 'GB2312'</param>
    /// <returns></returns>
    public static (HashSet<string> slots, bool foreignWriting) GetFontSlots(string txt, bool csRef, bool eaHint, bool langIsZH, bool fontIsBig5orGB2312, bool combinedLatin)
    {
        if (string.IsNullOrWhiteSpace(txt)) return ([], false);

        bool foreignChars = false;

        string ascii = combinedLatin ? "latin" : "ascii";
        string hansi = combinedLatin ? "latin" : "hAnsi";
        string ea = "eastAsia";
        string cs = "cs";

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


        var slots = new HashSet<string>();
        foreach (var c in txt)
        {
            if (FontComparison.IsForeign(c)) foreignChars = true;

            // East Asian if language is zh or font is Big5 or GB2312, otherwise High Ansi
            if (FontComparison.InRange(c, eaIfZHOrBig5orGB2312Ranges))
            {
                slots.Add((eaHint && (langIsZH || fontIsBig5orGB2312)) ? ea : hansi);
                continue;
            }

            // East Asian if language is zh, otherwise High Ansi
            if (FontComparison.InRange(c, eaIfZHRanges))
            {
                slots.Add((eaHint && langIsZH) ? ea : hansi);
                continue;
            }

            // East Asian if hint, otherwise High Ansi
            if (FontComparison.InRange(c, hansiOrEaIfHintRanges))
            {
                slots.Add((eaHint) ? ea : hansi);
                continue;
            }

            // East Asian
            if (FontComparison.InRange(c, eaRanges))
            {
                slots.Add((csRef) ? cs : ea);
                continue;
            }

            // ASCII
            if (FontComparison.InRange(c, asciiRanges))
            {
                slots.Add((csRef) ? cs : ascii);
                continue;
            }

            // High Ansi
            if (FontComparison.InRange(c, hansiRanges))
            {
                slots.Add((csRef) ? cs : hansi);
            }
        }

        return (slots, foreignChars);
    }
}

