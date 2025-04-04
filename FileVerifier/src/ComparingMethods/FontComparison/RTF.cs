using System;
using System.Collections.Generic;
using System.Globalization;
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
        var textInfo = new TextInfo();

        var doc = new RTFDomDocument();
        doc.Load(src);

        foreach (var p in doc.Elements.OfType<RTFDomParagraph>())
        {
            foreach (var r in p.Elements)
            {
                if (r is RTFDomText txt)
                {
                    CheckText(txt, textInfo);
                }
                else if (r is RTFDomField f)
                {
                    var texts = f.Elements.OfType<RTFDomElementContainer>().SelectMany(ec => f.Elements.OfType<RTFDomText>());
                    foreach (var fTxt in texts)
                    {
                        CheckText(fTxt, textInfo);
                    }
                }
            }
        }

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
    private static void CheckText(RTFDomText txt, TextInfo textInfo)
    {
        var fontName = FontComparison.NormalizeFontName(txt.Format.FontName);
        var textHex = FontComparison.GetHex(txt.Format.TextColor);
        var bgHex = FontComparison.GetHex(txt.Format.BackColor);

        if (!textInfo.ForeignWriting && FontComparison.IsForeign(txt.Text)) textInfo.ForeignWriting = true;

        textInfo.Fonts.Add(fontName);
        textInfo.TextColors.Add(textHex);
        textInfo.BgColors.Add(bgHex);
    }
}
