using System;
using System.Collections.Generic;
using System.Linq;
using AvaloniaDraft.ComparingMethods.ExifTool;
using Newtonsoft.Json.Linq;
using Convert = System.Convert;


namespace AvaloniaDraft.Helpers;

public enum ColorType
{
    Mono,
    G,
    GA,
    Index,
    RGB,
    RGBA,
    Other,
    Unknown
}

public class StandardizedImageMetadata
{
    public string Path { get; set; } = "";
    public string Name { get; set; } = "";

    public int ImgWidth { get; set; } = 0;
    public int ImgHeight { get; set; } = 0;

    public int BitDepth { get; set; } = 0;
    public ColorType ColorType { get; set; } = ColorType.Unknown;

    public double Gamma { get; set; } = 0.0;
    public string Rendering {get; set;} = "";

    public int PPUnitX { get; set; } = 0;
    public int PPUnitY { get; set; } = 0;
    public string PUnit { get; set; } = "";

    public Dictionary<string, object> AdditionalValues = new();

    /// <summary>
    /// Verifies if image resolution has been set
    /// </summary>
    /// <returns>True or false</returns>
    public bool VerifyResolution()
    {
        return (ImgWidth != 0 && ImgHeight != 0);
    }
    
    /// <summary>
    /// Verifies if bit depth has been set
    /// </summary>
    /// <returns>True or false</returns>
    public bool VerifyBitDepth()
    {
        return (BitDepth != 0);
    }
    
    /// <summary>
    /// Verifies if colortype has been set/is known
    /// </summary>
    /// <returns>True or false</returns>
    public bool VerifyColorType()
    {
        return (ColorType != ColorType.Unknown);
    }
    
    /// <summary>
    /// Verifies if physical units have been set
    /// </summary>
    /// <returns>True or false</returns>
    public bool VerifyPhysicalUnits()
    {
        return (PPUnitX != 0 && PPUnitY != 0 && PUnit != "");
    }
    
    /// <summary>
    /// Checks if the images share resolution
    /// </summary>
    /// <param name="img2">Image to compare with</param>
    /// <returns>True or false</returns>
    public bool CompareResolution(StandardizedImageMetadata img2)
    {
        return (img2.ImgWidth == ImgWidth && img2.ImgHeight == ImgHeight);
    }
    
    /// <summary>
    /// Checks if two images have the same bit depth
    /// </summary>
    /// <param name="img2">Image to compare with</param>
    /// <returns>True or false</returns>
    public bool CompareBitDepth(StandardizedImageMetadata img2)
    {
        return (img2.BitDepth == BitDepth);
    }

    /// <summary>
    /// Checks if two images have the same color type
    /// </summary>
    /// <param name="img2">Image to compare with</param>
    /// <returns>True or false</returns>
    public bool CompareColorType(StandardizedImageMetadata img2)
    {
        return (img2.ColorType == ColorType);
    }
    
    /// <summary>
    /// Checks if two images have the same physical units
    /// </summary>
    /// <param name="img2">Image to compare with</param>
    /// <returns>True or false</returns>
    public bool ComparePhysicalUnits(StandardizedImageMetadata img2)
    {
        return (img2.PPUnitX == PPUnitX && img2.PPUnitY == PPUnitY && img2.PUnit == PUnit);
    }
    
