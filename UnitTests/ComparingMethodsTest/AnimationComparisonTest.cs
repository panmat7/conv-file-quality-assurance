using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class IsPowerPointFileTests
{
    [TestCase("fmt/215", true)]  // Valid PowerPoint format
    [TestCase("fmt/126", true)]  // Valid PowerPoint format
    [TestCase("fmt/123", false)] // Invalid format
    [TestCase("", false)]        // Empty string
    [TestCase(null, false)]      // Null
    public void TestIsPowerPointFile(string? format, bool expected)
    {
        var result = format != null && FormatCodes.PronomCodesPresentationDocuments.Contains(format);
        Assert.That(result, Is.EqualTo(expected));
    }
}

[TestFixture]
public class CheckOdpFormatForAnimationTests : TestBase
{
    [Test]
    public void TestSuccess()
    {
        var filePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-animations.odp");
        
        var result = AnimationComparison.CheckOdpForAnimation(filePath);
        Assert.That(result, Is.True); // Non-PowerPoint file should pass
    }
    
    [Test]
    public void TestFail()
    {
        var filePath = Path.Combine(TestFileDirectory, "ODP", "odp-with-no-images.odp");
        
        var result = AnimationComparison.CheckOdpForAnimation(filePath);
        Assert.That(result, Is.False); // Both PowerPoint files should pass
    }
}

[TestFixture]
public class FileAnimationComparisonTests : TestBase
{
    [Test]
    public void TestNonPowerPointFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "PowerPoint", "non_powerpoint_file.txt");
        var files = new FilePair(filePath, "x-fmt/111", filePath, "x-fmt/111");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.That(result, Is.True); // Non-PowerPoint file should pass
    }
    
    [Test]
    public void TestBothPowerPointFiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_animations.ppt");
        var nFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_animations.pptx");
        var files = new FilePair(oFilePath, "fmt/126", nFilePath, "fmt/215");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.That(result, Is.True); // Both PowerPoint files should pass
    }
    
    [Test]
    public void TestInvalidOriginalFilePath()
    {
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_animations.pdf");
        var files = new FilePair("invalid_path.pptx", "fmt/215", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.That(result, Is.False); // Invalid file path should fail
    }

    [Test]
    public void TestInvalidNewFilePath()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_animations.ppt");
        var files = new FilePair(oFilePath, "fmt/126", "invalid_path.pdf", "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.That(result, Is.False); // Invalid new file path should fail
    }
    
    [Test]
    public void TestPowerPointFileWithAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_animations.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.That(result, Is.False); // PowerPoint file with animations should fail
    }

    [Test]
    public void TestPowerPointFileWithoutAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_without_animations.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_without_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.That(result, Is.True); // PowerPoint file without animations should pass
    }
    
    [Test]
    public void TestOlderPowerPointFileWithAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_animations.ppt");
        var nFilePath = Path.Combine(TestFileDirectory, "PDF", "presentation_with_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/126", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.That(result, Is.False); // PowerPoint file with animations should fail
    }
}

[TestFixture]
public class CheckPptxFormatForAnimationTests : TestBase
{
    [Test]
    public void TestPptxFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_with_animations.pptx");
        var result = AnimationComparison.CheckXmlBasedFormatForAnimation(filePath);
        Assert.That(result, Is.False); // File with animations should fail
    }

    [Test]
    public void TestPptxFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "PowerPoint", "presentation_without_animations.pptx");
        var result = AnimationComparison.CheckXmlBasedFormatForAnimation(filePath);
        Assert.That(result, Is.True); // File without animations should pass
    }
}