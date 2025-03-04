using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using ImageMagick;

namespace UnitTests.ComparingMethodsTest;

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