using System;
using System.Linq;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// Nonlinear Model Example: Nonlinear Functions and Constraints
///
/// This example demonstrates the use of all supported nonlinear functions:
/// - Arithmetic: +, -, *, /
/// - Power: Pow(x, exponent)
/// - Exponential: Exp(x)
/// - Logarithm: Log(x) - natural logarithm
/// - Square root: Sqrt(x)
/// - Absolute value: Abs(x)
/// - Trigonometric: Sin(x), Cos(x) - input in radians
///
/// Each example demonstrates a different nonlinear function or combination.
/// </summary>
public class Example2_NonlinearModel
{
    public static void Main1()
    {
        Console.WriteLine("SCIP.NET Nonlinear Model Example");
        Console.WriteLine();

        // ===== Example 1: Division Constraint =====
        Console.WriteLine("=== Example 1: Nonlinear Constraint - Division (a <= x1/x2 <= b) ===");
        RunDivisionConstraint();

        Console.WriteLine();

        // ===== Example 2: Power Function =====
        Console.WriteLine("=== Example 2: Nonlinear Objective - Power Function (minimize x^2 + y^2) ===");
        RunPowerFunction();

        Console.WriteLine();

        // ===== Example 3: Exponential Function =====
        Console.WriteLine("=== Example 3: Exponential Function (minimize e^x + e^y) ===");
        RunExponentialFunction();

        Console.WriteLine();

        // ===== Example 4: Logarithm Function =====
        Console.WriteLine("=== Example 4: Logarithm Function (minimize log(x) + log(y)) ===");
        RunLogarithmFunction();

        Console.WriteLine();

        // ===== Example 5: Absolute Value =====
        Console.WriteLine("=== Example 5: Absolute Value (minimize |x| + |y|) ===");
        RunAbsoluteValue();

        Console.WriteLine();

        // ===== Example 6: Square Root =====
        Console.WriteLine("=== Example 6: Square Root (minimize sqrt(x) + sqrt(y)) ===");
        RunSquareRoot();

        Console.WriteLine();

        // ===== Example 7: Sin and Cos Functions =====
        Console.WriteLine("=== Example 7: Trigonometric Functions (minimize sin(x) + cos(y)) ===");
        RunTrigonometricObjective();

        Console.WriteLine();

        // ===== Example 8: Trigonometric Constraint =====
        Console.WriteLine("=== Example 8: Trigonometric Constraint (sin(x) >= 0.5) ===");
        RunTrigonometricConstraint();

        Console.WriteLine();

        // ===== Example 9: Complex Trigonometric Expression =====
        Console.WriteLine("=== Example 9: Complex Expression (sin^2(x) + cos^2(x)) ===");
        RunComplexTrigonometric();

        Console.WriteLine();

        // ===== Example 10: Combined Nonlinear Functions =====
        Console.WriteLine("=== Example 10: Combined Functions (minimize x^2 + exp(y) + |z|) ===");
        RunCombinedFunctions();
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
    /// - NonlinearExpression supports: +, -, *, /, Pow, Exp, Log, Sqrt, Abs, Sin, Cos
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
                Console.WriteLine($"x1/x2 = {solution.GetValue(x1) / solution.GetValue(x2):F4}");
            }
        }
    }

    /// <summary>
    /// Example 2: Power Function - minimize x^2 + y^2
    ///
    /// This demonstrates how to create a nonlinear objective function using Pow().
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
    private static void RunPowerFunction()
    {
        using var model = new Model("power_function");

        // Create continuous variables x and y
        var x = model.AddVariable("x", 0.0, 10.0, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, 10.0, VariableType.Continuous);

        // Add linear constraint: x + y >= 3.0
        model.AddConstraint((x + y).Geq(3.0));

        // Create nonlinear objective: minimize x^2 + y^2
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
                Console.WriteLine($"x^2 = {Math.Pow(solution.GetValue(x), 2):F4}");
                Console.WriteLine($"y^2 = {Math.Pow(solution.GetValue(y), 2):F4}");
            }
        }
    }

    /// <summary>
    /// Example 3: Exponential Function - minimize e^x + e^y
    ///
    /// This demonstrates the use of Exp() for exponential functions.
    ///
    /// Problem formulation:
    ///   Minimize: e^x + e^y
    ///   Subject to: x + y >= 2.0
    ///   With: x, y in [0.0, 5.0]
    ///
    /// Key points:
    /// - Use NonlinearExpression.Exp(x) for e^x
    /// - Exponential functions grow very quickly
    /// - Useful for modeling growth or decay processes
    /// </summary>
    private static void RunExponentialFunction()
    {
        using var model = new Model("exponential_function");

        var x = model.AddVariable("x", 0.0, 5.0, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, 5.0, VariableType.Continuous);

        // Add constraint
        model.AddConstraint((x + y).Geq(2.0));

        // Create objective: minimize e^x + e^y
        var objExpr = NonlinearExpression.Exp(x) + NonlinearExpression.Exp(y);
        model.SetObjective(objExpr, ObjectiveSense.Minimize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                double yVal = solution.GetValue(y);
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x = {xVal:F4}, e^x = {Math.Exp(xVal):F4}");
                Console.WriteLine($"y = {yVal:F4}, e^y = {Math.Exp(yVal):F4}");
            }
        }
    }

    /// <summary>
    /// Example 4: Logarithm Function - minimize log(x) + log(y)
    ///
    /// This demonstrates the use of Log() for natural logarithm.
    ///
    /// Problem formulation:
    ///   Maximize: log(x) + log(y)
    ///   Subject to: x + y <= 20.0
    ///   With: x, y in [1.0, 10.0]
    ///
    /// Key points:
    /// - Use NonlinearExpression.Log(x) for natural logarithm (ln)
    /// - Logarithm is only defined for positive values
    /// - Useful for multiplicative relationships
    /// </summary>
    private static void RunLogarithmFunction()
    {
        using var model = new Model("logarithm_function");

        var x = model.AddVariable("x", 1.0, 10.0, VariableType.Continuous);
        var y = model.AddVariable("y", 1.0, 10.0, VariableType.Continuous);

        // Add constraint
        model.AddConstraint((x + y).Leq(20.0));

        // Create objective: maximize log(x) + log(y)
        var objExpr = NonlinearExpression.Log(x) + NonlinearExpression.Log(y);
        model.SetObjective(objExpr, ObjectiveSense.Maximize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                double yVal = solution.GetValue(y);
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x = {xVal:F4}, log(x) = {Math.Log(xVal):F4}");
                Console.WriteLine($"y = {yVal:F4}, log(y) = {Math.Log(yVal):F4}");
            }
        }
    }

    /// <summary>
    /// Example 5: Absolute Value - minimize |x| + |y|
    ///
    /// This demonstrates the use of Abs() for absolute value.
    ///
    /// Problem formulation:
    ///   Minimize: |x| + |y|
    ///   Subject to: x + y >= 5.0
    ///   With: x, y in [-10.0, 10.0]
    ///
    /// Key points:
    /// - Use NonlinearExpression.Abs(x) for absolute value |x|
    /// - Absolute value creates a "V" shaped function
    /// - Useful for modeling deviations or distances
    /// </summary>
    private static void RunAbsoluteValue()
    {
        using var model = new Model("absolute_value");

        var x = model.AddVariable("x", -10.0, 10.0, VariableType.Continuous);
        var y = model.AddVariable("y", -10.0, 10.0, VariableType.Continuous);

        // Add constraint
        model.AddConstraint((x + y).Geq(5.0));

        // Create objective: minimize |x| + |y|
        var objExpr = NonlinearExpression.Abs(x) + NonlinearExpression.Abs(y);
        model.SetObjective(objExpr, ObjectiveSense.Minimize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                double yVal = solution.GetValue(y);
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x = {xVal:F4}, |x| = {Math.Abs(xVal):F4}");
                Console.WriteLine($"y = {yVal:F4}, |y| = {Math.Abs(yVal):F4}");
            }
        }
    }

    /// <summary>
    /// Example 6: Square Root - minimize sqrt(x) + sqrt(y)
    ///
    /// This demonstrates the use of Sqrt() for square root.
    /// Note: Sqrt(x) is equivalent to Pow(x, 0.5).
    ///
    /// Problem formulation:
    ///   Minimize: sqrt(x) + sqrt(y)
    ///   Subject to: x + y >= 10.0
    ///   With: x, y in [0.0, 20.0]
    ///
    /// Key points:
    /// - Use NonlinearExpression.Sqrt(x) for square root
    /// - Square root is concave (increasing at decreasing rate)
    /// - Useful for diminishing returns models
    /// </summary>
    private static void RunSquareRoot()
    {
        using var model = new Model("square_root");

        var x = model.AddVariable("x", 0.0, 20.0, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, 20.0, VariableType.Continuous);

        // Add constraint
        model.AddConstraint((x + y).Geq(10.0));

        // Create objective: minimize sqrt(x) + sqrt(y)
        var objExpr = NonlinearExpression.Sqrt(x) + NonlinearExpression.Sqrt(y);
        model.SetObjective(objExpr, ObjectiveSense.Minimize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                double yVal = solution.GetValue(y);
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x = {xVal:F4}, sqrt(x) = {Math.Sqrt(xVal):F4}");
                Console.WriteLine($"y = {yVal:F4}, sqrt(y) = {Math.Sqrt(yVal):F4}");
            }
        }
    }

    /// <summary>
    /// Example 7: Trigonometric Functions - minimize sin(x) + cos(y)
    ///
    /// This demonstrates basic usage of sin() and cos() functions.
    ///
    /// Problem formulation:
    ///   Minimize: sin(x) + cos(y)
    ///   Subject to: x, y in [0, 2*PI]
    ///
    /// Key points:
    /// - Use NonlinearExpression.Sin(x) for sine
    /// - Use NonlinearExpression.Cos(x) for cosine
    /// - Input is in RADIANS (not degrees)
    /// - To convert degrees to radians: radians = degrees * Math.PI / 180
    /// </summary>
    private static void RunTrigonometricObjective()
    {
        using var model = new Model("trigonometric_objective");

        // Create continuous variables in radians [0, 2*PI]
        double twoPI = 2.0 * Math.PI;
        var x = model.AddVariable("x", 0.0, twoPI, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, twoPI, VariableType.Continuous);

        // Create objective: minimize sin(x) + cos(y)
        NonlinearExpression objExpr = NonlinearExpression.Sin(x) + NonlinearExpression.Cos(y);
        model.SetObjective(objExpr, ObjectiveSense.Minimize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                double yVal = solution.GetValue(y);
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F6}");
                Console.WriteLine($"x = {xVal:F6} radians ({xVal * 180.0 / Math.PI:F2} degrees)");
                Console.WriteLine($"y = {yVal:F6} radians ({yVal * 180.0 / Math.PI:F2} degrees)");
                Console.WriteLine($"sin(x) = {Math.Sin(xVal):F6}");
                Console.WriteLine($"cos(y) = {Math.Cos(yVal):F6}");
            }
        }
    }

    /// <summary>
    /// Example 8: Trigonometric Constraint - sin(x) >= 0.5
    ///
    /// This demonstrates using trigonometric functions in constraints.
    ///
    /// Problem formulation:
    ///   Maximize: x
    ///   Subject to: sin(x) >= 0.5
    ///   With: x in [0, 2*PI]
    ///
    /// Key points:
    /// - Trigonometric functions can be used in constraints
    /// - Creates feasible regions based on trigonometric conditions
    /// - Useful for periodic constraints or angle-based problems
    /// </summary>
    private static void RunTrigonometricConstraint()
    {
        using var model = new Model("trigonometric_constraint");

        double twoPI = 2.0 * Math.PI;
        var x = model.AddVariable("x", 0.0, twoPI, VariableType.Continuous);

        // Add constraint: sin(x) >= 0.5
        var sinX = NonlinearExpression.Sin(x);
        model.AddConstraint(sinX.Geq(0.5));

        // Maximize x
        model.SetObjective(x, ObjectiveSense.Maximize);

        Console.WriteLine("Solving...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                Console.WriteLine($"Optimal value (x): {xVal:F6} radians ({xVal * 180.0 / Math.PI:F2} degrees)");
                Console.WriteLine($"sin(x) = {Math.Sin(xVal):F6}");
                Console.WriteLine($"Constraint sin(x) >= 0.5: {Math.Sin(xVal) >= 0.5}");
            }
        }
    }

    /// <summary>
    /// Example 9: Complex Trigonometric Expression - sin(x)^2 + cos(x)^2
    ///
    /// This demonstrates combining trigonometric functions with other operations.
    ///
    /// Problem formulation:
    ///   Minimize: sin(x)^2 + cos(x)^2 (should be 1.0)
    ///   Subject to: x in [0, PI]
    ///
    /// This is a test case - the objective should always be 1.0
    /// due to the trigonometric identity: sin^2(x) + cos^2(x) = 1
    ///
    /// Key points:
    /// - Can combine trigonometric functions with Pow()
    /// - Demonstrates expression composition
    /// - Useful for verifying trigonometric identities in optimization
    /// </summary>
    private static void RunComplexTrigonometric()
    {
        using var model = new Model("complex_trigonometric");

        var x = model.AddVariable("x", 0.0, Math.PI, VariableType.Continuous);

        // Create expression: sin(x)^2 + cos(x)^2
        var sinX = NonlinearExpression.Sin(x);
        var cosX = NonlinearExpression.Cos(x);
        var sinSquared = NonlinearExpression.Pow(sinX, 2.0);
        var cosSquared = NonlinearExpression.Pow(cosX, 2.0);
        var objExpr = sinSquared + cosSquared;

        model.SetObjective(objExpr, ObjectiveSense.Minimize);

        Console.WriteLine("Solving sin(x)^2 + cos(x)^2...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F6} (expected: 1.0)");
                Console.WriteLine($"x = {xVal:F6} radians ({xVal * 180.0 / Math.PI:F2} degrees)");
                Console.WriteLine($"sin(x) = {Math.Sin(xVal):F6}, sin(x)^2 = {Math.Sin(xVal) * Math.Sin(xVal):F6}");
                Console.WriteLine($"cos(x) = {Math.Cos(xVal):F6}, cos(x)^2 = {Math.Cos(xVal) * Math.Cos(xVal):F6}");
            }
        }
    }

    /// <summary>
    /// Example 10: Combined Nonlinear Functions
    ///
    /// This demonstrates combining multiple types of nonlinear functions.
    ///
    /// Problem formulation:
    ///   Minimize: x^2 + exp(y) + |z|
    ///   Subject to: x + y + z >= 5.0
    ///   With: x, z in [-5.0, 5.0], y in [0.0, 5.0]
    ///
    /// Key points:
    /// - Can combine different types of nonlinear functions
    /// - Mix of convex (x^2, exp(y)) and non-convex (|z|) functions
    /// - Demonstrates flexibility of nonlinear expression system
    /// </summary>
    private static void RunCombinedFunctions()
    {
        using var model = new Model("combined_functions");

        var x = model.AddVariable("x", -5.0, 5.0, VariableType.Continuous);
        var y = model.AddVariable("y", 0.0, 5.0, VariableType.Continuous);
        var z = model.AddVariable("z", -5.0, 5.0, VariableType.Continuous);

        // Add constraint
        model.AddConstraint((x + y + z).Geq(5.0));

        // Create objective: minimize x^2 + exp(y) + |z|
        var xSquared = NonlinearExpression.Pow(x, 2.0);
        var expY = NonlinearExpression.Exp(y);
        var absZ = NonlinearExpression.Abs(z);
        var objExpr = xSquared + expY + absZ;

        model.SetObjective(objExpr, ObjectiveSense.Minimize);

        Console.WriteLine("Solving combined nonlinear functions...");
        var status = model.Optimize();
        Console.WriteLine($"Status: {status}");

        if (status == SolveStatus.Optimal)
        {
            var solution = model.GetBestSolution();
            if (solution != null)
            {
                double xVal = solution.GetValue(x);
                double yVal = solution.GetValue(y);
                double zVal = solution.GetValue(z);
                Console.WriteLine($"Optimal value: {solution.ObjectiveValue:F4}");
                Console.WriteLine($"x = {xVal:F4}, x^2 = {Math.Pow(xVal, 2):F4}");
                Console.WriteLine($"y = {yVal:F4}, exp(y) = {Math.Exp(yVal):F4}");
                Console.WriteLine($"z = {zVal:F4}, |z| = {Math.Abs(zVal):F4}");
            }
        }
    }
}
