using System;
using System.Collections.Generic;
using AvaloniaDraft.ComparingMethods.ExifTool;
using Newtonsoft.Json.Linq;


namespace AvaloniaDraft.Helpers;

public static class MetadataStandardizer
{
    
    public static Dictionary<string, object> StandardizeImageMetadata(ImageMetadata metadata, string format)
    {
        var standardized = new Dictionary<string, object>();
        
        standardized.Add("Path", metadata.SourceFile);
        standardized.Add("Name", metadata.File["FileName"]);
        
        if (FormatCodes.PronomCodesPNG.Contains(format))
        {
            ProcessPNG(metadata, ref standardized);
        } 
        else if (FormatCodes.PronomCodesJPEG.Contains(format))
        {
            ProcessJPEG(metadata, ref standardized);
        }

        else if (FormatCodes.PronomCodesBMP.Contains(format))
        {
            ProcessBMP(metadata, ref standardized);
        }
        else if (FormatCodes.PronomCodesTIFF.Contains(format))
        {
            ProcessTIFF(metadata, ref standardized);
        }
        
        return standardized;
    }

    private static void ProcessPNG(ImageMetadata metadata, ref Dictionary<string, object> standardized)
    {
        if (!metadata.AdditionalProperties.TryGetValue("PNG", out var pngData)) return;
        if (pngData is not JObject pngDictionary) return;
        
        standardized["ImgWidth"] = pngDictionary.TryGetValue("ImageWidth", out var w) ? w.Value<int?>() ?? 0 : 0;
        standardized["ImgHeight"] = pngDictionary.TryGetValue("ImageHeight", out var h) ? h.Value<int?>() ?? 0 : 0;
        standardized["BitDepth"] = pngDictionary.TryGetValue("BitDepth", out var dp) ? dp.Value<int?>() ?? 0 : 0;
        
        if (pngDictionary.TryGetValue("ColorType", out var ct) && ct.Value<string>() is string cts)
        {
            switch (cts)
            {
                case "Greyscale": standardized.Add("ColorType", "G"); break;
                case "RGB": standardized.Add("ColorType", "RGB"); break;
                case "Palette": standardized.Add("ColorType", "Index"); break;
                case "Grayscale with Alpha": standardized.Add("ColorType", "GA"); break;
                case "RGB with Alpha": standardized.Add("ColorType", "RGBA"); break;
                default: standardized.Add("ColorType", cts); break;
            }
        }
        else
            standardized.Add("ColorType", "");
        
        if (pngDictionary.TryGetValue("SRGBRendering", out var r) && r.Value<string>() is string rs)
            standardized.Add("Rendering", rs);
        else
            standardized.Add("Rendering", "");
        
        
        if (pngDictionary.TryGetValue("Gamma", out var g) && g.Value<double>() is double gd)
            standardized.Add("Gamma", gd);
        else
            standardized.Add("Gamma", 0.0);
        
        //Physical units
        if (pngDictionary.TryGetValue("PixelsPerUnitX", out var ppux) && ppux.Value<int>() is int ppuxi)
            standardized.Add("PPUnitX", ppuxi);
        else
            standardized.Add("PPUnitX", 0);
        
        if (pngDictionary.TryGetValue("PixelsPerUnitY", out var ppuy) && ppuy.Value<int>() is int ppuyi)
            standardized.Add("PPUnitY", ppuyi);
        else
            standardized.Add("PPUnitY", 0);
        
        if (pngDictionary.TryGetValue("PixelUnits", out var pu) && pu.Value<string>() is string pus)
            standardized.Add("PixelUnit", pus);
        else
            standardized.Add("Rendering", "");
    }

    private static void ProcessJPEG(ImageMetadata metadata, ref Dictionary<string, object> standardized)
    {
        //Height and width
        try
        {
            standardized.Add("ImgWidth", metadata.File["ImageWidth"]);
            standardized.Add("ImgHeight", metadata.File["ImageHeight"]);
        }
        catch
        {
            standardized.Add("ImgWidth", 0);
            standardized.Add("ImgHeight", 0);
        }

        //Color data
        try
        {
            var colorComp = Convert.ToInt64(metadata.File["ColorComponents"]);
            switch (colorComp)
            {
                //This requires further research - there is more nuance in color type extraction
                case 1: standardized.Add("ColorType", "G"); break;
                case 3: standardized.Add("ColorType", "RGB"); break;
                default: standardized.Add("ColorType", ""); break;
            }
            standardized.Add("BitDepth", metadata.File["BitsPerSample"]);
        }
        catch
        {
            standardized.TryAdd("BitDepth", 0);
            standardized.TryAdd("ColorType", "");
        }

        // JFIF metadata extraction
        if (metadata.AdditionalProperties.TryGetValue("JFIF", out var jfifData) &&
            jfifData is JObject jfifDictionary)
        {
            if (jfifDictionary.TryGetValue("ResolutionUnit", out var ru) &&
                ru.Value<string>() is string resolutionUnit)
                standardized.Add("PixelUnit", resolutionUnit);
            else
                standardized.Add("ResolutionUnit", "");
            
            if (jfifDictionary.TryGetValue("XResolution", out var xres) && xres.Value<int?>() is int xResolution)
                standardized.Add("PPUnitX", xResolution);
            else
                standardized.Add("PPUnitX", 0);
            
            if (jfifDictionary.TryGetValue("YResolution", out var yres) && yres.Value<int?>() is int yResolution)
                standardized.Add("PPUnitY", yResolution);
            else
                standardized.Add("PPUnitY", 0);
        }

        // EXIF metadata extraction (if JFIF is missing)
        if (!standardized.ContainsKey("PixelUnit") &&
            metadata.AdditionalProperties.TryGetValue("EXIF", out var exifData) &&
            exifData is JObject exifDictionary)
        {
            if (exifDictionary.TryGetValue("ResolutionUnit", out var ru) &&
                ru.Value<string>() is string resolutionUnit)
                standardized.Add("PixelUnit", resolutionUnit);
            else
                standardized.Add("PixelUnit", "");
            
            if (exifDictionary.TryGetValue("XResolution", out var xres) && xres.Value<int?>() is int xResolution)
                standardized.Add("PPUnitX", xResolution);
            else
                standardized.Add("PPUnitX", 0);
            
            if (exifDictionary.TryGetValue("YResolution", out var yres) && yres.Value<int?>() is int yResolution)
                standardized.Add("PPUnitY", yResolution);
            else
                standardized.Add("PPUnitY", 0);
        }

        // If PixelUnit is still missing, assume defaults
        if (!standardized.ContainsKey("PixelUnit"))
        {
            standardized.Add("PixelUnit", "inches");
            standardized.Add("PPUnitX", 72);
            standardized.Add("PPUnitY", 72);
        }
    }

