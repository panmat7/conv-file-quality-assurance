using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Immutable;
using Org.BouncyCastle.Asn1.Tsp;

namespace AvaloniaDraft.ComparingMethods;

public static class ODFontExtraction
{
    private static readonly List<(int start, int end)> CjkRanges =
    [
            (0x4E00, 0x9FFF),  // CJK Unified Ideographs
            (0x3400, 0x4DBF),  // CJK Unified Ideographs Extension A
            (0x20000, 0x2A6DF),  // CJK Unified Ideographs Extension B
            (0x2A700, 0x2B73F),  // CJK Unified Ideographs Extension C
            (0x2B740, 0x2B81F),  // CJK Unified Ideographs Extension D
            (0x2B820, 0x2CEAF),  // CJK Unified Ideographs Extension E
            (0x2CEB0, 0x2EBEF),  // CJK Unified Ideographs Extension F
            (0x30000, 0x3134F),  // CJK Unified Ideographs Extension G
            (0x31350, 0x323AF),  // CJK Unified Ideographs Extension H
            (0x2EBF0, 0x2EE5F),  // CJK Unified Ideographs Extension I
            (0x2E80, 0x2EFF),  // CJK Radicals Supplement
            (0x2F00, 0x2FDF),  // Kangxi Radicals
            (0x2FF0, 0x2FFF),  // Ideographic Description Characters
            (0x3000, 0x303F),  // CJK Symbols and Punctuation
            (0x31C0, 0x31EF),  // CJK Strokes
            (0x3200, 0x32FF),  // Enclosed CJK Letters and Months
            (0x3300, 0x33FF),  // CJK Compatibility
            (0xF900, 0xFAFF),  // CJK Compatibility Ideographs
            (0xFE30, 0xFE4F),  // CJK Compatibility Forms
            (0x1F200, 0x1F2FF),  // Enclosed Ideographic Supplement
            (0x2F800, 0x2FA1F)  // CJK Compatibility Ideographs Supplement
    ];


    private static readonly List<(int start, int end)> CtlRanges =
    [
        (0x0600, 0x06FF),  // Arabic
        (0x0750, 0x077F),  // Arabic Supplement
        (0x08A0, 0x08FF),  // Arabic Extended-A
        (0xFB50, 0xFDFF),  // Arabic Presentation Forms-A
        (0xFE70, 0xFEFF),  // Arabic Presentation Forms-B
        (0x0900, 0x097F),  // Devanagari
        (0x0980, 0x09FF),  // Bengali
        (0x0A00, 0x0A7F),  // Gurmukhi
        (0x0A80, 0x0AFF),  // Gujarati
        (0x0B00, 0x0B7F),  // Oriya
        (0x0B80, 0x0BFF),  // Tamil
        (0x0C00, 0x0C7F),  // Telugu
        (0x0C80, 0x0CFF),  // Kannada
        (0x0D00, 0x0D7F),  // Malayalam
        (0x0D80, 0x0DFF),  // Sinhala
        (0x1CD0, 0x1CFF),  // Vedic Extensions
        (0x0E00, 0x0E7F),  // Thai
        (0x0E80, 0x0EFF),  // Lao
        (0x1000, 0x109F),  // Myanmar
        (0x1780, 0x17FF),  // Khmer
        (0x0F00, 0x0FFF),  // Tibetan
        (0xA800, 0xA82F),  // Syloti Nagri
        (0xABC0, 0xABFF),  // Meetei Mayek
    ];



    /// <summary>
    /// Get the font information of a ODF file (ODT, ODP or ODS)
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoODF(string src)
    {
        var textInfo = new TextInfo();

        if (ExtractDocuments(src) is not (XDocument contentDoc, XDocument stylesDoc)) return null;

        // Get the default paragraph style
        var defaultParagraphStyle = stylesDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "default-style" &&
                                                XmlHelpers.GetAttributeByLocalName(e, "family") == "paragraph");

