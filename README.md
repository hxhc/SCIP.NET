# SCIP.NET

> **SCIP.NET** — A modern C# wrapper for the [SCIP](https://scipopt.org/) (Solving Constraint Integer Programs) optimization solver.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![SCIP](https://img.shields.io/badge/SCIP-10.0+-orange)](https://scipopt.org/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green)](LICENSE)

[中文文档](README.zh-CN.md)

---

## Important

This project was written by AI coding tools. The first commit was purely by [GLM 4.7](https://bigmodel.cn/) (aka. [Z.ai](https://chat.z.ai/)). I do not guarantee the correctness of the code.

## Overview

SCIP.NET is a modern C# wrapper for the SCIP optimization solver, providing a type-safe, easy-to-use API with natural mathematical expression syntax.

## Features

- **Type Safety** — Leverages C#'s strong type system
- **Natural Syntax** — Operator overloading for mathematical expressions: `x + 2 * y`, `(x + y).Leq(5)`
- **Resource Management** — Uses `SafeHandle` for RAII
- **Cross-Platform** — Supports .NET 8.0+, runs on Windows, Linux, and macOS
- **Error Handling** — Uses C# exception mechanism with structured hierarchy
- **High Performance** — Native interface calls via P/Invoke
- **Nonlinear Support** — Built-in expression tree: $\sin$, $\cos$, $\exp$, $\log$, $\sqrt{\cdot}$, $|\cdot|$, $x^n$
- **Solution Enumeration** — Enumerate all feasible solutions via SCIP's `countsols` constraint handler
- **Indicator Constraints** — Model logical implications: $z = 1 \implies a^\top x \leq b$

## Quick Start

```csharp
using System;
using ScipNet;
using ScipNet.Core;

// Create model
using var model = new Model("example");

// Create integer variables
var x = model.AddVariable("x", 0, 10, VariableType.Integer);
var y = model.AddVariable("y", 0, 10, VariableType.Integer);

Console.WriteLine($"Created variables: {x}, {y}");

// Set objective: maximize x + 2*y
model.SetObjective(x + 2 * y, ObjectiveSense.Maximize);

// Add constraints using natural syntax
model.AddConstraint((x + y).Leq(5));
model.AddConstraint((2 * x + y).Geq(3));
model.AddConstraint((x - y).Eq(1));

Console.WriteLine($"Added constraints: {model.Constraints.Count}");

// Solve
Console.WriteLine("Solving...");
var status = model.Optimize();

Console.WriteLine($"Solve status: {status}");

// Get solution
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

// Get statistics
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
```

## Documentation

Detailed documentation is available in the [`docs/`](docs/index.md) directory:

| Document | Description |
|----------|-------------|
| [Documentation Index](docs/index.md) | Overview, architecture, project structure |
| [Getting Started](docs/getting-started.md) | Installation, build, dependencies, quick start |
| [Basic Modeling](docs/basic-modeling.md) | Variables, linear expressions, constraints, solving, solutions, statistics |
| [Nonlinear Modeling](docs/nonlinear-modeling.md) | $\sin$, $\cos$, $\exp$, $\log$, $\sqrt{\cdot}$, $|\cdot|$, $x^n$ with examples |
| [Solution Pool](docs/solution-pool.md) | Enumerating all feasible solutions ($Count()$, $C(N,K)$, knapsack) |
| [Indicator Constraints](docs/indicator-constraints.md) | Implication constraints (`Implies()`) |
| [Parameter Reference](docs/parameter-reference.md) | SCIP parameter tuning, emphasis modes |

## Project Structure

```
SCIP.NET/
├── src/
│   └── ScipNet/
│       ├── ScipNet.cs           # Main entry & version info
│       ├── Core/
│       │   ├── Enums.cs         # VariableType, SolveStatus, etc.
│       │   ├── Model.cs         # Main optimization model
│       │   ├── Variable.cs      # Decision variables with operators
│       │   ├── LinearExpression.cs     # Linear expression DSL
│       │   ├── NonlinearExpression.cs  # Nonlinear expression tree
│       │   ├── Constraint.cs          # LinearConstraint, RangeConstraint
│       │   ├── NonlinearConstraint.cs # Nonlinear constraints
│       │   ├── IndicatorConstraint.cs # Indicator (implication) constraints
│       │   ├── Solution.cs     # Solution representation
│       │   └── Statistics.cs   # Solver statistics
│       └── Native/
│           ├── ScipHandle.cs       # SafeHandle wrapper
│           ├── ScipNativeMethods.cs # P/Invoke declarations
│           └── ErrorHandler.cs     # Exception hierarchy
├── examples/
│   ├── Example1_BasicModel.cs           # Basic LP/MIP
│   ├── Example2_NonlinearModel.cs       # 10 nonlinear examples
│   ├── Example3_SolutionPoolExample.cs  # Solution enumeration
│   └── Example4_KnapsackSolutionPool.cs # Knapsack + solution pool
├── docs/
│   ├── index.md                 # Documentation index
│   ├── getting-started.md       # Installation & quick start
│   ├── basic-modeling.md        # Variables, constraints, solving
│   ├── nonlinear-modeling.md    # Nonlinear functions
│   ├── solution-pool.md         # Solution enumeration
│   ├── indicator-constraints.md # Indicator constraints
│   └── parameter-reference.md   # Parameter configuration
└── README.md
```

## Build and Run

### Build Library

```bash
cd src/ScipNet
dotnet build
```

### Run Examples

```bash
cd examples
dotnet run
```

## Core Classes

| Class | Description |
|-------|-------------|
| `Model` | Optimization problem model — variable/constraint management, solving |
| `Variable` | Decision variable (Binary, Integer, Continuous) |
| `LinearExpression` | Linear expression with operator overloading |
| `NonlinearExpression` | Nonlinear expression tree ($\sin$, $\cos$, $\exp$, etc.) |
| `LinearConstraint` | Linear constraint: $a^\top x \leq b$, $a^\top x \geq b$, $a^\top x = b$ |
| `RangeConstraint` | Two-sided constraint: $\ell \leq a^\top x \leq u$ |
| `NonlinearConstraint` | Constraint with nonlinear expression |
| `IndicatorConstraint` | Implication: $z = 1 \implies a^\top x \leq b$ |
| `Solution` | Solution with variable value access |
| `Statistics` | Solver statistics (time, nodes, bounds, gap) |

## Enum Types

| Enum | Values |
|------|--------|
| `VariableType` | `Binary`, `Integer`, `Continuous` |
| `ObjectiveSense` | `Maximize`, `Minimize` |
| `SolveStatus` | `Optimal`, `Infeasible`, `Unbounded`, `TimeLimit`, `NodeLimit`, etc. |
| `ReturnCode` | `Okay`, `Error`, `NoMemory`, etc. |
| `ResultCode` | `DidNotRun`, `Feasible`, `Infeasible`, etc. |
| `Sense` | `LessThanOrEqual`, `Equal`, `GreaterThanOrEqual` |
| `ParamEmphasis` | `Default`, `Counter`, `Optimality`, `Feasibility`, etc. |

## Dependencies

- .NET 8.0+
- SCIP C library 10.0+ (requires separate installation)

## License

Apache License 2.0

## References

- [SCIP Official Documentation](https://scipopt.org/doc/html/)
- [SCIPpp (C++)](https://github.com/scipopt/scippp)
- [PySCIPOpt (Python)](https://github.com/scipopt/PySCIPOpt)
- [SCIP.NET Docs](docs/index.md)

## Contributing

Contributions are welcome! Please submit Pull Requests or create Issues.
