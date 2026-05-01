# SCIP.NET

> **SCIP.NET** — 一个现代的 C# 封装，用于 [SCIP](https://scipopt.org/)（求解约束整数规划）优化求解器。

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/)
[![SCIP](https://img.shields.io/badge/SCIP-9.0+-orange)](https://scipopt.org/)
[![License](https://img.shields.io/badge/License-Apache%202.0-green)](LICENSE)

[English Documentation](README.md)

---

## 重要

本项目由 AI 代码工具编写。首个 commit 纯粹由 [GLM 4.7](https://bigmodel.cn/)（又称 [Z.ai](https://chat.z.ai/)）完成。我不保证代码的正确性。

## 概述

SCIP.NET 是 SCIP 优化求解器的现代 C# 封装，提供类型安全、易于使用的 API，支持自然数学表达式语法。

## 特性

- **类型安全** — 充分利用 C# 的强类型系统
- **自然语法** — 运算符重载支持数学表达式：`x + 2 * y`、`(x + y).Leq(5)`
- **资源管理** — 使用 `SafeHandle` 确保 RAII
- **跨平台** — 支持 .NET 8.0+，可在 Windows、Linux、macOS 上运行
- **错误处理** — 结构化的 C# 异常层次
- **高性能** — 基于 P/Invoke 的原生接口调用
- **非线性支持** — 内置表达式树：$\sin$、$\cos$、$\exp$、$\log$、$\sqrt{\cdot}$、$|\cdot|$、$x^n$
- **解枚举** — 通过 SCIP 的 `countsols` 约束处理器枚举所有可行解
- **指示约束** — 建模逻辑蕴含：$z = 1 \implies a^\top x \leq b$

## 快速开始

```csharp
using System;
using ScipNet;
using ScipNet.Core;

// 创建模型
using var model = new Model("example");

// 创建整数变量
var x = model.AddVariable("x", 0, 10, VariableType.Integer);
var y = model.AddVariable("y", 0, 10, VariableType.Integer);

Console.WriteLine($"Created variables: {x}, {y}");

// 设置目标函数：最大化 x + 2*y
model.SetObjective(x + 2 * y, ObjectiveSense.Maximize);

// 添加约束（使用自然语法）
model.AddConstraint((x + y).Leq(5));
model.AddConstraint((2 * x + y).Geq(3));
model.AddConstraint((x - y).Eq(1));

Console.WriteLine($"Added constraints: {model.Constraints.Count}");

// 求解
Console.WriteLine("Solving...");
var status = model.Optimize();

Console.WriteLine($"Solve status: {status}");

// 获取解
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

// 获取统计信息
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

## 文档

详细文档位于 [`docs/`](docs/index.md) 目录：

| 文档 | 说明 |
|------|------|
| [文档索引](docs/index.md) | 概述、架构、项目结构 |
| [入门指南](docs/getting-started.md) | 安装、构建、依赖、快速开始 |
| [基础建模](docs/basic-modeling.md) | 变量、线性表达式、约束、求解、解 |
| [非线性建模](docs/nonlinear-modeling.md) | $\sin$、$\cos$、$\exp$、$\log$、$\sqrt{\cdot}$、$|\cdot|$、$x^n$ 及示例 |
| [解池](docs/solution-pool.md) | 枚举所有可行解（$Count()$、组合、背包问题） |
| [指示约束](docs/indicator-constraints.md) | 蕴含约束（`Implies()`） |
| [参数参考](docs/parameter-reference.md) | SCIP 参数调优、强调模式 |

## 项目结构

```
SCIP.NET/
├── src/
│   └── ScipNet/
│       ├── ScipNet.cs           # 主入口和版本信息
│       ├── Core/
│       │   ├── Enums.cs         # VariableType、SolveStatus 等枚举
│       │   ├── Model.cs         # 主优化模型
│       │   ├── Variable.cs      # 带有运算符的决策变量
│       │   ├── LinearExpression.cs     # 线性表达式 DSL
│       │   ├── NonlinearExpression.cs  # 非线性表达式树
│       │   ├── Constraint.cs          # LinearConstraint、RangeConstraint
│       │   ├── NonlinearConstraint.cs # 非线性约束
│       │   ├── IndicatorConstraint.cs # 指示（蕴含）约束
│       │   ├── Solution.cs     # 解表示
│       │   └── Statistics.cs   # 求解器统计
│       └── Native/
│           ├── ScipHandle.cs       # SafeHandle 包装
│           ├── ScipNativeMethods.cs # P/Invoke 声明
│           └── ErrorHandler.cs     # 异常层次
├── examples/
│   ├── Example1_BasicModel.cs           # 基础 LP/MIP
│   ├── Example2_NonlinearModel.cs       # 10 个非线性示例
│   ├── Example3_SolutionPoolExample.cs  # 解枚举
│   └── Example4_KnapsackSolutionPool.cs # 背包 + 解池
├── docs/
│   ├── index.md                 # 文档索引
│   ├── getting-started.md       # 安装和快速开始
│   ├── basic-modeling.md        # 变量、约束、求解
│   ├── nonlinear-modeling.md    # 非线性函数
│   ├── solution-pool.md         # 解枚举
│   ├── indicator-constraints.md # 指示约束
│   └── parameter-reference.md   # 参数配置
└── README.md
```

## 构建和运行

### 构建库

```bash
cd src/ScipNet
dotnet build
```

### 运行示例

```bash
cd examples
dotnet run
```

## 核心类

| 类 | 说明 |
|-------|------|
| `Model` | 优化问题模型 — 变量/约束管理、求解 |
| `Variable` | 决策变量（Binary、Integer、Continuous） |
| `LinearExpression` | 支持运算符重载的线性表达式 |
| `NonlinearExpression` | 非线性表达式树（$\sin$、$\cos$、$\exp$ 等） |
| `LinearConstraint` | 线性约束：$a^\top x \leq b$、$a^\top x \geq b$、$a^\top x = b$ |
| `RangeConstraint` | 双边约束：$\ell \leq a^\top x \leq u$ |
| `NonlinearConstraint` | 带有非线性表达式的约束 |
| `IndicatorConstraint` | 蕴含约束：$z = 1 \implies a^\top x \leq b$ |
| `Solution` | 带有变量值访问的解 |
| `Statistics` | 求解器统计（时间、节点数、界、间隙） |

## 枚举类型

| 枚举 | 值 |
|------|--------|
| `VariableType` | `Binary`、`Integer`、`Continuous` |
| `ObjectiveSense` | `Maximize`、`Minimize` |
| `SolveStatus` | `Optimal`、`Infeasible`、`Unbounded`、`TimeLimit`、`NodeLimit` 等 |
| `ReturnCode` | `Okay`、`Error`、`NoMemory` 等 |
| `ResultCode` | `DidNotRun`、`Feasible`、`Infeasible` 等 |
| `Sense` | `LessThanOrEqual`、`Equal`、`GreaterThanOrEqual` |
| `ParamEmphasis` | `Default`、`Counter`、`Optimality`、`Feasibility` 等 |

## 依赖项

- .NET 8.0+
- SCIP C 库 9.0+（需要单独安装）

## 许可证

Apache License 2.0

## 参考

- [SCIP 官方文档](https://scipopt.org/doc/html/)
- [SCIPpp (C++)](https://github.com/scipopt/scippp)
- [PySCIPOpt (Python)](https://github.com/scipopt/PySCIPOpt)
- [SCIP.NET 文档](docs/index.md)

## 贡献

欢迎贡献！请提交 Pull Request 或创建 Issue。
