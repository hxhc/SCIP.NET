using System;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// 非线性规划 + Indicator 约束示例
/// 演示非线性约束、非线性目标函数、解池和 Indicator 约束的使用
/// </summary>
public class Example2_NonlinearModel
{
    public static void Main()
    {
        Console.WriteLine("SCIP.NET Advanced Model Example");
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

        Console.WriteLine();

        // ===== 示例 4：Indicator 约束 =====
        Console.WriteLine("=== Example 4: Indicator Constraint (if z=1 then y <= 2) ===");
        RunIndicatorConstraint();
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
    /// 通过扩大可行域 + 关键参数配置，让 SCIP 持续产生多个可行解
    /// </summary>
    private static void RunSolutionPool()
    {
        using var model = new Model("solution_pool");
        // 创建 30 个 0-1 变量（每个代表一个物品是否选取）
        int n = 30;
        var x = new Variable[n];
        for (int i = 0; i < n; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);
        // 目标：最大化总价值（价值 = i+1，线性递增）
        var obj = new LinearExpression();
        for (int i = 0; i < n; i++)
            obj = obj + (i + 1) * x[i];
        model.SetObjective(obj, ObjectiveSense.Maximize);
        // 容量约束：重量之和 ≤ 20（重量 = 1，确保大量可行组合）
        var weight = new LinearExpression();
        for (int i = 0; i < n; i++)
            weight = weight + x[i];
        model.AddConstraint(weight.Leq(20));
        // ----- 关键参数 -----
        model.SetIntParam("limits/solutions", 100);     // 收集100个解
        model.SetIntParam("limits/maxsol", 100000);     // 解池上限
        model.SetIntParam("limits/maxorigsol", 100000); // 原始问题解池
        model.SetRealParam("limits/gap", 1.0);          // 禁止最优性终止
        model.SetRealParam("limits/time", 30);          // 30秒安全阀
        model.SetIntParam("display/verblevel", 4);      // 显示进度
        Console.WriteLine("Solving 0-1 knapsack...");
        Console.WriteLine($"limits/solutions    = {model.GetIntParam("limits/solutions")}");
        Console.WriteLine($"limits/maxsol       = {model.GetIntParam("limits/maxsol")}");
        Console.WriteLine($"limits/maxorigsol   = {model.GetIntParam("limits/maxorigsol")}");
        Console.WriteLine($"limits/gap          = {model.GetRealParam("limits/gap")}");
        Console.WriteLine($"limits/time         = {model.GetRealParam("limits/time")}");


        var status = model.Optimize();
        var allSols = model.GetSolutions().ToList();
        Console.WriteLine($"Status: {status}");
        Console.WriteLine($"Total solutions found: {allSols.Count}");
        // 展示前5个解
        int show = Math.Min(allSols.Count, 5);
        for (int i = 0; i < show; i++)
        {
            var sol = allSols[i];
            var selected = string.Join("", x.Select(v => sol.GetValue(v) > 0.5 ? "1" : "0"));
            Console.WriteLine($"  Sol {i + 1}: obj={sol.ObjectiveValue:F2}, selected={selected}");
            Console.WriteLine($"model.getNsols()={model.SolutionCount}");
        }
    }

    /// <summary>
    /// 演示 Indicator 约束
    /// 场景：工厂选择
    ///   z = 1 表示开启工厂，z = 0 表示关闭
    ///   如果开启工厂 (z=1)，则产量 y <= capacity
    ///   目标：最大化利润 x + y
    /// </summary>
    private static void RunIndicatorConstraint()
    {
        using var model = new Model("indicator_example");

        // z: 是否开启工厂（二元变量）
        var z = model.AddVariable("z", 0, 1, VariableType.Binary);
        // x: 产品 A 的产量
        var x = model.AddVariable("x", 0, 20, VariableType.Continuous);
        // y: 产品 B 的产量
        var y = model.AddVariable("y", 0, 20, VariableType.Continuous);

        // Indicator 约束：z = 1 → y <= 5（如果开工厂，B 的产量不超过 5）
        // 方式 1：使用 Implies 方法
        var yLeq5 = new LinearExpression().AddTerm(y, 1.0).Leq(5.0);
        model.AddConstraint(z.Implies(yLeq5));

        // Indicator 约束：z = 1 → x <= 10（如果开工厂，A 的产量不超过 10）
        // 方式 2：直接构造 IndicatorConstraint
        var xLeq10 = new LinearExpression().AddTerm(x, 1.0).Leq(10.0);
        model.AddConstraint(new IndicatorConstraint(z, xLeq10.Expression, xLeq10.Sense, xLeq10.RightHandSide));

        // 普通约束：x + y <= 12（资源限制）
        model.AddConstraint((x + y).Leq(12.0));

        // 目标：最大化 x + 2*y
        model.SetObjective(x + 2 * y, ObjectiveSense.Maximize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"z (factory open) = {solution.GetValue(z):F4}");
                Console.WriteLine($"x (product A)    = {solution.GetValue(x):F4}");
                Console.WriteLine($"y (product B)    = {solution.GetValue(y):F4}");
            }
        }
    }
}
