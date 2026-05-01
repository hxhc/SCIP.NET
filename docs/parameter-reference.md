# Parameter Reference

This document covers all methods for configuring SCIP solver parameters through SCIP.NET.

---

## Setting Parameters

SCIP.NET provides typed parameter setter methods on the `Model` class:

```csharp
model.SetBoolParam("parameter/name", true);
model.SetIntParam("parameter/name", 42);
model.SetLongParam("parameter/name", 100000L);
model.SetRealParam("parameter/name", 3.14);
model.SetStringParam("parameter/name", "value");
```

### Getting Parameters

```csharp
bool   val = model.GetBoolParam("parameter/name");
int    val = model.GetIntParam("parameter/name");
double val = model.GetRealParam("parameter/name");
string? val = model.GetStringParam("parameter/name");
```

---

## Emphasis Modes

`SetEmphasis()` quickly configures SCIP for specific solving strategies:

```csharp
model.SetEmphasis(ParamEmphasis.ModeName, quiet: true);
```

| Mode | Enum Value | Description |
|------|-----------|-------------|
| `Default` | `0` | Default parameter settings |
| `CPSolver` | `1` | CP solver mode (e.g., without LP relaxation) |
| `EasyCIP` | `2` | Solve easy problems quickly |
| `Feasibility` | `3` | Detect feasibility quickly |
| `HardLP` | `4` | Handle difficult LP problems |
| `Optimality` | `5` | Prove optimality quickly |
| `Counter` | `6` | Counting/enumeration mode (for `Count()`) |
| `PhaseFeas` | `7` | Feasibility phase of 3-phase solving |
| `PhaseImprove` | `8` | Improvement phase of 3-phase solving |
| `PhaseProof` | `9` | Proof phase of 3-phase solving |
| `Numerics` | `10` | Solve numerical problems |
| `Benchmark` | `11` | Benchmark mode |

---

## Key Parameters for Tuning

### Limits

| Parameter | Type | Description |
|-----------|------|-------------|
| `limits/time` | `double` | Time limit in seconds |
| `limits/memory` | `double` | Memory limit in MB |
| `limits/nodes` | `long` | Node limit |
| `limits/gap` | `double` | Relative gap limit |
| `limits/solutions` | `long` | Solution count limit |
| `limits/restarts` | `long` | Restart limit |

### Display

| Parameter | Type | Description |
|-----------|------|-------------|
| `display/verblevel` | `int` | Verbosity level (0=silent, 4=all) |

### Presolving

| Parameter | Type | Description |
|-----------|------|-------------|
| `presolving/maxrounds` | `int` | Max presolving rounds (-1=default, 0=disable) |

### Counting (Solution Pool)

| Parameter | Type | Description |
|-----------|------|-------------|
| `constraints/countsols/collect` | `bool` | Collect solutions (not just count) |
| `constraints/countsols/sollimit` | `long` | Max solutions to collect |
| `constraints/countsols/collectlazy` | `bool` | Collect lazy solutions |

### Separation

| Parameter | Type | Description |
|-----------|------|-------------|
| `separating/maxrounds` | `int` | Max separation rounds |
| `separating/maxroundsroot` | `int` | Max separation rounds at root |
| `separating/cutagelimit` | `int` | Cut age limit |

### Heuristics

