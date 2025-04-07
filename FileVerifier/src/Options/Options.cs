using AvaloniaDraft.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using AvaloniaDraft.FileManager;

namespace AvaloniaDraft.Options;

public enum SettingsProfile
{
    Default,
    Custom1,
    Custom2,
    Custom3
}

/// <summary>
/// Contains options for conversion
/// </summary>
public class Options
{
    public SettingsProfile Profile { get; set; }

    private string? dir; // The directory where settings are stored

    public double SizeComparisonThreshold { get; set; }
    public double PbpComparisonThreshold { get; set; }

    public Dictionary<string, Dictionary<string, bool>> FileFormatsEnabled { get; set; }
    public Dictionary<string, bool> MethodsEnabled { get; set; }

    public int? SpecifiedThreadCount { get; set; }
    public bool IgnoreUnsupportedFileType { get; set; }


    /// <summary>
    /// Initialize the options. This must be called before any other function
    /// </summary>
    public void Initialize()
    {
        SetDirPath();
        Profile = SettingsProfile.Default;

        FileFormatsEnabled = new Dictionary<string, Dictionary<string, bool>>();
        MethodsEnabled = new Dictionary<string, bool>();

        LoadSettings();
    }

    /// <summary>
    /// Initialize enabled formats
    /// </summary>
    public void InitializeEnabledFormats()
    {
        // Get every field of FormatCodes, which are lists of pronom ids for every file type
        var fcFields = typeof(FormatCodes).GetFields();
        foreach (var fld in fcFields)
        {
            var ff = fld.GetValue(null);
            if (ff is FileFormat fileFormat)
            {
                if (fileFormat.FormatCodes.Count != 1) continue;

                var type = fileFormat.FormatCodes[0].ToLower();

                if (!FileFormatsEnabled.ContainsKey(type)) FileFormatsEnabled.Add(type, new Dictionary<string, bool>());
                foreach (var fmt in fileFormat.PronomCodes)
                {
                    FileFormatsEnabled[type][fmt] = true;
                }
            }
        }
    }



    /// <summary>
    /// Get if a method is enabled or not
    /// </summary>
    /// /// <param name="method">The method</param>
    public bool GetMethod(Method method)
    {
        return GetMethod(method.Name);
    }


    /// <summary>
    /// Get if a method is enabled or not
    /// </summary>
    /// /// <param name="method">The name of the method</param>
    public bool GetMethod(string method)
    {
        if (MethodsEnabled.ContainsKey(method))
        {
            return MethodsEnabled[method];
        } 
        else
        {
            return false;
        }
    }



    /// <summary>
    /// Set a method to be enabled or not
    /// </summary>
    /// /// <param name="method">The name of the method</param>
    /// /// <param name="setTo">Enable or not. Leave out to toggle to its opposite value</param> 
    public void SetMethod(Method method, bool? setTo = null)
    {
        SetMethod(method.Name, setTo);
    }

    /// <summary>
    /// Set a method to be enabled or not
    /// </summary>
    /// /// <param name="methodName">The name of the method</param>
    /// /// <param name="setTo">Enable or not. Leave out to toggle to its opposite value</param> 
    public void SetMethod(string methodName, bool? setTo = null)
    {
        if (!MethodsEnabled.ContainsKey(methodName)) return;

        MethodsEnabled[methodName] = setTo ?? MethodsEnabled[methodName];
    }


    /// <summary>
    /// Get if a file format is enabled or not
    /// </summary>
    /// /// <param name="pronomUID">The file type</param>
    public bool? GetFileFormat(string pronomUID)
    {
        foreach (var ft in FileFormatsEnabled.Keys)
        {
            if (FileFormatsEnabled[ft].ContainsKey(pronomUID))
            {
                return FileFormatsEnabled[ft][pronomUID];
            }
        }

        return null;
    }

    /// <summary>
    /// Set a file type to be enabled or not
    /// </summary>
    /// /// <param name="pronomCode">The file type</param>
    /// /// <param name="setTo">Enable or not. Leave out to toggle to its opposite value</param> 
    public void SetFormat(string pronomCode, bool? setTo = null)
    {
        // Get the file type of the pronom code
        string? fileType = FileFormatsEnabled.Keys.FirstOrDefault(k => FileFormatsEnabled[k].ContainsKey(pronomCode));
        if (fileType == null) return;

        FileFormatsEnabled[fileType][pronomCode] = setTo ?? !FileFormatsEnabled[fileType][pronomCode];
    }