        // Get the default cell style
        var defaultCellStyle = stylesDoc.Descendants().FirstOrDefault(e => e.Name.LocalName == "style" &&
                                                XmlHelpers.GetAttributeByLocalName(e, "name") == "Default" &&
                                                XmlHelpers.GetAttributeByLocalName(e, "family") == "table-cell");

        // Get the default properties
        var defaultParagraphTextProperties = defaultParagraphStyle?.Descendants().FirstOrDefault(e => e.Name.LocalName == "text-properties");
        var defaultParagraphProperties = defaultParagraphStyle?.Descendants().FirstOrDefault(e => e.Name.LocalName == "paragraph-properties");
        var defaultCellProperties = defaultCellStyle?.Descendants().FirstOrDefault(e => e.Name.LocalName == "table-cell-properties");
        var defaultCellTextProperties = defaultCellStyle?.Descendants().FirstOrDefault(e => e.Name.LocalName == "text-properties");


        var contentRoot = contentDoc.Root;
        if (contentRoot == null) return null;

        var contentStyles = XmlHelpers.GetFirsElementByLocalName(contentRoot, "automatic-styles");
        if (contentStyles == null) return null;


        // Check every paragraph
        var paragraphs = contentDoc.Descendants().Where(e => e.Name.LocalName == "p");
        foreach (var p in paragraphs)
        {
            CheckElement(p, "paragraph", contentStyles, defaultParagraphProperties, defaultParagraphTextProperties, textInfo);
        }


        // Check every cell
        var cells = contentDoc.Descendants().Where(e => e.Name.LocalName == "table-cell");
        foreach (var cell in cells)
        {
            CheckElement(cell, "table-cell", contentStyles, defaultCellProperties, defaultCellTextProperties, textInfo);
        }

        
        // Check every frame
        var frames = contentDoc.Descendants().Where(e => e.Name.LocalName == "frame");
        foreach (var frame in frames)
        {
            CheckElement(frame, "graphic", contentStyles, null, defaultParagraphTextProperties, textInfo);
        }

        // Check every span
        var textSpans = contentDoc.Descendants().Where(e => e.Name.LocalName == "span");
        foreach (var span in textSpans)
        {
            CheckElement(span, null, contentStyles, null, defaultParagraphTextProperties, textInfo);
        }


        // Check every list item
        CheckListItems(contentDoc, contentStyles, textInfo);