| Parameter | Type | Description |
|-----------|------|-------------|
| `heuristics/rounding` | `bool` | Enable rounding heuristic |
| `heuristics/oneopt` | `bool` | Enable 1-opt heuristic |
| `heuristics/zirounding` | `bool` | Enable zero-half rounding |
| `heuristics/trivial` | `bool` | Enable trivial heuristic |
| `heuristics/shifting` | `bool` | Enable shifting heuristic |
| `heuristics/simpleRounding` | `bool` | Enable simple rounding |
| `heuristics/rens` | `bool` | Enable RENS heuristic |
| `heuristics/randrounding` | `bool` | Enable random rounding |
| `heuristics/objpscostdiving` | `bool` | Enable objective diving |
| `heuristics/nlpdiving` | `bool` | Enable NLP diving |
| `heuristics/mutation` | `bool` | Enable mutation |
| `heuristics/lpface` | `bool` | Enable LP face |
| `heuristics/lns` | `bool` | Enable LNS heuristic |
| `heuristics/intshifting` | `bool` | Enable integer shifting |
| `heuristics/indicator` | `bool` | Enable indicator heuristic |
| `heuristics/feaspump` | `bool` | Enable feasibility pump |
| `heuristics/dins` | `bool` | Enable DINS heuristic |
| `heuristics/coefdiving` | `bool` | Enable coefficient diving |
| `heuristics/clique` | `bool` | Enable clique heuristic |
| `heuristics/crossover` | `bool` | Enable crossover |
| `heuristics/completesol` | `bool` | Enable complete solution |
| `heuristics/octane` | `bool` | Enable OCTANE heuristic |
| `heuristics/oneopt` | `bool` | Enable 1-opt |
| `heuristics/trysol` | `bool` | Enable try solution |
| `heuristics/undercover` | `bool` | Enable undercover |
| `heuristics/vbounds` | `bool` | Enable V-bound heuristic |
| `heuristics/veclendive` | `bool` | Enable vector length diving |
| `heuristics/zeroobj` | `bool` | Enable zero objective |
| `heuristics/localbranching` | `bool` | Enable local branching |
| `heuristics/multistart` | `bool` | Enable multistart |

### Branching

| Parameter | Type | Description |
|-----------|------|-------------|
| `branching/leastinf/priority` | `int` | Least infeasible branching priority |
| `branching/mostinf/priority` | `int` | Most infeasible branching priority |
| `branching/pscost/priority` | `int` | Pseudo-cost branching priority |
| `branching/relpscost/priority` | `int` | Reliability pseudo-cost branching priority |
| `branching/cloud/priority` | `int` | Cloud branching priority |
| `branching/fullstrong/priority` | `int` | Full strong branching priority |
| `branching/inference/priority` | `int` | Inference branching priority |
| `branching/random/priority` | `int` | Random branching priority |
| `branching/vanillafullstrong/priority` | `int` | Vanilla full strong branching priority |

### Numerics

| Parameter | Type | Description |
|-----------|------|-------------|
| `numerics/feastol` | `double` | Feasibility tolerance |
| `numerics/dualfeastol` | `double` | Dual feasibility tolerance |
| `numerics/barrierconvtol` | `double` | Barrier convergence tolerance |

---

## Examples

### Enable Optimality Emphasis

```csharp
model.SetEmphasis(ParamEmphasis.Optimality, quiet: true);
model.SetRealParam("limits/time", 300);  // 5 minute limit
model.SetRealParam("limits/gap", 0.01);  // 1% gap
```

### Quick Feasibility Check

```csharp
model.SetEmphasis(ParamEmphasis.Feasibility, quiet: true);
model.SetIntParam("limits/nodes", 1000);
```

### Silent Mode (No Output)

```csharp
model.SetIntParam("display/verblevel", 0);
```

### Counting Configuration

```csharp
model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
model.SetBoolParam("constraints/countsols/collect", true);
model.SetLongParam("constraints/countsols/sollimit", 100000);
model.SetRealParam("limits/time", 60);
model.SetIntParam("display/verblevel", 0);
model.SetIntParam("presolving/maxrounds", 0);
```

### Custom Tuning

```csharp
model.SetIntParam("display/verblevel", 2);
model.SetRealParam("limits/time", 600);
model.SetRealParam("limits/gap", 0.05);
model.SetBoolParam("heuristics/feaspump", true);
model.SetBoolParam("heuristics/rens", false);
model.SetBoolParam("separating/maxroundsroot", 100);
```

---

## Parameter Types

SCIP.NET maps parameter types to C# types automatically:

| SCIP Type | C# Setter | C# Getter |
|-----------|-----------|-----------|
| `SCIP_Bool` | `SetBoolParam` | `GetBoolParam` |
| `SCIP_Int` | `SetIntParam` | `GetIntParam` |
| `SCIP_Longint` | `SetLongParam` | (use `SetLongParam`) |
| `SCIP_Real` | `SetRealParam` | `GetRealParam` |
| `SCIP_String` | `SetStringParam` | `GetStringParam` |

> **Note:** SCIP parameters use a hierarchical naming convention (e.g., `constraints/countsols/collect`). Use forward slashes regardless of the operating system. Refer to the [SCIP Parameter Documentation](https://scipopt.org/doc/html/PARAMETERS.php) for a complete list.
