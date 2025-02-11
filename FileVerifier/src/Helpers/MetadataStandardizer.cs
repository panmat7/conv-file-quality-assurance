using System.Collections.Generic;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetadataExtractor.Util;

namespace AvaloniaDraft.Helpers;

public class ImageMetadata
{
    public string SourcePath { get; set; }
    public Dictionary<string, object> ExifTool { get; set; }
    public Dictionary<string, object> File { get; set; }
    
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; }
}

public static class MetadataStandarizer
{
    public static Dictionary<string, object> StandardizeImageMetadata(ImageMetadata metadata, string format)
    {
        var standardized = new Dictionary<string, object>();
        
        standardized.Add("Path", metadata.SourcePath);
        standardized.Add("Name", metadata.File["FileName"]);
        
        if (FormatCodes.PronomCodesPNG.Contains(format))
        {
            if (metadata.AdditionalProperties.TryGetValue("PNG", out var pngData))
            {
                //Height width
                if (pngData.TryGetProperty("ImageWidth", out var w))
                {
                    standardized.Add("ImgWidth", w.GetInt32());
                }
                
                if (pngData.TryGetProperty("ImageHeight", out var h))
                {
                    standardized.Add("ImgHeight", h.GetInt32());
                }
                
                //Color data
                if (pngData.TryGetProperty("BitDepth", out var depth))
                {
                    standardized.Add("BitDepth", depth.GetInt32());
                }
                
                if (pngData.TryGetProperty("ColorType", out var colorType))
                {
                    var ct = colorType.GetString() ?? "";
                    
                    //Standardizing names
                    switch (ct)
                    {
                        case "Greyscale": standardized.Add("ColorType", "G"); break;
                        case "RGB": standardized.Add("ColorType", "RGB"); break;
                        case "Palette": standardized.Add("ColorType", "Index"); break;
                        case "Grayscale with Alpha": standardized.Add("ColorType", "GA"); break;
                        case "RGB with Alpha": standardized.Add("ColorType", "RGBA"); break;
                        default: standardized.Add("ColorType", ct); break;
                    }
                }
                
                if (pngData.TryGetProperty("SRGBRendering", out var rendering))
                {
                    standardized.Add("Rendering", rendering.GetString() ?? "");
                }
                
                if (pngData.TryGetProperty("Gamma", out var gamma))
                {
                    standardized.Add("Gamma", gamma.GetDecimal());
                }
                
                //Physical units
                if (pngData.TryGetProperty("PixelsPerUnitX", out var ppux))
                {
                    standardized.Add("PPUnitX", ppux.GetInt32());
                }
                
                if (pngData.TryGetProperty("PixelsPerUnitY", out var ppuy))
                {
                    standardized.Add("PPUnitY", ppuy.GetInt32());
                }
                
                if (pngData.TryGetProperty("PixelUnits", out var pu))
                {
                    standardized.Add("PixelUnit", pu.GetString() ?? "");
                }
            }
        } 
        else if (FormatCodes.PronomCodesJPEG.Contains(format))
        {
            //Height and width
            standardized.Add("ImgWidth", metadata.File["ImageWidth"]);
            standardized.Add("ImgHeight", metadata.File["ImageHeight"]);
            
            //Color data
            standardized.Add("BitDepth", metadata.File["BitsPerSample"]);

            switch (metadata.File["ColorComponents"])
            {
                case 1: standardized.Add("ColorType", "G"); break;
                case 3: standardized.Add("ColorType", "RGB"); break;
                default: standardized.Add("ColorType", ""); break;
            }
            
            //Physical units
            if (metadata.AdditionalProperties.TryGetValue("JFIF", out var jfifData))
            {
                if (jfifData.TryGetProperty("ResolutionUnit", out var resUnit))
                {
                    standardized.Add("PixelUnit", resUnit.GetString() ?? "");
                }

                if (jfifData.TryGetProperty("XResolution", out var xRes))
                {
                    standardized.Add("PPUnitX", xRes.GetInt32());
                }

                if (jfifData.TryGetProperty("YResolution", out var yRes))
                {
                    standardized.Add("PPUnitY", yRes.GetInt32());
                }
            }
            
            //Could be stored other places depending on file
            if (!standardized.ContainsKey("PixelUnit") && metadata.AdditionalProperties.TryGetValue("EXIF", out var exifData))
            {
                if (exifData.TryGetProperty("ResolutionUnit", out var resUnit))
                {
                    standardized.Add("PixelUnit", resUnit.GetString() ?? "");
                }

                if (exifData.TryGetProperty("XResolution", out var xRes))
                {
                    standardized.Add("PPUnitX", xRes.GetInt32());
                }

                if (exifData.TryGetProperty("YResolution", out var yRes))
                {
                    standardized.Add("PPUnitY", yRes.GetInt32());
                }
            }
            
            if (standardized.TryAdd("PixelUnit", "inches"))
            {
                //Assuming default values
                standardized.Add("PPUnitX", 72);
                standardized.Add("PPUnitY", 72);
            }
            
        }
        else if (FormatCodes.PronomCodesBMP.Contains(format))
        {
            //Height and width
            standardized.Add("ImgWidth", metadata.File["ImageWidth"]);
            standardized.Add("ImgHeight", metadata.File["ImageHeight"]);

            //Color data
            var bitdepth = metadata.File["BitDepth"];
            switch (bitdepth)
            {
                //This probably would require some confirmation. Based on my current info it seems correct but I am unsure
                //if these could be some variations triggered by side properties.
                case 1: standardized.Add("BitDepth", 1); standardized.Add("ColorType", "Mono"); break;
                case 2: standardized.Add("BitDepth", 2); standardized.Add("ColorType", "Index"); break;
                case 4: standardized.Add("BitDepth", 4); standardized.Add("ColorType", "Index"); break;
                case 8: standardized.Add("BitDepth", 8); standardized.Add("ColorType", "Index"); break;
                case 16: standardized.Add("BitDepth", 16); standardized.Add("ColorType", "RGB"); break; //High Color (565 or 555)
                case 24: standardized.Add("BitDepth", 8); standardized.Add("ColorType", "RGB"); break;
                case 32: standardized.Add("BitDepth", 8); standardized.Add("ColorType", "RGBA"); break;
            }
            
            //Physical units
            standardized["PPUnitX"] = metadata.File["PixelsPerMeterX"];
            standardized["PPUnitY"] = metadata.File["PixelsPerMeterY"];
            standardized["PixelUnit"] = "meters";
            
        }
        else if (FormatCodes.PronomCodesTIFF.Contains(format))
        {
            if (metadata.AdditionalProperties.TryGetValue("EXIF", out var exifData))
            {
                //Height and width
                if (exifData.TryGetProperty("ImageWidth", out var w))
                {
                    standardized.Add("ImgWidth", w.GetInt32());
                }
                
                if (exifData.TryGetProperty("ImageHeight", out var h))
                {
                    standardized.Add("ImgHeight", h.GetInt32());
                }
                
                //Color data
                if (exifData.TryGetProperty("BitsPerSample", out var bitdepth))
                {
                    var type = bitdepth.GetType();
                    
                    //When no separation between RGB, represented as number
                    if(type == typeof(int)) {
                        var bitspersample = bitdepth.GetInt32();

                        switch (bitspersample)
                        {
                            case 1:
                                standardized.Add("BitDepth", 1);
                                standardized.Add("ColorType", "Mono");
                                break;
                            case 4:
                                standardized.Add("BitDepth", 2);
                                standardized.Add("ColorType", "Index");
                                break;
                            case 8:
                                standardized.Add("BitDepth", 8);
                                standardized.Add("ColorType", "Index");
                                break;
                        }
                    }
                    
                    //Otherwise represented as string
                    if (type == typeof(string))
                    {
                        var bitspersample = bitdepth.GetString() ?? "";

                        switch (bitspersample)
                        {
                            case "8 8 8": standardized.Add("BitDepth", 8); standardized.Add("ColorType", "RGB"); break;
                            case "8 8 8 8": standardized.Add("BitDepth", 8); standardized.Add("ColorType", "RGBA"); break;
                        }
                    }
                }
                
                //Physical units
                if (exifData.TryGetProperty("ResolutionUnit", out var resUnit))
                {
                    standardized.Add("PixelUnit", resUnit.GetString() ?? "");
                }

                if (exifData.TryGetProperty("XResolution", out var xRes))
                {
                    standardized.Add("PPUnitX", xRes.GetInt32());
                }

                if (exifData.TryGetProperty("YResolution", out var yRes))
                {
                    standardized.Add("PPUnitY", yRes.GetInt32());
                }
            }
            
            
        }
        
        return standardized;
    }
}