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
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        var message = MimeMessage.Load(src);

        var htmlBody = message.HtmlBody;
        if (htmlBody != null)
        {
            var textInfo = GetTextInfoHtml(htmlBody, true);
            return textInfo;
        }
        else
        {
            var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
            return textInfo;
        }
    }

    /// <summary>
    /// Get font information from a HTML file
    /// </summary>
    /// <param name="src">The file path or HTML content</param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoHtml(string src, bool isContent = false, bool includeAltFonts = true)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();


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
        foreignWriting = allNodes.Any(n => FontComparison.IsForeign(n.InnerText));

        var fontNodes = doc.DocumentNode.SelectNodes("//font[@face]");
        if (fontNodes != null)
        {
            foreach (var node in fontNodes)
            {
                var font = node.Attributes["face"].Value;
                fonts.Add(FontComparison.NormalizeFontName(font));
            }
        }


        var styledNodes = doc.DocumentNode.SelectNodes("//*[@style]");
        if (styledNodes != null)
        {
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

                    if (name == "font-family")
                    {
                        var styleFonts = value.Split(',');
                        if (styleFonts.Length == 1 || (!includeAltFonts && styleFonts.Length >= 1))
                        {
                            fonts.Add(FontComparison.NormalizeFontName(styleFonts[0]));
                        }
                        else if (styleFonts.Length > 1 && includeAltFonts)
                        {
                            var fontChoices = new HashSet<string>();
                            foreach (var font in styleFonts)
                            {
                                fontChoices.Add(FontComparison.NormalizeFontName(font));
                            }
                            altFonts.Add(fontChoices);
                        }
                    }
                    else if (name == "color")
                    {
                        var hex = GetHex(value);
                        if (hex != null) textColors.Add(hex);
                    }
                    else if (name == "background-color" || name == "background")
                    {
                        var hex = GetHex(value);
                        if (hex != null) bgColors.Add(hex);
                    }
                }
            }
        }


        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
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
