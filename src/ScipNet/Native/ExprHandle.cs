using Microsoft.Win32.SafeHandles;

namespace ScipNet.Native;

/// <summary>
/// SafeHandle implementation for SCIP expression handles
/// </summary>
// SCIP表达式句柄的SafeHandle实现
public sealed class ExprHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Initialize from existing pointer
    /// </summary>
    // 从现有指针初始化
    public ExprHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    /// <summary>
    /// Release handle
    /// </summary>
    // 释放句柄
    protected override bool ReleaseHandle()
    {
        if (!IsInvalid)
        {
            ReturnCode ret = ScipNativeMethods.SCIPreleaseExpr(IntPtr.Zero, ref handle);
            return ret == ReturnCode.Okay;
        }
        return false;
    }

    /// <summary>
    /// Implicit conversion to IntPtr
    /// </summary>
    // 隐式转换为IntPtr
    public static implicit operator IntPtr(ExprHandle exprHandle)
    {
        return exprHandle.DangerousGetHandle();
    }
}
