namespace UnitTests.ComparingMethodsTest;

public abstract class TestBase
{
    protected static readonly string TestFileDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!
        .Parent!.Parent!.Parent!.FullName + "/ComparingMethodsTest/TestFiles/";
}