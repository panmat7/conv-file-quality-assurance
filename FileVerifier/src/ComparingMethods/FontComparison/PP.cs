using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using PP = DocumentFormat.OpenXml.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Drawing;

using ColorMine.ColorSpaces;
using System.Diagnostics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Presentation;
using System.Globalization;
using Org.BouncyCastle.Asn1.Tsp;

namespace AvaloniaDraft.ComparingMethods;

public static class PPFontExtraction
{
    private struct StyleProperties
    {
        public string? latinFont;
        public string? eaFont;
        public string? csFont;
        public string? textColor;
        public string? buColor;
        public string? buFont;

        public StyleProperties()
        {
            latinFont = null;
            eaFont = null;
            csFont = null;

            textColor = null;
            buColor = null;
            buFont = null;
        }
    }

    /// <summary>
    /// Get the font information of a PPTX file
    /// </summary>
    /// <param name="src">The file path</param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoPP(string src)
    {
        var textInfo = new TextInfo();

        var doc = PresentationDocument.Open(src, false);
        if (doc is null) return null;

        var presPart = doc.PresentationPart;

        var slideParts = presPart?.SlideParts;
        if (slideParts == null) return null;

        // Go through each slide
        foreach (var slidePart in slideParts)
        {
            var slideMasterPart = GetSlideMasterPart(slidePart);
            if (slideMasterPart == null) return null;

            var slideLayout = slidePart.SlideLayoutPart?.SlideLayout?.CommonSlideData;

            // Get a color dictionary for the slide master theme colors
            var colorDic = GetSlideColors(slideMasterPart);
            if (colorDic == null) return null;

            // Get the theme
            var themePart = slideMasterPart.ThemePart;

            // Check the default fonts
            var fontScheme = themePart?.Theme?.ThemeElements?.FontScheme;
            var fontSchemeXml = XElement.Parse(fontScheme?.OuterXml ?? "");
            var defFonts = MSOffice.GetDefaultFontsFromScheme(fontSchemeXml);
            if (defFonts == null) return null;
            var majFonts = defFonts.Value.major;
            var minFonts = defFonts.Value.minor;


            // Go through each shape
            var shapes = slidePart.Slide.Descendants<PP.Shape>();
            foreach (PP.Shape shape in shapes)
            {
                CheckShape(shape, slideMasterPart, slideLayout, colorDic, minFonts, majFonts, textInfo);
            }
        }

        return textInfo;
    }



    /// <summary>
    /// Check a shape
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="slideMasterPart"></param>
    /// <param name="slideLayout"></param>
    /// <param name="colorDic"></param>
    /// <param name="minFonts"></param>
    /// <param name="majFonts"></param>
    /// <param name="textInfo"></param>
    private static void CheckShape(PP.Shape shape, SlideMasterPart? slideMasterPart, CommonSlideData? slideLayout, Dictionary<string, string> colorDic, 
        Dictionary<string, string> minFonts, Dictionary<string, string> majFonts, TextInfo textInfo)
    {
        // Get fill if present
        var fill = shape.ShapeProperties?.GetFirstChild<SolidFill>();
        var bgColor = fill?.RgbColorModelHex?.Val?.Value ?? GetSchemeColor(fill?.SchemeColor, colorDic);
        if (!string.IsNullOrEmpty(bgColor))
        {
            textInfo.BgColors.Add(bgColor);
        }

        var textBody = shape.TextBody;
        if (textBody == null) return;

        // Get the placeholder shape
        var phShape = shape.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.PlaceholderShape;
        var phType = phShape?.Type?.Value;
        var phIndex = phShape?.Index?.Value;
        var placeholderShape = slideLayout?.ShapeTree?.Descendants<PP.Shape>().FirstOrDefault(s =>
        {
            var ph = s.NonVisualShapeProperties?.ApplicationNonVisualDrawingProperties?.PlaceholderShape;

            var sameIndex = phIndex != null && ph?.Index?.Value == phIndex;
            var sameType = phType != null && ph?.Type?.Value == phType;

            return sameIndex || sameType;
        });

        // Go through each paragraph
        var paragraphs = textBody.Descendants<Paragraph>();
        if (paragraphs == null) return;
        foreach (var paragraph in paragraphs)
        {
            CheckParagraph(paragraph, placeholderShape, slideMasterPart, phType, colorDic, minFonts, majFonts, textInfo);
        }
    }


