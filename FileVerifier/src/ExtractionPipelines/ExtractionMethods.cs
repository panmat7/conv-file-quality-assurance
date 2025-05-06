using System.Collections.Generic;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ProgramManager;

namespace AvaloniaDraft.ExtractionPipelines;

public static class ExtractionMethods
{
    /// <summary>
    /// Gets some metadata from images and returns them as a dictionary.
    /// </summary>
    /// <param name="file">The file to be worked on.</param>
    /// <returns>Data in a name-to-value dictionary, or null if an error occured.</returns>
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
        if (!string.IsNullOrEmpty(standardized.PUnit))
            metaDict["PhysicalUnits"] = $"{standardized.PPUnitX}x{standardized.PPUnitY} per {standardized.PUnit}";
        
        
        
        return metaDict;
    }

    /// <summary>
    /// Checks if a spreadsheet has a risk of a table-break, and returns the result as a dictionary. 
    /// </summary>
    /// <param name="file">The file to be worked on.</param>
    /// <returns>Name-to-Value dictionary regarding table-breaks.</returns>
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