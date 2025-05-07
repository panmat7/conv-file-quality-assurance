using DocumentFormat.OpenXml.Math;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaDraft.ComparingMethods;


/// <summary>
/// Contains the script code for each language.
/// From: https://stackoverflow.com/a/17755855 
/// </summary>
public static class ScriptCodes
{
    private static readonly Dictionary<string, Dictionary<string, string>> Scripts = LoadScripts();

    /// <summary>
    /// Get the script of a language
    /// </summary>
    /// <param name="code">Two letter iso code (xx) or locale code (xx-YY)</param>
    /// <returns></returns>
    public static string? GetScript(string? code)
    {
        if (string.IsNullOrEmpty(code)) return null;

        var codeParts = code.Split('-');
        var lang = codeParts.FirstOrDefault() ?? "";
        var region = codeParts.ElementAtOrDefault(1) ?? "";

        var langScripts = Scripts.GetValueOrDefault(lang);
        if (langScripts == null) return null;

        return langScripts.GetValueOrDefault(region) ?? langScripts.GetValueOrDefault("");
    }


    /// <summary>
    /// Load the script codes from JSON file
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, Dictionary<string, string>> LoadScripts()
    {
        var path = GetScriptsFilePath();
        if (path == null) return [];

        try
        {
            var json = File.ReadAllText(path);
            var scripts = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);

            return scripts ?? [];
        } 
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load script codes: {e}");
            return [];
        }
    }


    /// <summary>
    /// Get the path to the script codes JSON file
    /// </summary>
    /// <returns></returns>
    private static string? GetScriptsFilePath()
    {
        // Find directory
        var dir = Directory.GetCurrentDirectory();
        while (dir != null)
        {
            if (Path.GetFileName(dir) == "conv-file-quality-assurance")
            {
                break;
            }
            dir = Directory.GetParent(dir)?.FullName;
        }

        try
        {
            var path = Path.Join(dir, "scriptCodes.json");
            return path;
        }
        catch
        {
            Console.WriteLine("Scripts file not found");
            return null;
        }
    }
}