# SCIP.NET

SCIP.NET - A modern C# wrapper for the SCIP (Solving Constraint Integer Programs) optimization solver.

[中文文档](README.zh-CN.md)

## Overview

SCIP.NET is a modern C# wrapper for the SCIP optimization solver, providing a type-safe, easy-to-use API with natural mathematical expression syntax.

## Features

- **Type Safety**: Leverages C#'s strong type system
- **Natural Syntax**: Supports operator overloading and expression syntax
- **Resource Management**: Uses SafeHandle for RAII
- **Cross-Platform**: Supports .NET 8.0+, runs on Windows, Linux, and macOS
- **Error Handling**: Uses C# exception mechanism
- **High Performance**: Native interface calls via P/Invoke

## Quick Start

```csharp
using ScipNet;

// Create model
using var model = new Model("example");

// Create variables
var x = model.AddVariable("x", 0, 10, 1, VariableType.Integer);
var y = model.AddVariable("y", 0, 10, 2, VariableType.Integer);

// Set objective function (maximize)
model.SetObjectiveSense(ObjectiveSense.Maximize);

// Add constraints (natural syntax)
model.AddConstraint((x + y).Leq(5));
model.AddConstraint((2 * x + y).Geq(3));
model.AddConstraint((x - y).Eq(1));

// Optimize
var status = model.Optimize();

// Get solution
if (status == SolveStatus.Optimal)
{
    var solution = model.GetBestSolution();
    Console.WriteLine($"Optimal value: {solution.ObjectiveValue}");
    Console.WriteLine($"x = {solution.GetValue(x)}");
    Console.WriteLine($"y = {solution.GetValue(y)}");
}
```

## Project Structure

```
SCIP.NET/
├── src/
│   └── ScipNet/
│       ├── ScipNet.csproj       # Project file
│       ├── ScipNet.cs            # Main entry
│       ├── Core/                # Core classes
│       │   ├── Enums.cs         # Enum types
│       │   ├── Model.cs         # Model class
│       │   ├── Variable.cs      # Variable class
│       │   ├── LinearExpression.cs # Linear expression
│       │   ├── Constraint.cs     # Constraint class
│       │   ├── Solution.cs      # Solution class
│       │   └── Statistics.cs    # Statistics class
│       └── Native/              # Native interface
│           ├── ScipHandle.cs    # SafeHandle wrapper
│           ├── ScipNativeMethods.cs # P/Invoke declarations
│           └── ErrorHandler.cs  # Error handling
├── examples/
│   ├── Examples.csproj       # Example project
│   └── Example1_BasicModel.cs # Basic example
├── plans/
│   ├── architecture-design.md   # Architecture design
│   └── native-dll-interop-analysis.md # Native DLL interop analysis
└── README.md
```

## Core Classes

### Model
Represents an optimization problem model, providing variable and constraint management, solving, etc.

### Variable
Represents a decision variable, supporting Binary, Integer, and Continuous types.

### LinearExpression
Represents a linear expression, supporting operator overloading.

### Constraint
Constraint base class, including LinearConstraint and RangeConstraint.

### Solution
Represents a SCIP solution, providing variable value access.

### Statistics
Represents solving statistics, including solving time, node count, etc.

## Enum Types

- **VariableType**: Binary, Integer, Continuous
- **ObjectiveSense**: Maximize, Minimize
- **SolveStatus**: Optimal, Infeasible, Unbounded, etc.
- **ReturnCode**: Okay, Error, NoMemory, etc.
- **ResultCode**: DidNotRun, Feasible, Infeasible, etc.

## Build and Run

### Build Library

```bash
cd src/ScipNet
dotnet build
```

### Run Example

```bash
cd examples
dotnet run
```

## Dependencies

- .NET 8.0+
- SCIP C library (requires separate installation)

## License

Apache License 2.0

## References

- [SCIP Official Documentation](https://scipopt.org/doc/html/)
- [SCIPpp Source Code](https://github.com/scipopt/scippp)
- [PySCIPOpt Source Code](https://github.com/scipopt/PySCIPOpt)

## Contributing

Contributions are welcome! Please submit Pull Requests or create Issues.
