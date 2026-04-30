using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ScipNet.Native;

    /// <summary>
    /// SafeHandle implementation for SCIP handle
    /// </summary>
    // SCIP 句柄的安全句柄实现
public sealed class ScipHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Initialize new instance
    /// </summary>
    // 初始化新实例
    public ScipHandle() : base(true)
    {
    }

    /// <summary>
    /// Initialize from existing pointer
    /// </summary>
    // 从现有指针初始化
    public ScipHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
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
            ReturnCode ret = ScipNativeMethods.SCIPfree(ref handle);
            return ret == ReturnCode.Okay;
        }
        return false;
    }

    /// <summary>
    /// Implicit conversion to IntPtr
    /// </summary>
    // 隐式转换为 IntPtr
    public static implicit operator IntPtr(ScipHandle scipHandle)
    {
        return scipHandle.DangerousGetHandle();
    }
}
