using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents solving statistics
/// </summary>
public sealed class Statistics
{
    private readonly Model _model;

/// <summary>
/// Gets solving time (in seconds)
/// </summary>
    public double SolvingTime => ScipNativeMethods.SCIPgetSolvingTime(_model.ScipHandle);

/// <summary>
/// Gets total number of nodes
/// </summary>
    public long TotalNodes => ScipNativeMethods.SCIPgetNNodes(_model.ScipHandle);

/// <summary>
/// Gets number of open nodes (remaining nodes)
/// </summary>
    public int OpenNodes => ScipNativeMethods.SCIPgetNNodesLeft(_model.ScipHandle);

/// <summary>
/// Gets primal bound
/// </summary>
    public double PrimalBound => ScipNativeMethods.SCIPgetPrimalbound(_model.ScipHandle);

/// <summary>
/// Gets dual bound
/// </summary>
    public double DualBound => ScipNativeMethods.SCIPgetDualbound(_model.ScipHandle);

/// <summary>
/// Gets gap
/// </summary>
    public double Gap => ScipNativeMethods.SCIPgetGap(_model.ScipHandle);

/// <summary>
/// Gets number of LP iterations
/// </summary>
    public long NLpIterations => ScipNativeMethods.SCIPgetNLPIterations(_model.ScipHandle);

/// <summary>
/// Gets number of solutions found
/// </summary>
    public int NSolutionsFound => ScipNativeMethods.SCIPgetNSols(_model.ScipHandle);

    internal Statistics(Model model)
    {
        _model = model;
    }

/// <summary>
/// Gets statistics summary
/// </summary>
    public override string ToString()
    {
        return $"SolvingTime: {SolvingTime:F2}s, " +
               $"Nodes: {TotalNodes}, " +
               $"OpenNodes: {OpenNodes}, " +
               $"PrimalBound: {PrimalBound:F4}, " +
               $"DualBound: {DualBound:F4}, " +
               $"Gap: {Gap:P2}";
    }
}
