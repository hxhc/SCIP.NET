using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ScipNet.Native;

    /// <summary>
    /// SafeHandle implementation for SCIP handle
    /// </summary>
public sealed class ScipHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Initialize new instance
    /// </summary>
    public ScipHandle() : base(true)
    {
    }

    /// <summary>
    /// Initialize from existing pointer
    /// </summary>
    public ScipHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
    {
        SetHandle(handle);
    }

    /// <summary>
    /// Release handle
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
    /// Implicit conversion to IntPtr
    /// </summary>
    public static implicit operator IntPtr(ScipHandle scipHandle)
    {
        return scipHandle.DangerousGetHandle();
    }
}
