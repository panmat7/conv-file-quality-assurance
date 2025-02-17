using System.Collections.Immutable;
using System.Linq;

namespace AvaloniaDraft.Helpers;

/// <summary>
/// Stores file format codes and pronom codes
/// </summary>
public class FileFormat
{
    private ImmutableList<string> FormatCodes { get; }
    private ImmutableList<string> PronomCodes { get; }

    public FileFormat(ImmutableList<string> formatCodes, ImmutableList<string> pronomCodes)
    {
        FormatCodes = formatCodes;
        PronomCodes = pronomCodes;
    }

    public FileFormat(FileFormat[] fileFormats)
    {
        FormatCodes = fileFormats.SelectMany(f => f.FormatCodes).Distinct().ToImmutableList();
        PronomCodes = fileFormats.SelectMany(f => f.PronomCodes).Distinct().ToImmutableList();
    }
    
    /// <summary>
    /// Checks if a pronom code is contained in PronomCodes
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public bool Contains(string code) => FormatCodes.Contains(code);
}

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

/// <summary>
/// A helper class containing lists of PRONOM codes for file formats and groups
/// </summary>
public static class FormatCodes
{
    //TEXT DOCUMENTS 
    public static readonly FileFormat PronomCodesDOC = new FileFormat(["doc"], [
        "fmt/39", //Word Document 6.0/95
        "fmt/40", //Word Document 97-2003
    ]);


    public static readonly FileFormat PronomCodesDOCM = new FileFormat(["docm"], [

    ]);

    public static readonly FileFormat PronomCodesDOCX = new FileFormat(["docx"], [
        "fmt/412", //Word for Windows 2007 onwards
        "fmt/413",
    ]);


    public static readonly FileFormat PronomCodesDOT = new FileFormat(["dot"], [

    ]);

    public static readonly FileFormat PronomCodesDOTM = new FileFormat(["dotm"], [

    ]);

    public static readonly FileFormat PronomCodesDOTX = new FileFormat(["dotx"], [

    ]);

    public static readonly FileFormat PronomCodesODT = new FileFormat(["odt"], [
        "fmt/290", //OpenDocument Text 1.1
        "fmt/291", //OpenDocument Text 1.2
        "fmt/1756", //OpenDocument Text 1.3
    ]);

    public static readonly FileFormat PronomCodesTextDocuments = new FileFormat([
        PronomCodesDOC,
        PronomCodesDOCM,
        PronomCodesDOCX,
        PronomCodesDOT,
        PronomCodesDOTM,
        PronomCodesDOTX,
        PronomCodesODT,
    ]);

    //PDFs
    public static readonly FileFormat PronomCodesPDF = new FileFormat(["pdf"], [
        "fmt/14", //PDF1.0
        "fmt/15", //PDF1.1
        "fmt/16", //PDF1.2
        "fmt/17", //PDF1.3
        "fmt/18", //PDF1.4
        "fmt/19", //PDF1.5
        "fmt/20", //PDF1.6
        "fmt/276" //PDF1.7
    ]);

    public static readonly FileFormat PronomCodesPDF1A = new FileFormat(["pdf1a"], [
        "fmt/95", //PDF/A 1a
        "fmt/354", //PDF/A 1b
    ]);

    public static readonly FileFormat PronomCodesPDF2A = new FileFormat(["pdf2a"], [
        "fmt/476", //PDF/A 2a
        "fmt/477", //PDF/A 2b
        "fmt/478", //PDF/A 2u
    ]);

    public static readonly FileFormat PronomCodesPDF3A = new FileFormat(["pdf3a"], [
        "fmt/479", //PDF/A 3a
        "fmt/480", //PDF/A 3b
        "fmt/481", //PDF/A 3u
    ]);

    public static readonly FileFormat PronomCodesPDF4A = new FileFormat(["pdf4a"], [
        "fmt/1910", //PDF/A 4
        "fmt/1911", //PDF/A 4e
        "fmt/1912", //PDF/A 4f
    ]);

    public static readonly FileFormat PronomCodesPDFA = new FileFormat([
        PronomCodesPDF1A,
        PronomCodesPDF2A,
        PronomCodesPDF3A,
        PronomCodesPDF4A,
    ]);

