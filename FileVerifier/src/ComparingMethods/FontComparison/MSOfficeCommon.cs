using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AvaloniaDraft.ComparingMethods;

public static class MSOffice
{
    public static string? GetOfficeColorFromName(string colorName)
    {
        return colorName switch
        {
            "yellow" => "FFFF00",
            "green" => "00FF00",
            "cyan" => "00FFFF",
            "magenta" => "FF00FF",
            "blue" => "0000FF",
            "red" => "FF0000",
            "darkBlue" => "000080",
            "darkCyan" => "008080",
            "darkGreen" => "008000",
            "darkMagenta" => "800080",
            "darkRed" => "800000",
            "darkYellow" => "808000",
            "darkGray" => "808080",
            "ligthGray" => "C0C0C0",
            "black" => "000000",
            _ => null,
        };
    }


    public static (Dictionary<string, string> major, Dictionary<string, string> minor)? GetDefaultFontsFromScheme(XElement fontScheme)
    {
        var majorFontsXml = XmlHelpers.GetFirsElementByLocalName(fontScheme, "majorFont");
        var minorFontsXml = XmlHelpers.GetFirsElementByLocalName(fontScheme, "minorFont");

        if (majorFontsXml == null || minorFontsXml == null) return null;

        return (GetDefaultFonts(majorFontsXml), (GetDefaultFonts(minorFontsXml)));
    }


    public static Dictionary<string, string> GetDefaultFonts(XElement fontList)
    {
        var dic = new Dictionary<string, string>();

        foreach (var font in fontList.Descendants())
        {
            (string? key, string? value) = font.Name.LocalName switch
            {
                "latin" => ("Latin", XmlHelpers.GetAttributeByLocalName(font, "typeface")),
                "ea" => ("EastAsia", XmlHelpers. GetAttributeByLocalName(font, "typeface")),
                "cs" => ("ComplexScript", XmlHelpers.GetAttributeByLocalName(font, "typeface")),

                "font" => (XmlHelpers.GetAttributeByLocalName(font, "script"), XmlHelpers.GetAttributeByLocalName(font, "typeface")),

                _ => (null, null),
            };

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value)) dic[key] = value;
        }

        // Make sure to contain defaults
        if (!dic.ContainsKey("Latin")) dic.Add("Latin", "");
        if (!dic.ContainsKey("EastAsia")) dic.Add("EastAsia", "");
        if (!dic.ContainsKey("ComplexScript")) dic.Add("ComplexScript", "");

        return dic;
    }
}

