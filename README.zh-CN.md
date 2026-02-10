# SCIP.NET

SCIP.NET - 一个现代的 C# 封装，用于 SCIP（求解约束整数规划）优化求解器。

[English Documentation](README.md)

## 概述

SCIP.NET 是 SCIP 优化求解器的现代 C# 封装，提供类型安全、易于使用的 API，支持自然数学表达式语法。

## 特性

- **类型安全**：充分利用 C# 的强类型系统
- **自然语法**：支持运算符重载和表达式语法
- **资源管理**：使用 SafeHandle 确保 RAII
- **跨平台**：支持 .NET 8.0+，可在 Windows、Linux、macOS 上运行
- **错误处理**：使用 C# 异常机制
- **高性能**：基于 P/Invoke 的原生接口调用

## 快速开始

```csharp
using ScipNet;

// 创建模型
using var model = new Model("example");

// 创建变量
var x = model.AddVariable("x", 0, 10, 1, VariableType.Integer);
var y = model.AddVariable("y", 0, 10, 2, VariableType.Integer);

// 设置目标函数（最大化）
model.SetObjectiveSense(ObjectiveSense.Maximize);

// 添加约束（自然语法）
model.AddConstraint((x + y).Leq(5));
model.AddConstraint((2 * x + y).Geq(3));
model.AddConstraint((x - y).Eq(1));

// 优化
var status = model.Optimize();

// 获取解
if (status == SolveStatus.Optimal)
{
    var solution = model.GetBestSolution();
    Console.WriteLine($"Optimal value: {solution.ObjectiveValue}");
    Console.WriteLine($"x = {solution.GetValue(x)}");
    Console.WriteLine($"y = {solution.GetValue(y)}");
}
```

## 项目结构

```
SCIP.NET/
├── src/
│   └── ScipNet/
│       ├── ScipNet.csproj       # 项目文件
│       ├── ScipNet.cs            # 主入口
│       ├── Core/                # 核心类
│       │   ├── Enums.cs         # 枚举类型
│       │   ├── Model.cs         # 模型类
│       │   ├── Variable.cs      # 变量类
│       │   ├── LinearExpression.cs # 线性表达式
│       │   ├── Constraint.cs     # 约束类
│       │   ├── Solution.cs      # 解类
│       │   └── Statistics.cs    # 统计类
│       └── Native/              # 原生接口
│           ├── ScipHandle.cs    # SafeHandle 包装
│           ├── ScipNativeMethods.cs # P/Invoke 声明
│           └── ErrorHandler.cs  # 错误处理
├── examples/
│   ├── Examples.csproj       # 示例项目
│   └── Example1_BasicModel.cs # 基础示例
├── plans/
│   ├── architecture-design.md   # 架构设计文档
│   └── native-dll-interop-analysis.md # Native DLL 互操作分析
└── README.md
```

## 核心类

### Model
代表优化问题模型，提供变量和约束管理、求解等功能。

### Variable
代表决策变量，支持 Binary、Integer、Continuous 类型。

### LinearExpression
代表线性表达式，支持运算符重载。

### Constraint
约束基类，包括 LinearConstraint 和 RangeConstraint。

### Solution
代表 SCIP 解，提供变量值访问。

### Statistics
代表求解统计信息，包括求解时间、节点数等。

## 枚举类型

- **VariableType**：Binary、Integer、Continuous
- **ObjectiveSense**：Maximize、Minimize
- **SolveStatus**：Optimal、Infeasible、Unbounded 等
- **ReturnCode**：Okay、Error、NoMemory 等
- **ResultCode**：DidNotRun、Feasible、Infeasible 等

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

## 依赖项

- .NET 8.0+
- SCIP C 库（需要单独安装）

## 许可证

Apache License 2.0

## 参考

- [SCIP 官方文档](https://scipopt.org/doc/html/)
- [SCIPpp 源代码](https://github.com/scipopt/scippp)
- [PySCIPOpt 源代码](https://github.com/scipopt/PySCIPOpt)

## 贡献

欢迎贡献！请提交 Pull Request 或创建 Issue。
