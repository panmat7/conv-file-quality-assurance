using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AvaloniaDraft.Helpers;

/**********************
 * !!!README!!!
 * 
 * Det her er vanskelig å sikre at disse er korrekte uten å legge de til manuelt.
 * Stol kun på koder med komentar på siden, andre er ikke dobbeltsjekket
 * Hvis du skal legge til flere pls legg til en kommentar med faktiske format navn, som man kan finne her
 * https://www.nationalarchives.gov.uk/PRONOM/PUID/proPUIDSearch.aspx?status=new
 *
 * !!!README!!!
 **********************/


public static class FileExtensions
{
    public static readonly ImmutableList<string> list =
    [
        "doc",
        "docm",
        "docx",
        "dot",
        "dotm",
        "dotx",
        "odt",
        "pdf",
        "pdf1a",
        "pdf2a",
        "pdf3a",
        "pdf4a",
        "ppt",
        "pptm",
        "pptx",
        "odp",
        "pot",
        "pps",
        "ppsx",
        "ppsm",
        "potx",
        "potm",
        "xml",
        "xls",
        "xlsm",
        "xlsx",
        "ods",
        "csv",
        "png",
        "jpeg",
        "gif"
    ];
}

/// <summary>
/// A helper class containing lists of PRONOM codes for file formats and groups
/// </summary>
public static class FormatCodes
{
    //TEXT DOCUMENTS 
    public static readonly ImmutableList<string> PronomCodesDOC =
    [
        "fmt/39", //Word Document 6.0/95
        "fmt/40", //Word Document 97-2003
    ];

    public static readonly ImmutableList<string> PronomCodesDOCM =
    [
        
    ];
    
    public static readonly ImmutableList<string> PronomCodesDOCX =
    [
        "fmt/412", //Word for Windows 2007 onwards
        "fmt/413",
    ];
    
    public static readonly ImmutableList<string> PronomCodesDOT =
    [
        
    ];
    
    public static readonly ImmutableList<string> PronomCodesDOTM =
    [
        
    ];
    
    public static readonly ImmutableList<string> PronomCodesDOTX =
    [
        
    ];
    
    public static readonly ImmutableList<string> PronomCodesODT =
    [
        "fmt/290",  //OpenDocument Text 1.1
        "fmt/291",  //OpenDocument Text 1.2
        "fmt/1756", //OpenDocument Text 1.3
    ];
    
    public static readonly ImmutableList<string> PronomCodesTextDocuments = PronomCodesDOC
        .Concat(PronomCodesDOCM)
        .Concat(PronomCodesDOCX)
        .Concat(PronomCodesDOT)
        .Concat(PronomCodesDOTM)
        .Concat(PronomCodesDOTX)
        .Concat(PronomCodesODT)
        .ToImmutableList();
    
    //PDFs
    public static readonly ImmutableList<string> PronomCodesPDF =
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

    public static readonly ImmutableList<string> PronomCodesPDF1A =
    [
        "fmt/95", //PDF/A 1a
        "fmt/354", //PDF/A 1b
    ];
    
    public static readonly ImmutableList<string> PronomCodesPDF2A =
    [
        "fmt/95", //PDF/A 1a
        "fmt/354", //PDF/A 1b
    ];
    
    public static readonly ImmutableList<string> PronomCodesPDF3A =
    [
        "fmt/476", //PDF/A 2a
        "fmt/477", //PDF/A 2b
        "fmt/478", //PDF/A 2u
    ];
    
    public static readonly ImmutableList<string> PronomCodesPDF4A =
    [
        "fmt/479", //PDF/A 3a
        "fmt/480", //PDF/A 3b
        "fmt/481", //PDF/A 3u
    ];
    
    public static readonly ImmutableList<string> PronomCodesPDFA = PronomCodesPDF1A
        .Concat(PronomCodesPDF2A)
        .Concat(PronomCodesPDF3A)
        .Concat(PronomCodesPDF4A)
        .ToImmutableList();
    
    //PRESENTATIONS
    public static readonly ImmutableList<string> PronomCodesPPT = 
    [
        "fmt/125", //PowerPoint Presentation 95
        "fmt/126", //PowerPoint Presentation 97-2003
    ];
    
