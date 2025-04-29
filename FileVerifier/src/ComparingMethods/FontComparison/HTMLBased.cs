using MimeKit;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace AvaloniaDraft.ComparingMethods;

public static class HtmlBasedFontExtraction
{
    /// <summary>
    /// Get font information from an EML file
    /// </summary>
    /// <param name="src">The file path</param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoEml(string src)
    {
        var message = MimeMessage.Load(src);

        var htmlBody = message.HtmlBody;
        if (htmlBody != null)
        {
            return GetTextInfoHtml(htmlBody, true);
        }
        else
        {
            return new TextInfo();
        }
    }

    /// <summary>
    /// Get font information from a HTML file
    /// </summary>
    /// <param name="src">The file path or HTML content</param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoHtml(string src, bool isContent = false, bool includeAltFonts = true)
    {
        var textInfo = new TextInfo();

        var doc = new HtmlDocument();

        if (isContent)
        {
            doc.LoadHtml(src);
        }
        else
        {
            doc.Load(src);
        }

        var allNodes = doc.DocumentNode.ChildNodes;
        textInfo.ForeignWriting = allNodes.Any(n => FontComparison.IsForeign(n.InnerText));

        var fontNodes = doc.DocumentNode.SelectNodes("//font[@face]") ?? Enumerable.Empty<HtmlNode>();
        foreach (var node in fontNodes)
        {
            var font = node.Attributes["face"].Value;
            textInfo.Fonts.Add(FontComparison.NormalizeFontName(font));
        }

        var styledNodes = doc.DocumentNode.SelectNodes("//*[@style]") ?? Enumerable.Empty<HtmlNode>();
        foreach (var node in styledNodes)
        {
            var styleAttr = node.Attributes["style"];

            var styleVal = styleAttr.Value.Replace("&quot;", "");
            var attributes = styleVal.Split(';');

            foreach (var attr in attributes)
            {
                var parts = attr.Split(":");
                if (parts.Length != 2) continue;

                var name = parts[0];
                name = name.TrimStart();
                var value = attr.Substring(name.Length + 1);

                switch(name)
                {
                    case "font-family":
                        GetFontFromFontFamilyAttribute(value, includeAltFonts, textInfo);
                        break;

                    case "color":
                        var txtHex = GetHex(value);
                        if (txtHex != null) textInfo.TextColors.Add(txtHex);
                        break;

                    case "background-color":
                    case "background":
                        var bgHex = GetHex(value);
                        if (bgHex != null) textInfo.BgColors.Add(bgHex);
                        break;
                }
            }
        }

        return textInfo;
    }




    /// <summary>
    /// Gets the font or fonts from a "font-family" attribute
    /// </summary>
    /// <param name="attributeValue"></param>
    /// <param name="includeAltFonts"></param>
    /// <param name="fonts"></param>
    /// <param name="altFonts"></param>
    private static void GetFontFromFontFamilyAttribute(string attributeValue, bool includeAltFonts, TextInfo textInfo)
    {
        var styleFonts = attributeValue.Split(',');
        if (styleFonts.Length == 1 || (!includeAltFonts && styleFonts.Length >= 1))
        {
            textInfo.Fonts.Add(FontComparison.NormalizeFontName(styleFonts[0]));
        }
        else if (styleFonts.Length > 1 && includeAltFonts)
        {
            var fontChoices = new HashSet<string>();
            foreach (var font in styleFonts)
            {
                fontChoices.Add(FontComparison.NormalizeFontName(font));
            }
            textInfo.AltFonts.Add(fontChoices);
        }
    }


    /// <summary>
    /// Get the hex value of a HTML color
    /// </summary>
    /// <param name="rgbString"></param>
    /// <returns></returns>
    private static string? GetHex(string rgbString)
    {
        if (rgbString[0] == '#')
        {
            return rgbString.Substring(1);
        }

        var match = FontComparison.RgbRegex().Match(rgbString);
        if (!match.Success) return null;

        var r = int.Parse(match.Groups[1].Value);
        var g = int.Parse(match.Groups[2].Value);
        var b = int.Parse(match.Groups[3].Value);

        return FontComparison.GetHex((r, g, b));
    }
}
