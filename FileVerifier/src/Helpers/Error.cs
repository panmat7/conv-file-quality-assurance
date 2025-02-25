namespace AvaloniaDraft.Helpers;

/// <summary>
/// Enum representing severity of an error. Default to unset.
/// </summary>
public enum ErrorSeverity
{
    Unset,
    Low,
    Medium,
    High
}

public enum ErrorType
{
    Unset,
    Metadata,
    Visual,
    KnownErrorSource,
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
}