using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents a SCIP solution
/// </summary>
public sealed class Solution
{
    private readonly Model _model;
    private readonly IntPtr _solPtr;

    /// <summary>
    /// Gets the objective function value
    /// </summary>
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
    public double GetValue(Variable variable)
    {
        return ScipNativeMethods.SCIPgetSolVal(_model.ScipHandle, _solPtr, variable.VarPtr);
    }

    /// <summary>
    /// Checks if the solution is feasible
    /// </summary>
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
