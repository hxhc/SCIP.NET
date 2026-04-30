using System;
using System.Linq;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// Solution Pool Example - Enumerate all feasible solutions using Count() + Sparse Solutions
///
/// Problem: Select K items from N items (C(N, K) combinations)
///
/// This example follows the official SCIP counting/sparse solution flow:
/// 1. Create model and variables
/// 2. Set Counter emphasis and enable solution collection
/// 3. Call Count() to enumerate all feasible solutions
/// 4. Call GetSparseSolutionsWithVariables() to retrieve concrete solution values
///
/// Each "sparse solution" may contain multiple "concrete solutions".
/// The unrolling is handled automatically by GetSparseSolutionsWithVariables().
/// </summary>
public class SolutionPoolExample
{
    public static void Main()
    {
        Console.WriteLine("=== Solution Pool Example: Enumerate All Feasible Solutions ===\n");

        int N = 10;
        int K = 5;
        int maxTime = 5;

        Console.WriteLine($"Problem: Select {K} items from {N} items");
        Console.WriteLine($"Theoretical solutions: C({N}, {K}) = {Combination(N, K)}");
        Console.WriteLine($"Time limit: {maxTime} seconds\n");

        using var model = new Model("solution_pool");

        // Step 1: Create binary variables
        var x = new Variable[N];
        for (int i = 0; i < N; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        // Step 2: Add constraint: sum(x[i]) == K (must select exactly K items)
        var sum = new LinearExpression();
        for (int i = 0; i < N; i++)
            sum = sum + x[i];
        model.AddConstraint(sum.Eq(K));

        // Step 3: Configure for counting
        // - Counter emphasis: optimized for enumeration
        // - countsols/collect: store solutions for later retrieval
        // - countsols/sollimit: max number of solutions to collect
        // - presolving/maxrounds: disable presolving to avoid variable transformation issues
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);
        model.SetIntParam("presolving/maxrounds", 0); // Disable presolving to keep original variables

        // Step 4: Count all feasible solutions
        // Note: Count() internally calls SCIPincludeConshdlrCountsols() to
        // register the countsols constraint handler, then SCIPsetParamsCountsols()
        // and SCIPcount() to perform the enumeration.
        Console.WriteLine("Counting feasible solutions...");
        var status = model.Count();
        Console.WriteLine($"Solve status: {status} (Infeasible is expected after counting)");

        // Step 5: Get the total count
        long totalCount = model.GetCountedSolutionsCount();
        Console.WriteLine($"\nTotal counted solutions: {totalCount}");
        Console.WriteLine($"Theoretical solutions:   {Combination(N, K)}");
        if (totalCount > 0)
            Console.WriteLine($"Coverage: {(double)totalCount / Combination(N, K) * 100:F2}%");

        // Step 6: Retrieve all sparse solutions and unroll into concrete solutions
        // This calls SCIPgetCountedSparseSols() to get sparse solution structs,
        // then SCIPsparseSolGetFirstSol/NextSol to iterate through each concrete solution.
        Console.WriteLine("\nRetrieving concrete solutions from sparse solutions...");
        var solutions = model.GetSparseSolutionsWithVariables();
        Console.WriteLine($"Retrieved {solutions.Count} concrete solutions");

        if (solutions.Count == 0)
        {
            Console.WriteLine("No solutions retrieved.");
            return;
        }

        // Step 7: Display all solutions with better formatting
        Console.WriteLine($"\n=== All {solutions.Count} Solutions ===");
        Console.WriteLine($"Format: [Sol#] Selected Items (count)");
        Console.WriteLine(new string('-', 60));

        for (int i = 0; i < solutions.Count; i++)
        {
            var sol = solutions[i];
            var selected = sol
                .Where(kvp => kvp.Value > 0.5)
                .Select(kvp => kvp.Key.Name)
                .OrderBy(n => n)
                .ToList();

            // Format: [001] x0, x1, x2, x3, x4 (5)
            string solNum = (i + 1).ToString("D3"); // Zero-padded, 3 digits
            string items = string.Join(", ", selected);
            Console.WriteLine($"[{solNum}] {items} ({selected.Count})");

            // Add extra line every 10 solutions for better readability
            if ((i + 1) % 10 == 0 && i + 1 < solutions.Count)
            {
                Console.WriteLine();
            }
        }
        Console.WriteLine(new string('-', 60));

        // Step 8: Verify each solution
        Console.WriteLine("\n=== Verification Results ===");

        // Check each solution
        int invalidCount = 0;
        var invalidSolutions = new List<int>();

        for (int i = 0; i < solutions.Count; i++)
        {
            int selectedCount = solutions[i].Count(kvp => kvp.Value > 0.5);
            if (selectedCount != K)
            {
                invalidCount++;
                invalidSolutions.Add(i + 1);
            }
        }

        if (invalidCount == 0)
        {
            Console.WriteLine($"  ✅ All {solutions.Count} solutions are valid!");
            Console.WriteLine($"  ✅ Each solution selects exactly {K} items as required");
        }
        else
        {
            Console.WriteLine($"  ⚠️  Found {invalidCount} invalid solutions:");
            Console.WriteLine($"  ⚠️  Invalid solution numbers: {string.Join(", ", invalidSolutions)}");
        }

        // Step 9: Statistical analysis - variable selection frequency
        Console.WriteLine("\n=== Variable Selection Frequency ===");
        var frequency = new Dictionary<string, int>();
        foreach (var sol in solutions)
        {
            foreach (var kvp in sol)
            {
                if (kvp.Value > 0.5)
                {
                    string varName = kvp.Key.Name;
                    frequency.TryGetValue(varName, out int count);
                    frequency[varName] = count + 1;
                }
            }
        }

        // Display frequency with visual bar
        int maxFreq = frequency.Values.Max();
        foreach (var varName in frequency.Keys.OrderBy(n => n))
        {
            int count = frequency[varName];
            double pct = (double)count / solutions.Count * 100;
            int barLength = (int)(pct / 2); // Scale to 50 chars max
            string bar = new string('█', barLength);
            Console.WriteLine($"  {varName}: {count}/{solutions.Count} ({pct:F1}%) {bar}");
        }

        // Step 10: Detailed Summary
        Console.WriteLine("\n=== Summary ===");
        Console.WriteLine($"  Problem:            Select {K} items from {N} items");
        Console.WriteLine($"  Theoretical:        C({N},{K}) = {Combination(N, K)}");
        Console.WriteLine($"  Counted:            {totalCount}");
        Console.WriteLine($"  Retrieved:          {solutions.Count}");
        Console.WriteLine($"  Coverage:           {(double)totalCount / Combination(N, K) * 100:F2}%");
        Console.WriteLine($"  Valid Solutions:    {solutions.Count - invalidCount}/{solutions.Count}");

        if (invalidCount == 0)
        {
            Console.WriteLine($"  Status:             ✅ SUCCESS - All solutions enumerated correctly!");
        }
        else
        {
            Console.WriteLine($"  Status:             ⚠️  WARNING - {invalidCount} invalid solutions found");
        }
    }

    /// <summary>
    /// Calculate combination number C(n, k)
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
