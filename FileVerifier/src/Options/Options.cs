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

namespace AvaloniaDraft.Options;

/// <summary>
/// Contains options for conversion
/// </summary>
class Options
{
    public Dictionary<string, Dictionary<string, bool>> fileFormatsEnabled { get; set; }
    public Dictionary<string, bool> methodsEnabled { get; set; }

    public int? specifiedThreadCount { get; set; }
    public bool errorOnUnsupportedFileType { get; set; }


    /// <summary>
    /// Initialize the options. This must be called before any other function
    /// </summary>
    /// <param name="optionsJSONSrc">Json file to intialize from. Leave out to set default settings</param>
    public void Initialize(string? optionsJSONSrc = null)
    {
        fileFormatsEnabled = new Dictionary<string, Dictionary<string, bool>>();

        methodsEnabled = new Dictionary<string, bool>();

        if (optionsJSONSrc != null)
        {
            ImportJSON(optionsJSONSrc);
        }
        else
        {
            SetDefaultSettings();
        }
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
    public bool? GetMethod(string method)
    {
        if (!methodsEnabled.ContainsKey(method)) return null;
        return methodsEnabled[method];
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
        errorOnUnsupportedFileType = false;
    }

    /// <summary>
    /// Export the current options to a JSON file
    /// </summary>
    /// <param name="dir">The directory where the JSON file is to be exported</param>
    public void ExportJSON(string path)
    {
        try
        {
            string jsonString = JsonSerializer.Serialize(this);
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
    public void ImportJSON(string src)
    {
        try
        {
            var seralizerOptions = new JsonSerializerOptions { 
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
                this.errorOnUnsupportedFileType = opt.errorOnUnsupportedFileType;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to load settings", ex);
        }
    }
}
