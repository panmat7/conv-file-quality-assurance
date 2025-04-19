using System.Collections.Generic;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.ExtractionPipelines;

public static class ExtractionMethods
{
    public static Dictionary<string, string>? GetImageMetadataInfo(SingleFile file)
    {
        //Get metadata
        var meta = GlobalVariables.ExifTool.GetExifDataImageMetadata([file.FilePath])?[0];

        if (meta == null) return null;
        
        //Standardize
        var standardized = MetadataStandardizer.StandardizeImageMetadata(meta, file.FileFormat);

        if (standardized == null)
            return null;

        Dictionary<string, string> metaDict = new();
        
        metaDict["Resolution"] = $"{standardized.ImgWidth}x{standardized.ImgHeight}";
        metaDict["ColorType"] = $"{standardized.ColorType.ToString()}";
        metaDict["BitDepth"] = $"{standardized.BitDepth}";
        if(FormatCodes.PronomCodesGIF.Contains(file.FileFormat)) metaDict["FrameCount"] = $"{standardized.FrameCount}";
        metaDict["PhysicalUnits"] = $"{standardized.PPUnitX}x{standardized.PPUnitY} per {standardized.PUnit}";
        
        return metaDict;
    }

    public static Dictionary<string, string> CheckSpreadsheetBreak(SingleFile file)
    {
        var result = new Dictionary<string, string>();
        
        if (FormatCodes.PronomCodesCSV.Contains(file.FileFormat))
        {
            var res = SpreadsheetComparison.PossibleLineBreakCsv(file.FilePath);

            if (res is null) result["TableBreak"] = "Check Failed";
            else if (res.Value) result["TableBreak"] = "Possible break detected";
            else result["TableBreak"] = "No break detected";
        }
        else if (FormatCodes.PronomCodesODS.Contains(file.FileFormat))
        {
            var res = SpreadsheetComparison.PossibleSpreadsheetBreakOpenDoc(file.FilePath);
                
            if(res is null) result["TableBreak"] = "Check Failed";
            else if(res.Count == 0) result["TableBreak"] = "No break detected";
            else result["TableBreak"] = "Possible break due to tables or images detected";
        }
        else if (FormatCodes.PronomCodesXLSX.Contains(file.FileFormat))
        {
            var res = SpreadsheetComparison.PossibleSpreadsheetBreakExcel(file.FilePath);
            
            if (res.Count == 0) result["TableBreak"] = "No break detected";
            else result["TableBreak"] = "Possible break due to tables or images detected";
        }

        return result;
    }
}