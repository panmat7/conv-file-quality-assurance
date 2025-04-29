using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AvaloniaDraft.ComparingMethods;

public static class XmlHelpers
{
    public static string? GetAttributeByLocalName(XElement? element, string localName)
    {
        if (element == null) return null;
        return element.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value;
    }

    public static XElement? GetFirsElementByLocalName(XElement element, string localName)
    {
        return element.Elements().FirstOrDefault(e => e.Name.LocalName == localName);
    }
}
