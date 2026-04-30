using System;
using System.Linq;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// Advanced Model Example: Nonlinear Programming + Indicator Constraints
///
/// This example demonstrates:
/// - Nonlinear constraints (e.g., division, exponentiation)
/// - Nonlinear objective functions
/// - Solution pool for collecting multiple feasible solutions
/// - Indicator constraints (conditional constraints based on binary variables)
/// - CP Solver mode (Constraint Programming mode without LP relaxation)
///
/// Each example is in a separate method for clarity.
/// </summary>
public class Example2_NonlinearModel
{
    public static void Main1()
    {
        Console.WriteLine("SCIP.NET Advanced Model Example");
        Console.WriteLine();

        // ===== Example 1: Nonlinear Constraint =====
        Console.WriteLine("=== Example 1: Nonlinear Constraint (a <= x1/x2 <= b) ===");
        RunDivisionConstraint();

        Console.WriteLine();

        // ===== Example 2: Nonlinear Objective =====
        Console.WriteLine("=== Example 2: Nonlinear Objective (minimize x^2 + y^2) ===");
        RunNonlinearObjective();

        Console.WriteLine();

        // ===== Example 3: Solution Pool =====
        Console.WriteLine("=== Example 3: Solution Pool (Collecting many feasible solutions) ===");
        RunSolutionPool();

        Console.WriteLine();

        // ===== Example 4: Indicator Constraint =====
        Console.WriteLine("=== Example 4: Indicator Constraint (if z=1 then y <= 2) ===");
        RunIndicatorConstraint();

        Console.WriteLine();

        // ===== Example 5: SetEmphasis - CP Solver Mode =====
        Console.WriteLine("=== Example 5: SetEmphasis - CP Solver Mode ===");
        RunSetEmphasisCPSolver();
    }

