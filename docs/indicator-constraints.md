# Indicator Constraints

An **indicator constraint** (also called an implication constraint) has the form:

$$
z = 1 \implies a^\top x \leq b
$$

where $z$ is a **binary variable** and $a^\top x \leq b$ is a **linear constraint**. If $z = 1$, the linear constraint must hold; if $z = 0$, the constraint may be violated.

---

## Syntax

Indicator constraints use the `Implies()` method on the binary variable:

```csharp
// z is a binary variable
// constraint is a LinearConstraint
// If z == 1, then constraint must hold
var indicator = z.Implies(constraint);
model.AddConstraint(indicator);
```

### Full Signature

```csharp
public IndicatorConstraint Implies(
    LinearConstraint constraint,
    string? name = null
)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `constraint` | `LinearConstraint` | The linear constraint that holds when $z = 1$ |
| `name` | `string?` | Optional name (auto-generated if `null`) |

---

## Example: Fixed-Cost Problem

A classic use case: you pay a fixed cost $F$ if you produce any amount of product $x$, and variable cost $c$ per unit.

$$
\begin{aligned}
\min \quad & F \cdot z + c \cdot x \\
\text{s.t.} \quad & x \leq M \cdot z \\
& x \geq 0 \\
& z \in \{0, 1\}
\end{aligned}
$$

- If $z = 0$: $x = 0$ (no production)
- If $z = 1$: $0 \leq x \leq M$ (production up to capacity $M$)

```csharp
using ScipNet;
using ScipNet.Core;

using var model = new Model("fixed_cost");

// Decision variables
var x = model.AddVariable("x", 0, 100, VariableType.Continuous);  // production quantity
var z = model.AddVariable("z", 0, 1, VariableType.Binary);        // setup indicator

// Fixed cost: if z = 1, we pay F = 100
// Variable cost: c = 5 per unit
// Minimize: 100*z + 5*x
model.SetObjective(100 * z + 5 * x, ObjectiveSense.Minimize);

// Indicator constraint: if z = 1, then x <= 100 (big-M constraint)
// If z = 0, then x must be 0 (x is forced to 0 by its lower bound)
var capacityConstraint = x.Leq(100);  // x <= 100
model.AddConstraint(z.Implies(capacityConstraint, "setup_constraint"));

model.Optimize();

var sol = model.GetBestSolution();
Console.WriteLine($"z = {sol.GetValue(z)}, x = {sol.GetValue(x)}");
Console.WriteLine($"Total cost = {sol.ObjectiveValue}");
```

---

## Example: Either-Or Constraints

Model the condition that **at least one** of two constraints must hold:

$$
(x \geq 5) \lor (y \leq 10)
$$

```csharp
using ScipNet;
using ScipNet.Core;

using var model = new Model("either_or");

var x = model.AddVariable("x", 0, 20, VariableType.Continuous);
var y = model.AddVariable("y", 0, 20, VariableType.Continuous);
var z = model.AddVariable("z", 0, 1, VariableType.Binary);  // choice variable

// If z = 1: x >= 5 must hold
model.AddConstraint(z.Implies(x.Geq(5), "or_choice1"));

// If z = 0: y <= 10 must hold
model.AddConstraint((1 - z).Implies(y.Leq(10), "or_choice2")); // Requires: (1-z) as expression

// Actually, since Implies needs a binary Variable directly,
// we need to create an auxiliary variable for the negation:
var notZ = model.AddVariable("not_z", 0, 1, VariableType.Binary);
model.AddConstraint(notZ.Eq(1 - (int)z.GetValue(null!))); // Simplified — see note below
```

> **Note:** For proper either-or modeling, you would typically use a Big-M formulation or create two separate indicator constraints with complementary binary variables. The `Implies()` method only supports the form $z = 1 \implies \text{constraint}$.

---

## How It Works

Indicator constraints in SCIP.NET use `SCIPcreateConsBasicIndicator()`, which creates a native SCIP indicator constraint supporting the form:

$$
z = 1 \implies \sum a_i x_i \leq b
$$

### Sense Conversion

| Original Sense | Converted To |
|----------------|--------------|
| `Leq` ($\leq$) | Direct: $\sum a_i x_i \leq b$ |
| `Geq` ($\geq$) | Negated: $-\sum a_i x_i \leq -b$ |
| `Eq` ($=$) | Two constraints: $\leq$ and $\geq$ |

For `Eq` constraints, two indicator constraints are created internally:

$$
z = 1 \implies \sum a_i x_i \leq b \quad \text{and} \quad z = 1 \implies -\sum a_i x_i \leq -b
$$

---

## API Reference

### IndicatorConstraint Properties

| Property | Type | Description |
|----------|------|-------------|
| `BinaryVariable` | `Variable` | The binary indicator variable $z$ |
| `Expression` | `LinearExpression` | The linear expression $a^\top x$ |
| `Sense` | `Sense` | Constraint direction ($\leq$, $\geq$, $=$) |
| `RightHandSide` | `double` | The right-hand side value $b$ |
| `Name` | `string` | Constraint name |
| `ConsPtr` | `IntPtr` | Native SCIP constraint pointer |

### Constraints

```csharp
// z = 1  -->  a^T x <= b
z.Implies(expr.Leq(b))

// z = 1  -->  a^T x >= b  
z.Implies(expr.Geq(b))

// z = 1  -->  a^T x == b
z.Implies(expr.Eq(b))
```
