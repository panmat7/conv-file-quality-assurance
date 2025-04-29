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
public class GeneralDocsToPdfTransparencyComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenarioDocx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "transparent.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestCorrectScenarioDocxDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Transparency", "correct_transparency.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "Transparency", "correct_transparency.pdf");
        
        ImageExtraction.ExtractImagesFromDocxToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(1));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(1));
            });
    
            var result = TransparencyComparison.CompareTransparencyInImagesOnDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.True);
        }
        finally
        {
            ImageExtraction.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtraction.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }
    
    [Test]
    public void TestFailScenarioDocx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "transparent.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "missing_transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromDocx(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void TestFailScenarioDocxDisk()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "transparent.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "missing_transparent.pdf");
        
        ImageExtraction.ExtractImagesFromDocxToDisk(oFilePath, TestExtractionODirectory);
        ImageExtraction.ExtractImagesFromPdfToDisk(nFilePath, TestExtractionNDirectory);
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(Directory.Exists(TestExtractionODirectory), Is.True);
                Assert.That(Directory.Exists(TestExtractionNDirectory), Is.True);
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Has.Length.EqualTo(1));
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Has.Length.EqualTo(1));
            });
    
            var result = TransparencyComparison.CompareTransparencyInImagesOnDisk(TestExtractionODirectory, TestExtractionNDirectory);
            Assert.That(result, Is.False); // Two files with same color profiles should pass
        }
        finally
        {
            ImageExtraction.DeleteSavedFiles(TestExtractionODirectory);
            ImageExtraction.DeleteSavedFiles(TestExtractionNDirectory);
            Assert.Multiple(() =>
            {
                Assert.That(Directory.GetFiles(TestExtractionODirectory), Is.Empty);
                Assert.That(Directory.GetFiles(TestExtractionNDirectory), Is.Empty);
            });
        }
    }

    [Test]
    public void TestCorrectScenarioOdt()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "transparent.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenarioOdt()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "transparent.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "incorrect_transparency.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }
    
    [Test]
    public void TestCorrectScenarioOds()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODS", "ods-transparent.ods");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "ods-transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenarioOds()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODS", "ods-transparent.ods");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "ods-transparent-fail.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromOpenDocuments(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.False);
    }

    [Test]
    public void TestCorrectScenarioPptx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "transparent.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenarioPptx()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "transparent.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "incorrect_transparency.pdf");
        
        var oImages = ImageExtraction.ExtractImagesFromXmlBasedPowerPoint(oFilePath);
        var nImages = ImageExtraction.GetNonDuplicatePdfImages(nFilePath);
        
        var result = TransparencyComparison.GeneralDocsToPdfTransparencyComparison(oImages, nImages);
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