    //PRESENTATIONS
    public static readonly FileFormat PronomCodesPPT = new FileFormat(["ppt"], [
        "fmt/125", //PowerPoint Presentation 95
        "fmt/126", //PowerPoint Presentation 97-2003
    ]);

    public static readonly FileFormat PronomCodesPPTM = new FileFormat(["pptm"], [

    ]);

    public static readonly FileFormat PronomCodesPPTX = new FileFormat(["pptx"], [
        "fmt/215" //PowerPoint for Windows 2007 onwards
    ]);

    public static readonly FileFormat PronomCodesODP = new FileFormat(["odp"], [
        "fmt/138", //OpenDocument Presentation 1.0
        "fmt/292", //OpenDocument Presentation 1.1
        "fmt/293", //OpenDocument Presentation 1.2
        "fmt/1754" //OpenDocument Presentation 1.3
    ]);

    public static readonly FileFormat PronomCodesPresentationDocuments = new FileFormat([
        PronomCodesPPT,
        PronomCodesPPTM,
        PronomCodesPPTX,
        PronomCodesODP,
    ]);

    //SPREADSHEETS
    public static readonly FileFormat PronomCodesXLS = new FileFormat(["xls"], [
        "fmt/61", //Excel 97 Workbook (xls) 8
        "fmt/62", //Excel 2000-2003 Workbook (xls) 8X
    ]);

    public static readonly FileFormat PronomCodesXLSM = new FileFormat(["xlsm"], [

    ]);

    public static readonly FileFormat PronomCodesXLSX = new FileFormat(["xlsx"], [

    ]);

    public static readonly FileFormat PronomCodesODS = new FileFormat(["ods"], [
        "fmt/137", //OpenDocument Spreadsheet 1.0
        "fmt/294", //OpenDocument Spreadsheet 1.1
        "fmt/295", //OpenDocument Spreadsheet 1.2
        "fmt/1755" //OpenDocument Spreadsheet 1.3
    ]);

    public static readonly FileFormat PronomCodesCSV = new FileFormat(["csv"], [
        "fmt/800", //CSV Schema

        "x-fmt/18" //Comma Separated Values 
    ]);

    public static readonly FileFormat PronomCodesSpreadsheets = new FileFormat([
        PronomCodesXLS,
        PronomCodesXLSM,
        PronomCodesXLSX,
        PronomCodesODS,
        PronomCodesCSV,
    ]);

    //IMAGES
    public static readonly FileFormat PronomCodesPNG = new FileFormat(["png"], [
        "fmt/11", //PNG 1.0
        "fmt/12", //PNG 1.1
        "fmt/13" //PNG 1.2
    ]);

    public static readonly FileFormat PronomCodesJPEG = new FileFormat(["jpeg", "jpg"], [
        "fmt/41", //Raw JPEG
        "fmt/42", //JPEG 1.00
        "fmt/43", //JPEG 1.01
        "fmt/44" //JPEG 1.02
    ]);

    public static readonly FileFormat PronomCodesTIFF = new FileFormat(["tiff"], [
        "fmt/7", //Tagged Image File Format 3
        "fmt/8", //Tagged Image File Format 4
        "fmt/9", //Tagged Image File Format 5
        "fmt/10", //Tagged Image File Format 6
        "fmt/353" //Tagged Image File Format
    ]);

    public static readonly FileFormat PronomCodesBMP = new FileFormat(["bmp"], [
        "fmt/114", //Windows Bitmap 1.0
        "fmt/115", //Windows Bitmap 2.0
        "fmt/116", //Windows Bitmap 3.0
        "fmt/117", //Windows Bitmap 3.0 NT
        "fmt/118", //Windows Bitmap 4.0
        "fmt/119" //Windows Bitmap 5.0
    ]);

    public static readonly FileFormat PronomCodesGIF = new FileFormat(["gif"], [
        "fmt/3", //GIF 87a
        "fmt/4" //GIF 89a
    ]);

    public static readonly FileFormat PronomCodesImages = new FileFormat([
        PronomCodesPNG,
        PronomCodesJPEG,
        PronomCodesTIFF,
        PronomCodesBMP,
        PronomCodesGIF
    ]);
}