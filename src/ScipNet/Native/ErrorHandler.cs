using ScipNet.Core;

namespace ScipNet.Native;

/// <summary>
/// SCIP exception base class
/// </summary>
// SCIP 异常基类
public class ScipException : Exception
{
    /// <summary>
    /// Associated return code
    /// </summary>
    // 关联的返回码
    public ReturnCode ReturnCode { get; }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    // 初始化新实例
    public ScipException(ReturnCode returnCode, string? message = null)
        : base(message ?? returnCode.ToString())
    {
        ReturnCode = returnCode;
    }

    /// <summary>
    /// Initializes a new instance (with inner exception)
    /// </summary>
    // 初始化新实例（包含内部异常）
    public ScipException(ReturnCode returnCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ReturnCode = returnCode;
    }
}

/// <summary>
/// Insufficient memory exception
/// </summary>
// 内存不足异常
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
// LP 求解器异常
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
// 参数异常
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
// 无效调用异常
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
// 不可行异常
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
// 错误处理工具类
public static class ErrorHandler
{
    /// <summary>
    /// Checks return code and throws exception on error
    /// </summary>
    // 检查返回码，错误时抛出异常
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
    // 检查返回码，返回是否成功
    public static bool TryCheckReturnCode(ReturnCode returnCode)
    {
        return returnCode == ReturnCode.Okay;
    }
}
