using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using AODL;
using AODL.Document.TextDocuments;
using AODL.Document.Content.Text;

namespace AvaloniaDraft.ComparingMethods;

public static class ODFontExtraction
{

    /// <summary>
    /// Get the text info of a ODT file
    /// </summary>
    /// <param name="src">The file path</param>
    /// <returns></returns>
    /*public static TextInfo? GetTextInfoODT(string src)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        WordprocessingDocument doc = WordprocessingDocument.Open(src, false);

        // Zip to extract xml files
        var zip = ZipFile.OpenRead(src);
        var content = zip.Entries.FirstOrDefault(e => e.Name == "content.xml");
        if (content == null) return null;

        // Get styles.xml
        var styles = zip.Entries.FirstOrDefault(e => e.Name == "styles.xml");
        if (styles == null) return null;
        var stylesReader = new StreamReader(styles.Open());
        var xmlStyles = stylesReader.ReadToEnd();
        var stylesDoc = XDocument.Parse(xmlStyles);

        var styleNamespace = stylesDoc?.Root?.GetNamespaceOfPrefix("style");
        if (styleNamespace == null) return null;

        // Get the default paragraph style
        var defaultParagraphStyle = stylesDoc?.Descendants().FirstOrDefault(e => e.Name.LocalName == "default-style" &&
                                                e.Attribute(styleNamespace + "family")?.Value == "paragraph");
        if (defaultParagraphStyle == null) return null;

        // Get the default font
        var defaultTextProperties = defaultParagraphStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "text-properties");
        var dFont = defaultTextProperties?.Attributes().FirstOrDefault(e => e.Name.LocalName == "font-name")?.Value;
        if (dFont is not string defaultFont) return null;
        defaultFont = FontComparison.NormalizeFontName(defaultFont);


        var contentReader = new StreamReader(content.Open());
        var xmlContent = contentReader.ReadToEnd();
        XDocument contentDoc = XDocument.Parse(xmlContent);

        // Check each style
        var contentStyles = contentDoc.Descendants().Where(e => e.Name.LocalName == "style");
        foreach (var style in contentStyles)
        {
            // Check paragraph properties
            var stylePropParagraph = style.Descendants().FirstOrDefault(d => d.Name.LocalName == "paragraph-properties");
            if (stylePropParagraph != null)
            {
                ODReadStyleProperties(stylePropParagraph, defaultFont, null, textColors, bgColors);
            }

            // Check text properties
            var stylePropText = style.Descendants().FirstOrDefault(d => d.Name.LocalName == "text-properties");
            if (stylePropText != null)
            {
                ODReadStyleProperties(stylePropText, defaultFont, fonts, textColors, bgColors);
            }
        }


        // Check for foreign characters
        var texts = contentDoc.Descendants().Where(d => d.Name.LocalName == "p" || d.Name.LocalName == "span");
        foreach (var t in texts)
        {
            if (FontComparison.IsForeign(t.Value))
            {
                foreignWriting = true;
                break;
            }
        }


        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }*/




    public static TextInfo? GetTextInfoODT(string src)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        var doc = new TextDocument();
        doc.Load(src);

