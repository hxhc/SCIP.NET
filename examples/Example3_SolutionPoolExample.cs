using System;
using System.Linq;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// 综合解池生成器 - 包含三种策略收集大量可行解
/// 问题：从 N 个物品中选择 K 个
/// 解空间：C(N, K) 种组合
/// </summary>
public class SolutionPoolExample
{
    public static void Main()
    {
        Console.WriteLine("=== 综合解池生成器 ===\n");

        int N = 10;
        int K = 5;
        int maxTime = 5;

        Console.WriteLine($"问题：从 {N} 个物品中选择 {K} 个");
        Console.WriteLine($"理论解数：C({N}, {K}) = {Combination(N, K)}");
        Console.WriteLine($"时间限制：{maxTime} 秒\n");

        // ===== 运行三种不同的策略 =====

        Console.WriteLine("========================================");
        Console.WriteLine("策略 1: 基础模式 (Counter + Gap)");
        Console.WriteLine("========================================");
        RunBasicStrategy(N, K, maxTime);

        Console.WriteLine("\n========================================");
        Console.WriteLine("策略 2: 激进模式 (禁用 Presolving + 启发式)");
        Console.WriteLine("========================================");
        RunAggressiveStrategy(N, K, maxTime);

        Console.WriteLine("\n========================================");
        Console.WriteLine("策略 3: 无目标函数模式 (纯可行解收集)");
        Console.WriteLine("========================================");
        RunNoObjectiveStrategy(N, K, maxTime);
    }

    /// <summary>
    /// 策略 1: 基础模式
    /// 使用 Counter 模式 + 大 Gap + 节点限制
    /// </summary>
    private static void RunBasicStrategy(int N, int K, int maxTime)
    {
        using var model = new Model("basic_pool");

        // 创建变量
        var x = new Variable[N];
        for (int i = 0; i < N; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        // 约束：必须选择 K 个
        var sum = new LinearExpression();
        for (int i = 0; i < N; i++)
            sum = sum + x[i];
        model.AddConstraint(sum.Eq(K));

        // 目标：最大化选中数量（平坦目标）
        model.SetObjective(sum, ObjectiveSense.Maximize);

        // ===== 策略 1 参数 =====
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);

        // 求解 - 使用 Count() 来枚举所有可行解
        Console.WriteLine("开始计数可行解...");
        var status = model.Count();

        // 显示结果
        DisplayResults(model, x, "基础模式", status);
    }

    /// <summary>
    /// 策略 2: 激进模式
    /// 禁用 Presolving + 启用多种启发式 + 禁用剪枝
    /// </summary>
    private static void RunAggressiveStrategy(int N, int K, int maxTime)
    {
        using var model = new Model("aggressive_pool");

        // 创建变量
        var x = new Variable[N];
        for (int i = 0; i < N; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        // 约束：必须选择 K 个
        var sum = new LinearExpression();
        for (int i = 0; i < N; i++)
            sum = sum + x[i];
        model.AddConstraint(sum.Eq(K));

        // 目标：最大化选中数量
        model.SetObjective(sum, ObjectiveSense.Maximize);

        // ===== 策略 2 参数（激进） =====
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);

        // 求解 - 使用 Count() 来枚举所有可行解
        Console.WriteLine("开始计数可行解（激进策略）...");
        var status = model.Count();

        // 显示结果
        DisplayResults(model, x, "激进模式", status);
    }

    /// <summary>
    /// 策略 3: 无目标函数模式
    /// 不设置目标函数，专注于收集所有可行解
    /// </summary>
    private static void RunNoObjectiveStrategy(int N, int K, int maxTime)
    {
        using var model = new Model("noobj_pool");

        // 创建变量
        var x = new Variable[N];
        for (int i = 0; i < N; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        // 约束：必须选择 K 个
        var sum = new LinearExpression();
        for (int i = 0; i < N; i++)
            sum = sum + x[i];
        model.AddConstraint(sum.Eq(K));

        // 不设置目标函数！SCIP 会寻找所有可行解

        // ===== 策略 3 参数（无目标函数） =====
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);

        // 求解 - 使用 Count() 来枚举所有可行解
        Console.WriteLine("开始计数可行解（无目标函数）...");
        var status = model.Count();

        // 显示结果
        DisplayResults(model, x, "无目标函数模式", status);
    }

    /// <summary>
    /// 显示求解结果
    /// </summary>
    private static void DisplayResults(Model model, Variable[] x, string strategyName, SolveStatus status)
    {
        // Count() 执行后状态会是 Infeasible，这是预期行为
        Console.WriteLine($"\n=== {strategyName} 结果 ===");
        Console.WriteLine($"求解状态: {status} (Infeasible 是预期的)");

        // 获取计数的解数量
        long count = model.GetCountedSolutionsCount();
        Console.WriteLine($"计数的解数量: {count}");
        Console.WriteLine($"理论解数: {Combination(x.Length, 5)}");
        Console.WriteLine($"覆盖率: {(double)count / Combination(x.Length, 5) * 100:F2}%");

        if (count > 0)
        {
            // 尝试获取稀疏解
            try
            {
                var (vars, nvars, sols, nsols) = model.GetCountedSparseSolutions();
                Console.WriteLine($"\n稀疏解信息:");
                Console.WriteLine($"  活跃变量数: {nvars}");
                Console.WriteLine($"  稀疏解数: {nsols}");

                // 释放稀疏解
                model.FreeCountedSparseSolutions(ref sols);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n⚠️  获取稀疏解失败: {ex.Message}");
            }

            Console.WriteLine($"\n✅ 成功收集了 {count} 个可行解！");
        }
        else
        {
            Console.WriteLine("⚠️  未收集到任何解！");
        }
    }

    /// <summary>
    /// 计算组合数 C(n, k)
    /// </summary>
    private static long Combination(int n, int k)
    {
        if (k > n) return 0;
        if (k == 0 || k == n) return 1;
        if (k > n - k) k = n - k;

        long result = 1;
        for (int i = 1; i <= k; i++)
        {
            result = result * (n - i + 1) / i;
        }
        return result;
    }
}
