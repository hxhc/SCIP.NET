# SCIP.NET 解池使用指南 - 生成大量可行解

## 📋 目录

- [概述](#概述)
- [关键概念](#关键概念)
- [为什么 Count() 返回 Infeasible？](#为什么-count-返回-infeasible)
- [快速开始](#快速开始)
- [详细说明](#详细说明)
- [参数配置](#参数配置)
- [代码示例](#代码示例)
- [常见问题](#常见问题)
- [最佳实践](#最佳实践)

## 概述

SCIP.NET 提供了两种不同的方式来处理解：

### 1. `Optimize()` - 寻找最优解
- 只寻找**一个或少数几个最优解**
- 返回 `Optimal` 状态
- 解存储在默认解池中
- 使用 `GetSolutions()` 获取解

### 2. `Count()` - 枚举所有可行解
- **收集所有可行解**
- 返回 `Infeasible` 状态（这是预期行为）
- 解存储在 countsols 约束处理器中
- 使用 `GetCountedSolutionsCount()` 获取解数量

## 关键概念

### Count() 的内部机制

```
用户调用 Count()
    ↓
SCIPsetParamsCountsols() - 设置安全计数参数（禁用 restarts 等）
    ↓
SCIPcount() - 启动计数过程
    ↓
countsols 约束处理器找到可行解
    ↓
内部将这些解标记为 "infeasible" 报告给 SCIP 核心
    ↓
SCIP 核心认为没有找到可行解 → 返回 Infeasible 状态
    ↓
实际上：countsols 已成功收集所有可行解！
```

### 为什么状态是 Infeasible？

**这是预期行为，不是错误！**

SCIP 的 countsols 约束处理器故意将找到的可行解标记为 "infeasible"，原因：

1. **防止剪枝**：避免 SCIP 基于原始界进行剪枝
2. **保证完整性**：确保所有可行解都被计数，不会丢失
3. **内部机制**：这是 SCIP 计数功能的实现方式

**重要**：返回 `Infeasible` 状态**不意味着**没有找到可行解。相反，它表示所有可行解已被成功收集！

## 为什么 Count() 返回 Infeasible？

### 短答案
✅ **所有计数的解都是满足约束的可行解！**

### 详细解释

| 层面 | 状态 | 说明 |
|------|------|------|
| **countsols 内部** | 可行 | 每个找到的解都满足所有约束 |
| **报告给 SCIP 核心** | Infeasible | 故意标记，防止剪枝 |
| **SCIP 核心** | Infeasible | 认为没有找到解 |
| **实际结果** | ✅ 可行 | countsols 已收集所有可行解 |

### 验证解的正确性

```csharp
// Count() 找到的解都是满足约束的
model.Count();
long count = model.GetCountedSolutionsCount();
// count = 所有满足约束的可行解数量
```

## 快速开始

### 最简单的示例

```csharp
using ScipNet;
using ScipNet.Core;

// 创建模型
using var model = new Model("example");

// 添加变量（例如：10个二元变量）
var x = new Variable[10];
for (int i = 0; i < 10; i++)
    x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

// 添加约束（例如：必须选择5个）
var sum = new LinearExpression();
for (int i = 0; i < 10; i++)
    sum = sum + x[i];
model.AddConstraint(sum.Eq(5));

// 设置目标（可选，但可以设置平坦目标）
model.SetObjective(sum, ObjectiveSense.Maximize);

// ===== 关键设置 =====
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);

// 枚举所有可行解
var status = model.Count();
Console.WriteLine($"状态: {status}");  // 输出: Infeasible (正常)

// 获取解数量
long count = model.GetCountedSolutionsCount();
Console.WriteLine($"找到 {count} 个可行解");
```

## 详细说明

### 步骤 1: 创建模型和变量

```csharp
using var model = new Model("solution_pool");

// 创建二元变量
var x = new Variable[N];
for (int i = 0; i < N; i++)
    x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);
```

### 步骤 2: 添加约束

```csharp
// 约束：sum(x[i]) = K
var sum = new LinearExpression();
for (int i = 0; i < N; i++)
    sum = sum + x[i];
model.AddConstraint(sum.Eq(K));
```

### 步骤 3: 设置目标（可选）

```csharp
// 平坦目标：所有系数相同
model.SetObjective(sum, ObjectiveSense.Maximize);

// 或者不设置目标函数
// model.SetObjective(...)  // 不调用
```

### 步骤 4: 配置计数参数

```csharp
// 必需参数
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);

// 可选：设置解限制
model.SetLongParam("constraints/countsols/sollimit", 100000);
```

### 步骤 5: 执行计数

```csharp
var status = model.Count();
// status = SolveStatus.Infeasible (这是预期的)
```

### 步骤 6: 获取结果

```csharp
// 获取计数的解数量
long count = model.GetCountedSolutionsCount();

Console.WriteLine($"找到 {count} 个可行解");
Console.WriteLine($"状态: {status}");  // Infeasible 是正常的
```

## 参数配置

### 必需参数

| 参数 | 值 | 说明 |
|------|-----|------|
| `SetEmphasis(ParamEmphasis.Counter)` | `Counter` | 启用计数模式 |
| `SetBoolParam("constraints/countsols/collect")` | `true` | 收集解（而不是只计数） |

### 可选参数

| 参数 | 类型 | 推荐值 | 说明 |
|------|------|--------|------|
| `constraints/countsols/sollimit` | long | 100000 | 最大解数限制 |
| `limits/time` | double | 60 | 时间限制（秒） |
| `display/verblevel` | int | 0 | 显示级别（0=静默） |

### 自动设置的参数

`Count()` 方法会自动调用 `SCIPsetParamsCountsols()`，这会设置：
- ✅ 禁用 restarts（关键）
- ✅ 禁用 dual reductions
- ✅ 其他安全的计数参数

**注意**：不需要手动设置这些参数，`Count()` 会自动处理。

## 代码示例

### 示例 1: 基础用法

```csharp
using ScipNet;
using ScipNet.Core;

// 问题：从 10 个物品中选择 5 个
int N = 10, K = 5;

using var model = new Model("basic_example");

// 创建变量
var x = new Variable[N];
for (int i = 0; i < N; i++)
    x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

// 添加约束
var sum = new LinearExpression();
for (int i = 0; i < N; i++)
    sum = sum + x[i];
model.AddConstraint(sum.Eq(K));

// 设置目标
model.SetObjective(sum, ObjectiveSense.Maximize);

// 配置计数
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);

// 执行计数
var status = model.Count();

// 获取结果
long count = model.GetCountedSolutionsCount();
Console.WriteLine($"找到 {count} 个可行解");
Console.WriteLine($"理论解数: C({N}, {K}) = {Combination(N, K)}");
```

### 示例 2: 带时间限制

```csharp
using var model = new Model("timed_example");

// ... 创建模型和约束 ...

// 配置计数
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);
model.SetRealParam("limits/time", 30);  // 30秒时间限制

// 执行计数
var status = model.Count();

// 获取结果
long count = model.GetCountedSolutionsCount();
Console.WriteLine($"在 30 秒内找到 {count} 个可行解");
```

### 示例 3: 无目标函数

```csharp
using var model = new Model("no_obj_example");

// ... 创建模型和约束 ...

// 不设置目标函数！
// model.SetObjective(...);  // 不调用

// 配置计数
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);

// 执行计数
var status = model.Count();

// 获取结果
long count = model.GetCountedSolutionsCount();
Console.WriteLine($"找到 {count} 个可行解（无目标函数）");
```

## 常见问题

### Q1: 为什么 Count() 返回 Infeasible？

**A:** 这是预期行为！SCIP 的 countsols 约束处理器内部机制会将找到的可行解标记为 "infeasible" 报告给 SCIP 核心，以防止剪枝。实际上所有解都是满足约束的可行解。

### Q2: 如何判断是否成功收集了解？

**A:** 检查 `GetCountedSolutionsCount()` 的返回值：

```csharp
long count = model.GetCountedSolutionsCount();
if (count > 0)
{
    Console.WriteLine($"✅ 成功收集了 {count} 个可行解");
}
else
{
    Console.WriteLine("⚠️ 未收集到任何解");
}
```

### Q3: 状态是 Infeasible，解是否满足约束？

**A:** **是的！** 所有计数的解都是满足约束的可行解。`Infeasible` 只是 SCIP 核心的报告状态，不影响解的正确性。

### Q4: 如何获取每个解的具体值？

**A:** 目前 SCIP.NET 提供了稀疏解接口：

```csharp
// 获取稀疏解（相对于 active variables）
var (vars, nvars, sols, nsols) = model.GetCountedSparseSolutions();

// 处理稀疏解...
// 注意：这些解可能需要转换回原始变量空间

// 释放稀疏解
model.FreeCountedSparseSolutions(ref sols);
```

### Q5: Count() 和 Optimize() 的区别？

| 特性 | Count() | Optimize() |
|------|---------|------------|
| 目的 | 枚举所有可行解 | 寻找最优解 |
| 返回状态 | Infeasible（预期） | Optimal |
| 解数量 | 可能很多 | 通常一个或少数几个 |
| 解存储 | countsols 约束处理器 | 默认解池 |
| 获取方法 | GetCountedSolutionsCount() | GetSolutions() |
| 适用场景 | 需要所有可行解 | 需要最优解 |

### Q6: 为什么我之前只得到 1 个解？

**A:** 可能的原因：
1. ❌ 使用了 `Optimize()` 而不是 `Count()`
2. ❌ 没有设置 `constraints/countsols/collect = true`
3. ❌ 没有使用 `ParamEmphasis.Counter`
4. ❌ 没有设置 `constraints/countsols/sollimit`

**正确用法**：
```csharp
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);
model.Count();  // 不是 Optimize()
```

### Q7: 可以同时使用 Count() 和 Optimize() 吗？

**A:** **不可以。** Count() 会改变 SCIP 的内部状态，之后不能再使用 Optimize()。如果需要两个功能，需要创建两个独立的模型。

```csharp
// ✅ 正确：两个独立的模型
using var model1 = new Model("count");
// ... 设置模型 ...
model1.Count();

using var model2 = new Model("optimize");
// ... 设置相同的模型 ...
model2.Optimize();

// ❌ 错误：同一个模型
using var model = new Model("model");
model.Count();    // 改变了内部状态
model.Optimize(); // 可能无法正常工作
```

### Q8: 如何提高收集效率？

**A:** 优化建议：
1. ✅ 使用 `ParamEmphasis.Counter` 模式
2. ✅ 设置合理的 `sollimit`
3. ✅ 根据需要设置时间限制
4. ✅ 禁用不必要的输出（`display/verblevel = 0`）
5. ❌ 不需要手动设置复杂的启发式参数

```csharp
// 推荐配置
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);
model.SetRealParam("limits/time", 60);
model.SetIntParam("display/verblevel", 0);
```

## 最佳实践

### 1. 选择合适的方法

```csharp
// 需要所有可行解 → 使用 Count()
model.Count();

// 需要最优解 → 使用 Optimize()
model.Optimize();
```

### 2. 正确处理状态

```csharp
var status = model.Count();

// Count() 后状态是 Infeasible 是正常的
if (status == SolveStatus.Infeasible)
{
    long count = model.GetCountedSolutionsCount();
    Console.WriteLine($"✅ 成功收集 {count} 个可行解");
}
else
{
    Console.WriteLine($"⚠️ 意外状态: {status}");
}
```

### 3. 验证解的数量

```csharp
long count = model.GetCountedSolutionsCount();
long theoretical = Combination(N, K);

Console.WriteLine($"收集的解数: {count}");
Console.WriteLine($"理论解数: {theoretical}");
Console.WriteLine($"覆盖率: {(double)count / theoretical * 100:F2}%");
```

### 4. 使用时间限制

```csharp
// 对于大问题，设置时间限制
model.SetRealParam("limits/time", 30);

var status = model.Count();
long count = model.GetCountedSolutionsCount();

Console.WriteLine($"在 30 秒内收集了 {count} 个解");
```

### 5. 简化参数设置

```csharp
// ✅ 推荐：简洁配置
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);
model.Count();

// ❌ 不推荐：过度配置
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetIntParam("presolving/maxrounds", 0);  // 不需要
model.SetIntParam("limits/nodes", 1000000);    // 不需要
model.SetRealParam("limits/gap", 100.0);       // 不需要
// ... 更多不需要的参数 ...
model.Count();
```

## 总结

### 关键点

1. ✅ 使用 `Count()` 而不是 `Optimize()` 来枚举所有可行解
2. ✅ `Infeasible` 状态是预期行为，不是错误
3. ✅ 所有计数的解都满足约束
4. ✅ 使用 `GetCountedSolutionsCount()` 获取解数量
5. ✅ 必须设置 `ParamEmphasis.Counter` 和 `constraints/countsols/collect`

### 完整示例

```csharp
using ScipNet;
using ScipNet.Core;

// 创建模型
using var model = new Model("example");

// 添加变量和约束
var x = new Variable[10];
for (int i = 0; i < 10; i++)
    x[i] = model.AddVariable($"x{i}", 0, 1, VariableType.Binary);

var sum = new LinearExpression();
for (int i = 0; i < 10; i++)
    sum = sum + x[i];
model.AddConstraint(sum.Eq(5));

// 设置目标
model.SetObjective(sum, ObjectiveSense.Maximize);

// 配置计数
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);

// 执行计数
var status = model.Count();

// 获取结果
long count = model.GetCountedSolutionsCount();
Console.WriteLine($"状态: {status} (Infeasible 是正常的)");
Console.WriteLine($"✅ 成功收集 {count} 个可行解");
```

## 参考资料

- [SCIP 官方文档：计数/枚举可行解](https://scipopt.org/doc/html/COUNTER.php)
- [SCIP Doxygen：cons_countsols.h](https://scipopt.org/doc/html/cons__countsols_8h.php)
- [SCIP.NET 示例：SolutionPoolExample.cs](../examples/SolutionPoolExample.cs)
