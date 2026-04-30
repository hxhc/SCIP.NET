using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents solving statistics
/// </summary>
// 表示求解统计信息
public sealed class Statistics
{
    private readonly Model _model;

/// <summary>
/// Gets solving time (in seconds)
/// </summary>
    // 获取求解时间（秒）
    public double SolvingTime => ScipNativeMethods.SCIPgetSolvingTime(_model.ScipHandle);

/// <summary>
/// Gets total number of nodes
/// </summary>
    // 获取节点总数
    public long TotalNodes => ScipNativeMethods.SCIPgetNNodes(_model.ScipHandle);

/// <summary>
/// Gets number of open nodes (remaining nodes)
/// </summary>
    // 获取开放节点数（剩余节点）
    public int OpenNodes => ScipNativeMethods.SCIPgetNNodesLeft(_model.ScipHandle);

/// <summary>
/// Gets primal bound
/// </summary>
    // 获取原始界
    public double PrimalBound => ScipNativeMethods.SCIPgetPrimalbound(_model.ScipHandle);

/// <summary>
/// Gets dual bound
/// </summary>
    // 获取对偶界
    public double DualBound => ScipNativeMethods.SCIPgetDualbound(_model.ScipHandle);

/// <summary>
/// Gets gap
/// </summary>
    // 获取间隙
    public double Gap => ScipNativeMethods.SCIPgetGap(_model.ScipHandle);

/// <summary>
/// Gets number of LP iterations
/// </summary>
    // 获取LP迭代次数
    public long NLpIterations => ScipNativeMethods.SCIPgetNLPIterations(_model.ScipHandle);

/// <summary>
/// Gets number of solutions found
/// </summary>
    // 获取找到的解的数量
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
