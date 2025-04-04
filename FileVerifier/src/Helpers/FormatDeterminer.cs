using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;

namespace AvaloniaDraft.Helpers;

//Based on code share by Jared Beach
//https://stackoverflow.com/a/66055866
public static class FormatDeterminer
{
    private static readonly Dictionary<string, string[][]> SignatureTable = new()
    {
        {
            "jpeg",
            [
                ["FF", "D8", "FF"],
            ]
        },
        {
            "gif",
            [
                ["47", "49", "46", "38", "37", "61"],
                ["47", "49", "46", "38", "39", "61"]
            ]
        },
        {
            "png",
            [
                ["89", "50", "4E", "47", "0D", "0A", "1A", "0A"]
            ]
        },
        {
            "bmp",
            [
                ["42", "4D"]
            ]
        },
        {
            "tiff",
            [
                ["49", "49", "2A", "00"],
                ["4D", "4D", "00", "2A"]
            ]
        }
    };

    public static string? GetImageFormat(byte[] imageData)
    {
        foreach (var entry in SignatureTable)
        {
            var match = false;
            foreach (var signature in entry.Value)
            {
                var imgBytes = string.Join("", imageData.Take(signature.Length).Select(b => b.ToString("X2")));

                if (imgBytes != string.Join("", signature)) continue;
                
                match = true;
                break;

            }

            if (!match) continue;
            
            //If not jpeg - can return, if jpeg - need to check for jpeg end signature
            if (entry.Key != "jpeg")
                return entry.Key;
            
            var nToLastByte = imageData[^2].ToString("X2");
            var lastByte = imageData[^1].ToString("X2");
            
            
            if(nToLastByte == "FF" && lastByte == "D9") 
                return entry.Key;
        }
        
        return null;
    }
}