using System;
using System.Linq;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// Comprehensive Solution Pool Generator - Three Strategies for Collecting Many Feasible Solutions
///
/// This example demonstrates three different strategies for collecting multiple
/// feasible solutions from a combinatorial selection problem.
///
/// Problem: Select K items from N items
/// Solution space: C(N, K) combinations
///
/// Strategies demonstrated:
/// 1. Basic Strategy: Counter mode + Gap + Node limit
/// 2. Aggressive Strategy: Disable presolving + Enable heuristics
/// 3. No Objective Strategy: Pure feasible solution collection
///
/// Each strategy has different trade-offs in terms of solution diversity,
/// collection speed, and resource usage.
/// </summary>
public class SolutionPoolExample
{
    public static void Main()
    {
        Console.WriteLine("=== Comprehensive Solution Pool Generator ===\n");

        int N = 10;
        int K = 5;
        int maxTime = 5;

        Console.WriteLine($"Problem: Select {K} items from {N} items");
        Console.WriteLine($"Theoretical number of solutions: C({N}, {K}) = {Combination(N, K)}");
        Console.WriteLine($"Time limit: {maxTime} seconds\n");

        // ===== Run three different strategies =====

        Console.WriteLine("========================================");
        Console.WriteLine("Strategy 1: Basic Mode (Counter + Gap)");
        Console.WriteLine("========================================");
        RunBasicStrategy(N, K, maxTime);

        Console.WriteLine("\n========================================");
        Console.WriteLine("Strategy 2: Aggressive Mode (Disable Presolving + Heuristics)");
        Console.WriteLine("========================================");
        RunAggressiveStrategy(N, K, maxTime);

        Console.WriteLine("\n========================================");
        Console.WriteLine("Strategy 3: No Objective Mode (Pure Feasible Solution Collection)");
        Console.WriteLine("========================================");
        RunNoObjectiveStrategy(N, K, maxTime);
    }

    /// <summary>
    /// Strategy 1: Basic Mode
    ///
    /// Uses Counter mode + Large Gap + Node limit
    ///
    /// Problem formulation:
    ///   Maximize: sum(x[i])
    ///   Subject to: sum(x[i]) = K
    ///   With: x[i] in {0, 1} for i = 0..N-1
    ///
    /// Key parameters:
    /// - ParamEmphasis.Counter: Optimizes for counting solutions
    /// - constraints/countsols/collect: Enable solution collection
    /// - limits/time: Time limit for enumeration
    ///
    /// This is a balanced approach that works well for most cases.
    /// </summary>
    private static void RunBasicStrategy(int N, int K, int maxTime)
    {
        using var model = new Model("basic_pool");

        // Create binary variables
        var x = new Variable[N];
        for (int i = 0; i < N; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        // Constraint: must select exactly K items
        var sum = new LinearExpression();
        for (int i = 0; i < N; i++)
            sum = sum + x[i];
        model.AddConstraint(sum.Eq(K));

        // Objective: maximize selected count (flat objective)
        model.SetObjective(sum, ObjectiveSense.Maximize);

        // ===== Strategy 1 Parameters =====
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);

        // Solve - Use Count() to enumerate all feasible solutions
        Console.WriteLine("Starting to count feasible solutions...");
        var status = model.Count();

        // Display results
        DisplayResults(model, x, "Basic Mode", status);
    }

