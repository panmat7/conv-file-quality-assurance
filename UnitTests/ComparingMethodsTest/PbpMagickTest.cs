using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;

namespace UnitTests.ComparingMethodsTest;

using NUnit.Framework;
using System.IO;

[TestFixture]
public class PbpMagickTest
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
                _testFileDirectory = curDir + "/UnitTests/ComparingMethodsTest/TestFiles/Images";
                return;
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        
        throw new Exception("Failed to find project directory \"conv-file-quality-assurance\"");
    }

    [Test]
    public void CalculateImageSimilarity_IdenticalImages_ReturnsZeroDistance()
    {
        var filePath = Path.Combine(_testFileDirectory, "225x225.png");
        var files = new FilePair(filePath, filePath);

        var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

        Assert.That(similarity, Is.EqualTo(0).Within(0.01), "Similarity between identical images should be 0.");
    }

    [Test]
    public void CalculateImageSimilarity_DifferentImages_ReturnsNonZeroDistance()
    {
        var original = Path.Combine(_testFileDirectory, "black.png");
        var modified = Path.Combine(_testFileDirectory, "white.png");
        var files = new FilePair(original, modified);

        var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

        Assert.That(similarity, Is.GreaterThan(0), "Different images should have a non-zero distance.");
    }

    [Test]
    public void CalculateImageSimilarity_ResizedImage_ReturnsDistance()
    {
        var original = Path.Combine(_testFileDirectory, "225x225.png");
        var resized = Path.Combine(_testFileDirectory, "450x450.png");
        var files = new FilePair(original, resized);

        var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

        Assert.That(similarity, Is.GreaterThanOrEqualTo(0), "Function should return a value even for resized images.");
    }

    [Test]
    public void CalculateImageSimilarity_InvalidFilePaths_ReturnsMinusOne()
    {
        var files = new FilePair("nonexistent1.png", "nonexistent2.png");

        var similarity = PbpComparisonMagick.CalculateImageSimilarity(files);

        Assert.That(similarity, Is.EqualTo(-1), "Invalid file paths should return -1.");
    }
}
