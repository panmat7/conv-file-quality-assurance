using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using Xunit;
using Assert = Xunit.Assert;

namespace UnitTests.ComparingMethods;

public abstract class TestBase
{
    public static readonly string TestFileDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!
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
    public void TestPowerPointFileWithAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/126", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.False(result); // PowerPoint file with animations should fail
    }

    [Fact]
    public void TestPowerPointFileWithoutAnimations()
    {
        var oFilePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pptx");
        var nFilePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pdf");
        var files = new FilePair(oFilePath, "fmt/126", nFilePath, "fmt/19");
        var result = AnimationComparison.FileAnimationComparison(files);
        Assert.True(result); // PowerPoint file without animations should pass
    }
}

public class CheckPptxFilesForAnimationTests : TestBase
{
    [Fact]
    public void TestPptxFileWithAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_with_animations.pptx");
        var result = AnimationComparison.CheckPptxFilesForAnimation(filePath);
        Assert.False(result); // File with animations should fail
    }

    [Fact]
    public void TestPptxFileWithoutAnimations()
    {
        var filePath = Path.Combine(TestFileDirectory, "presentation_without_animations.pptx");
        var result = AnimationComparison.CheckPptxFilesForAnimation(filePath);
        Assert.True(result); // File without animations should pass
    }
}