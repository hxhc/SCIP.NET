using ScipNet.Core;

namespace ScipNet.Native;

/// <summary>
/// SCIP 异常基类
/// </summary>
public class ScipException : Exception
{
    /// <summary>
    /// 关联的返回码
    /// </summary>
    public ReturnCode ReturnCode { get; }

    /// <summary>
    /// 初始化新实例
    /// </summary>
    public ScipException(ReturnCode returnCode, string? message = null)
        : base(message ?? returnCode.ToString())
    {
        ReturnCode = returnCode;
    }

    /// <summary>
    /// 初始化新实例（带内部异常）
    /// </summary>
    public ScipException(ReturnCode returnCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ReturnCode = returnCode;
    }
}

/// <summary>
/// 内存不足异常
/// </summary>
public sealed class ScipMemoryException : ScipException
{
    public ScipMemoryException(string? message = null)
        : base(ReturnCode.NoMemory, message ?? "Insufficient memory")
    {
    }
}

/// <summary>
/// LP 求解器异常
/// </summary>
public sealed class ScipLpException : ScipException
{
    public ScipLpException(string? message = null)
        : base(ReturnCode.LpError, message ?? "LP solver error")
    {
    }
}

/// <summary>
/// 参数异常
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
/// 无效调用异常
/// </summary>
public sealed class ScipInvalidCallException : ScipException
{
    public ScipInvalidCallException(string? message = null)
        : base(ReturnCode.InvalidCall, message ?? "Invalid method call at this time")
    {
    }
}

/// <summary>
/// 不可行异常
/// </summary>
public sealed class ScipInfeasibleException : ScipException
{
    public ScipInfeasibleException(string? message = null)
        : base(ReturnCode.InvalidData, message ?? "Problem is infeasible")
    {
    }
}

/// <summary>
/// 错误处理工具类
/// </summary>
public static class ErrorHandler
{
    /// <summary>
    /// 检查返回码并在出错时抛出异常
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
    /// 检查返回码并返回是否成功
    /// </summary>
    public static bool TryCheckReturnCode(ReturnCode returnCode)
    {
        return returnCode == ReturnCode.Okay;
    }
}
