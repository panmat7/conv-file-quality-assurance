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

        foreach (var p in doc.Elements.OfType<RTFDomParagraph>())
        {
            foreach (var r in p.Elements)
            {
                if (r is RTFDomText txt)
                {
                    CheckText(txt, fonts, textColors, bgColors, ref foreignWriting);
                }
                else if (r is RTFDomField f)
                {
                    var texts = f.Elements.OfType<RTFDomElementContainer>().SelectMany(ec => f.Elements.OfType<RTFDomText>());
                    foreach (var fTxt in texts)
                    {
                        CheckText(fTxt, fonts, textColors, bgColors, ref foreignWriting);
                    }
                }
            }
        }

        var textInfo = new TextInfo(fonts, textColors, bgColors, altFonts, foreignWriting);
        return textInfo;
    }


    /// <summary>
    /// Add formatting of RTF text
    /// </summary>
    /// <param name="txt"></param>
    /// <param name="fonts"></param>
    /// <param name="textColors"></param>
    /// <param name="bgColors"></param>
    /// <param name="foreignWriting"></param>
    private static void CheckText(RTFDomText txt, HashSet<string> fonts, HashSet<string> textColors, HashSet<string> bgColors, ref bool foreignWriting)
    {
        var fontName = FontComparison.NormalizeFontName(txt.Format.FontName);
        var textHex = FontComparison.GetHex(txt.Format.TextColor);
        var bgHex = FontComparison.GetHex(txt.Format.BackColor);

        if (!foreignWriting && FontComparison.IsForeign(txt.Text)) foreignWriting = true;

        fonts.Add(fontName);
        textColors.Add(textHex);
        bgColors.Add(bgHex);
    }
}
