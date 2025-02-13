using AvaloniaDraft.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AvaloniaDraft.Options;

/// <summary>
/// Contains options for conversion
/// </summary>
class Options
{
    private Dictionary<string, Dictionary<string, bool>> formatsEnabled;

    private Dictionary<string, bool> methodsEnabled;
    private Dictionary<string, bool> filetypesEnabled;

    public int? specifiedThreadCount { get; set; }
    public bool errorOnUnsupportedFileType { get; set; }


    public Options(string? optionsJSONSrc = null)
    {
        InitializeEnabledFormats();


        methodsEnabled = new Dictionary<string, bool>();
        filetypesEnabled = new Dictionary<string, bool>();

        if (optionsJSONSrc != null)
        {
            ImportJSON(optionsJSONSrc);
        }
        else
        {
            SetDefaultSettings();
        }
    }

    public void InitializeEnabledFormats()
    {
        formatsEnabled = new Dictionary<string, Dictionary<string, bool>>();

        var extensions = FileExtensions.list;

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
                    var ext = name.Substring(prefix.Length);
                    if (extensions.Contains(ext))
                    {
                        foreach (var fmt in list)
                        {
                            formatsEnabled[ext][fmt] = true;
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
    /// /// <param name="setTo">Enabled or not. Leave out to toggle to its opposite value</param> 
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
    /// Get if a method is enabled or not
    /// </summary>
    /// /// <param name="filetype">The file type</param>
    public bool? GetFiletype(string filetype)
    {
        if (!filetypesEnabled.ContainsKey(filetype)) return null;

        return filetypesEnabled[filetype];
    }

    /// <summary>
    /// Set a file type to be enabled or not
    /// </summary>
    /// /// <param name="filetype">The file type</param>
    /// /// <param name="setTo">Enable or not. Leave out to toggle to its opposite value</param> 
    public void SetFiletype(string filetype, bool? setTo = null)
    {
        if (!filetypesEnabled.ContainsKey(filetype)) return;

        bool value;
        if (setTo != null)
        {
            value = setTo.Value;
        }
        else
        {
            value = !filetypesEnabled[filetype];
        }

        filetypesEnabled[filetype] = value;
    }

    public void SetDefaultSettings()
    {
        foreach (var m in Methods.GetList())
        {
            methodsEnabled[m.Name] = true;
        }

        /*foreach (var ft in FileTypes.GetList())
        {
            filetypesEnabled[ft] = true;
        }*/

        specifiedThreadCount = null;
        errorOnUnsupportedFileType = false;
    }

    public void ExportJSON(string dir)
    {
        // TODO
    }

    public void ImportJSON(string src)
    {
        // TODO
    }
}
