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
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
        Assert.True(result);
    }

    [Test]
    public void TestNonEncryptedZipFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.zip");
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
        Assert.False(result);
    }
    
    [Test]
    public void TestEncryptedRarFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.rar");
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
        Assert.True(result);
    }

    [Test]
    public void TestNonEncryptedRarFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.rar");
        var result = EncryptionChecker.IsCompressedEncrypted(filePath);
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
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.Encrypted, result);
    }

    [Test]
    public void TestNonEncryptedPdfFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.pdf");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }
    
    [Test]
    public void TestEncryptedOdtFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.odt");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.Encrypted, result);
    }

    [Test]
    public void TestNonEncryptedOdtFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.odt");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }
    
    [Test]
    public void TestEncryptedDocxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.docx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.Encrypted, result);
    }

    [Test]
    public void TestNonEncryptedDocxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "nonencrypted.docx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }
    
    [Test]
    public void TestEncryptedPptxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.pptx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.Encrypted, result);
    }
    
    [Test]
    public void TestEncryptedOdpFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "encrypted.odp");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.Encrypted, result);
    }
    
    [Test]
    public void TestCorruptedOdtFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "corrupted.odt");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }
    
    [Test]
    public void TestCorruptedPdfFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "corrupted.pdf");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }
    
    [Test]
    public void TestCorruptedDocxFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "corrupted.docx");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }

    [Test]
    public void TestUnsupportedFile()
    {
        var filePath = Path.Combine(TestFileDirectory, "CorruptedFiles", "225x225.png");
        var result = EncryptionChecker.CheckForEncryption(filePath);
        Assert.Equal(ReasonForIgnoring.None, result);
    }
}