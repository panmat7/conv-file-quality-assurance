using AvaloniaDraft.FileManager;

namespace UnitTests.FileManagerTest;

[TestFixture]
public class ComparisonPipelines
{
    [Test]
    public void PNGPipelinesTest()
    {
        var pjPipeline = PngPipelines.GetPNGPipelines("fmt/43"); //To jpeg
        var nonePipeline = PngPipelines.GetPNGPipelines("none");
        
        if(pjPipeline is null) Assert.Fail();
        if(nonePipeline is not null) Assert.Fail();
        
        Assert.That(pjPipeline.Method.Name, Is.EqualTo("PNGToJPEGPipeline"));
    }
}