    /// <summary>
    /// Check a paragraph
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="placeholderShape"></param>
    /// <param name="slideMasterPart"></param>
    /// <param name="phType"></param>
    /// <param name="colorDic"></param>
    /// <param name="minFonts"></param>
    /// <param name="majFonts"></param>
    /// <param name="textInfo"></param>
    private static void CheckParagraph(Paragraph paragraph, PP.Shape? placeholderShape, SlideMasterPart? slideMasterPart, PlaceholderValues? phType, 
        Dictionary<string, string> colorDic, Dictionary<string, string> minFonts, Dictionary<string, string> majFonts, TextInfo textInfo)
    {
        var pProp = paragraph.ParagraphProperties;
        int? lvl = pProp?.Level?.Value;

        bool hasLevel = (lvl != null);
        bool phHasNonBullet = placeholderShape?.TextBody?.Descendants<Paragraph>().Any(p => p.ParagraphProperties?.Level is null) ?? true;
        bool isExplicitlyNotBullet = paragraph.Descendants<NoBullet>().Any();
        bool hasExplicitBullet = paragraph.Descendants<CharacterBullet>().Any() ||
            paragraph.Descendants<AutoNumberedBullet>().Any() || paragraph.Descendants<BulletFont>().Any();

        bool isBullet = (hasExplicitBullet || !phHasNonBullet || hasLevel) && !isExplicitlyNotBullet;

        // Check the style properties for the placeholder shape
        StyleProperties sldLayoutStyle;
        if (placeholderShape != null)
        {
            var listStyle = placeholderShape.TextBody?.ListStyle;
            var listStyleXml = XElement.Parse(listStyle?.OuterXml ?? "");
            sldLayoutStyle = GetDefaultStyle(listStyleXml, colorDic, lvl);
        }
        else
        {
            sldLayoutStyle = new StyleProperties();
        }

        // Check the slide master for default style properties
        StyleProperties sldMasterStyle;
        if (phType == PP.PlaceholderValues.Title || phType == PP.PlaceholderValues.CenteredTitle)
        {
            var titleStyle = slideMasterPart?.SlideMaster.Descendants<PP.TitleStyle>().FirstOrDefault();
            var titleStyleXml = XElement.Parse(titleStyle?.OuterXml ?? "");
            sldMasterStyle = GetDefaultStyle(titleStyleXml, colorDic, lvl);
        }
        else
        {
            var bodyStyle = slideMasterPart?.SlideMaster.Descendants<PP.BodyStyle>().FirstOrDefault();
            var bodyStyleXml = XElement.Parse(bodyStyle?.OuterXml ?? "");
            sldMasterStyle = GetDefaultStyle(bodyStyleXml, colorDic, lvl);
        }


        // Check bullet properties
        if (isBullet)
        {
            // Check bullet font
            var buFont = pProp?.GetFirstChild<BulletFont>()?.Typeface?.Value ?? sldLayoutStyle.buFont ?? sldMasterStyle.buFont;
            if (!string.IsNullOrEmpty(buFont)) textInfo.Fonts.Add(FontComparison.NormalizeFontName(buFont));

            // Check bullet color
            var buColor = pProp?.GetFirstChild<BulletColor>();
            var buHex = GetBulletColor(buColor, colorDic) ?? sldLayoutStyle.buColor ?? sldMasterStyle.buColor;
            if (!string.IsNullOrEmpty(buHex)) textInfo.TextColors.Add(buHex);
        }


        // Check each run
        var runs = paragraph.Descendants<Run>();
        if (runs == null) return;
        foreach (var run in runs)
        {
            CheckRun(run, majFonts, minFonts, colorDic, sldLayoutStyle, sldMasterStyle, textInfo);
        }
    }


