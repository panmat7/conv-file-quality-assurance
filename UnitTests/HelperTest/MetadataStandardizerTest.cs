using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.Helpers;

namespace UnitTests.HelperTest;

public class MetadataStandardizerTest
{
    [Test]
    public void StandardizeImageMetadataTest()
    {
        var pngPath = @"C:\Users\kaczm\Documents\bachelor\ds\archive\dddd\0001.png";
        var jpgPath = @"C:\Users\kaczm\Documents\bachelor\ds\archive\dddd\a.jpg";
        var tifPath = @"C:\Users\kaczm\Documents\bachelor\ds\archive\dddd\tif32bit.tiff";
        var bmpPath = @"C:\Users\kaczm\Documents\bachelor\ds\archive\dddd\dadw.bmp";
        
        var pngData = ExifToolStatic.GetExifDataImageMetadata([pngPath], GlobalVariables.ExifPath);
        var jpgData = ExifToolStatic.GetExifDataImageMetadata([jpgPath], GlobalVariables.ExifPath);
        var tifData = ExifToolStatic.GetExifDataImageMetadata([tifPath], GlobalVariables.ExifPath);
        var bmpData = ExifToolStatic.GetExifDataImageMetadata([bmpPath], GlobalVariables.ExifPath);
        
        var pngStan = MetadataStandardizer.StandardizeImageMetadata(pngData![0], "fmt/13");
        var jpgStan = MetadataStandardizer.StandardizeImageMetadata(jpgData![0], "fmt/44");
        var tifStan = MetadataStandardizer.StandardizeImageMetadata(tifData![0], "fmt/353");
        var bmpStan = MetadataStandardizer.StandardizeImageMetadata(bmpData![0], "fmt/116");
        
        Assert.IsTrue(true);
    }
}