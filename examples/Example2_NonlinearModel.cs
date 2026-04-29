using System;
using System.Linq;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// 非线性规划 + Indicator 约束示例
/// 演示非线性约束、非线性目标函数、解池和 Indicator 约束的使用
/// </summary>
public class Example2_NonlinearModel
{
    public static void Main1()
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
        Console.WriteLine("=== Example 3: Solution Pool (大量可行解) ===");
        RunSolutionPool();

        Console.WriteLine();

        // ===== 示例 4：Indicator 约束 =====
        Console.WriteLine("=== Example 4: Indicator Constraint (if z=1 then y <= 2) ===");
        RunIndicatorConstraint();

        Console.WriteLine();

        // ===== 示例 5：SetEmphasis - CP Solver 模式 =====
        Console.WriteLine("=== Example 5: SetEmphasis - CP Solver 模式 ===");
        RunSetEmphasisCPSolver();
    }

    /// <summary>
    /// 演示非线性约束：a ≤ x1/x2 ≤ b
    /// </summary>
    private static void RunDivisionConstraint()
    {
        using var model = new Model("division_constraint");

        var x1 = model.AddVariable("x1", 1.0, 10.0, VariableType.Continuous);
        var x2 = model.AddVariable("x2", 1.0, 10.0, VariableType.Continuous);

        model.SetObjective(x1 + x2, ObjectiveSense.Maximize);

        var ratio = (NonlinearExpression)x1 / x2;
        model.AddConstraint(ratio.Between(0.5, 2.0));

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
            }
        }
    }

    /// <summary>
    /// 演示非线性目标函数：minimize x^2 + y^2
    /// </summary>
    private static void RunNonlinearObjective()
    {
        using var model = new Model("nonlinear_objective");

        var x = model.AddVariable("x", 0.0, 10.0, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, 10.0, VariableType.Continuous);

        model.AddConstraint((x + y).Geq(3.0));

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
            }
        }
    }

    /// <summary>
    /// 演示解池：获取大量可行解
    /// 策略：二进制变量 + 平坦目标（产生大量最优解）+ 禁用presolving（强迫分支）
    /// </summary>
    private static void RunSolutionPool()
    {
        using var model = new Model("solution_pool");

        // 创建 50 个二进制变量，可行域 ≈ C(50,25) = 1.26e14
        int n = 50;
        var x = new Variable[n];
        for (int i = 0; i < n; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        Console.WriteLine($"变量：{n} 个二进制变量 (0-1)");
        Console.WriteLine("目标：maximize sum(x[i]) (平坦目标，所有系数=1)");
        Console.WriteLine("约束：sum(x[i]) <= 25 (可行域巨大)");
        Console.WriteLine("策略：禁用presolving + 大gap → SCIP持续分支，收集大量可行解");

        // === 目标：maximize sum(x[i]) - 平坦目标 ===
        var objExpr = new LinearExpression();
        for (int i = 0; i < n; i++)
            objExpr = objExpr + x[i];  // 所有权重都是 1
        // model.SetObjective(objExpr, ObjectiveSense.Maximize);

        // === 约束：sum(x[i]) <= 25 ===
        var sumExpr = new LinearExpression();
        for (int i = 0; i < n; i++)
            sumExpr = sumExpr + x[i];
        model.AddConstraint(sumExpr.Leq(25.0));

        Console.WriteLine($"约束数量：{model.Constraints.Count}");
        Console.WriteLine($"理论最优解数量：C(50,25) ≈ 1.26e14");

        // ----- 关键参数：让 SCIP 持续分支，收集大量解 -----
        // 解池容量
        model.SetIntParam("limits/maxsol", 100000);
        model.SetIntParam("limits/maxorigsol", 100000);

        // 关键：设置很大的 gap，阻止 SCIP 证明最优性
        // 这样 SCIP 会持续探索分支，产生大量不同可行解
        model.SetRealParam("limits/gap", 100.0);  // 100% gap

        // 时间限制：给足够时间探索
        model.SetRealParam("limits/time", 60);  // 60秒

        // 收集解
        model.SetBoolParam("constraints/countsols/collect", true);

        // 禁用 presolving，强迫进入分支
        try { model.SetIntParam("presolving/maxrounds", 0); } catch { }

        // 禁用一些快速启发式
        try { model.SetIntParam("heuristics/trivial/freq", -1); } catch { }

        model.SetIntParam("display/verblevel", 4);
        Console.WriteLine("\n开始求解（平坦目标 + 大gap，产生大量可行解）...");

        // ===== 调用优化并获取解池 =====
        Console.WriteLine("正在求解...");
        var status = model.Optimize();
        Console.WriteLine($"\n求解状态: {status}");

        // 获取解池中的所有解
        var solutions = model.GetSolutions();
        Console.WriteLine($"\n解池中解的数量: {solutions.Count}");

        if (solutions.Count > 0)
        {
            // 显示最优解
            var bestSol = model.GetBestSolution();
            if (bestSol != null)
            {
                Console.WriteLine($"\n=== 最优解 (目标值: {bestSol.ObjectiveValue:F2}) ===");
                var selected = string.Concat(x.Select(v => bestSol.GetValue(v) > 0.5 ? "1" : "0"));
                Console.WriteLine($"  选中变量: {selected.Substring(0, Math.Min(40, selected.Length))}...");
                Console.WriteLine($"  选中数量: {selected.Count(c => c == '1')}");
            }

            // 显示前20个解（展示多样性）
            int displayCount = Math.Min(20, solutions.Count);
            Console.WriteLine($"\n=== 前 {displayCount} 个解（展示多样性）===");
            for (int idx = 0; idx < displayCount; idx++)
            {
                var sol = solutions[idx];
                var selected = string.Concat(x.Select(v => sol.GetValue(v) > 0.5 ? "1" : "0"));
                Console.WriteLine($"  解 {idx + 1}: obj={sol.ObjectiveValue:F2}, " +
                    $"选中={selected.Count(c => c == '1')}");
            }

            if (solutions.Count > 20)
                Console.WriteLine($"  ... 还有 {solutions.Count - 20} 个解未显示");

            // 统计信息
            Console.WriteLine($"\n=== 解池统计 ===");
            Console.WriteLine($"  解数量: {solutions.Count}");
            Console.WriteLine($"  最优目标值: {solutions.Max(s => s.ObjectiveValue):F2}");
            Console.WriteLine($"  最差目标值: {solutions.Min(s => s.ObjectiveValue):F2}");
            Console.WriteLine($"  平均目标值: {solutions.Average(s => s.ObjectiveValue):F2}");

            // 检查解的差异性
            Console.WriteLine($"\n=== 解的差异性检查（前100个）===");
            int distinctCount = 1;
            var firstPattern = string.Concat(x.Select(v => solutions[0].GetValue(v) > 0.5 ? "1" : "0"));
            for (int i = 1; i < Math.Min(100, solutions.Count); i++)
            {
                var pattern = string.Concat(x.Select(v => solutions[i].GetValue(v) > 0.5 ? "1" : "0"));
                if (pattern != firstPattern)
                    distinctCount++;
            }
            Console.WriteLine($"  前100个解中有 {distinctCount} 个不同模式");
        }
        else
        {
            Console.WriteLine("\n未收集到解，尝试调整参数...");
            var bestSol = model.GetBestSolution();
            if (bestSol != null)
                Console.WriteLine($"最佳目标值: {bestSol.ObjectiveValue:F2}");
        }
    }

    /// <summary>
    /// 演示 Indicator 约束
    /// </summary>
    private static void RunIndicatorConstraint()
    {
        using var model = new Model("indicator_example");

        var z = model.AddVariable("z", 0, 1, VariableType.Binary);
        var x = model.AddVariable("x", 0, 20, VariableType.Continuous);
        var y = model.AddVariable("y", 0, 20, VariableType.Continuous);

        var yLeq5 = new LinearExpression().AddTerm(y, 1.0).Leq(5.0);
        model.AddConstraint(z.Implies(yLeq5));

        var xLeq10 = new LinearExpression().AddTerm(x, 1.0).Leq(10.0);
        model.AddConstraint(new IndicatorConstraint(z, xLeq10.Expression, xLeq10.Sense, xLeq10.RightHandSide));

        model.AddConstraint((x + y).Leq(12.0));
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

    /// <summary>
    /// 演示 SetEmphasis - CP Solver 模式
    /// 使用 SetEmphasis 将 SCIP 设置为纯 CP 求解器模式（禁用 LP 松弛）
    /// </summary>
    private static void RunSetEmphasisCPSolver()
    {
        using var cpModel = new Model("cp_test");
        var z = cpModel.AddVariable("z", 0, 1, VariableType.Binary);
        var x = cpModel.AddVariable("x", 0, 20, VariableType.Continuous);
        var y = cpModel.AddVariable("y", 0, 20, VariableType.Continuous);

        var yLeq5 = new LinearExpression().AddTerm(y, 1.0).Leq(5.0);
        cpModel.AddConstraint(z.Implies(yLeq5));

        cpModel.AddConstraint((x + y).Leq(12.0));
        cpModel.SetObjective(x + 2 * y, ObjectiveSense.Maximize);

        // 设置为 CP Solver 模式（禁用 LP 松弛，更像纯 CP 求解器）
        cpModel.SetEmphasis(ParamEmphasis.CPSolver);
        Console.WriteLine("使用 CP Solver 模式求解（禁用 LP 松弛）...");

        var status = cpModel.Optimize();
        Console.WriteLine($"CP Solver 状态: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = cpModel.GetBestSolution();
            if (solution != null)
            {
                Console.WriteLine($"最优值: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"z = {solution.GetValue(z):F4}");
                Console.WriteLine($"x = {solution.GetValue(x):F4}");
                Console.WriteLine($"y = {solution.GetValue(y):F4}");
            }
        }
    }
}
