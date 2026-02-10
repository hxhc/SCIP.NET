using System;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// 基础模型示例
/// 创建一个简单的线性规划模型并求解
/// </summary>
public class Example1_BasicModel
{
    public static void Main()
    {
        Console.WriteLine("SCIP.NET Basic Model Example");
        // Console.WriteLine($"SCIP.NET Version: {ScipNet.Version}");
        // Console.WriteLine($"SCIP Version: {ScipNet.ScipVersion}");
        Console.WriteLine();

        // 创建模型
        using var model = new Model("example");

        // 创建变量
        var x = model.AddVariable("x", 0, 10, VariableType.Integer);
        var y = model.AddVariable("y", 0, 10, VariableType.Integer);

        Console.WriteLine($"Created variables: {x}, {y}");

        // 设置目标函数：最大化 x + 2*y
        model.SetObjective(x + 2 * y, ObjectiveSense.Maximize);

        // 添加约束（使用自然语法）
        model.AddConstraint((x + y).Leq(5));
        model.AddConstraint((2 * x + y).Geq(3));
        model.AddConstraint((x - y).Eq(1));

        Console.WriteLine($"Added constraints: {model.Constraints.Count}");

        // 优化
        Console.WriteLine("Solving...");
        var status = model.Optimize();

        Console.WriteLine($"Solve status: {status}");

        // 获取解
        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x = {solution.GetValue(x):F4}");
                Console.WriteLine($"y = {solution.GetValue(y):F4}");
            }
        }

        // 获取统计信息
        var statistics = model.GetStatistics();
        Console.WriteLine();
        Console.WriteLine("Statistics:");
        Console.WriteLine($"  Solving time: {statistics.SolvingTime:F2}s");
        Console.WriteLine($"  Total nodes: {statistics.TotalNodes}");
        Console.WriteLine($"  Open nodes: {statistics.OpenNodes}");
        Console.WriteLine($"  Primal bound: {statistics.PrimalBound:F4}");
        Console.WriteLine($"  Dual bound: {statistics.DualBound:F4}");
        Console.WriteLine($"  Gap: {statistics.Gap:P2}");
        Console.WriteLine($"  LP iterations: {statistics.NLpIterations}");
        Console.WriteLine($"  Solutions found: {statistics.NSolutionsFound}");
    }
}
