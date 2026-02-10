using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ScipNet.Native;

/// <summary>
/// SCIP 句柄的 SafeHandle 实现
/// </summary>
public sealed class ScipHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// 初始化新实例
    /// </summary>
    public ScipHandle() : base(true)
    {
    }

    /// <summary>
    /// 从现有指针初始化
    /// </summary>
    public ScipHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
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
            ReturnCode ret = ScipNativeMethods.SCIPfree(ref handle);
            return ret == ReturnCode.Okay;
        }
        return false;
    }

    /// <summary>
    /// 隐式转换为 IntPtr
    /// </summary>
    public static implicit operator IntPtr(ScipHandle scipHandle)
    {
        return scipHandle.DangerousGetHandle();
    }
}
