# Getting Started

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) or later
- [SCIP Optimization Suite 10.0+](https://scipopt.org/download/) installed on your system

### Installing SCIP

The SCIP C library (`libscip`) must be available on your system's library path.

#### Windows

1. Download the SCIP Optimization Suite from [scipopt.org](https://scipopt.org/download/)
2. Add the directory containing `libscip.dll` to your `PATH` environment variable
3. Or copy `libscip.dll` to your project's output directory

#### Linux

```bash
# Installing via package manager (if available)
sudo apt-get install libscip

# Or build from source
git clone https://github.com/scipopt/scip.git
cd scip
mkdir build && cd build
cmake ..
make -j$(nproc)
sudo make install
```

#### macOS

```bash
# Using Homebrew
brew tap scipopt/scipopt
brew install scipoptsuite
```

## Building the Library

```bash
cd src/ScipNet
dotnet build
```

This produces `src/ScipNet/bin/Debug/net8.0/ScipNet.dll`.

## Quick Start

Here's a complete example — a small MIP that maximizes $x + 2y$ subject to linear constraints:

```csharp
using System;
using ScipNet;
using ScipNet.Core;

// Create a model (IDisposable — use 'using' for proper cleanup)
using var model = new Model("example");

// Create integer variables with bounds [0, 10]
var x = model.AddVariable("x", 0, 10, VariableType.Integer);
var y = model.AddVariable("y", 0, 10, VariableType.Integer);

Console.WriteLine($"Created variables: {x}, {y}");

// Set objective: maximize x + 2*y
model.SetObjective(x + 2 * y, ObjectiveSense.Maximize);

// Add constraints using natural syntax
model.AddConstraint((x + y).Leq(5));      // x + y <= 5
model.AddConstraint((2 * x + y).Geq(3));  // 2*x + y >= 3
model.AddConstraint((x - y).Eq(1));       // x - y == 1

Console.WriteLine($"Added constraints: {model.Constraints.Count}");

// Solve
Console.WriteLine("Solving...");
var status = model.Optimize();

Console.WriteLine($"Solve status: {status}");

// Retrieve solution
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

// Print solver statistics
var statistics = model.GetStatistics();
Console.WriteLine();
Console.WriteLine("Statistics:");
Console.WriteLine($"  Solving time:  {statistics.SolvingTime:F2}s");
Console.WriteLine($"  Total nodes:   {statistics.TotalNodes}");
Console.WriteLine($"  Open nodes:    {statistics.OpenNodes}");
Console.WriteLine($"  Primal bound:  {statistics.PrimalBound:F4}");
Console.WriteLine($"  Dual bound:    {statistics.DualBound:F4}");
Console.WriteLine($"  Gap:           {statistics.Gap:P2}");
Console.WriteLine($"  LP iterations: {statistics.NLpIterations}");
Console.WriteLine($"  Solutions:     {statistics.NSolutionsFound}");
```

## Running the Examples

```bash
cd examples
dotnet run
```

The `RunExamples.cs` file controls which examples execute. Uncomment the desired example:

```csharp
// Example 1: Basic Model
// Example1_BasicModel.Run();

// Example 2: Nonlinear Model
// Example2_NonlinearModel.Run();

// Example 3: Solution Pool
// Example3_SolutionPoolExample.Run();

// Example 4: Knapsack Solution Pool
Example4_KnapsackSolutionPool.Run();
```

## Next Steps

| Topic | Description |
|-------|-------------|
| [Basic Modeling](basic-modeling.md) | Learn about variables, linear expressions, constraints, and solving |
| [Nonlinear Modeling](nonlinear-modeling.md) | Use $\sin$, $\cos$, $\exp$, $\log$, and other nonlinear functions |
| [Solution Pool](solution-pool.md) | Enumerate all feasible solutions |
| [Indicator Constraints](indicator-constraints.md) | Model logical implications |
| [Parameter Reference](parameter-reference.md) | Tune solver performance |