    /// <summary>
    /// Checks if two images have the same physical units, converts if needed
    /// </summary>
    /// <param name="img2">Image to compare with</param>
    /// <returns>True or false</returns>
    public bool ComparePhysicalUnitsFlexible(StandardizedImageMetadata img2)
    {
        try
        {
            var oXcm = ConvertToCm(PUnit, PPUnitX);
            var oYcm = ConvertToCm(PUnit, PPUnitY);
            var nXcm = ConvertToCm(img2.PUnit, img2.PPUnitX);
            var nYcm = ConvertToCm(img2.PUnit, img2.PPUnitY);
            
            return oXcm == nXcm && oYcm == nYcm;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Returns a list of errors regarding additional values
    /// </summary>
    /// <param name="img2">Image to compare with</param>
    /// <returns>List of errors</returns>
    public List<Error> GetMissingAdditionalValues(StandardizedImageMetadata img2)
    {
        List<Error> addVErrors = new();
        
        foreach(var (key, value) in AdditionalValues)
        {
            if (img2.AdditionalValues.TryGetValue(key, out var value2))
            {
                if (value is JObject g1 && value2 is JObject g2)
                {
                    var properties1 = g1.Properties().Select(p => p.Name).ToList();
                    var properties2 = g2.Properties().Select(p => p.Name).ToList();
                    
                    var missing = properties1.Except(properties2).ToList();
                    var added = properties2.Except(properties1).ToList();

                    if (missing.Count > 0)
                    {
                        addVErrors.Add(new Error(
                            "MissingMetadataGroupMembers",
                            $"Missing following members in metadata group {key}: {string.Join(", ", missing)}",
                            ErrorSeverity.Medium,
                            ErrorType.Metadata
                        ));
                    }

                    if (added.Count > 0)
                    {
                        addVErrors.Add(new Error(
                            "AdditionalMetadataGroupMembers",
                            $"New members in metadata group {key}: {string.Join(", ", added)}",
                            ErrorSeverity.Low,
                            ErrorType.Metadata
                        ));
                    }
                }
                else
                {
                    addVErrors.Add(new Error(
                        "MetadataGroupError",
                        $"Could not extract metadata in following group: {key}",
                        ErrorSeverity.Medium,
                        ErrorType.Metadata
                    ));
                }
            }
            else
            {
                addVErrors.Add(new Error(
                    "MissingMetadataGroup",
                    $"The following metadata group is missing in new: {key}",
                    ErrorSeverity.Medium,
                    ErrorType.Metadata
                ));
            }
        }
        
        //Also need to check the ones present 
        var newGroups = img2.AdditionalValues.Keys.Except(AdditionalValues.Keys).ToList();

        foreach (var group in newGroups)
        {
            addVErrors.Add(new Error(
                "NewMetadataGroup",
                $"The following metadata group is present only in new: {group}",
                ErrorSeverity.Low,
                ErrorType.Metadata
            ));
        }
        
        return addVErrors;
    }
    
    /// <summary>
    /// Helper function that standardizes and converts unit measurments to cm
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static int ConvertToCm(string unit, int value)
    {
        return unit.ToLower() switch
        {
            "cm" => value,
            "m" => value * 100,
            "inch" or "inches" => (int)Math.Round(value * 2.54),
            _ => throw new ArgumentException($"Unknown unit: {unit}")
        };
    }
}

/// <summary>
/// Extenstion functions to make code more readable and less complex
/// </summary>
public static class JObjectExtensions
{
    /// <summary>
    /// Tries to safely get a property as an int
    /// </summary>
    /// <param name="jObject">The object that is worked on</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>The property value or 0</returns>
    public static int GetIntValue(this JObject jObject, string propertyName)
    {
        return jObject.TryGetValue(propertyName, out var i) ? i.Value<int?>() ?? 0 : 0;
    }

    /// <summary>
    /// Tries to safely get a property as a string
    /// </summary>
    /// <param name="jObject">The object that is worked on</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>The property value or an empty string</returns>
    public static string GetStringValue(this JObject jObject, string propertyName)
    {
        return jObject.TryGetValue(propertyName, out var r) ? r.Value<string?>() ?? "" : "";
    }
}

/// <summary>
/// Extenstion functions to make code more readable and less complex
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Tries to safely get a property as an int
    /// </summary>
    /// <param name="dictionary">The dictionary that is worked on</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>The property value or 0</returns>
    public static int GetIntValue(this Dictionary<string, object> dictionary, string propertyName)
    {
        return dictionary.TryGetValue(propertyName, out var w) && w is int iw 
            ? iw
            : ConvertToInt(w);
    }
    
    /// <summary>
    /// Tries to safely get a property as a string
    /// </summary>
    /// <param name="dictionary">The dictionary that is worked on</param>
    /// <param name="propertyName">Name of the property</param>
    /// <returns>The property value or an empty string</returns>
    public static string GetStringValue(this Dictionary<string, object> dictionary, string propertyName)
    {
        return (dictionary.TryGetValue("FileName", out var n) ? n.ToString() : "") ?? "";
    }
    
    /// <summary>
    /// Tries to convert a value to int
    /// </summary>
    /// <param name="value">Object to convert</param>
    /// <returns>The converted value</returns>
    private static int ConvertToInt(object? value)
    {
        return value switch
        {
            int intValue => intValue, // Already an int
            long longValue => (int)longValue, // Safe conversion (possible truncation)
            double doubleValue => (int)doubleValue, // Possible loss of precision
            string strValue when int.TryParse(strValue, out int parsedInt) => parsedInt, // Convert from string
            _ => 0 // Default value if conversion fails
        };
    }
}

public static class MetadataStandardizer
{
    