    private static void ProcessBMP(ImageMetadata metadata, ref Dictionary<string, object> standardized)
    {
        //Height and width
        try
        {
            standardized.Add("ImgWidth", metadata.File["ImageWidth"]);
            standardized.Add("ImgHeight", metadata.File["ImageHeight"]);
        }
        catch
        {
            standardized.Add("ImgWidth", 0);
            standardized.Add("ImgHeight", 0);
        }

        try
        {
            //Color data
            var bitdepth = Convert.ToInt64(metadata.File["BitDepth"]);
            switch (bitdepth)
            {
                //This probably would require some confirmation. Based on my current info it seems correct, but I am unsure
                //if these could be some variations triggered by side properties.
                case 1:
                    standardized.Add("BitDepth", 1);
                    standardized.Add("ColorType", "Mono");
                    break;
                case 2:
                    standardized.Add("BitDepth", 2);
                    standardized.Add("ColorType", "Index");
                    break;
                case 4:
                    standardized.Add("BitDepth", 4);
                    standardized.Add("ColorType", "Index");
                    break;
                case 8:
                    standardized.Add("BitDepth", 8);
                    standardized.Add("ColorType", "Index");
                    break;
                case 16:
                    standardized.Add("BitDepth", 16);
                    standardized.Add("ColorType", "RGB");
                    break; //High Color (565 or 555)
                case 24:
                    standardized.Add("BitDepth", 8);
                    standardized.Add("ColorType", "RGB");
                    break;
                case 32:
                    standardized.Add("BitDepth", 8);
                    standardized.Add("ColorType", "RGBA");
                    break;
            }
        }
        catch
        {
            standardized.TryAdd("BitDepth", 0);
            standardized.TryAdd("ColorType", "");
        }
        
        try
        {
            //Physical units
            standardized["PPUnitX"] = metadata.File["PixelsPerMeterX"];
            standardized["PPUnitY"] = metadata.File["PixelsPerMeterY"];
            standardized["PixelUnit"] = "meters";
        }
        catch
        {
            standardized.TryAdd("PPUnitX", 0);
            standardized.TryAdd("PPUnitY", 0);
            standardized.TryAdd("PixelUnit", "");
        }
    }

    private static void ProcessTIFF(ImageMetadata metadata, ref Dictionary<string, object> standardized)
    {
        if (!metadata.AdditionalProperties.TryGetValue("EXIF", out var exifData) ||
            exifData is not JObject exifDictionary)
            return;

        //Height and width
        try
        {
            standardized.Add("ImgWidth", metadata.File["ImageWidth"]);
            standardized.Add("ImgHeight", metadata.File["ImageHeight"]);
        }
        catch
        {
            standardized.Add("ImgWidth", 0);
            standardized.Add("ImgHeight", 0);
        }

        //Color data
        try
        {
            var bitdepth = (exifDictionary["BitsPerSample"] ?? 0).Value<int>();
            switch (bitdepth)
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
        catch (FormatException)
        {
            var bitdepth = (exifDictionary["BitsPerSample"] ?? "").Value<string>();
            switch (bitdepth)
            {
                case "8 8 8":
                    standardized.Add("BitDepth", 8);
                    standardized.Add("ColorType", "RGB");
                    break;
                case "8 8 8 8":
                    standardized.Add("BitDepth", 8);
                    standardized.Add("ColorType", "RGBA");
                    break;
            }
        }
        catch
        {
            standardized.TryAdd("BitDepth", 0);
            standardized.TryAdd("ColorType", "");
        }
        
     
        standardized.Add("PixelUnit", exifDictionary["ResolutionUnit"] ?? "");
        standardized.Add("PPUnitX", exifDictionary["XResolution"] ?? 0);
        standardized.Add("PPUnitY", exifDictionary["YResolution"] ?? 0);
    }
}