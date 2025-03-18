using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class ImageToImageTransparencyComparison : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "transparent.png");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "transparent.png");
        
        var files = new FilePair(oFilePath, "fmt/12", nFilePath, "fmt/12");
        var result = TransparencyComparison.ImageToImageTransparencyComparison(files);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "transparent.png");
        var nFilePath = Path.Combine(TestFileDirectory, "Images", "transparent.jpg");
        
        var files = new FilePair(oFilePath, "fmt/12", nFilePath, "fmt/43");
        var result = TransparencyComparison.ImageToImageTransparencyComparison(files);
        Assert.That(result, Is.False);
    }
}

[TestFixture]
public class ImageToPdfTransparencyComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "transparent.png");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var files = new FilePair(oFilePath, "fmt/12", nFilePath, "fmt/276");
        var result = TransparencyComparison.ImageToPdfTransparencyComparison(files);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "Images", "225x225.png");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var files = new FilePair(oFilePath, "fmt/12", nFilePath, "fmt/276");
        var result = TransparencyComparison.ImageToPdfTransparencyComparison(files);
        Assert.That(result, Is.False);
    }
}

[TestFixture]
public class PdfToPdfTransparencyComparisonTest : TestBase
{
    [Test]
    public void TestCorrectScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        
        var files = new FilePair(oFilePath, "fmt/276", nFilePath, "fmt/276");
        var result = TransparencyComparison.PdfToPdfTransparencyComparison(files);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PDF", "transparent.pdf");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "missing_transparent.pdf");
        
        var files = new FilePair(oFilePath, "fmt/276", nFilePath, "fmt/276");
        var result = TransparencyComparison.PdfToPdfTransparencyComparison(files);
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
        
        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/276");
        var result = TransparencyComparison.DocxToPdfTransparencyComparison(files);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "TestDocuments", "transparent.docx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "missing_transparent.pdf");
        
        var files = new FilePair(oFilePath, "fmt/412", nFilePath, "fmt/276");
        var result = TransparencyComparison.DocxToPdfTransparencyComparison(files);
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
        
        var files = new FilePair(oFilePath, "fmt/1756", nFilePath, "fmt/276");
        var result = TransparencyComparison.OdtAndOdpToPdfTransparencyComparison(files);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "ODT", "transparent.odt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "incorrect_transparency.pdf");
        
        var files = new FilePair(oFilePath, "fmt/1756", nFilePath, "fmt/276");
        var result = TransparencyComparison.OdtAndOdpToPdfTransparencyComparison(files);
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
        
        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/276");
        var result = TransparencyComparison.XmlBasedPowerPointToPdfTransparencyComparison(files);
        Assert.That(result, Is.True);
    }
    
    [Test]
    public void TestFailScenario()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "transparent.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "incorrect_transparency.pdf");
        
        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/276");
        var result = TransparencyComparison.XmlBasedPowerPointToPdfTransparencyComparison(files);
        Assert.That(result, Is.False);
    }
}