    /// <summary>
    /// Example 1: Nonlinear Constraint - Division (a <= x1/x2 <= b)
    ///
    /// This demonstrates how to create a constraint with a nonlinear expression.
    /// The constraint requires the ratio of two variables to be within a range.
    ///
    /// Problem formulation:
    ///   Maximize: x1 + x2
    ///   Subject to: 0.5 <= x1/x2 <= 2.0
    ///   With: x1, x2 in [1.0, 10.0]
    ///
    /// Key points:
    /// - Use (NonlinearExpression) cast to enable nonlinear operations
    /// - NonlinearExpression supports: +, -, *, /, Pow, Exp, Log, Sqrt, Abs
    /// - Constraints can be created with Leq(), Geq(), Eq(), or Between()
    /// </summary>
    private static void RunDivisionConstraint()
    {
        using var model = new Model("division_constraint");

        // Create continuous variables x1 and x2
        var x1 = model.AddVariable("x1", 1.0, 10.0, VariableType.Continuous);
        var x2 = model.AddVariable("x2", 1.0, 10.0, VariableType.Continuous);

        // Set linear objective: maximize x1 + x2
        model.SetObjective(x1 + x2, ObjectiveSense.Maximize);

        // Create nonlinear constraint: 0.5 <= x1/x2 <= 2.0
        // Note: Cast to NonlinearExpression to use division operator
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
    /// Example 2: Nonlinear Objective Function - minimize x^2 + y^2
    ///
    /// This demonstrates how to create a nonlinear objective function.
    /// The objective is to minimize the Euclidean distance from the origin.
    ///
    /// Problem formulation:
    ///   Minimize: x^2 + y^2
    ///   Subject to: x + y >= 3.0
    ///   With: x, y in [0.0, 10.0]
    ///
    /// Key points:
    /// - Use NonlinearExpression.Pow(base, exponent) for power operations
    /// - Nonlinear expressions can be combined with +, -, *, /
    /// - The objective function can be linear or nonlinear
    /// </summary>
    private static void RunNonlinearObjective()
    {
        using var model = new Model("nonlinear_objective");

        // Create continuous variables x and y
        var x = model.AddVariable("x", 0.0, 10.0, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, 10.0, VariableType.Continuous);

        // Add linear constraint: x + y >= 3.0
        model.AddConstraint((x + y).Geq(3.0));

        // Create nonlinear objective: minimize x^2 + y^2
        // Use NonlinearExpression.Pow() for power operations
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
    /// Example 3: Solution Pool - Collecting Many Feasible Solutions
    ///
    /// This demonstrates how to use SCIP's solution pool to collect multiple
    /// feasible solutions from a combinatorial problem.
    ///
    /// Strategy:
    /// - Binary variables create a large feasible space (C(50,25) ≈ 1.26e14 combinations)
    /// - Flat objective function (all coefficients = 1) creates many optimal solutions
    /// - Disable presolving to force branching
    /// - Set large gap to prevent early optimality proof
    /// - Collect solutions as they are found
    ///
    /// Problem formulation:
    ///   Maximize: sum(x[i])
    ///   Subject to: sum(x[i]) <= 25
    ///   With: x[i] in {0, 1} for i = 0..49
    ///
    /// Key parameters:
    /// - limits/maxsol: Maximum number of solutions to store
    /// - limits/gap: Large gap (100%) prevents optimality proof
    /// - limits/time: Time limit for exploration
    /// - constraints/countsols/collect: Enable solution collection
    /// - presolving/maxrounds: Disable presolving (0 rounds)
    /// </summary>
    private static void RunSolutionPool()
    {
        using var model = new Model("solution_pool");

        // Create 50 binary variables, feasible space ≈ C(50,25) = 1.26e14
        int n = 50;
        var x = new Variable[n];
        for (int i = 0; i < n; i++)
            x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

        Console.WriteLine($"Variables: {n} binary variables (0-1)");
        Console.WriteLine("Objective: maximize sum(x[i]) (flat objective, all coefficients=1)");
        Console.WriteLine("Constraint: sum(x[i]) <= 25 (huge feasible space)");
        Console.WriteLine("Strategy: Disable presolving + large gap → SCIP keeps branching, collecting many feasible solutions");

        // === Objective: maximize sum(x[i]) - flat objective ===
        var objExpr = new LinearExpression();
        for (int i = 0; i < n; i++)
            objExpr = objExpr + x[i];  // All weights are 1
        // model.SetObjective(objExpr, ObjectiveSense.Maximize);

        // === Constraint: sum(x[i]) <= 25 ===
        var sumExpr = new LinearExpression();
        for (int i = 0; i < n; i++)
            sumExpr = sumExpr + x[i];
        model.AddConstraint(sumExpr.Leq(25.0));

        Console.WriteLine($"Number of constraints: {model.Constraints.Count}");
        Console.WriteLine($"Theoretical number of optimal solutions: C(50,25) ≈ 1.26e14");

        // ----- Key parameters: make SCIP keep branching to collect many solutions -----
        // Solution pool capacity
        model.SetIntParam("limits/maxsol", 100000);
        model.SetIntParam("limits/maxorigsol", 100000);

        // Key: Set large gap to prevent SCIP from proving optimality
        // This makes SCIP keep exploring branches and generating different feasible solutions
        model.SetRealParam("limits/gap", 100.0);  // 100% gap

        // Time limit: give enough time for exploration
        model.SetRealParam("limits/time", 60);  // 60 seconds

        // Collect solutions
        model.SetBoolParam("constraints/countsols/collect", true);

        // Disable presolving to force branching
        try { model.SetIntParam("presolving/maxrounds", 0); } catch { }

        // Disable some fast heuristics
        try { model.SetIntParam("heuristics/trivial/freq", -1); } catch { }

        model.SetIntParam("display/verblevel", 4);
        Console.WriteLine("\nStarting to solve (flat objective + large gap, generating many feasible solutions)...");

        // ===== Call optimization and get solution pool =====
        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"\nSolve status: {status}");

        // Get all solutions from the solution pool
        var solutions = model.GetSolutions();
        Console.WriteLine($"\nNumber of solutions in pool: {solutions.Count}");

        if (solutions.Count > 0)
        {
            // Display the best solution
            var bestSol = model.GetBestSolution();
            if (bestSol != null)
            {
                Console.WriteLine($"\n=== Best solution (objective value: {bestSol.ObjectiveValue:F2}) ===");
                var selected = string.Concat(x.Select(v => bestSol.GetValue(v) > 0.5 ? "1" : "0"));
                Console.WriteLine($"  Selected variables: {selected.Substring(0, Math.Min(40, selected.Length))}...");
                Console.WriteLine($"  Number selected: {selected.Count(c => c == '1')}");
            }

            // Display first 20 solutions (show diversity)
            int displayCount = Math.Min(20, solutions.Count);
            Console.WriteLine($"\n=== First {displayCount} solutions (showing diversity) ===");
            for (int idx = 0; idx < displayCount; idx++)
            {
                var sol = solutions[idx];
                var selected = string.Concat(x.Select(v => sol.GetValue(v) > 0.5 ? "1" : "0"));
                Console.WriteLine($"  Solution {idx + 1}: obj={sol.ObjectiveValue:F2}, " +
                    $"selected={selected.Count(c => c == '1')}");
            }

            if (solutions.Count > 20)
                Console.WriteLine($"  ... and {solutions.Count - 20} more solutions not shown");

            // Statistics
            Console.WriteLine($"\n=== Solution Pool Statistics ===");
            Console.WriteLine($"  Number of solutions: {solutions.Count}");
            Console.WriteLine($"  Best objective value: {solutions.Max(s => s.ObjectiveValue):F2}");
            Console.WriteLine($"  Worst objective value: {solutions.Min(s => s.ObjectiveValue):F2}");
            Console.WriteLine($"  Average objective value: {solutions.Average(s => s.ObjectiveValue):F2}");

            // Check solution diversity
            Console.WriteLine($"\n=== Solution Diversity Check (first 100) ===");
            int distinctCount = 1;
            var firstPattern = string.Concat(x.Select(v => solutions[0].GetValue(v) > 0.5 ? "1" : "0"));
            for (int i = 1; i < Math.Min(100, solutions.Count); i++)
            {
                var pattern = string.Concat(x.Select(v => solutions[i].GetValue(v) > 0.5 ? "1" : "0"));
                if (pattern != firstPattern)
                    distinctCount++;
            }
            Console.WriteLine($"  Found {distinctCount} different patterns in first 100 solutions");
        }
        else
        {
            Console.WriteLine("\nNo solutions collected, try adjusting parameters...");
            var bestSol = model.GetBestSolution();
            if (bestSol != null)
                Console.WriteLine($"Best objective value: {bestSol.ObjectiveValue:F2}");
        }
    }

    /// <summary>
    /// Example 4: Indicator Constraints
    ///
    /// This demonstrates how to create conditional constraints that are only active
    /// when a binary variable has a specific value.
    ///
    /// Problem formulation:
    ///   Maximize: x + 2*y
    ///   Subject to:
    ///     if z = 1 then y <= 5
    ///     if z = 1 then x <= 10
    ///     x + y <= 12
    ///   With: z in {0, 1}, x, y in [0, 20]
    ///
    /// Key points:
    /// - Use z.Implies(constraint) to create: if z=1 then constraint is active
    /// - Indicator constraints link binary decisions to continuous constraints
    /// - Two ways to create: using Implies() or using IndicatorConstraint constructor
    /// </summary>
    private static void RunIndicatorConstraint()
    {
        using var model = new Model("indicator_example");

        // Create binary variable z (decision) and continuous variables x, y
        var z = model.AddVariable("z", 0, 1, VariableType.Binary);
        var x = model.AddVariable("x", 0, 20, VariableType.Continuous);
        var y = model.AddVariable("y", 0, 20, VariableType.Continuous);

        // Create indicator constraint: if z = 1 then y <= 5
        // Using the convenient Implies() method
        var yLeq5 = new LinearExpression().AddTerm(y, 1.0).Leq(5.0);
        model.AddConstraint(z.Implies(yLeq5));

        // Create indicator constraint: if z = 1 then x <= 10
        // Using the IndicatorConstraint constructor directly
        var xLeq10 = new LinearExpression().AddTerm(x, 1.0).Leq(10.0);
        model.AddConstraint(new IndicatorConstraint(z, xLeq10.Expression, xLeq10.Sense, xLeq10.RightHandSide));

        // Add regular constraint
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
    /// Example 5: SetEmphasis - CP Solver Mode
    ///
    /// This demonstrates how to configure SCIP to behave more like a pure
    /// Constraint Programming (CP) solver by disabling LP relaxation.
    ///
    /// Problem formulation:
    ///   Maximize: x + 2*y
    ///   Subject to:
    ///     if z = 1 then y <= 5
    ///     x + y <= 12
    ///   With: z in {0, 1}, x, y in [0, 20]
    ///
    /// Key points:
    /// - SetEmphasis(ParamEmphasis.CPSolver) disables LP relaxation
    /// - CP mode is better for highly combinatorial problems
    /// - Trade-off: faster for some problems, but might not find optimal solution
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

        // Set to CP Solver mode (disable LP relaxation, behave more like a pure CP solver)
        cpModel.SetEmphasis(ParamEmphasis.CPSolver);
        Console.WriteLine("Solving with CP Solver mode (LP relaxation disabled)...");

        var status = cpModel.Optimize();
        Console.WriteLine($"CP Solver status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = cpModel.GetBestSolution();
            if (solution != null)
            {
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"z = {solution.GetValue(z):F4}");
                Console.WriteLine($"x = {solution.GetValue(x):F4}");
                Console.WriteLine($"y = {solution.GetValue(y):F4}");
            }
        }
    }
}
