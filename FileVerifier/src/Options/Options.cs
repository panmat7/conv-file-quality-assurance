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
using Aspose.Slides.Warnings;

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
    public SettingsProfile profile { get; set; }

    private string? dir;

    public Dictionary<string, Dictionary<string, bool>> fileFormatsEnabled { get; set; }
    public Dictionary<string, bool> methodsEnabled { get; set; }

    public int? specifiedThreadCount { get; set; }
    public bool ignoreUnsupportedFileType { get; set; }


    /// <summary>
    /// Initialize the options. This must be called before any other function
    /// </summary>
    public void Initialize()
    {
        SetDirPath();
        profile = SettingsProfile.Default;

        fileFormatsEnabled = new Dictionary<string, Dictionary<string, bool>>();
        methodsEnabled = new Dictionary<string, bool>();

        LoadSettings();
    }

    /// <summary>
    /// Initialize enabled formats
    /// </summary>
    public void InitializeEnabledFormats()
    {
        var extensions = FileExtensions.list;

        // Get every field of FormatCodes, which are lists of pronom ids for every file type
        var fcFields = typeof(FormatCodes).GetFields();
        foreach (var f in fcFields)
        {
            var l = f.GetValue(null);
            if (l is ImmutableList<string> list)
            {
                var name = f.Name;

                var prefix = "PronomCodes";
                if (name.StartsWith(prefix))
                {
                    var ext = name.Substring(prefix.Length).ToLower();
                    if (extensions.Contains(ext))
                    {
                        fileFormatsEnabled.Add(ext, new Dictionary<string, bool>());
                        foreach (var fmt in list)
                        {
                            fileFormatsEnabled[ext][fmt] = true;
                        }
                    }
                }
            }
        }
    }


    /// <summary>
    /// Get if a method is enabled or not
    /// </summary>
    /// /// <param name="method">The name of the method</param>
    public bool GetMethod(string method)
    {
        if (methodsEnabled.ContainsKey(method))
        {
            return methodsEnabled[method];
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
    public void SetMethod(string method, bool? setTo = null)
    {
        if (!methodsEnabled.ContainsKey(method)) return;

        bool value;
        if (setTo != null)
        {
            value = setTo.Value;
        }
        else
        {
            value = !methodsEnabled[method];
        }

        methodsEnabled[method] = value;
    }


    /// <summary>
    /// Get if a file format is enabled or not
    /// </summary>
    /// /// <param name="pronomUID">The file type</param>
    public bool? GetFileFormat(string pronomUID)
    {
        foreach (var ft in fileFormatsEnabled.Keys)
        {
            if (fileFormatsEnabled[ft].ContainsKey(pronomUID))
            {
                return fileFormatsEnabled[ft][pronomUID];
            }
        }

        return null;
    }

    /// <summary>
    /// Set a file type to be enabled or not
    /// </summary>
    /// /// <param name="pronomUID">The file type</param>
    /// /// <param name="setTo">Enable or not. Leave out to toggle to its opposite value</param> 
    public void SetFormat(string pronomUID, bool? setTo = null)
    {
        string? filetype = null;
        foreach (var ft in fileFormatsEnabled.Keys)
        {
            if (fileFormatsEnabled[ft].ContainsKey(pronomUID))
            {
                filetype = ft;
                break;
            }
        }

        if (filetype == null) return;

        bool value;
        if (setTo != null)
        {
            value = setTo.Value;
        }
        else
        {
            value = !fileFormatsEnabled[filetype][pronomUID];
        }


        fileFormatsEnabled[filetype][pronomUID] = value;
    }

    /// <summary>
    /// Set all file formats of a filetype to be enabled or not
    /// </summary>
    /// <param name="filetype">The file type</param>
    /// <param name="setTo">Enable or not</param>
    public void SetFiletype(string filetype, bool setTo)
    {
        if (!fileFormatsEnabled.ContainsKey(filetype)) return;

        foreach (var fmt in fileFormatsEnabled[filetype].Keys)
        {
            fileFormatsEnabled[filetype][fmt] = setTo;
        }
    }


    /// <summary>
    /// Set the options to their default values
    /// </summary>
    public void SetDefaultSettings()
    {
        foreach (var m in Methods.GetList())
        {
            methodsEnabled[m.Name] = true;
        }

        InitializeEnabledFormats();

        specifiedThreadCount = null;
        ignoreUnsupportedFileType = true;
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
        return dir + "/" + profile switch
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
            Trace.WriteLine(jsonString);
            File.WriteAllText(path, jsonString);

            string checkJson = File.ReadAllText(path);
            Trace.WriteLine("Written JSON: " + checkJson);
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
                    this.fileFormatsEnabled = opt.fileFormatsEnabled;
                    this.methodsEnabled = opt.methodsEnabled;
                    this.specifiedThreadCount = opt.specifiedThreadCount;
                    this.ignoreUnsupportedFileType = opt.ignoreUnsupportedFileType;
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
