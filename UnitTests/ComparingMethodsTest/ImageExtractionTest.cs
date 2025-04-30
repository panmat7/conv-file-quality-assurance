using AvaloniaDraft.ComparingMethods;
using ImageMagick;

namespace UnitTests.ComparingMethodsTest;

[TestFixture]
public class ImageExtractionTest
{
    [Test]
    public void GetExpectedPronomFromImageTest()
    {
        const string expectedPronom = "png";
        var pronom = ImageExtractionToDisk.GetExpectedPronomFromImage(MagickFormat.Png);
        Assert.That(pronom, Is.EqualTo(expectedPronom));
    }
    [Test]
    public void GetExpectedPronomFromImageTest2()
    {
        const string expectedPronom = "jpeg";
        var pronom = ImageExtractionToDisk.GetExpectedPronomFromImage(MagickFormat.Jpeg);
        Assert.That(pronom, Is.EqualTo(expectedPronom));
    }
    [Test]
    public void GetExpectedPronomFromImageTest3()
    {
        const string expectedPronom = "tiff";
        var pronom = ImageExtractionToDisk.GetExpectedPronomFromImage(MagickFormat.Tiff);
        Assert.That(pronom, Is.EqualTo(expectedPronom));
    }
    [Test]
    public void GetExpectedPronomFromImageTest4()
    {
        const string expectedPronom = "gif";
        var pronom = ImageExtractionToDisk.GetExpectedPronomFromImage(MagickFormat.Gif);
        Assert.That(pronom, Is.EqualTo(expectedPronom));
    }
}