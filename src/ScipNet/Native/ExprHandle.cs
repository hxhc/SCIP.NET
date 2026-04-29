using Microsoft.Win32.SafeHandles;

namespace ScipNet.Native;

/// <summary>
/// SCIP 表达式句柄的 SafeHandle 实现
/// </summary>
public sealed class ExprHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// 从现有指针初始化
    /// </summary>
    public ExprHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    /// <summary>
    /// 释放句柄
    /// </summary>
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
    /// 隐式转换为 IntPtr
    /// </summary>
    public static implicit operator IntPtr(ExprHandle exprHandle)
    {
        return exprHandle.DangerousGetHandle();
    }
}