    public static readonly ImmutableList<string> PronomCodesPPTM = 
    [
        "fmt/479",
        "fmt/487"
    ];
    
    public static readonly ImmutableList<string> PronomCodesPPTX = 
    [
        "fmt/215" //PowerPoint for Windows 2007 onwards
    ];
    
    public static readonly ImmutableList<string> PronomCodesODP = 
    [
        "fmt/138", //OpenDocument Presentation 1.0
        "fmt/292", //OpenDocument Presentation 1.1
        "fmt/293", //OpenDocument Presentation 1.2
        "fmt/1754" //OpenDocument Presentation 1.3
    ];
    
    public static readonly ImmutableList<string> PronomCodesPOT =
    [
        "fmt/126"
    ];
    
    public static readonly ImmutableList<string> PronomCodesPPS =
    [
        "fmt/126"
    ];
    
    public static readonly ImmutableList<string> PronomCodesPPSX =
    [
        "fmt/629"
    ];
    
    public static readonly ImmutableList<string> PronomCodesPPSM =
    [
        "fmt/630"
    ];
    
    public static readonly ImmutableList<string> PronomCodesPOTX =
    [
        "fmt/631"
    ];
    
    public static readonly ImmutableList<string> PronomCodesPOTM =
    [
        "fmt/633"
    ];
    
    public static readonly ImmutableList<string> PronomCodesXML =
    [
        "fmt/101"
    ];

    public static readonly ImmutableList<string> PronomCodesXMLBasedPowerPoint = PronomCodesPPTX
        .Concat(PronomCodesPPSX)
        .Concat(PronomCodesPOTX)
        .ToImmutableList();
    
    public static readonly ImmutableList<string> PronomCodesPresentationDocuments = PronomCodesPPT
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
        .ToImmutableList();
    
    //SPREADSHEETS
    public static readonly ImmutableList<string> PronomCodesXLS = 
    [
        "fmt/61", //Excel 97 Workbook (xls) 8
        "fmt/62", //Excel 2000-2003 Workbook (xls) 8X
    ];
    
    public static readonly ImmutableList<string> PronomCodesXLSM = 
    [
        
    ];
    
    public static readonly ImmutableList<string> PronomCodesXLSX = 
    [
        "fmt/214"
    ];
    
    public static readonly ImmutableList<string> PronomCodesODS = 
    [
        "fmt/137", //OpenDocument Spreadsheet 1.0
        "fmt/294", //OpenDocument Spreadsheet 1.1
        "fmt/295", //OpenDocument Spreadsheet 1.2
        "fmt/1755" //OpenDocument Spreadsheet 1.3
    ];
    
    public static readonly ImmutableList<string> PronomCodesCSV = 
    [
        "fmt/800", //CSV Schema
        
        "x-fmt/18" //Comma Separated Values 
    ];
    
    public static readonly ImmutableList<string> PronomCodesSpreadsheets = PronomCodesXLS
        .Concat(PronomCodesXLSM)
        .Concat(PronomCodesXLSX)
        .Concat(PronomCodesODS)
        .Concat(PronomCodesCSV)
        .ToImmutableList();
    
    //IMAGES
    public static readonly ImmutableList<string> PronomCodesPNG = 
    [
        "fmt/11", //PNG 1.0
        "fmt/12", //PNG 1.1
        "fmt/13"  //PNG 1.2
    ];
    
    public static readonly ImmutableList<string> PronomCodesJPEG = 
    [
        "fmt/41", //Raw JPEG
        "fmt/42", //JPEG 1.00
        "fmt/43", //JPEG 1.01
        "fmt/44", //JPEG 1.02
    ];
    
    public static readonly ImmutableList<string> PronomCodesGIF = 
    [
        "fmt/3", //GIF 87a
        "fmt/4"  //GIF 89a
    ];
    
    public static readonly ImmutableList<string> PronomCodesImages = PronomCodesPNG
        .Concat(PronomCodesJPEG)
        .Concat(PronomCodesGIF)
        .ToImmutableList();
}