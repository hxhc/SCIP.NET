using ScipNet.Core;

namespace ScipNet.Native;

/// <summary>
/// SCIP exception base class
/// </summary>
public class ScipException : Exception
{
    /// <summary>
    /// Associated return code
    /// </summary>
    public ReturnCode ReturnCode { get; }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public ScipException(ReturnCode returnCode, string? message = null)
        : base(message ?? returnCode.ToString())
    {
        ReturnCode = returnCode;
    }

    /// <summary>
    /// Initializes a new instance (with inner exception)
    /// </summary>
    public ScipException(ReturnCode returnCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ReturnCode = returnCode;
    }
}

/// <summary>
/// Insufficient memory exception
/// </summary>
public sealed class ScipMemoryException : ScipException
{
    public ScipMemoryException(string? message = null)
        : base(ReturnCode.NoMemory, message ?? "Insufficient memory")
    {
    }
}

/// <summary>
/// LP solver exception
/// </summary>
public sealed class ScipLpException : ScipException
{
    public ScipLpException(string? message = null)
        : base(ReturnCode.LpError, message ?? "LP solver error")
    {
    }
}

/// <summary>
/// Parameter exception
/// </summary>
public sealed class ScipParameterException : ScipException
{
    public ScipParameterException(string parameterName, ReturnCode returnCode)
        : base(returnCode, $"Parameter error for '{parameterName}': {returnCode}")
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}

/// <summary>
/// Invalid call exception
/// </summary>
public sealed class ScipInvalidCallException : ScipException
{
    public ScipInvalidCallException(string? message = null)
        : base(ReturnCode.InvalidCall, message ?? "Invalid method call at this time")
    {
    }
}

/// <summary>
/// Infeasible exception
/// </summary>
public sealed class ScipInfeasibleException : ScipException
{
    public ScipInfeasibleException(string? message = null)
        : base(ReturnCode.InvalidData, message ?? "Problem is infeasible")
    {
    }
}

/// <summary>
/// Error handling utility class
/// </summary>
public static class ErrorHandler
{
    /// <summary>
    /// Checks return code and throws exception on error
    /// </summary>
    public static void CheckReturnCode(ReturnCode returnCode, string? context = null)
    {
        if (returnCode == ReturnCode.Okay)
        {
            return;
        }

        var exception = returnCode switch
        {
            ReturnCode.NoMemory => new ScipMemoryException(context),
            ReturnCode.LpError => new ScipLpException(context),
            ReturnCode.ParameterUnknown or ReturnCode.ParameterWrongType or ReturnCode.ParameterWrongValue
                => new ScipParameterException(context ?? "unknown", returnCode),
            ReturnCode.InvalidCall => new ScipInvalidCallException(context),
            ReturnCode.InvalidData => new ScipInfeasibleException(context),
            _ => new ScipException(returnCode, context)
        };

        throw exception;
    }

    /// <summary>
    /// Checks return code and returns whether successful
    /// </summary>
    public static bool TryCheckReturnCode(ReturnCode returnCode)
    {
        return returnCode == ReturnCode.Okay;
    }
}
