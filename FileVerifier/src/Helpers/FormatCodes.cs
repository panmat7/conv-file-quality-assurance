using System.Collections.Generic;
using System.Linq;

namespace AvaloniaDraft.Helpers;

/// <summary>
/// A helper class containing lists of PRONOM codes for file formats and groups
/// </summary>
public static class FormatCodes
{
    //TEXT DOCUMENTS 
    public static readonly List<string> PronomCodesDOC =
    [
        "fmt/40",
    ];

    public static readonly List<string> PronomCodesDOM =
    [
        "fmt/477"
    ];
    
    public static readonly List<string> PronomCodesDOX =
    [
        "fmt/412"
    ];
    
    public static readonly List<string> PronomCodesDOT =
    [
        "fmt/39"
    ];
    
    public static readonly List<string> PronomCodesDOTM =
    [
        "fmt/478"
    ];
    
    public static readonly List<string> PronomCodesDOTX =
    [
        "fmt/413"
    ];
    
    public static readonly List<string> PronomCodesODT =
    [
        "fmt/139"
    ];
    
    public static readonly List<string> PronomCodesTextDocuments = PronomCodesDOC
        .Concat(PronomCodesDOM)
        .Concat(PronomCodesDOX)
        .Concat(PronomCodesDOT)
        .Concat(PronomCodesDOTM)
        .Concat(PronomCodesDOTX)
        .Concat(PronomCodesODT)
        .ToList();
    
    //PDFs
    public static readonly List<string> PronomCodesPDF =
    [
        "fmt/14", //PDF1.0
        "fmt/15", //PDF1.1
        "fmt/16", //PDF1.2
        "fmt/17", //PDF1.3
        "fmt/18", //PDF1.4
        "fmt/19", //PDF1.5
        "fmt/20", //PDF1.6
        "fmt/276" //PDF1.7
    ];

    public static readonly List<string> PronomCodesPDF1A =
    [
        "fmt/95", //PDF/A 1a
        "fmt/354", //PDF/A 1b
    ];
    
    public static readonly List<string> PronomCodesPDF2A =
    [
        "fmt/95", //PDF/A 1a
        "fmt/354", //PDF/A 1b
    ];
    
    public static readonly List<string> PronomCodesPDF3A =
    [
        "fmt/476", //PDF/A 2a
        "fmt/477", //PDF/A 2b
        "fmt/478", //PDF/A 2u
    ];
    
    public static readonly List<string> PronomCodesPDF4A =
    [
        "fmt/479", //PDF/A 3a
        "fmt/480", //PDF/A 3b
        "fmt/481", //PDF/A 3u
    ];
    
    public static readonly List<string> PronomCodesPDFA = PronomCodesPDF1A
        .Concat(PronomCodesPDF2A)
        .Concat(PronomCodesPDF3A)
        .Concat(PronomCodesPDF4A)
        .ToList();
    
    //PRESENTATIONS
    public static readonly List<string> PronomCodesPPT = 
    [
        "fmt/126"
    ];
    
    public static readonly List<string> PronomCodesPPTM = 
    [
        "fmt/479",
        "fmt/487"
    ];
    
    public static readonly List<string> PronomCodesPPTX = 
    [
        
        "fmt/215"
    ];
    
    public static readonly List<string> PronomCodesODP = 
    [
        "fmt/139"
    ];
    
    public static readonly List<string> PronomCodesPOT =
    [
        "fmt/126"
    ];
    
    public static readonly List<string> PronomCodesPPS =
    [
        "fmt/126"
    ];
    
    public static readonly List<string> PronomCodesPPSX =
    [
        "fmt/629"
    ];
    
    public static readonly List<string> PronomCodesPPSM =
    [
        "fmt/630"
    ];
    
    public static readonly List<string> PronomCodesPOTX =
    [
        "fmt/631"
    ];
    
    public static readonly List<string> PronomCodesPOTM =
    [
        "fmt/633"
    ];
    
    public static readonly List<string> PronomCodesXML =
    [
        "fmt/101"
    ];

    public static readonly List<string> PronomCodesXMLBasedPowerPoint = PronomCodesPPTX
        .Concat(PronomCodesPPSX)
        .Concat(PronomCodesPOTX)
        .ToList();
    
    public static readonly List<string> PronomCodesPresentationDocuments = PronomCodesPPT
        .Concat(PronomCodesPPTM)
        .Concat(PronomCodesPPTX)
        .Concat(PronomCodesODP)
        .Concat(PronomCodesPOT)
        .Concat(PronomCodesPPS)
        .Concat(PronomCodesPOTM)
        .Concat(PronomCodesPOTX)
        .Concat(PronomCodesPPSX)
        .Concat(PronomCodesPPSM)
        .Concat(PronomCodesXML)
        .ToList();
    
    //SPREADSHEETS
    public static readonly List<string> PronomCodesXLS = 
    [
        "fmt/61"
    ];
    
    public static readonly List<string> PronomCodesXLSM = 
    [
        "fmt/475"
    ];
    
    public static readonly List<string> PronomCodesXLSX = 
    [
        "fmt/214"
    ];
    
    public static readonly List<string> PronomCodesODS = 
    [
        "fmt/142"
    ];
    
    public static readonly List<string> PronomCodesCSV = 
    [
        "fmt/78"
    ];
    
    public static readonly List<string> PronomCodesSpreadsheets = PronomCodesXLS
        .Concat(PronomCodesXLSM)
        .Concat(PronomCodesXLSX)
        .Concat(PronomCodesODS)
        .Concat(PronomCodesCSV)
        .ToList();
    
    //IMAGES
    public static readonly List<string> PronomCodesPNG = 
    [
        "fmt/11",
        "fmt/12"
    ];
    
    public static readonly List<string> PronomCodesJPEG = 
    [
        "fmt/41",
        "fmt/43"
    ];
    
    public static readonly List<string> PronomCodesGIF = 
    [
        "fmt/4"
    ];
    
    public static readonly List<string> PronomCodesImages = PronomCodesPNG
        .Concat(PronomCodesJPEG)
        .Concat(PronomCodesGIF)
        .ToList();
}