        foreach (var c in doc.Content)
        {
            if (c is not Paragraph) continue;
        }


        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }



    /// <summary>
    /// Get text information from an ODS file
    /// </summary>
    /// <param name="src">The file path</param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoODS(string src)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        // Zip to extract xml files
        var zip = ZipFile.OpenRead(src);
        var content = zip.Entries.FirstOrDefault(e => e.Name == "content.xml");
        if (content == null) return null;

        // Get styles.xml
        var styles = zip.Entries.FirstOrDefault(e => e.Name == "styles.xml");
        if (styles == null) return null;
        var stylesReader = new StreamReader(styles.Open());
        var xmlStyles = stylesReader.ReadToEnd();
        var stylesDoc = XDocument.Parse(xmlStyles);

        // Get namespaces
        var styleNamespace = stylesDoc?.Root?.GetNamespaceOfPrefix("style");
        if (styleNamespace == null) return null;
        var tableNamespace = stylesDoc?.Root?.GetNamespaceOfPrefix("table");
        if (tableNamespace == null) return null;

        // Get the default text style properties
        var defaultTextProperties = stylesDoc?.Descendants().FirstOrDefault(e => e.Name.LocalName == "text-properties");
        if (defaultTextProperties == null) return null;

        // Get the default font
        var dFont = defaultTextProperties?.Attributes().FirstOrDefault(e => e.Name.LocalName == "font-name")?.Value;
        if (dFont is not string defaultFont) return null;
        defaultFont = FontComparison.NormalizeFontName(defaultFont);

        var contentReader = new StreamReader(content.Open());
        var xmlContent = contentReader.ReadToEnd();
        XDocument contentDoc = XDocument.Parse(xmlContent);
        var cells = contentDoc.Descendants().Where(e => e.Name.LocalName == "table-cell");
        var contentStyles = contentDoc.Descendants().Where(e => e.Name.LocalName == "style");

        // Check each cell's text properties, only if they have text
        foreach (var cell in cells)
        {
            var text = cell.Descendants().FirstOrDefault(e => e.Name.LocalName == "p");

            var styleName = cell.Attribute(tableNamespace + "style-name")?.Value;
            if (styleName is null) continue;

            var style = contentStyles.FirstOrDefault(e => e.Attribute(styleNamespace + "name")?.Value == styleName);
            var stylePropText = style?.Descendants().FirstOrDefault(d => d.Name.LocalName == "text-properties");

            // If there is text, but no text style properties, add default font
            if (text != null && stylePropText == null)
            {
                fonts.Add(defaultFont);
            }
        }

        // Check text style
        foreach (var style in contentStyles)
        {
            // Check cell properties
            var stylePropCell = style.Descendants().FirstOrDefault(d => d.Name.LocalName == "table-cell-properties");
            if (stylePropCell != null)
            {
                ODReadStyleProperties(stylePropCell, defaultFont, fonts, textColors, bgColors);
            }

            // Check text properties
            if (style.Attribute(styleNamespace + "family")?.Value != "text") continue;
            var stylePropText = style.Descendants().FirstOrDefault(d => d.Name.LocalName == "text-properties");
            if (stylePropText != null)
            {
                ODReadStyleProperties(stylePropText, defaultFont, fonts, textColors, bgColors);
            }
        }


        // Check for foreign characters
        var texts = contentDoc.Descendants().Where(d => d.Name.LocalName == "p" || d.Name.LocalName == "span");
        foreach (var t in texts)
        {
            if (FontComparison.IsForeign(t.Value))
            {
                foreignWriting = true;
                break;
            }
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }


    /// <summary>
    /// Get the text information from an ODP file
    /// </summary>
    /// <param name="src">The file path</param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoODP(string src)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        // Zip to extract xml files
        var zip = ZipFile.OpenRead(src);
        var content = zip.Entries.FirstOrDefault(e => e.Name == "content.xml");
        if (content == null) return null;

        var contentReader = new StreamReader(content.Open());
        var xmlContent = contentReader.ReadToEnd();
        XDocument contentDoc = XDocument.Parse(xmlContent);

        // Check each style
        var contentStyles = contentDoc.Descendants().Where(e => e.Name.LocalName == "style");
        foreach (var style in contentStyles)
        {
            // Check paragraph properties
            var stylePropParagraph = style.Descendants().FirstOrDefault(d => d.Name.LocalName == "paragraph-properties");
            if (stylePropParagraph != null)
            {
                ODReadStyleProperties(stylePropParagraph, null, null, textColors, bgColors);
            }

            // Check text properties
            var stylePropText = style.Descendants().FirstOrDefault(d => d.Name.LocalName == "text-properties");
            if (stylePropText != null)
            {
                ODReadStyleProperties(stylePropText, null, fonts, textColors, bgColors);
            }
        }


        // Check for foreign characters
        var texts = contentDoc.Descendants().Where(d => d.Name.LocalName == "p" || d.Name.LocalName == "span");
        foreach (var t in texts)
        {
            if (FontComparison.IsForeign(t.Value))
            {
                foreignWriting = true;
                break;
            }
        }


        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }



    /// <summary>
    /// Reads the properties of a style-properties node, and gets font, text color and background color
    /// </summary>
    /// <param name="e">The style property element</param>
    /// <param name="defaultFont">The default font, if not present</param>
    /// <param name="fonts"></param>
    /// <param name="textColors"></param>
    /// <param name="bgColors"></param>
    private static void ODReadStyleProperties(XElement e, string? defaultFont, HashSet<string>? fonts, HashSet<string> textColors, HashSet<string> bgColors)
    {
        var attrBgColor = e.Attributes().FirstOrDefault(a => a.Name.LocalName == "background-color");
        var attrTextColor = e.Attributes().FirstOrDefault(a => a.Name.LocalName == "color");
        var attrFont = e.Attributes().FirstOrDefault(a => a.Name.LocalName is "font-name" or "font-family");

        // Check background color
        if (!string.IsNullOrEmpty(attrBgColor?.Value))
        {
            var hex = ODGetHex(attrBgColor.Value);
            if (hex != null) bgColors.Add(hex);
        }

        // Check the text color
        if (!string.IsNullOrEmpty(attrTextColor?.Value))
        {
            var hex = ODGetHex(attrTextColor.Value);

            if (hex != null) textColors.Add(hex);
        }

        // Check the font
        if (fonts != null)
        {
            if (!string.IsNullOrEmpty(attrFont?.Value))
            {
                fonts.Add(FontComparison.NormalizeFontName(attrFont.Value));
            }
            else if (defaultFont != null)
            {
                fonts.Add(defaultFont);
            }
        }

    }


    /// <summary>
    /// Get the hex from a color in Open Document context
    /// </summary>
    /// <param name="col"></param>
    /// <returns></returns>
    private static string? ODGetHex(string col)
    {
        string? hex = null;

        if (col != "transparent")
        {
            if (col[0] == '#')
            {
                hex = col.Substring(1).ToUpper(); // Remove the '#'
            }
            else
            {
                //hex = GetOfficeColorFromName(col);
            }
        }

        return hex;
    }
}
