using AvaloniaDraft.ComparingMethods;

namespace UnitTests.ComparingMethodsTest;

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