using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents a SCIP solution
/// </summary>
// 表示 SCIP 解
public sealed class Solution
{
    private readonly Model _model;
    private readonly IntPtr _solPtr;

    /// <summary>
    /// Gets the objective function value
    /// </summary>
    // 获取目标函数值
    public double ObjectiveValue { get; private set; }

    internal Solution(Model model, IntPtr solPtr, double objectiveValue)
    {
        _model = model;
        _solPtr = solPtr;
        ObjectiveValue = objectiveValue;
    }

    internal Solution(Model model, IntPtr solPtr)
        : this(model, solPtr, ScipNativeMethods.SCIPgetPrimalbound(model.ScipHandle))
    {
    }

    internal IntPtr SolPtr => _solPtr;

    /// <summary>
    /// Gets the value of the variable in the solution
    /// </summary>
    // 获取变量在解中的值
    public double GetValue(Variable variable)
    {
        return ScipNativeMethods.SCIPgetSolVal(_model.ScipHandle, _solPtr, variable.VarPtr);
    }

    /// <summary>
    /// Checks if the solution is feasible
    /// </summary>
    // 检查解是否可行
    public bool IsFeasible()
    {
        // TODO: Implement SCIPisFeasible call
        return true;
    }

    public override string ToString()
    {
        return $"Solution (obj={ObjectiveValue:F4})";
    }
}