    /// <summary>
    /// Check a run
    /// </summary>
    /// <param name="run"></param>
    /// <param name="majFonts"></param>
    /// <param name="minFonts"></param>
    /// <param name="colorDic"></param>
    /// <param name="sldLayoutStyle"></param>
    /// <param name="sldMasterStyle"></param>
    /// <param name="textInfo"></param>
    private static void CheckRun(Run run, Dictionary<string, string> majFonts, Dictionary<string, string> minFonts,
        Dictionary<string, string> colorDic, StyleProperties sldLayoutStyle, StyleProperties sldMasterStyle,
        TextInfo textInfo)
    {
        var runProp = run.RunProperties;
        var lang = runProp?.Language?.Value;

        // Check marking
        var highlightCol = runProp?.GetFirstChild<Highlight>()?
            .GetFirstChild<RgbColorModelHex>()?.Val?.Value;
        if (highlightCol != null) textInfo.BgColors.Add(highlightCol);

        if (string.IsNullOrWhiteSpace(run.InnerText)) return;

        // Check text color
        var solidFill = runProp?.GetFirstChild<SolidFill>();
        var textHex = GetSolidFillColor(solidFill, colorDic);
        var textColor = textHex ?? sldLayoutStyle.textColor ?? sldMasterStyle.textColor;
        if (textColor != null) textInfo.TextColors.Add(textColor);

        // Get fonts
        var csRef = runProp?.Descendants<RightToLeft>().Any() ?? false;
        var langIsZh = (!string.IsNullOrEmpty(lang) && lang.Contains("zh"));
        (var slots, var foreignChars) = MSOffice.GetFontSlotsAndCheckForForeignCharacters(run.InnerText, csRef, false, langIsZh, false, true);
        if (foreignChars) textInfo.ForeignWriting = true;

        var rLatinFont = runProp?.GetFirstChild<LatinFont>()?.Typeface?.Value;
        var rEaFont = runProp?.GetFirstChild<EastAsianFont>()?.Typeface?.Value;
        var rCsFont = runProp?.GetFirstChild<ComplexScriptFont>()?.Typeface?.Value;


        foreach (var slot in slots)
        {
            string? font = slot switch
            {
                "latin" => rLatinFont ?? sldLayoutStyle.latinFont ?? sldMasterStyle.latinFont,
                "eastAsia" => rEaFont ?? sldLayoutStyle.eaFont ?? sldMasterStyle.eaFont,
                "cs" => rCsFont ?? sldLayoutStyle.csFont ?? sldMasterStyle.csFont,
                _ => null,
            };

            if (string.IsNullOrEmpty(font)) continue;


            if (font.First() == '+') // A default font
            {
                GetDefaultFont(font, lang, majFonts, minFonts, textInfo);
            }
            else // Specific font
            {
                textInfo.Fonts.Add(FontComparison.NormalizeFontName(font));
            }
        }
    }


    /// <summary>
    /// Perform font substitution if the font is not explicitly stated
    /// </summary>
    /// <param name="font"></param>
    /// <param name="lang"></param>
    /// <param name="majFonts"></param>
    /// <param name="minFonts"></param>
    /// <param name="textInfo"></param>
    private static void GetDefaultFont(string font, string? lang, 
        Dictionary<string, string> majFonts, Dictionary<string, string> minFonts, TextInfo textInfo)
    {
        var fontThemeParts = font.Split('-');
        var type = fontThemeParts.FirstOrDefault(); // Minor or major
        var fontDic = type switch
        {
            "+mj" => majFonts,
            "+mn" => minFonts,
            _ => null,
        };
        if (fontDic == null) return;

        var script = ScriptCodes.GetScript(lang);
        if (script != null)
        {
            var fontUsed = fontDic.GetValueOrDefault(script);
            if (!string.IsNullOrEmpty(fontUsed)) textInfo.Fonts.Add(FontComparison.NormalizeFontName(fontUsed));
        }
        else
        {
            var scriptType = fontThemeParts.ElementAtOrDefault(1);
            if (scriptType == null) return;

            scriptType = scriptType switch
            {
                "lt" => "Latin",
                "ea" => "EastAsia",
                "cs" => "ComplexScript",
                _ => scriptType,
            };
            var fontUsed = fontDic.GetValueOrDefault(scriptType);

            if (!string.IsNullOrEmpty(fontUsed)) textInfo.Fonts.Add(FontComparison.NormalizeFontName(fontUsed));
        }
    }