        return textInfo;
    }


    /// <summary>
    /// Check list items of the file
    /// </summary>
    /// <param name="contentDoc"></param>
    /// <param name="contentStyles"></param>
    /// <param name="textInfo"></param>
    private static void CheckListItems(XDocument contentDoc, XElement? contentStyles, TextInfo textInfo)
    {
        if (contentStyles == null) return;

        var listItems = contentDoc.Descendants().Where(i => i.Name.LocalName == "list-item" &&
            i.Elements().Any(e => e.Name.LocalName != "list"));

        foreach (var item in listItems)
        {
            var level = item.Ancestors().Count(e => e.Name.LocalName == "list");
            var list = item.Parent;
            if (list == null || list.Name.LocalName != "list") continue;

            var listStyle = GetStyle(list, contentStyles);
            if (listStyle == null) continue;

            var itemStyle = listStyle.Elements().FirstOrDefault(e => XmlHelpers.GetAttributeByLocalName(e, "level") == level.ToString());
            if (itemStyle == null) continue;

            var textProperties = itemStyle.Descendants().FirstOrDefault(d => d.Name.LocalName == "text-properties");
            ReadStyleProperties(textProperties, null, null, textInfo, true);
        }
    }
    

    /// <summary>
    /// Add the style properties of an element
    /// </summary>
    /// <param name="element"></param>
    /// <param name="elementName"></param>
    /// <param name="contentStyles"></param>
    /// <param name="defaultElementProperties"></param>
    /// <param name="defaultTextProperties"></param>
    /// <param name="textInfo"></param>
    private static void CheckElement(XElement element, string? elementName, XElement contentStyles, XElement? defaultElementProperties, XElement? defaultTextProperties,
        TextInfo textInfo)
    {
        var style = GetStyle(element, contentStyles);
        if (style == null) return;

        // Check element properties
        if (elementName != null)
        {
            var elementStyleProperties = style.Descendants().FirstOrDefault(d => d.Name.LocalName == $"{elementName}-properties");
            ReadStyleProperties(elementStyleProperties, defaultElementProperties, null, textInfo);
        }

        var txt = element.Value;
        if (string.IsNullOrEmpty(txt)) return;

        // Check text properties
        var textStyleProperties = style.Descendants().FirstOrDefault(d => d.Name.LocalName == "text-properties");
        ReadStyleProperties(textStyleProperties, defaultTextProperties, txt, textInfo);

        if (!textInfo.ForeignWriting && FontComparison.IsForeign(txt)) textInfo.ForeignWriting = true;
    }


    /// <summary>
    /// Reads the properties of a style-properties node, and gets font, text color and background color
    /// </summary>
    /// <param name="p">The style property element</param>
    /// <param name="d">The default style property element</param>
    /// <param name="text">The text of the element</param>
    /// <param name="fonts"></param>
    /// <param name="textColors"></param>
    /// <param name="bgColors"></param>
    private static void ReadStyleProperties(XElement? p, XElement? d, string? text, TextInfo textInfo, bool isBullet = false)
    {
        if (p == null && d == null) return;

        var attrBgColor = p?.Attributes().FirstOrDefault(a => a.Name.LocalName == "background-color" || a.Name.LocalName == "fill-color") ?? 
            d?.Attributes().FirstOrDefault(a => a.Name.LocalName == "background-color" || a.Name.LocalName == "fill-color");

        var attrTextColor = p?.Attributes().FirstOrDefault(a => a.Name.LocalName == "color") ??
            d?.Attributes().FirstOrDefault(a => a.Name.LocalName == "color");

        // Check the background color
        var bgHex = ODGetHex(attrBgColor?.Value);
        if (bgHex != null) textInfo.BgColors?.Add(bgHex);


        if (text != null)
        {
            // Check the text color
            var txtHex = ODGetHex(attrTextColor?.Value);
            if (txtHex != null) textInfo.TextColors?.Add(txtHex);

            // Check the font
            ReadFontsFromSyleProperties(text, isBullet, p, d, textInfo);
        }
    }


    /// <summary>
    /// Add the fonts from style properties
    /// </summary>
    /// <param name="text"></param>
    /// <param name="isBullet"></param>
    /// <param name="properties"></param>
    /// <param name="defaultProperties"></param>
    /// <param name="fonts"></param>
    private static void ReadFontsFromSyleProperties(string text, bool isBullet, XElement? properties, XElement? defaultProperties, TextInfo textInfo)
    {
        if (properties == null) return;

        if (!isBullet)
        {
            CheckFontsNonBulletProperties(text, properties, defaultProperties, textInfo);
        }
        else
        {
            CheckFontsBulletProperties(properties, defaultProperties, textInfo);
        }
    }


    /// <summary>
    /// Check the properties of a non-bullet element
    /// </summary>
    /// <param name="text"></param>
    /// <param name="properties"></param>
    /// <param name="defaultProperties"></param>
    /// <param name="textInfo"></param>
    private static void CheckFontsNonBulletProperties(string text, XElement properties, XElement? defaultProperties, TextInfo textInfo)
    {
        var gotten = new Dictionary<CharClassification, bool>();
        gotten[CharClassification.CJK] = false;
        gotten[CharClassification.CTL] = false;
        gotten[CharClassification.Other] = false;

        // Get the correct font based on classification
        var classifications = GetCharClassifications(text);
        foreach (var classification in classifications)
        {
            if (gotten[classification]) continue;

            gotten[classification] = true;

            var fontAttributeClassification = classification switch
            {
                CharClassification.CJK => "-asian",
                CharClassification.CTL => "-complex",
                CharClassification.Other => "",
                _ => "",
            };

            var fontName = $"font-name{fontAttributeClassification}";
            var fontFamily = $"font-family{fontAttributeClassification}";
            var font = properties?.Attributes().FirstOrDefault(a => a.Name.LocalName == fontName || a.Name.LocalName == fontFamily)?.Value ??
                defaultProperties?.Attributes().FirstOrDefault(a => a.Name.LocalName == fontName || a.Name.LocalName == fontFamily)?.Value;
            if (font != null)
            {
                textInfo.Fonts.Add(FontComparison.NormalizeFontName(font));
            }
        }
    }


    /// <summary>
    /// Check the properties of a bullet element
    /// </summary>
    /// <param name="properties"></param>
    /// <param name="defaultProperties"></param>
    /// <param name="textInfo"></param>
    private static void CheckFontsBulletProperties(XElement properties, XElement? defaultProperties, TextInfo textInfo)
    {
        var font = properties?.Attributes().FirstOrDefault(a => a.Name.LocalName == "font-name" || a.Name.LocalName == "font-family")?.Value ??
            defaultProperties?.Attributes().FirstOrDefault(a => a.Name.LocalName == "font-name" || a.Name.LocalName == "font-family")?.Value;
        if (font != null)
        {
            textInfo.Fonts.Add(FontComparison.NormalizeFontName(font));
        }
    }





    /// <summary>
    /// Get the hex from a color in Open Document context
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    private static string? ODGetHex(string? col)
    {
        if (string.IsNullOrEmpty(col)) return null;
        string? hex = null;

        if (col[0] == '#')
        {
            hex = col.Substring(1).ToUpper(); // Remove the '#'
        }

        return hex;
    }



    private enum CharClassification
    {
        CJK,
        CTL,
        Other
    }

    private static CharClassification GetCharClassification(char c)
    {
        if (FontComparison.InRange(c, CjkRanges)) return CharClassification.CJK;
        if (FontComparison.InRange(c, CtlRanges)) return CharClassification.CJK;
        return CharClassification.Other;
    }


    private static HashSet<CharClassification> GetCharClassifications(string str)
    {
        var classifications = new HashSet<CharClassification>();
        foreach (char c in str)
        {
            classifications.Add(GetCharClassification(c));
        }

        return classifications;
    }



    private static XElement? GetStyle(XElement element, XElement styles)
    {
        var styleName = XmlHelpers.GetAttributeByLocalName(element, "style-name");
        if (styleName == null) return null;

        var style = styles.Descendants().FirstOrDefault(e => XmlHelpers.GetAttributeByLocalName(e, "name") == styleName);

        return style;
    }


    /// <summary>
    /// Extract the content and styles documents of an open document
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    private static (XDocument contentDoc, XDocument stylesDoc)? ExtractDocuments(string src)
    {
        try
        {
            // Zip to extract xml files
            var zip = ZipFile.OpenRead(src);
            var content = zip.Entries.FirstOrDefault(e => e.Name == "content.xml");
            var styles = zip.Entries.FirstOrDefault(e => e.Name == "styles.xml");
            if (content == null || styles == null) return null;

            // Parse content
            var contentReader = new StreamReader(content.Open());
            var xmlContent = contentReader.ReadToEnd();
            XDocument contentDoc = XDocument.Parse(xmlContent);
            if (contentDoc == null) return null;

            // Parse styles
            var stylesReader = new StreamReader(styles.Open());
            var xmlStyles = stylesReader.ReadToEnd();
            var stylesDoc = XDocument.Parse(xmlStyles);
            if (stylesDoc == null) return null;

            return (contentDoc, stylesDoc);
        } 
        catch
        {
            return null;
        }
    }
}
