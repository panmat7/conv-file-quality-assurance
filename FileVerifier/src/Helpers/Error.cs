using System.Collections.Generic;
using System.Linq;

namespace AvaloniaDraft.Helpers;

/// <summary>
/// Enum representing severity of an error. Default to unset.
/// </summary>
public enum ErrorSeverity
{
    Unset,
    Low,
    Medium,
    High,
    Internal //ONLY USE FOR CRASH DURING COMPARISON
}

public enum ErrorType
{
    Unset,
    Metadata,
    Visual,
    KnownErrorSource,
    FileError,
}

/// <summary>
/// Class used to store errors found during conversion.
/// </summary>
public class Error
{
    public string Name { get; }
    public string Description { get; }
    public ErrorSeverity Severity { get; }
    public ErrorType ErrorType { get; }
    public object? ErrorValue { get; }

    public Error()
    {
        Name = string.Empty;
        Description = string.Empty;
        Severity = ErrorSeverity.Unset;
        ErrorType = ErrorType.Unset;
        ErrorValue = null;
    }
    
    public Error(string name, string description, ErrorSeverity severity = ErrorSeverity.Unset, 
        ErrorType errorType = ErrorType.Unset, object? errorValue = null)
    {
        Name = name;
        Description = description;
        Severity = severity;
        ErrorType = errorType;
        ErrorValue = errorValue;
    }

    public string FormatErrorMessage()
    {
        return $"{Name}: {Description} \n\tError severity: {GetSeverityString()} \n\tError type: {GetErrorTypeString()}"
               + (ErrorValue == null ? "" : "\n\t" + ErrorValue.ToString());
    }

    private string GetSeverityString()
    {
        switch (Severity)
        {
            case ErrorSeverity.Unset: return "Unset";
            case ErrorSeverity.Low: return "Low";
            case ErrorSeverity.Medium: return "Medium";
            case ErrorSeverity.High: return "High";
            case ErrorSeverity.Internal: return "Internal";
        }
        
        return string.Empty;
    }

    private string GetErrorTypeString()
    {
        switch (ErrorType)
        {
            case ErrorType.Unset: return "Unset";
            case ErrorType.KnownErrorSource: return "Known source of errors";
            case ErrorType.Visual: return "Visual";
            case ErrorType.Metadata: return "Metadata";
            case ErrorType.FileError: return "File error";
        }
        
        return string.Empty;
    }
}

public static class ListExtencions
{
    public static string GenerateErrorString(this List<Error> errors)
    {
        if(errors == null || errors.Count == 0) return "No Errors Found";
        
        return errors.Select(e => e.FormatErrorMessage()).Aggregate((a, b) => $"{a}\n\n{b}");
    }
}