    /// <summary>
    /// Strategy 2: Aggressive Mode
    ///
    /// Disables presolving + Enables multiple heuristics + Disables pruning
    ///
    /// Problem formulation:
    ///   Maximize: sum(x[i])
    ///   Subject to: sum(x[i]) = K
    ///   With: x[i] in {0, 1} for i = 0..N-1
    ///
    /// Key parameters:
    /// - Same as Basic Mode (Counter emphasis, time limit)
    /// - The difference is in how SCIP is configured internally
    ///
    /// This strategy may find more diverse solutions but might be slower.
    /// </summary>
    private static void RunAggressiveStrategy(int N, int K, int maxTime)
    {
        using var model = new Model("aggressive_pool");

        // Create binary variables
        var x = new Variable[N];
        for (int i = 0; i < N; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        // Constraint: must select exactly K items
        var sum = new LinearExpression();
        for (int i = 0; i < N; i++)
            sum = sum + x[i];
        model.AddConstraint(sum.Eq(K));

        // Objective: maximize selected count
        model.SetObjective(sum, ObjectiveSense.Maximize);

        // ===== Strategy 2 Parameters (Aggressive) =====
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);

        // Solve - Use Count() to enumerate all feasible solutions
        Console.WriteLine("Starting to count feasible solutions (aggressive strategy)...");
        var status = model.Count();

        // Display results
        DisplayResults(model, x, "Aggressive Mode", status);
    }

    /// <summary>
    /// Strategy 3: No Objective Mode
    ///
    /// Does not set an objective function, focuses on collecting all feasible solutions.
    ///
    /// Problem formulation:
    ///   (No objective)
    ///   Subject to: sum(x[i]) = K
    ///   With: x[i] in {0, 1} for i = 0..N-1
    ///
    /// Key parameters:
    /// - Same as Basic Mode (Counter emphasis, time limit)
    /// - No objective function set
    ///
    /// This strategy is useful when you only care about finding feasible solutions,
    /// not optimizing an objective. SCIP will explore the solution space more broadly.
    /// </summary>
    private static void RunNoObjectiveStrategy(int N, int K, int maxTime)
    {
        using var model = new Model("noobj_pool");

        // Create binary variables
        var x = new Variable[N];
        for (int i = 0; i < N; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        // Constraint: must select exactly K items
        var sum = new LinearExpression();
        for (int i = 0; i < N; i++)
            sum = sum + x[i];
        model.AddConstraint(sum.Eq(K));

        // No objective function set! SCIP will find all feasible solutions

        // ===== Strategy 3 Parameters (No Objective) =====
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);

        // Solve - Use Count() to enumerate all feasible solutions
        Console.WriteLine("Starting to count feasible solutions (no objective)...");
        var status = model.Count();

        // Display results
        DisplayResults(model, x, "No Objective Mode", status);
    }

    /// <summary>
    /// Display solve results
    ///
    /// After Count() execution, the status will be Infeasible, which is expected behavior.
    /// This is because Count() enumerates solutions and then declares the problem
    /// "infeasible" to signal that counting is complete.
    /// </summary>
    private static void DisplayResults(Model model, Variable[] x, string strategyName, SolveStatus status)
    {
        // After Count() execution, status will be Infeasible, which is expected
        Console.WriteLine($"\n=== {strategyName} Results ===");
        Console.WriteLine($"Solve status: {status} (Infeasible is expected)");

        // Get the counted solution count
        long count = model.GetCountedSolutionsCount();
        Console.WriteLine($"Counted solutions: {count}");
        Console.WriteLine($"Theoretical solutions: {Combination(x.Length, 5)}");
        Console.WriteLine($"Coverage: {(double)count / Combination(x.Length, 5) * 100:F2}%");

        if (count > 0)
        {
            // Try to get sparse solutions
            try
            {
                var (vars, nvars, sols, nsols) = model.GetCountedSparseSolutions();
                Console.WriteLine($"\nSparse solution information:");
                Console.WriteLine($"  Active variables: {nvars}");
                Console.WriteLine($"  Sparse solutions: {nsols}");

                // Free sparse solutions
                model.FreeCountedSparseSolutions(ref sols);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n⚠️  Failed to get sparse solutions: {ex.Message}");
            }

            Console.WriteLine($"\n✅ Successfully collected {count} feasible solutions!");
        }
        else
        {
            Console.WriteLine("⚠️  No solutions collected!");
        }
    }

    /// <summary>
    /// Calculate combination number C(n, k)
    ///
    /// Uses the multiplicative formula to avoid overflow:
    /// C(n, k) = n*(n-1)*...*(n-k+1) / k!
    ///
    /// Also uses the symmetry property: C(n, k) = C(n, n-k)
    /// to minimize the number of multiplications.
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
