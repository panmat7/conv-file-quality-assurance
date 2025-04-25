using AvaloniaDraft.Helpers;

namespace UnitTests.HelperTest;

[TestFixture]
public class ErrorClassTests
{
    [Test]
    public void TestErrorClass()
    {
        var e = new Error(
            "Name",
            "Description",
            ErrorSeverity.Medium,
            ErrorType.FileError
        );

        var e2 = new Error(
            "OtherName",
            "OtherDescription",
            ErrorSeverity.Medium,
            ErrorType.FileError
        );
        
        var e3 = new Error(
            "Name",
            "Description",
            ErrorSeverity.Medium,
            ErrorType.FileError
        );

        var formatted = e.FormatErrorMessage();
        if (!formatted.Contains("Name") || !formatted.Contains("Description") || !formatted.Contains("Medium") ||
            !formatted.Contains("File error"))
            Assert.Fail();
        
        if(!e.Equals(e3) || e.Equals(e2)) Assert.Fail();
        
        var list = new List<Error>();
        
        if(list.GenerateErrorString() != "No Errors Found") Assert.Fail();
        
        list.Add(e);
        list.Add(e2);
        list.Add(e3);

        var combinedFormatted = list.GenerateErrorString();
        
        Assert.Pass();
    }
}