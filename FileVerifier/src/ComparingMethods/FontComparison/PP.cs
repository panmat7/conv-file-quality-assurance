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
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

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
            var themePart = slideMasterPart?.ThemePart;

            // Check the default fonts
            var fontScheme = themePart?.Theme?.ThemeElements?.FontScheme;
            if (fontScheme == null) return null;

            /// TODO: ADD A MAP FOR DEFAULT FONTS


            // Go through each shape
            var shapes = slidePart.Slide.Descendants<PP.Shape>();
            foreach (PP.Shape shape in shapes)
            {
                // Get fill if present
                var fill = shape.ShapeProperties?.GetFirstChild<SolidFill>();
                var bgColor = fill?.RgbColorModelHex?.Val?.Value ?? GetSchemeColor(fill?.SchemeColor, colorDic);
                if (!string.IsNullOrEmpty(bgColor))
                {
                    bgColors.Add(bgColor);
                }

                var textBody = shape.TextBody;
                if (textBody == null) continue;

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
                foreach (Paragraph paragraph in paragraphs)
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
                        var listStyle = placeholderShape?.TextBody?.ListStyle;
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


                    if (isBullet)
                    {
                        /// TODO: Sett i egen funksjon (GetBulletProperties())

                        // Check bullet font
                        var buFont = pProp?.GetFirstChild<BulletFont>()?.Typeface?.Value ?? sldLayoutStyle.buFont ?? sldMasterStyle.buFont;
                        if (!string.IsNullOrEmpty(buFont)) fonts.Add(FontComparison.NormalizeFontName(buFont));

                        // Check bullet color
                        var buColor = pProp?.GetFirstChild<BulletColor>();
                        var buHex = GetBulletColor(buColor, colorDic) ?? sldLayoutStyle.buColor ?? sldMasterStyle.buColor;
                        if (!string.IsNullOrEmpty(buHex)) textColors.Add(buHex);
                    }


                    // Check each run
                    var runs = paragraph?.Descendants<Run>();
                    foreach (var run in runs)
                    {
                        var runProp = run.RunProperties;

                        // Check marking
                        var highlightCol = runProp?.GetFirstChild<Highlight>()?
                            .GetFirstChild<RgbColorModelHex>()?.Val?.Value;
                        if (highlightCol != null) bgColors.Add(highlightCol);

                        if (string.IsNullOrWhiteSpace(run.InnerText)) continue;

                        // Check text color
                        var solidFill = runProp?.GetFirstChild<SolidFill>();
                        var textHex = GetSolidFillColor(solidFill, colorDic);
                        var textColor = textHex ?? sldLayoutStyle.textColor ?? sldMasterStyle.textColor;
                        if (textColor != null) textColors.Add(textColor);


                        // Get fonts
                        (var foreignChars, var classifications) = GetFontClassifications(run.InnerText);

                        if (foreignChars) foreignWriting = true;


                        var rLatinFont = runProp?.GetFirstChild<LatinFont>()?.Typeface?.Value;
                        var rEaFont = runProp?.GetFirstChild<EastAsianFont>()?.Typeface?.Value;
                        var rCsFont = runProp?.GetFirstChild<ComplexScriptFont>()?.Typeface?.Value;

                        foreach (var classification in classifications)
                        {
                            var usedFont = classification switch
                            {
                                "latin" => rLatinFont ?? sldLayoutStyle.latinFont ?? sldMasterStyle.latinFont,
                                "ea" => rEaFont ?? sldLayoutStyle.eaFont ?? sldMasterStyle.eaFont,
                                "cs" => rCsFont ?? sldLayoutStyle.csFont ?? sldMasterStyle.csFont,
                                _ => null,
                            };
                            if (usedFont == null) continue;

                            /// TODO: Sjekk font
                        }
                    }
                }
            }
        }


        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }


    private static (bool foreignChars, HashSet<string> classifications) GetFontClassifications(string txt)
    {
        var foreignChars = FontComparison.IsForeign(txt);

        var classifications = new HashSet<string>();

        return (foreignChars, classifications);
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
            var typeface = buFont.Attributes().FirstOrDefault(a => a.Name.LocalName == "typeface")?.Value;
            if (typeface != null) style.buFont = FontComparison.NormalizeFontName(typeface);
        }

        var defProperties = lvlStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "defRPr");
        if (defProperties == null) return style;

        // Text color
        var solidFill = defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "solidFill");
        var txtHex = GetSolidFillHexXml(solidFill, colorDic);
        if (txtHex != null) style.textColor = txtHex;

        // Fonts
        style.latinFont = XmlHelpers.GetAttributeByLocalName(defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "latin"), "typeface");
        style.eaFont = XmlHelpers.GetAttributeByLocalName(defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "ea"), "typeface");
        style.csFont = XmlHelpers.GetAttributeByLocalName(defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "cs"), "typeface");

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
            if (scheme != null && colorDic.ContainsKey(scheme))
            {
                var baseColor = ColorTranslator.FromHtml("#" + colorDic[scheme]);

                var lumMod = schemeClr.Descendants().FirstOrDefault(e => e.Name.LocalName == "lumMod")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "val")?.Value;
                var lumOff = schemeClr.Descendants().FirstOrDefault(e => e.Name.LocalName == "lumOff")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "val")?.Value;
                var tint = schemeClr.Descendants().FirstOrDefault(e => e.Name.LocalName == "tint")?.Attributes().FirstOrDefault(a => a.Name.LocalName == "val")?.Value;


                var lumModInt = int.Parse(lumMod ?? "0");
                var lumOffInt = int.Parse(lumOff ?? "0");
                var tintInt = int.Parse(tint ?? "0");

                (int r, int g, int b) = AdjustColor((baseColor.R, baseColor.G, baseColor.B), lumModInt, lumOffInt, tintInt);
                hex = FontComparison.GetHex((r, g, b));

                return hex;
            }
        }

        return null;
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
    private static string? GetMappedColor(ColorScheme colorScheme, string? colorIndex)
    {
        const string black = "000000";
        const string white = "FFFFFF";

        if (colorIndex == null) return black;

        return colorIndex switch
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
        if (schemeCol != null)
        {
            var scheme = schemeCol.Val;

            if (colorDic.ContainsKey(scheme))
            {
                var baseColorHex = colorDic[scheme];
                var lumMod = schemeCol.GetFirstChild<LuminanceModulation>()?.Val?.Value;
                var lumOff = schemeCol.GetFirstChild<LuminanceOffset>()?.Val?.Value;
                var tint = schemeCol.GetFirstChild<Tint>()?.Val?.Value;

                var color = ColorTranslator.FromHtml("#" + baseColorHex);

                (int r, int g, int b) = AdjustColor((color.R, color.G, color.B), lumMod, lumOff, tint);
                var hex = FontComparison.GetHex((r, g, b));
                return hex;
            }
        }
        return null;
    }



    /// <summary>
    /// Adjust the luminance and tint of a color
    /// </summary>
    /// <param name="rgb">The RGB values of the color</param>
    /// <param name="lumMod">Luminance modulation</param>
    /// <param name="lumOff">Luminance offset</param>
    /// <param name="tint">Luminance offset</param>
    /// <returns></returns>
    private static (int, int, int) AdjustColor((int r, int g, int b) rgb, int? lumMod, int? lumOff, int? tint)
    {
        float f = 1.0f / 100000.0f; // Factor

        var rgbCol = new Rgb { R = rgb.r, B = rgb.b, G = rgb.g };
        var hslCol = rgbCol.To<ColorMine.ColorSpaces.Hsl>();

        // Apply luminance
        var lum = false;
        if (lumMod is int lMod && lMod != 0)
        {
            hslCol.L *= lMod * f;
            lum = true;
        }
        if (lumOff is int lOff && lOff != 0)
        {
            hslCol.L += lOff * f * 100;
            lum = true;
        }

        // Apply tint
        if (!lum && tint is int tintInt && tintInt != 0)
        {
            // TODO
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

        string? hex = null;

        var rgbCol = bulletColor?.GetFirstChild<RgbColorModelHex>();
        if (rgbCol != null)
        {
            hex = rgbCol.Val;
        }
        else
        {
            var schemeCol = bulletColor?.GetFirstChild<SchemeColor>();

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
