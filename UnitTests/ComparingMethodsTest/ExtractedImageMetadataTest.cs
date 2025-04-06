using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class ExtractedImageMetadataTest
{
    private string _testFileDirectory = "";

    [SetUp]
    public void Setup()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                _testFileDirectory = curDir + "/UnitTests/ComparingMethodsTest/TestFiles";
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }
    
    [Test]
    public void CompareExtractedImageMetadataTest()
    {
        var PDF = @"C:\Users\kaczm\Documents\bachelor\ds\test\extTest.pdf";
        var DOCX = @"C:\Users\kaczm\Documents\bachelor\ds\test\extTest.docx";
        var ODT = _testFileDirectory + "/TestDocuments/Image8Pages.odt";
        
        var imgPDF = ImageExtraction.GetNonDuplicatePdfImages(PDF);
        var imgODT = ImageExtraction.ExtractImagesFromOpenDocuments(ODT);
        var imgDOCX = ImageExtraction.ExtractImagesFromDocx(DOCX);

        var pair = new FilePair();
        
        ExtractedImageMetadata.CompareExtractedImages(pair, imgDOCX, imgPDF);
        
    }
}