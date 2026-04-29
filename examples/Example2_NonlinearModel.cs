using System;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// 非线性规划示例
/// 演示非线性约束、非线性目标函数和解池的使用
/// </summary>
public class Example2_NonlinearModel
{
    public static void Main()
    {
        Console.WriteLine("SCIP.NET Nonlinear Model Example");
        Console.WriteLine();

        // ===== 示例 1：非线性约束 =====
        Console.WriteLine("=== Example 1: Nonlinear Constraint (a <= x1/x2 <= b) ===");
        RunDivisionConstraint();

        Console.WriteLine();

        // ===== 示例 2：非线性目标函数 =====
        Console.WriteLine("=== Example 2: Nonlinear Objective (minimize x^2 + y^2) ===");
        RunNonlinearObjective();

        Console.WriteLine();

        // ===== 示例 3：解池 =====
        Console.WriteLine("=== Example 3: Solution Pool ===");
        RunSolutionPool();
    }

    /// <summary>
    /// 演示非线性约束：a ≤ x1/x2 ≤ b
    /// 即：0.5 ≤ x1/x2 ≤ 2.0
    /// 约束：maximize x1 + x2
    /// </summary>
    private static void RunDivisionConstraint()
    {
        using var model = new Model("division_constraint");

        // 连续变量 x1 ∈ [1, 10], x2 ∈ [1, 10]
        var x1 = model.AddVariable("x1", 1.0, 10.0, VariableType.Continuous);
        var x2 = model.AddVariable("x2", 1.0, 10.0, VariableType.Continuous);

        // 线性目标：maximize x1 + x2
        model.SetObjective(x1 + x2, ObjectiveSense.Maximize);

        // 非线性约束：0.5 <= x1/x2 <= 2.0
        // x1/x2 在表达式中表示为 x1 * x2^(-1)
        var ratio = (NonlinearExpression)x1 / x2;
        model.AddConstraint(ratio.Between(0.5, 2.0));

        Console.WriteLine($"Constraints: {model.Constraints.Count}");
        foreach (var cons in model.Constraints)
        {
            Console.WriteLine($"  {cons}");
        }

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x1 = {solution.GetValue(x1):F4}");
                Console.WriteLine($"x2 = {solution.GetValue(x2):F4}");
                Console.WriteLine($"x1/x2 = {solution.GetValue(x1) / solution.GetValue(x2):F4}");
            }
        }
    }

    /// <summary>
    /// 演示非线性目标函数：minimize x^2 + y^2
    /// 约束：x + y >= 3
    /// </summary>
    private static void RunNonlinearObjective()
    {
        using var model = new Model("nonlinear_objective");

        var x = model.AddVariable("x", 0.0, 10.0, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, 10.0, VariableType.Continuous);

        // 线性约束：x + y >= 3
        model.AddConstraint((x + y).Geq(3.0));

        // 非线性目标：minimize x^2 + y^2
        // 使用 NonlinearExpression 构建表达式树
        NonlinearExpression objExpr = NonlinearExpression.Pow(x, 2.0) + NonlinearExpression.Pow(y, 2.0);
        model.SetObjective(objExpr, ObjectiveSense.Minimize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x = {solution.GetValue(x):F4}");
                Console.WriteLine($"y = {solution.GetValue(y):F4}");
                Console.WriteLine($"x^2 + y^2 = {Math.Pow(solution.GetValue(x), 2) + Math.Pow(solution.GetValue(y), 2):F4}");
            }
        }
    }

    /// <summary>
    /// 演示解池：获取多个解
    /// 整数规划问题通常有多个可行解
    /// </summary>
    private static void RunSolutionPool()
    {
        using var model = new Model("solution_pool");

        // 设置解池大小
        model.SetIntParam("limits/maxsol", 50);

        var x = model.AddVariable("x", 0, 10, VariableType.Integer);
        var y = model.AddVariable("y", 0, 10, VariableType.Integer);
        var z = model.AddVariable("z", 0, 10, VariableType.Integer);

        model.SetObjective(x + y + z, ObjectiveSense.Maximize);
        model.AddConstraint((x + 2 * y + 3 * z).Leq(10));

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        // 从解池获取所有解
        var solutions = model.GetSolutions();
        Console.WriteLine($"Solution pool size: {solutions.Count}");

        // 显示前 10 个解
        int displayCount = Math.Min(solutions.Count, 10);
        for (int i = 0; i < displayCount; i++)
        {
            var sol = solutions[i];
            Console.WriteLine($"  Solution {i + 1}: obj={sol.ObjectiveValue:F2}, x={sol.GetValue(x)}, y={sol.GetValue(y)}, z={sol.GetValue(z)}");
        }

        if (solutions.Count > displayCount)
        {
            Console.WriteLine($"  ... and {solutions.Count - displayCount} more solutions");
        }
    }
}
