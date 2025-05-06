using AvaloniaDraft.ComparisonPipelines;
using UnitTests.ComparingMethodsTest;

namespace UnitTests.ProgramManagerTest;

[TestFixture]
public class ComparisonPipelines : TestBase
{
    [Test]
    public void PNGPipelinesTest()
    {
        var pjPipeline = ImagePipelines.GetImagePipelines("fmt/43"); //To jpeg
        var dpPipeline = DocxPipelines.GetDocxPipeline("fmt/95"); //To pdf
        var nonePipeline = ImagePipelines.GetImagePipelines("none");
        
        if(pjPipeline is null) Assert.Fail();
        if(nonePipeline is not null) Assert.Fail();
        
        Assert.That(dpPipeline?.Method.Name, Is.EqualTo("DocxToPdfPipeline"));
        Assert.That(pjPipeline?.Method.Name, Is.EqualTo("ImageToImagePipeline"));
    }
}