namespace AvaloniaDraft.Helpers;

public static class FormatCodes
{
    public static string[] TextDocumentFormats =
    [
        //Word
        "fmt/40", //DOC
        "fmt/477", //DOCM
        "fmt/412", //DOCX
        "fmt/39", //DOT
        "fmt/478", //DOTM
        "fmt/413", //DOTX
            
        //Open Document
        "fmt/135" //ODT
    ];

    public static string[] PresentationFormats =
    [
        //PowerPoint
        "fmt/126", //PPT
        "fmt/479", //PPTM
        "fmt/214", //PPTX
            
        //OpenDocument
        "fmt/139" //ODP
    ];

    public static string[] SpreadsheetFormats = [
        //Excel
        "fmt/61", //XLS
        "fmt/475", //XLSM
        "fmt/215", //XLTX
            
        //OpenDocument
        "fmt/142", //ODS
            
        //Other
        "fmt78", //CSV
    ];
}