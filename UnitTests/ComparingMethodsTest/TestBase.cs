namespace UnitTests.ComparingMethodsTest;

public abstract class TestBase
{
    protected static readonly string TestFileDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName + "/ComparingMethodsTest/TestFiles/";
    protected static readonly string TestExtractionODirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName + "/ComparingMethodsTest/TestFiles/OTempFilesTest/";
    protected static readonly string TestExtractionNDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName + "/ComparingMethodsTest/TestFiles/NTempFilesTest/";
}