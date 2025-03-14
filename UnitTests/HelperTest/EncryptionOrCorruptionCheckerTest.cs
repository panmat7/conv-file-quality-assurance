using AvaloniaDraft.FileManager;
using UnitTests.ComparingMethodsTest;
using AvaloniaDraft.Helpers;
using Assert = Xunit.Assert;

namespace UnitTests.HelperTest;

[TestFixture]
public class IsCompressedEncryptedTest : TestBase
{
    [Test]
    public void TestEncryptedZipFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.zip");
        var result = EncryptionOrCorruptionChecker.IsCompressedEncrypted(filePath);
        Assert.True(result);
    }

    [Test]
    public void TestNonEncryptedZipFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.zip");
        var result = EncryptionOrCorruptionChecker.IsCompressedEncrypted(filePath);
        Assert.False(result);
    }
    
    [Test]
    public void TestEncryptedRarFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.rar");
        var result = EncryptionOrCorruptionChecker.IsCompressedEncrypted(filePath);
        Assert.True(result);
    }

    [Test]
    public void TestNonEncryptedRarFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.rar");
        var result = EncryptionOrCorruptionChecker.IsCompressedEncrypted(filePath);
        Assert.False(result);
    }
}

[TestFixture]
public class CheckFileEncryptionOrCorruptionTest : TestBase
{
    [Test]
    public void TestEncryptedPdfFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.pdf");
        var result = EncryptionOrCorruptionChecker.CheckFileEncryptionOrCorruption(filePath);
        Assert.Equal(ReasonForIgnoring.EncryptedOrCorrupted, result);
    }

    [Test]
    public void TestNonEncryptedPdfFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.pdf");
        var result = EncryptionOrCorruptionChecker.CheckFileEncryptionOrCorruption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }
}