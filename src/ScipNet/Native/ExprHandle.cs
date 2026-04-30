using Microsoft.Win32.SafeHandles;

namespace ScipNet.Native;

/// <summary>
/// SafeHandle implementation for SCIP expression handles
/// </summary>
public sealed class ExprHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Initialize from existing pointer
    /// </summary>
    public ExprHandle(IntPtr handle, bool ownsHandle) : base(ownsHandle)
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
            ReturnCode ret = ScipNativeMethods.SCIPreleaseExpr(IntPtr.Zero, ref handle);
            return ret == ReturnCode.Okay;
        }
        return false;
    }

    /// <summary>
    /// Implicit conversion to IntPtr
    /// </summary>
    public static implicit operator IntPtr(ExprHandle exprHandle)
    {
        return exprHandle.DangerousGetHandle();
    }
}
