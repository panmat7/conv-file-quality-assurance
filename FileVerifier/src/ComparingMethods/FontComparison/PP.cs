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
    private struct DefaultStyle
    {
        public string? font;
        public string? textColor;
        public string? buColor;
        public string? buFont;

        public DefaultStyle()
        {
            font = null;
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
        if (slideParts != null)
        {
            // Go through each slide
            foreach (var slidePart in slideParts)
            {
                var slideMasterPart = GetSlideMasterPart(slidePart);
                if (slideMasterPart == null) return null;

                var slideLayout = slidePart.SlideLayoutPart?.SlideLayout?.CommonSlideData;

                // Get a color dictionary for the slide master theme colors
                var colorDic = GetSlideColors(slideMasterPart);

                // Get the theme
                var themePart = slideMasterPart?.ThemePart;

                // Check the default fonts
                var fontScheme = themePart?.Theme?.ThemeElements?.FontScheme;
                if (fontScheme == null) return null;
                var majFont = fontScheme.MajorFont?.LatinFont?.Typeface?.Value;
                var minFont = fontScheme.MinorFont?.LatinFont?.Typeface?.Value;
                if (majFont == null || minFont == null) return null;

                majFont = FontComparison.NormalizeFontName(majFont);
                minFont = FontComparison.NormalizeFontName(minFont);


                /// TODO: ADD A MAP FOR ALL DEFAULT FONTS OF ALL LANGUAGES


                // Go through each shape/element
                var shapes = slidePart.Slide.Descendants<PP.Shape>();
                foreach (PP.Shape shape in shapes)
                {
                    // Get fill if present
                    var fill = shape.ShapeProperties?.GetFirstChild<SolidFill>();
                    if (fill != null)
                    {
                        string? hex = "";

                        var rgbCol = fill.RgbColorModelHex;
                        var schemeCol = fill.SchemeColor;
                        if (rgbCol != null)
                        {
                            hex = rgbCol?.Val?.Value;
                        }
                        else
                        {
                            hex = GetSchemeColor(schemeCol, colorDic);
                        }

                        if (!string.IsNullOrEmpty(hex))
                        {
                            bgColors.Add(hex);
                        }
                    }

                    var text = shape.TextBody;
                    if (text == null) continue;

                    bool isList = (text.ListStyle != null);

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
                    var paragraphs = text.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>();
                    foreach (var paragraph in paragraphs)
                    {
                        var pProp = paragraph?.ParagraphProperties;
                        int? lvl = pProp?.Level?.Value;

                        bool hasLevel = lvl != null;
                        bool phHasNonBullet = placeholderShape?.TextBody?.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>().Any(p => p.ParagraphProperties?.Level is null) ?? true;
                        bool isExplicitlyNotBullet = paragraph.Descendants<NoBullet>().Any();
                        bool hasExplicitBullet = paragraph.Descendants<CharacterBullet>().Any() ||
                            paragraph.Descendants<AutoNumberedBullet>().Any() || paragraph.Descendants<BulletFont>().Any();

                        bool isBullet = (hasExplicitBullet || !phHasNonBullet || hasLevel) && !isExplicitlyNotBullet;

                        // Check the style properties for the placeholder shape
                        DefaultStyle dShapeStyleSldLayout;
                        if (placeholderShape != null)
                        {
                            var listStyle = placeholderShape?.TextBody?.ListStyle;
                            var listStyleXml = XElement.Parse(listStyle?.OuterXml ?? "");
                            dShapeStyleSldLayout = GetDefaultStyle(listStyleXml, colorDic, minFont, majFont, null, lvl);
                        }
                        else
                        {
                            dShapeStyleSldLayout = new DefaultStyle();
                        }

                        // Check the slide master for default colors
                        DefaultStyle dShapeStyleSldMaster;
                        if (phType == PP.PlaceholderValues.Title || phType == PP.PlaceholderValues.CenteredTitle)
                        {
                            var titleStyle = slideMasterPart?.SlideMaster.Descendants<PP.TitleStyle>().FirstOrDefault();
                            var titleStyleXml = XElement.Parse(titleStyle?.OuterXml ?? "");
                            dShapeStyleSldMaster = GetDefaultStyle(titleStyleXml, colorDic, minFont, majFont, majFont, lvl);
                        }
                        else
                        {
                            var bodyStyle = slideMasterPart?.SlideMaster.Descendants<PP.BodyStyle>().FirstOrDefault();
                            var bodyStyleXml = XElement.Parse(bodyStyle?.OuterXml ?? "");
                            dShapeStyleSldMaster = GetDefaultStyle(bodyStyleXml, colorDic, minFont, majFont, minFont, lvl);
                        }


                        if (isBullet)
                        {
                            // Check bullet font
                            var buFont = pProp?.GetFirstChild<BulletFont>()?.Typeface?.Value;
                            if (!string.IsNullOrEmpty(buFont))
                            {
                                fonts.Add(FontComparison.NormalizeFontName(buFont));
                            }
                            else
                            {
                                if (dShapeStyleSldLayout.buFont != null)
                                {
                                    fonts.Add(dShapeStyleSldLayout.buFont);
                                }
                                else if (dShapeStyleSldMaster.buFont != null)
                                {

                                    fonts.Add(dShapeStyleSldMaster.buFont);
                                }
                            }

                            // Check bullet color
                            var buColor = pProp?.GetFirstChild<BulletColor>();
                            var buHex = GetBulletColor(buColor, colorDic);
                            if (!string.IsNullOrEmpty(buHex))
                            {
                                textColors.Add(buHex);
                            }
                            else
                            {
                                if (dShapeStyleSldLayout.buColor != null)
                                {
                                    textColors.Add(dShapeStyleSldLayout.buColor);
                                }
                                else if (dShapeStyleSldMaster.buColor != null)
                                {
                                    textColors.Add(dShapeStyleSldMaster.buColor);
                                }
                            }
                        }


                        // Check each run
                        var runs = paragraph?.Descendants<DocumentFormat.OpenXml.Drawing.Run>();
                        foreach (var run in runs)
                        {
                            var runProp = run.RunProperties;

                            // Check marking
                            var highlightCol = runProp?.GetFirstChild<DocumentFormat.OpenXml.Drawing.Highlight>()?
                                .GetFirstChild<DocumentFormat.OpenXml.Drawing.RgbColorModelHex>()?.Val?.Value;
                            if (highlightCol != null)
                            {
                                bgColors.Add(highlightCol);
                            }

                            if (string.IsNullOrWhiteSpace(run.InnerText)) continue;

                            // Get font
                            var font = runProp?.GetFirstChild<LatinFont>();
                            if (font != null)
                            {
                                var typeface = font.Typeface;
                                if (typeface != null)
                                {
                                    var tf = typeface.Value;
                                    if (!string.IsNullOrEmpty(tf))
                                    {
                                        if (tf[0] == '+') // Default font
                                        {
                                            if (tf == "+mj-lt") // Major font
                                            {
                                                fonts.Add(majFont);
                                            }
                                            else if (tf == "+mn-lt") // Minor font
                                            {
                                                fonts.Add(minFont);
                                            }
                                        }
                                        else
                                        {
                                            fonts.Add(FontComparison.NormalizeFontName(typeface));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (dShapeStyleSldLayout.font != null)
                                {
                                    fonts.Add(dShapeStyleSldLayout.font);
                                }
                                else if (dShapeStyleSldMaster.font != null)
                                {
                                    fonts.Add(dShapeStyleSldMaster.font);
                                }
                            }


                            // Check for foreign characters
                            if (!foreignWriting && FontComparison.IsForeign(run.InnerText)) foreignWriting = true;


                            // Check text color
                            var solidFill = runProp?.GetFirstChild<SolidFill>();
                            var hex = GetSolidFillColor(solidFill, colorDic);
                            if (hex != null && hex != "")
                            {
                                textColors.Add(hex);
                            }
                            else
                            {
                                if (dShapeStyleSldLayout.textColor != null)
                                {
                                    textColors.Add(dShapeStyleSldLayout.textColor);
                                }
                                else if (dShapeStyleSldMaster.textColor != null)
                                {
                                    textColors.Add(dShapeStyleSldMaster.textColor);
                                }
                            }
                        }
                    }
                }
            }
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
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
    private static DefaultStyle GetDefaultStyle(XElement? style, Dictionary<string, string> colorDic, string dMinFont, string dMajFont, string? dFont, int? level)
    {
        var dStyle = new DefaultStyle();
        if (style == null) return dStyle;

        XElement? lvlStyle;
        if (level != null)
        {
            lvlStyle = style.Descendants().FirstOrDefault(e => e.Name.LocalName == $"lvl{level}pPr");
        }
        else
        {
            lvlStyle = style.Descendants().FirstOrDefault(e => e.Name.LocalName == $"dfRPr");
            if (lvlStyle == null) lvlStyle = style.Descendants().FirstOrDefault(e => e.Name.LocalName == "lvl1pPr");
        }
        if (lvlStyle == null) return dStyle;


        // Bullet color
        var buClr = lvlStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "buClr");
        if (buClr != null)
        {
            var hex = GetSolidFillHexXml(buClr, colorDic);
            if (hex != null) dStyle.buColor = hex;

        }

        // Bullet font
        var buFont = lvlStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "buFont");
        if (buFont != null)
        {
            var typeface = buFont.Attributes().FirstOrDefault(a => a.Name.LocalName == "typeface")?.Value;
            if (typeface != null) dStyle.buFont = FontComparison.NormalizeFontName(typeface);
        }

        var defProperties = lvlStyle.Descendants().FirstOrDefault(e => e.Name.LocalName == "defRPr");
        if (defProperties == null) return dStyle;

        // Text color
        var solidFill = defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "solidFill");
        if (solidFill != null)
        {
            var hex = GetSolidFillHexXml(solidFill, colorDic);
            if (hex != null) dStyle.textColor = hex;
        }

        // Font
        var latinTypeface = defProperties.Descendants().FirstOrDefault(e => e.Name.LocalName == "latin");
        if (latinTypeface != null)
        {
            var typeface = latinTypeface.Attributes().FirstOrDefault(a => a.Name.LocalName == "typeface")?.Value;
            if (typeface != null)
            {
                if (typeface[0] == '+')
                {
                    if (typeface == "+mj-lt")
                    {
                        dStyle.font = dMajFont;
                    }
                    else if (typeface == "+mn-lt")
                    {
                        dStyle.font = dMinFont;
                    }
                }
                else
                {
                    dStyle.font = FontComparison.NormalizeFontName(typeface);
                }
            }
        }
        else if (dFont != null)
        {
            dStyle.font = dFont;
        }


        return dStyle;
    }


    /// <summary>
    /// Get the the hex value of a solid fill XML element
    /// </summary>
    /// <param name="solidFill"></param>
    /// <param name="colorDic"></param>
    /// <returns></returns>
    private static string? GetSolidFillHexXml(XElement solidFill, Dictionary<string, string> colorDic)
    {

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
    private static Dictionary<string, string> GetSlideColors(SlideMasterPart slideMasterPart)
    {
        var colorDic = new Dictionary<string, string>();


        var colorMap = slideMasterPart.SlideMaster.ColorMap;
        if (colorMap == null) return colorDic;

        var themePart = slideMasterPart.ThemePart;
        var colorScheme = themePart?.Theme?.ThemeElements?.ColorScheme;
        if (colorScheme == null) return null;

        colorDic["accent1"] = GetMappedColor(colorScheme, colorMap.Accent1);
        colorDic["accent2"] = GetMappedColor(colorScheme, colorMap.Accent2);
        colorDic["accent3"] = GetMappedColor(colorScheme, colorMap.Accent3);
        colorDic["accent4"] = GetMappedColor(colorScheme, colorMap.Accent4);
        colorDic["accent5"] = GetMappedColor(colorScheme, colorMap.Accent5);
        colorDic["accent6"] = GetMappedColor(colorScheme, colorMap.Accent6);
        colorDic["tx1"] = GetMappedColor(colorScheme, colorMap.Text1);
        colorDic["tx2"] = GetMappedColor(colorScheme, colorMap.Text2);
        colorDic["bg1"] = GetMappedColor(colorScheme, colorMap.Background1);
        colorDic["bg2"] = GetMappedColor(colorScheme, colorMap.Background2);
        colorDic["hlink"] = GetMappedColor(colorScheme, colorMap.Hyperlink);
        colorDic["folHlink"] = GetMappedColor(colorScheme, colorMap.FollowedHyperlink);

        return colorDic;
    }


    /// <summary>
    /// Get the correctly mapped color
    /// </summary>
    /// <param name="colorScheme"></param>
    /// <param name="colorIndex"></param>
    /// <returns></returns>
    private static string GetMappedColor(ColorScheme colorScheme, string? colorIndex)
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