    public static StandardizedImageMetadata StandardizeImageMetadata(ImageMetadata metadata, string format)
    {
        var standardized = new StandardizedImageMetadata
        {
            Path = metadata.SourceFile,
            Name = metadata.File.GetStringValue("FileName")
        };

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

    private static void ProcessPNG(ImageMetadata metadata, ref StandardizedImageMetadata standardized)
    {
        if (!metadata.AdditionalProperties.TryGetValue("PNG", out var pngData)) return;
        if (pngData is not JObject pngDictionary) return;

        standardized.ImgWidth = pngDictionary.GetIntValue("ImageWidth");
        standardized.ImgHeight = pngDictionary.GetIntValue("ImageHeight");
        standardized.BitDepth = pngDictionary.GetIntValue("BitDepth");

        var ct = pngDictionary.GetStringValue("ColorType");
        
        if (ct != "")
        {
            standardized.ColorType = ct switch
            {
                "Greyscale" => ColorType.G,
                "RGB" => ColorType.RGB,
                "Palette" => ColorType.Index,
                "Grayscale with Alpha" => ColorType.GA,
                "RGB with Alpha" => ColorType.RGBA,
                _ => ColorType.Other
            };
        }
        else
            standardized.ColorType = ColorType.Unknown;

        standardized.Rendering = pngDictionary.GetStringValue("SRGBRendering");
        standardized.Gamma = pngDictionary.TryGetValue("Gamma", out var g) ? g.Value<double?>() ?? 0.0 : 0.0;
        standardized.PPUnitX = pngDictionary.GetIntValue("PixelsPerUnitX");
        standardized.PPUnitY = pngDictionary.GetIntValue("PixelsPerUnitY");
        standardized.PUnit = pngDictionary.GetStringValue("PixelUnits");
        
        //Getting rest
        foreach (var additional in metadata.AdditionalProperties)
        {
            if (additional.Key != "PNG" && additional.Key != "ICC_Profile" && !standardized.AdditionalValues.ContainsKey(additional.Key))
            {
                standardized.AdditionalValues.Add(additional.Key, additional.Value);
            }
        }
    }

    private static void ProcessJPEG(ImageMetadata metadata, ref StandardizedImageMetadata standardized)
    {
        //Height and width
        standardized.ImgWidth = metadata.File.GetIntValue("ImageWidth");
        standardized.ImgHeight = metadata.File.GetIntValue("ImageHeight");

        //Color data
        try
        {
            var colorComp = Convert.ToInt64(metadata.File["ColorComponents"]);
            standardized.ColorType = colorComp switch
            {
                //This requires further research - there is more nuance in color type extraction
                1 => ColorType.G,
                3 => ColorType.RGB,
                _ => ColorType.Other
            };
            standardized.BitDepth = metadata.File.GetIntValue("BitsPerSample");
        }
        catch
        {
            // ignored as failure values are set in constructor 
        }

        // JFIF metadata extraction
        if (metadata.AdditionalProperties.TryGetValue("JFIF", out var jfifData) &&
            jfifData is JObject jfifDictionary)
        {
            standardized.PUnit = jfifDictionary.GetStringValue("ResolutionUnit");
            standardized.PPUnitX = jfifDictionary.GetIntValue("XResolution");
            standardized.PPUnitY = jfifDictionary.GetIntValue("YResolution");
        }

        // EXIF metadata extraction (if JFIF is missing)
        if (standardized.PUnit == "" &&
            metadata.AdditionalProperties.TryGetValue("EXIF", out var exifData) &&
            exifData is JObject exifDictionary)
        {
            standardized.PUnit = exifDictionary.GetStringValue("ResolutionUnit");
            standardized.PPUnitX = exifDictionary.GetIntValue("XResolution");
            standardized.PPUnitY = exifDictionary.GetIntValue("YResolution");
        }

        // If PixelUnit is still missing, assume defaults
        if (standardized.PUnit == "")
        {
            standardized.PUnit = "inches";
            standardized.PPUnitX = 72;
            standardized.PPUnitY = 72;
        }

        foreach (var additional in metadata.AdditionalProperties)
        {
            if (additional.Key != "JFIF" && additional.Key != "EXIF" && additional.Key != "ICC_Profile" &&  
                !standardized.AdditionalValues.ContainsKey(additional.Key))
            {
                standardized.AdditionalValues.Add(additional.Key, additional.Value);
            }
        }
    }

    private static void ProcessBMP(ImageMetadata metadata, ref StandardizedImageMetadata standardized)
    {
        //Height and width
        standardized.ImgWidth = metadata.File.GetIntValue("ImageWidth");
        standardized.ImgHeight = metadata.File.GetIntValue("ImageHeight");

        try
        {
            //Color data
            var bitdepth = Convert.ToInt64(metadata.File["BitDepth"]);
            switch (bitdepth)
            {
                //This probably would require some confirmation. Based on my current info it seems correct, but I am unsure
                //if these could be some variations triggered by side properties.
                case 1:
                    standardized.BitDepth = 1;
                    standardized.ColorType = ColorType.Mono;
                    break;
                case 2:
                    standardized.BitDepth = 2;
                    standardized.ColorType = ColorType.Index;
                    break;
                case 4:
                    standardized.BitDepth = 4;
                    standardized.ColorType = ColorType.Index;
                    break;
                case 8:
                    standardized.BitDepth = 8;
                    standardized.ColorType = ColorType.Index;
                    break;
                case 16:
                    standardized.BitDepth = 16;
                    standardized.ColorType = ColorType.RGB;
                    break; //High Color (565 or 555)
                case 24:
                    standardized.BitDepth = 8;
                    standardized.ColorType = ColorType.RGB;
                    break;
                case 32:
                    standardized.BitDepth = 8;
                    standardized.ColorType = ColorType.RGBA;
                    break;
            }
        }
        catch
        {
            // ignored as failure values are set in constructor 
        }
        
        //Physical units
        standardized.PPUnitX = metadata.File.GetIntValue("PixelsPerMeterX");

        standardized.PPUnitY = metadata.File.GetIntValue("PixelsPerMeterY");
        
        standardized.PUnit = "meters";

        foreach (var additional in metadata.AdditionalProperties)
        {
            if (additional.Key != "ICC_Profile" && !standardized.AdditionalValues.ContainsKey(additional.Key))
            {
                standardized.AdditionalValues.Add(additional.Key, additional.Value);
            }
        }
    }

    private static void ProcessTIFF(ImageMetadata metadata, ref StandardizedImageMetadata standardized)
    {
        if (!metadata.AdditionalProperties.TryGetValue("EXIF", out var exifData) ||
            exifData is not JObject exifDictionary)
            return;

        //Height and width
        standardized.ImgWidth = exifDictionary.GetIntValue("ImageWidth");
        standardized.ImgHeight = exifDictionary.GetIntValue("ImageHeight");

        //Color data
        try
        {
            var bitdepth = (exifDictionary["BitsPerSample"] ?? 0).Value<int>();
            switch (bitdepth)
            {
                case 1:
                    standardized.BitDepth = 1;
                    standardized.ColorType = ColorType.Mono;
                    break;
                case 4:
                    standardized.BitDepth = 2;
                    standardized.ColorType = ColorType.Index;
                    break;
                case 8:
                    standardized.BitDepth = 8;
                    standardized.ColorType = ColorType.Index;
                    break;
            }
        }
        catch (FormatException)
        {
            var bitdepth = (exifDictionary["BitsPerSample"] ?? "").Value<string>();
            switch (bitdepth)
            {
                case "8 8 8":
                    standardized.BitDepth = 8;
                    standardized.ColorType = ColorType.RGB;
                    break;
                case "8 8 8 8":
                    standardized.BitDepth = 8;
                    standardized.ColorType = ColorType.RGBA;
                    break;
            }
        }
        catch
        {
            // ignored as failure values are set in constructor 
        }

        standardized.PUnit = exifDictionary.GetStringValue("ResolutionUnit");
        standardized.PPUnitX = exifDictionary.GetIntValue("XResolution");
        standardized.PPUnitY = exifDictionary.GetIntValue("YResolution");

        foreach (var additional in metadata.AdditionalProperties)
        {
            if (additional.Key != "EXIF" && additional.Key != "ICC_Profile" && !standardized.AdditionalValues.ContainsKey(additional.Key))
            {
                standardized.AdditionalValues.Add(additional.Key, additional.Value);
            }
        }
    }
}