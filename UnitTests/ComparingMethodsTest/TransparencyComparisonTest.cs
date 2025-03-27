using AvaloniaDraft.ComparingMethods;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class PdfToPdfTransparencyComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.PdfToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "missing_transparent.pdf");
        
        var oImages = ImageExtraction.GetNonDuplicatePdfImages(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.PdfToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }
}

[TestFixture]
public class DocxToPdfTransparencyComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "transparent.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.DocxToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "transparent.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "missing_transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.DocxToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }
}

[TestFixture]
public class OdtAndOdpToPdfTransparencyComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "transparent.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.OpenDocumentToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "transparent.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "incorrect_transparency.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.OpenDocumentToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void TestCorrectScenario1()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODS", "ods-transparent.ods");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "ods-transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.OpenDocumentToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario1()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODS", "ods-transparent.ods");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "ods-transparent-fail.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.OpenDocumentToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }
}

[TestFixture]
public class XmlBasedPowerPointToPdfTransparencyComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "transparent.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.XmlBasedPowerPointToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "transparent.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "incorrect_transparency.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.XmlBasedPowerPointToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }
}

[TestFixture]
public class XlsxToPdfColorProfileComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_transparent.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_transparent.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(oFilePath);

        var result = TransparencyComparison.XlsxToPdfColorProfileComparison(oImages, nImages, imagesOverCells);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Spreadsheet", "excel_with_one_missing_profile_over_cells.xlsx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "excel_with_one_missing_profile_over_cells.pdf");

        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        var imagesOverCells = ImageExtraction.GetNonAnchoredImagesFromXlsx(oFilePath);

        var result = TransparencyComparison.XlsxToPdfColorProfileComparison(oImages, nImages, imagesOverCells);
        Assert.That(result, Is.True);
    }
}
