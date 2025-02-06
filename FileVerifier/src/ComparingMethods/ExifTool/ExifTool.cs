using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AvaloniaDraft.ComparingMethods.ExifTool;

public class ExifTool : IDisposable
{
    private readonly Process _process;
    private readonly StreamWriter _stdin;
    private readonly StreamReader _stdout;
    private readonly StreamReader _stderr;
    private bool _disposed;

    public ExifTool()
    {
        var path = GetExifPath();

        var psi = new ProcessStartInfo
        {
            FileName = path,
            Arguments = "-stay_open True -@",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        _process = new Process();
        _process.StartInfo = psi;

        try
        {
            _process.Start();
            _stdin = _process.StandardInput;
            _stdout = _process.StandardOutput;
            _stderr = _process.StandardError;
        }
        catch
        {
            //Should program continue running when unable to create exiftool instance?
        }
    }
    
    ~ExifTool() { Dispose(); }

    private static string? GetExifPath()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                return curDir + @"\FileVerifier\src\ComparingMethods\ExifTool\exiftool.exe";
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        

        return null;
    }

    public async Task<List<Dictionary<string, object>>?> GetExifData(string[] files)
    {
        if(_disposed) return null; //Maybe throw exception?

        var command = $"-j -quiet {string.Join(" ", files)}";
        
        await _stdin.WriteAsync(command);
        await _stdin.WriteAsync("-execute");
        await _stdin.FlushAsync();
        
        
        
        return null;
    }
    
    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            _stdin.WriteLine("-stay_open\nFalse");
            _stdin.Flush();
            _process.WaitForExit(1000);
        }
        catch
        {
            // Ignore if process is already dead
        }

        _stdin?.Dispose();
        _stdout?.Dispose();
        _stderr?.Dispose();
        _process?.Dispose();
        _disposed = true;
    }
}