    /// <summary>
    /// Get the default style of a shape or shape type
    /// </summary>
    /// <param name="style"></param>
    /// <param name="colorDic"></param>
    /// <param name="dMinFont"></param>
    /// <param name="dMajFont"></param>
    /// <param name="dFont"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    private static StyleProperties GetDefaultStyle(XElement? styleElement, Dictionary<string, string> colorDic, int? level)
    {
        const string typefaceStr = "typeface";
        var style = new StyleProperties();
        if (styleElement == null) return style;

        XElement? lvlStyle;
        if (level != null)
        {
            lvlStyle = styleElement.Descendants().FirstOrDefault(e => e.Name.LocalName == $"lvl{level}pPr");
        }
        else
        {
            lvlStyle = styleElement.Descendants().FirstOrDefault(e => e.Name.LocalName == $"dfRPr");
            if (lvlStyle == null) lvlStyle = styleElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "lvl1pPr");
        }
        if (lvlStyle == null) return style;


        // Bullet color
        var buClr = lvlStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "buClr");
        var buHex = GetSolidFillHexXml(buClr, colorDic);
        if (buHex != null) style.buColor = buHex;

        // Bullet font
        var buFont = lvlStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "buFont");
        if (buFont != null)
        {
            var typeface = buFont.Attributes().FirstOrDefault(a => a.Name.LocalName == typefaceStr)?.Value;
            if (typeface != null) style.buFont = FontComparison.NormalizeFontName(typeface);
        }

        var defProperties = lvlStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "defRPr");
        if (defProperties == null) return style;

        // Text color
        var solidFill = defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "solidFill");
        var txtHex = GetSolidFillHexXml(solidFill, colorDic);
        if (txtHex != null) style.textColor = txtHex;

        // Fonts
        style.latinFont = XmlHelpers.GetAttributeByLocalName(defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "latin"), typefaceStr);
        style.eaFont = XmlHelpers.GetAttributeByLocalName(defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "ea"), typefaceStr);
        style.csFont = XmlHelpers.GetAttributeByLocalName(defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "cs"), typefaceStr);

        return style;
    }


    /// <summary>
    /// Get the the hex value of a solid fill XML element
    /// </summary>
    /// <param name="solidFill"></param>
    /// <param name="colorDic"></param>
    /// <returns></returns>
    private static string? GetSolidFillHexXml(XElement? solidFill, Dictionary<string, string> colorDic)
    {
        if (solidFill == null) return null;

        // First check rgb
        string? hex = solidFill.Descendants().FirstOrDefault(e => e.Name.LocalName == "srgbClr")?.Value;
        if (hex != null) return hex;

        // Check scheme color 
        var schemeClr = solidFill.Descendants().FirstOrDefault(e => e.Name.LocalName == "schemeClr");
        if (schemeClr != null)
        {
            // Scheme
            var scheme = schemeClr.Attribute("val")?.Value;
            if (scheme != null && colorDic.TryGetValue(scheme, out string? value))
            {
                var baseColor = ColorTranslator.FromHtml("#" + value);

                var lumModStr = schemeClr.Descendants().FirstOrDefault(e => e.Name.LocalName == "lumMod")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "val")?.Value;
                var lumOffStr = schemeClr.Descendants().FirstOrDefault(e => e.Name.LocalName == "lumOff")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "val")?.Value;
                var tintStr = schemeClr.Descendants().FirstOrDefault(e => e.Name.LocalName == "tint")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "val")?.Value;


                var lumMod = GetFractionValue(lumModStr);
                var lumOff = GetFractionValue(lumOffStr);
                var tint = GetFractionValue(tintStr);

                (int r, int g, int b) = AdjustColor((baseColor.R, baseColor.G, baseColor.B), lumMod, lumOff, tint);
                hex = FontComparison.GetHex((r, g, b));

                return hex;
            }
        }

        return null;
    }


    /// <summary>
    /// Get a fraction normalized to a 0-1 value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private static double? GetFractionValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        try
        {
            if (value.Last() == '%')
            {
                value = value.Remove(value.Length - 1);
                return double.Parse(value)/100.0;
            }
            else
            {
                double f = 1.0f / 100000.0f; // Factor
                return double.Parse(value) * f;
            }
        } 
        catch
        {
            return null;
        }
    }


    /// <summary>
    /// Get the colors of a slide
    /// </summary>
    /// <param name="slideMasterPart"></param>
    /// <returns></returns>
    private static Dictionary<string, string>? GetSlideColors(SlideMasterPart slideMasterPart)
    {
        var colorDic = new Dictionary<string, string>();


        var colorMap = slideMasterPart.SlideMaster.ColorMap;
        if (colorMap == null) return colorDic;

        var themePart = slideMasterPart.ThemePart;
        var colorScheme = themePart?.Theme?.ThemeElements?.ColorScheme;
        if (colorScheme == null) return null;

        colorDic["accent1"] = GetMappedColor(colorScheme, colorMap.Accent1) ?? "";
        colorDic["accent2"] = GetMappedColor(colorScheme, colorMap.Accent2) ?? "";
        colorDic["accent3"] = GetMappedColor(colorScheme, colorMap.Accent3) ?? "";
        colorDic["accent4"] = GetMappedColor(colorScheme, colorMap.Accent4) ?? "";
        colorDic["accent5"] = GetMappedColor(colorScheme, colorMap.Accent5) ?? "";
        colorDic["accent6"] = GetMappedColor(colorScheme, colorMap.Accent6) ?? "";
        colorDic["tx1"] = GetMappedColor(colorScheme, colorMap.Text1) ?? "";
        colorDic["tx2"] = GetMappedColor(colorScheme, colorMap.Text2) ?? "";
        colorDic["bg1"] = GetMappedColor(colorScheme, colorMap.Background1) ?? "";
        colorDic["bg2"] = GetMappedColor(colorScheme, colorMap.Background2) ?? "";
        colorDic["hlink"] = GetMappedColor(colorScheme, colorMap.Hyperlink) ?? "";
        colorDic["folHlink"] = GetMappedColor(colorScheme, colorMap.FollowedHyperlink)  ?? "";

        return colorDic;
    }


    /// <summary>
    /// Get the correctly mapped color
    /// </summary>
    /// <param name="colorScheme"></param>
    /// <param name="colorIndex"></param>
    /// <returns></returns>
    private static string? GetMappedColor(ColorScheme colorScheme, EnumValue<ColorSchemeIndexValues>? colorIndex)
    {
        var colorIndexString = (colorIndex?.Value is ColorSchemeIndexValues c) ? 
            ((IEnumValue)c).Value : null;
        if (colorIndexString == null) return null;

        const string black = "000000";
        const string white = "FFFFFF";

        return colorIndexString switch
        {
            "accent1" => colorScheme.Accent1Color?.RgbColorModelHex?.Val ?? black,
            "accent2" => colorScheme.Accent2Color?.RgbColorModelHex?.Val ?? black,
            "accent3" => colorScheme.Accent3Color?.RgbColorModelHex?.Val ?? black,
            "accent4" => colorScheme.Accent4Color?.RgbColorModelHex?.Val ?? black,
            "accent5" => colorScheme.Accent5Color?.RgbColorModelHex?.Val ?? black,
            "accent6" => colorScheme.Accent6Color?.RgbColorModelHex?.Val ?? black,
            "dk1" => colorScheme.Dark1Color?.RgbColorModelHex?.Val ?? black,
            "dk2" => colorScheme.Dark2Color?.RgbColorModelHex?.Val ?? black,
            "lt1" => colorScheme.Light1Color?.RgbColorModelHex?.Val ?? white,
            "lt2" => colorScheme.Light2Color?.RgbColorModelHex?.Val ?? white,
            "hlink" => colorScheme.Hyperlink?.RgbColorModelHex?.Val ?? black,
            "folHlink" => colorScheme.FollowedHyperlinkColor?.RgbColorModelHex?.Val ?? black,
            _ => black // Default to black if unknown
        };
    }


    /// <summary>
    /// Get the slide master part of a slide part
    /// </summary>
    /// <param name="slidePart"></param>
    /// <returns></returns>
    private static SlideMasterPart? GetSlideMasterPart(SlidePart slidePart)
    {
        var slideRels = slidePart.Parts;
        foreach (IdPartPair sr in slideRels)
        {
            if (sr.OpenXmlPart is not SlideLayoutPart slideLayoutPart) continue;

            var slideLayoutRels = slideLayoutPart.Parts;
            foreach (IdPartPair slr in slideLayoutRels)
            {
                if (slr.OpenXmlPart is not SlideMasterPart slideMasterPart) continue;

                return slideMasterPart;
            }
        }

        return null;
    }


    /// <summary>
    /// Get the hex value of a scheme color
    /// </summary>
    /// <param name="schemeCol"></param>
    /// <param name="colorDic"></param>
    /// <returns></returns>
    private static string? GetSchemeColor(SchemeColor? schemeCol, Dictionary<string, string> colorDic)
    {
        var scheme = (schemeCol?.Val?.Value is SchemeColorValues c) ?
            ((IEnumValue)c).Value : null;

        if (schemeCol != null && scheme != null && colorDic.TryGetValue(scheme, out var baseColorHex))
        {
            var lumModInt = schemeCol.GetFirstChild<LuminanceModulation>()?.Val?.Value;
            var lumOffInt = schemeCol.GetFirstChild<LuminanceOffset>()?.Val?.Value;
            var tintInt = schemeCol.GetFirstChild<Tint>()?.Val?.Value;

            var lumMod = GetFractionValue(lumModInt?.ToString());
            var lumOff = GetFractionValue(lumOffInt?.ToString());
            var tint = GetFractionValue(tintInt?.ToString());

            var color = ColorTranslator.FromHtml("#" + baseColorHex);

            (int r, int g, int b) = AdjustColor((color.R, color.G, color.B), lumMod, lumOff, tint);
            var hex = FontComparison.GetHex((r, g, b));
            return hex;
        }
        return null;
    }



    /// <summary>
    /// Adjust the luminance and tint of a color
    /// </summary>
    /// <param name="rgb">The RGB values of the color</param>
    /// <param name="lumMod">Luminance modulation</param>
    /// <param name="lumOff">Luminance offset</param>
    /// <param name="tint">Tint</param>
    /// <returns></returns>
    private static (int, int, int) AdjustColor((int r, int g, int b) rgb, double? lumMod, double? lumOff, double? tint)
    {
        var rgbCol = new Rgb { R = rgb.r, B = rgb.b, G = rgb.g };

        // Apply tint
        if (tint is double dTint)
        {
            var whiteProportion = 1.0 - dTint;
            rgbCol.R = rgbCol.R * dTint + 255 * whiteProportion;
            rgbCol.G = rgbCol.G * dTint + 255 * whiteProportion;
            rgbCol.B = rgbCol.B * dTint + 255 * whiteProportion;
        }

        var hslCol = rgbCol.To<ColorMine.ColorSpaces.Hsl>();

        // Apply luminance
        if (lumMod is double dLMod)
        {
            hslCol.L *= dLMod;
        }
        if (lumOff is double dLOff)
        {
            hslCol.L += dLOff * 100;
        }
        var newRgb = hslCol.ToRgb();

        return ((int)Math.Round(newRgb.R), (int)Math.Round(newRgb.G), (int)Math.Round(newRgb.B));
    }



    /// <summary>
    /// Get a BulletColor's hex value
    /// </summary>
    /// <param name="bulletColor"></param>
    /// <param name="colorDic"></param>
    /// <returns></returns>
    private static string? GetBulletColor(BulletColor? bulletColor, Dictionary<string, string> colorDic)
    {
        if (bulletColor == null) return null;

        string? hex;

        var rgbCol = bulletColor.GetFirstChild<RgbColorModelHex>();
        if (rgbCol != null)
        {
            hex = rgbCol.Val;
        }
        else
        {
            var schemeCol = bulletColor.GetFirstChild<SchemeColor>();

            hex = GetSchemeColor(schemeCol, colorDic);
        }

        return hex;
    }


    /// <summary>
    /// Get the hex value of a solid fill
    /// </summary>
    /// <param name="solidFill"></param>
    /// <param name="colorDic"></param>
    /// <returns></returns>
    private static string? GetSolidFillColor(SolidFill? solidFill, Dictionary<string, string> colorDic)
    {
        string? hex = null;

        var rgbCol = solidFill?.GetFirstChild<RgbColorModelHex>();
        if (rgbCol != null)
        {
            hex = rgbCol.Val;
        }
        else
        {
            var schemeCol = solidFill?.GetFirstChild<SchemeColor>();

            hex = GetSchemeColor(schemeCol, colorDic);
        }

        return hex;
    }
}
