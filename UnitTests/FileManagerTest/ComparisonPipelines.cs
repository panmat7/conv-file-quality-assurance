using AvaloniaDraft.ComparingMethods.ComparisonPipelines;
using AvaloniaDraft.FileManager;
using UnitTests.ComparingMethodsTest;

namespace UnitTests.FileManagerTest;

[TestFixture]
public class ComparisonPipelines : TestBase
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