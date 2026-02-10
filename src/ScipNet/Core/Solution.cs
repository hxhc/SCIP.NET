using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// 代表 SCIP 解
/// </summary>
public sealed class Solution
{
    private readonly Model _model;
    private readonly IntPtr _solPtr;

    /// <summary>
    /// 获取目标函数值
    /// </summary>
    public double ObjectiveValue { get; private set; }

    internal Solution(Model model, IntPtr solPtr)
    {
        _model = model;
        _solPtr = solPtr;
        ObjectiveValue = ScipNativeMethods.SCIPgetPrimalbound(_model.ScipHandle);
    }

    internal IntPtr SolPtr => _solPtr;

    /// <summary>
    /// 获取变量在解中的值
    /// </summary>
    public double GetValue(Variable variable)
    {
        return ScipNativeMethods.SCIPgetSolVal(_model.ScipHandle, _solPtr, variable.VarPtr);
    }

    /// <summary>
    /// 检查解是否可行
    /// </summary>
    public bool IsFeasible()
    {
        // TODO: 实现 SCIPisFeasible 调用
        return true;
    }

    public override string ToString()
    {
        return $"Solution (obj={ObjectiveValue:F4})";
    }
}
