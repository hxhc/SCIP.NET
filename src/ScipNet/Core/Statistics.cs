using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// 代表求解统计信息
/// </summary>
public sealed class Statistics
{
    private readonly Model _model;

    /// <summary>
    /// 获取求解时间（秒）
    /// </summary>
    public double SolvingTime => ScipNativeMethods.SCIPgetSolvingTime(_model.ScipHandle);

    /// <summary>
    /// 获取总节点数
    /// </summary>
    public long TotalNodes => ScipNativeMethods.SCIPgetNNodes(_model.ScipHandle);

    /// <summary>
    /// 获取开放节点数（剩余节点数）
    /// </summary>
    public int OpenNodes => ScipNativeMethods.SCIPgetNNodesLeft(_model.ScipHandle);

    /// <summary>
    /// 获取原始界
    /// </summary>
    public double PrimalBound => ScipNativeMethods.SCIPgetPrimalbound(_model.ScipHandle);

    /// <summary>
    /// 获取对偶界
    /// </summary>
    public double DualBound => ScipNativeMethods.SCIPgetDualbound(_model.ScipHandle);

    /// <summary>
    /// 获取间隙
    /// </summary>
    public double Gap => ScipNativeMethods.SCIPgetGap(_model.ScipHandle);

    /// <summary>
    /// 获取 LP 迭代次数
    /// </summary>
    public long NLpIterations => ScipNativeMethods.SCIPgetNLPIterations(_model.ScipHandle);

    /// <summary>
    /// 获取找到的解数量
    /// </summary>
    public int NSolutionsFound => ScipNativeMethods.SCIPgetNSols(_model.ScipHandle);

    internal Statistics(Model model)
    {
        _model = model;
    }

    /// <summary>
    /// 获取统计摘要
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
