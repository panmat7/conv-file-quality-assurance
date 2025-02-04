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
        "fmt/39", //Word 6.0/95
        "fmt/40", //Word 97-2003
    ];

    public static readonly List<string> PronomCodesDOCM =
    [
        
    ];
    
    public static readonly List<string> PronomCodesDOCX =
    [
        "fmt/412" //Word for Windows 2007 onwards
    ];
    
    public static readonly List<string> PronomCodesDOT =
    [
        
    ];
    
    public static readonly List<string> PronomCodesDOTM =
    [
        
    ];
    
    public static readonly List<string> PronomCodesDOTX =
    [
        
    ];
    
    public static readonly List<string> PronomCodesODT =
    [
        "fmt/290",  //OpenDocument Text 1.1
        "fmt/291",  //OpenDocument Text 1.2
        "fmt/1756", //OpenDocument Text 1.3
    ];
    
    public static readonly List<string> PronomCodesTextDocuments = PronomCodesDOC
        .Concat(PronomCodesDOCM)
        .Concat(PronomCodesDOCX)
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
        "fmt/125", //PowerPoint Presentation 95
        "fmt/126", //PowerPoint Presentation 97-2003
    ];
    
    public static readonly List<string> PronomCodesPPTM = 
    [
        
    ];
    
    public static readonly List<string> PronomCodesPPTX = 
    [
        "fmt/215" //PowerPoint for Windows 2007 onwards
    ];
    
    public static readonly List<string> PronomCodesODP = 
    [
        "fmt/138", //OpenDocument Presentation 1.0
        "fmt/292", //OpenDocument Presentation 1.1
        "fmt/293", //OpenDocument Presentation 1.2
        "fmt/1754" //OpenDocument Presentation 1.3
    ];
    
    public static readonly List<string> PronomCodesPresentationDocuments = PronomCodesPPT
        .Concat(PronomCodesPPTM)
        .Concat(PronomCodesPPTX)
        .Concat(PronomCodesODP)
        .ToList();
    
    //SPREADSHEETS
    public static readonly List<string> PronomCodesXLS = 
    [
        "fmt/61", //Excel 97 Workbook (xls) 8
        "fmt/62", //Excel 2000-2003 Workbook (xls) 8X
    ];
    
    public static readonly List<string> PronomCodesXLSM = 
    [
        
    ];
    
    public static readonly List<string> PronomCodesXLSX = 
    [
        
    ];
    
    public static readonly List<string> PronomCodesODS = 
    [
        "fmt/137", //OpenDocument Spreadsheet 1.0
        "fmt/294", //OpenDocument Spreadsheet 1.1
        "fmt/295", //OpenDocument Spreadsheet 1.2
        "fmt/1755" //OpenDocument Spreadsheet 1.3
    ];
    
    public static readonly List<string> PronomCodesCSV = 
    [
        "fmt/800", //CSV Schema
        
        "x-fmt/18" //Comma Separated Values 
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
        "fmt/11", //PNG 1.0
        "fmt/12", //PNG 1.1
        "fmt/13"  //PNG 1.2
    ];
    
    public static readonly List<string> PronomCodesJPEG = 
    [
        "fmt/41", //Raw JPEG
        "fmt/42", //JPEG 1.00
        "fmt/43", //JPEG 1.01
        "fmt/44", //JPEG 1.02
    ];
    
    public static readonly List<string> PronomCodesGIF = 
    [
        "fmt/3", //GIF 87a
        "fmt/4"  //GIF 89a
    ];
    
    public static readonly List<string> PronomCodesImages = PronomCodesPNG
        .Concat(PronomCodesJPEG)
        .Concat(PronomCodesGIF)
        .ToList();
}