using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTests.ComparingMethods;

public abstract class TestBase
{
    protected static readonly string TestFileDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName + "/ComparingMethods/TestFiles/PowerPoint";
}

public class IsPowerPointFileTests
{
    [Xunit.Theory]
    [InlineData("fmt/215", true)]  // Valid PowerPoint format
    [InlineData("fmt/126", true)]  // Valid PowerPoint format
    [InlineData("fmt/123", false)] // Invalid format
    [InlineData("", false)]        // Empty string
    [InlineData(null, false)]      // Null
    public void TestIsPowerPointFile(string? format, bool expected)
    {
        var result = format != null && AnimationComparison.IsPowerPointFile(format);
        Assert.Equal(expected, result);
    }
}

public class FileAnimationComparisonTests : TestBase
{
    [Fact]
    public void TestNonPowerPointFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "non_powerpoint_file.txt");
        var files = new FilePair(filePath, "x-fmt/111", filePath, "x-fmt/111");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.True(result); // Non-PowerPoint file should pass
    }
    
    [Fact]
    public void TestBothPowerPointFiles()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.ppt");
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pptx");
        var files = new FilePair(oFilePath, "fmt/126", nFilePath, "fmt/215");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.True(result); // Both PowerPoint files should pass
    }
    
    [Fact]
    public void TestInvalidOriginalFilePath()
    {
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pdf");
        var files = new FilePair("invalid_path.pptx", "fmt/215", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.False(result); // Invalid file path should fail
    }

    [Fact]
    public void TestInvalidNewFilePath()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.ppt");
        var files = new FilePair(oFilePath, "fmt/126", "invalid_path.pdf", "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.False(result); // Invalid new file path should fail
    }
    
    [Fact]
    public void TestPowerPointFileWithAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.False(result); // PowerPoint file with animations should fail
    }

    [Fact]
    public void TestPowerPointFileWithoutAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/215", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.True(result); // PowerPoint file without animations should pass
    }
    
    [Fact]
    public void TestOlderPowerPointFileWithAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.ppt");
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/126", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.False(result); // PowerPoint file with animations should fail
    }

    [Fact]
    public void TestOlderPowerPointFileWithoutAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_without_animations.ppt");
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/126", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.True(result); // PowerPoint file without animations should pass
    }
}

public class CheckPptxFormatForAnimationTests : TestBase
{
    [Fact]
    public void TestPptxFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pptx");
        var result = AnimationComparison.CheckPptxFormatForAnimation(filePath);
        Assert.False(result); // File with animations should fail
    }

    [Fact]
    public void TestPptxFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pptx");
        var result = AnimationComparison.CheckPptxFormatForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
}

public class CheckOtherFormatsForAnimationTests : TestBase
{
    [Fact]
    public void TestPptFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.ppt");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.False(result); // File with animations should fail
    }

    [Fact]
    public void TestPptFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.ppt");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
    
    [Fact]
    public void TestPptmFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pptm");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.False(result); // File with animations should not pass
    }
    
    [Fact]
    public void TestPptmFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pptm");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
    
    [Fact]
    public void TestPotxFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.potx");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.False(result); // File with animations should not pass
    }
    
    [Fact]
    public void TestPotxFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.potx");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
    
    [Fact]
    public void TestPotmFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.potm");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.False(result); // File with animations should not pass
    }
    
    [Fact]
    public void TestPotmFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.potm");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
    
    [Fact]
    public void TestPpsxFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.ppsx");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.False(result); // File with animations should not pass
    }
    
    [Fact]
    public void TestPpsxFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.ppsx");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
    
    [Fact]
    public void TestPpsmFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.ppsm");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.False(result); // File with animations should not pass
    }
    
    [Fact]
    public void TestPpsmFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.ppsm");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
    
    [Fact]
    public void TestXmlFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.xml");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.False(result); // File with animations should fail
    }

    [Fact]
    public void TestXmlFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.xml");
        var result = AnimationComparison.CheckOtherFormatsForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
}