    /// <summary>
    /// Check if the file formats of a file pair are both enabled
    /// </summary>
    /// <param name="fp"></param>
    /// <returns></returns>
    public bool FormatsAreEnabled(FilePair fp)
    {
        if ((GetFileFormat(fp.OriginalFileFormat), GetFileFormat(fp.NewFileFormat)) is not (bool oEnabled, bool nEnabled)) return false;
        return (oEnabled && nEnabled);
    }

    /// <summary>
    /// Set all file formats of a filetype to be enabled or not
    /// </summary>
    /// <param name="filetype">The file type</param>
    /// <param name="setTo">Enable or not</param>
    public void SetFiletype(string filetype, bool setTo)
    {
        if (!FileFormatsEnabled.ContainsKey(filetype)) return;

        foreach (var fmt in FileFormatsEnabled[filetype].Keys)
        {
            FileFormatsEnabled[filetype][fmt] = setTo;
        }
    }


    /// <summary>
    /// Set the options to their default values
    /// </summary>
    public void SetDefaultSettings()
    {
        foreach (var m in Methods.GetList())
        {
            MethodsEnabled[m.Name] = true;
        }

        InitializeEnabledFormats();

        SizeComparisonThreshold = 0.0;
        PbpComparisonThreshold = 0.0;

        SpecifiedThreadCount = null;
        IgnoreUnsupportedFileType = true;
    }


    /// <summary>
    /// Save settings to the selected settings profile (Options.profile)
    /// </summary>
    public void SaveSettings()
    {
        if (dir != null) ExportJSON(GetFilePath());
    }


    /// <summary>
    /// Load settings from the selected settings profile (Options.profile)
    /// </summary>
    public void LoadSettings()
    {
        if (dir != null) ImportJSON(GetFilePath());
    }


    /// <summary>
    /// Set the directory of the settings files
    /// </summary>
    private void SetDirPath()
    {
        var currentDir = Directory.GetCurrentDirectory();
        

        while (currentDir != null)
        {
            if (Path.GetFileName(currentDir) == "FileVerifier")
            {
                dir = Path.Join(currentDir, "settings");
                return;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
    }

    /// <summary>
    /// Get the path to the current settings profile file
    /// </summary>
    /// <returns></returns>
    private string GetFilePath()
    {
        return dir + "/" + Profile switch
        {
            SettingsProfile.Default => "default",
            SettingsProfile.Custom1 => "custom1",
            SettingsProfile.Custom2 => "custom2",
            SettingsProfile.Custom3 => "custom3",
        } + ".json";
    }

    /// <summary>
    /// Export the current options to a JSON file
    /// </summary>
    /// <param name="path">The directory where the JSON file is to be exported</param>
    private void ExportJSON(string path)
    {
        try
        {
            var jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to save settings", ex);
        }
    }


    /// <summary>
    /// Import option values from a JSON file
    /// </summary>
    /// <param name="src">The JSON file to import</param>
    private void ImportJSON(string src)
    {
        try
        {
            FileFormatsEnabled.Clear();

            if (File.Exists(src))
            {
                var seralizerOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var jsonString = File.ReadAllText(src);

                var o = JsonSerializer.Deserialize<Options>(jsonString, seralizerOptions);
                if (o is Options opt)
                {
                    this.FileFormatsEnabled = opt.FileFormatsEnabled;
                    this.MethodsEnabled = opt.MethodsEnabled;
                    this.SpecifiedThreadCount = opt.SpecifiedThreadCount;
                    this.IgnoreUnsupportedFileType = opt.IgnoreUnsupportedFileType;
                    this.SizeComparisonThreshold = opt.SizeComparisonThreshold;
                    this.PbpComparisonThreshold = opt.PbpComparisonThreshold;
                }
            } 
            else
            {
                SetDefaultSettings();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load settings", ex);
        }
    }
}
