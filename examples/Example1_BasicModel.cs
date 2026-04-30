using System;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// Basic Model Example
/// Demonstrates how to create a simple linear programming model and solve it
/// 
/// This example shows:
/// - Creating a model with variables
/// - Setting an objective function (maximization)
/// - Adding constraints using natural syntax
/// - Solving the optimization problem
/// - Retrieving the optimal solution and statistics
/// 
/// Problem formulation:
///   Maximize: x + 2*y
///   Subject to:
///     x + y <= 5
///     2*x + y >= 3
///     x - y = 1
///     x, y are integers in [0, 10]
/// </summary>
public class Example1_BasicModel
{
    public static void Main1()
    {
        Console.WriteLine("SCIP.NET Basic Model Example");
        // Console.WriteLine($"SCIP.NET Version: {ScipNet.Version}");
        // Console.WriteLine($"SCIP Version: {ScipNet.ScipVersion}");
        Console.WriteLine();

        // Create a new model named "example"
        // The Model class implements IDisposable, so we use 'using' for proper cleanup
        using var model = new Model("example");

        // Create integer variables x and y with bounds [0, 10]
        // Variables can be: Integer, Continuous, Binary, or ImplicitInteger
        var x = model.AddVariable("x", 0, 10, VariableType.Integer);
        var y = model.AddVariable("y", 0, 10, VariableType.Integer);

        Console.WriteLine($"Created variables: {x}, {y}");

        // Set the objective function: maximize x + 2*y
        // ObjectiveSense can be Maximize or Minimize
        model.SetObjective(x + 2 * y, ObjectiveSense.Maximize);

        // Add constraints using natural syntax
        // Leq() = Less than or equal (<=)
        // Geq() = Greater than or equal (>=)
        // Eq()  = Equal (==)
        model.AddConstraint((x + y).Leq(5));
        model.AddConstraint((2 * x + y).Geq(3));
        model.AddConstraint((x - y).Eq(1));

        Console.WriteLine($"Added constraints: {model.Constraints.Count}");

        // Solve the optimization problem
        Console.WriteLine("Solving...");
        var status = model.Optimize();

        Console.WriteLine($"Solve status: {status}");

        // Retrieve the optimal solution if the problem was solved successfully
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

        // Get solver statistics
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
