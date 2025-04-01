using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtfDomParser;

namespace AvaloniaDraft.ComparingMethods;

public static class RtfFontExtraction
{

    /// <summary>
    /// Get the font information of a RTF file
    /// </summary>
    /// <param name="src"></param>
    /// <returns></returns>
    public static TextInfo? GetTextInfoRTF(string src)
    {
        var foreignWriting = false;
        var fonts = new HashSet<string>();
        var altFonts = new HashSet<HashSet<string>>();
        var textColors = new HashSet<string>();
        var bgColors = new HashSet<string>();

        var doc = new RTFDomDocument();
        doc.Load(src);

        foreach (var e in doc.Elements)
        {
            if (e is RTFDomParagraph p)
            {
                // var pColorHex = FontComparison.GetHex(p.Format.BackColor);

                foreach (var r in p.Elements)
                {

                    if (r is RTFDomText txt)
                    {
                        var fontName = FontComparison.NormalizeFontName(txt.Format.FontName);
                        var textHex = FontComparison.GetHex(txt.Format.TextColor);
                        var bgHex = FontComparison.GetHex(txt.Format.BackColor);

                        if (!foreignWriting && FontComparison.IsForeign(txt.Text)) foreignWriting = true;

                        fonts.Add(fontName);
                        textColors.Add(textHex);
                        bgColors.Add(bgHex);
                    }
                    else if (r is RTFDomField f)
                    {
                        foreach (var fieldElement in f.Elements)
                        {
                            if (fieldElement is RTFDomElementContainer ec)
                            {
                                foreach (var el in ec.Elements)
                                {
                                    if (el is RTFDomText fTxt)
                                    {
                                        var fontName = FontComparison.NormalizeFontName(fTxt.Format.FontName);
                                        var textHex = FontComparison.GetHex(fTxt.Format.TextColor);
                                        var bgHex = FontComparison.GetHex(fTxt.Format.BackColor);

                                        if (!foreignWriting && FontComparison.IsForeign(fTxt.Text)) foreignWriting = true;

                                        fonts.Add(fontName);
                                        textColors.Add(textHex);
                                        bgColors.Add(bgHex);
                                    }
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
}
