using System.Collections.Generic;
using System.Runtime.InteropServices;
using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// 代表 SCIP 优化问题模型
/// </summary>
public sealed class Model : IDisposable
{
    private readonly ScipHandle _scipHandle;
    private readonly Dictionary<string, Variable> _variables;
    private readonly Dictionary<string, Constraint> _constraints;
    private readonly Dictionary<IntPtr, Variable> _varPtrToVarMap; // Mapping from var pointer to Variable object
    private readonly Dictionary<string, Variable> _varNameToVarMap; // Mapping from var name to Variable object
    private bool _disposed;

    /// <summary>
    /// 获取模型名称
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// 获取目标函数方向
    /// </summary>
    public ObjectiveSense ObjectiveSense { get; private set; }

    /// <summary>
    /// 获取所有变量
    /// </summary>
    public IReadOnlyCollection<Variable> Variables => _variables.Values;

    /// <summary>
    /// 获取所有约束
    /// </summary>
    public IReadOnlyCollection<Constraint> Constraints => _constraints.Values;

    /// <summary>
    /// 创建一个新的 SCIP 模型
    /// </summary>
    /// <param name="name">模型名称</param>
    /// <param name="includeDefaultPlugins">是否包含默认插件</param>
    public Model(string name = "model", bool includeDefaultPlugins = true)
    {
        ReturnCode ret = ScipNativeMethods.SCIPcreate(out IntPtr scipPtr);
        ErrorHandler.CheckReturnCode(ret, "Failed to create SCIP instance");

        _scipHandle = new ScipHandle(scipPtr, true);
        _variables = new Dictionary<string, Variable>();
        _constraints = new Dictionary<string, Constraint>();
        _varPtrToVarMap = new Dictionary<IntPtr, Variable>();
        _varNameToVarMap = new Dictionary<string, Variable>(); // For name-based lookup
        Name = name;
        ObjectiveSense = ObjectiveSense.Minimize;

        if (includeDefaultPlugins)
        {
            IncludeDefaultPlugins();
        }

        CreateProblem(name);
    }

    /// <summary>
    /// 添加变量到模型
    /// </summary>
    public Variable AddVariable(
        string name,
        double lowerBound,
        double upperBound,
        VariableType type)
    {
        ReturnCode ret = ScipNativeMethods.SCIPcreateVarBasic(
            _scipHandle,
            out IntPtr varPtr,
            name,
            lowerBound,
            upperBound,
            0.0,
            type);
        ErrorHandler.CheckReturnCode(ret, $"Failed to create variable {name}");

        ret = ScipNativeMethods.SCIPaddVar(_scipHandle, varPtr);
        ErrorHandler.CheckReturnCode(ret, $"Failed to add variable {name}");

        var variable = new Variable(this, name, varPtr, type, lowerBound, upperBound);
        _variables[name] = variable;
        _varPtrToVarMap[varPtr] = variable; // Add to pointer-to-variable mapping
        _varNameToVarMap[name] = variable; // Add to name-to-variable mapping
        return variable;
    }

    /// <summary>
    /// 添加约束到模型
    /// </summary>
    public T AddConstraint<T>(T constraint) where T : Constraint
    {
        // 设置约束的模型引用
        constraint.Model = this;

        IntPtr consPtr = constraint.AddToModel();
        _constraints[constraint.Name] = constraint;
        return constraint;
    }

    /// <summary>
    /// 设置目标函数方向
    /// </summary>
    public void SetObjectiveSense(ObjectiveSense sense)
    {
        ObjectiveSense = sense;
        ReturnCode ret = ScipNativeMethods.SCIPsetObjsense(_scipHandle, sense);
        ErrorHandler.CheckReturnCode(ret, "Failed to set objective sense");
    }

    /// <summary>
    /// 设置目标函数（线性）
    /// </summary>
    /// <param name="expression">目标函数表达式</param>
    /// <param name="sense">目标函数方向</param>
    public void SetObjective(LinearExpression expression, ObjectiveSense sense)
    {
        // 设置目标函数方向
        SetObjectiveSense(sense);
        
        // 设置目标函数系数
        foreach (var kvp in expression.Coefficients)
        {
            ReturnCode ret = ScipNativeMethods.SCIPchgVarObj(_scipHandle, kvp.Key.VarPtr, kvp.Value);
            ErrorHandler.CheckReturnCode(ret, $"Failed to set objective coefficient for variable {kvp.Key.Name}");
        }
    }

    /// <summary>
    /// 设置目标函数（非线性，通过 epigraph reformulation 自动转换）
    /// </summary>
    /// <param name="expression">非线性目标函数表达式</param>
    /// <param name="sense">目标函数方向</param>
    public void SetObjective(NonlinearExpression expression, ObjectiveSense sense)
    {
        // 设置目标函数方向
        SetObjectiveSense(sense);

        // 创建辅助连续变量 objvar（目标系数为 1.0），注册到 _variables 以确保 Dispose 时释放
        ReturnCode ret = ScipNativeMethods.SCIPcreateVarBasic(
            _scipHandle,
            out IntPtr objVarPtr,
            "__objvar__",
            double.NegativeInfinity,
            double.PositiveInfinity,
            1.0,
            VariableType.Continuous);
        ErrorHandler.CheckReturnCode(ret, "Failed to create objective variable");

        ret = ScipNativeMethods.SCIPaddVar(_scipHandle, objVarPtr);
        ErrorHandler.CheckReturnCode(ret, "Failed to add objective variable");

        var objVar = new Variable(this, "__objvar__", objVarPtr, VariableType.Continuous,
            double.NegativeInfinity, double.PositiveInfinity);
        _variables[objVar.Name] = objVar;

        // 构建原生表达式树
        IntPtr exprPtr = expression.BuildExpr(_scipHandle);
        try
        {
            // epigraph reformulation:
            //   minimize f(x)  →  constraint: f(x) - objvar <= 0,  即 f(x) <= objvar
            //   maximize f(x)  →  constraint: f(x) - objvar >= 0,  即 f(x) >= objvar
            double lhs = sense == ObjectiveSense.Minimize ? double.NegativeInfinity : 0.0;
            double rhs = sense == ObjectiveSense.Minimize ? 0.0 : double.PositiveInfinity;

            ret = ScipNativeMethods.SCIPcreateConsBasicNonlinear(
                _scipHandle,
                out IntPtr consPtr,
                "__objcons__",
                exprPtr,
                lhs,
                rhs);
            ErrorHandler.CheckReturnCode(ret, "Failed to create objective constraint");

            // 将 objvar 的系数 -1.0 添加到约束的线性部分
            // 约束实际表达: f(x) - 1.0*objvar <= 0 (min) 或 >= 0 (max)
            ret = ScipNativeMethods.SCIPaddLinearVarNonlinear(_scipHandle, consPtr, objVarPtr, -1.0);
            ErrorHandler.CheckReturnCode(ret, "Failed to add linear var to objective constraint");

            // 添加约束
            ret = ScipNativeMethods.SCIPaddCons(_scipHandle, consPtr);
            ErrorHandler.CheckReturnCode(ret, "Failed to add objective constraint");

            // 记录约束（由 Model 管理释放）
            var objCons = new NonlinearConstraint(expression, lhs, rhs, "__objcons__");
            objCons.Model = this;
            objCons.SetConsPtrInternal(consPtr);
            _constraints[objCons.Name] = objCons;
        }
        finally
        {
            // 释放表达式（约束已捕获）
            ScipNativeMethods.SCIPreleaseExpr(_scipHandle, ref exprPtr);
        }
    }

/// <summary>
/// 优化模型
/// </summary>
public SolveStatus Optimize()
{
    ReturnCode ret = ScipNativeMethods.SCIPsolve(_scipHandle);
    ErrorHandler.CheckReturnCode(ret, "Failed to solve");

    return ScipNativeMethods.SCIPgetStatus(_scipHandle);
}

/// <summary>
/// Count/enumerate all feasible solutions (instead of just solving for optimal)
/// After calling this method, the status will be Infeasible, which is expected behavior.
///
/// IMPORTANT: This method must be called BEFORE GetSparseSolutionsWithVariables().
/// The countsols constraint handler is automatically included by this method.
///
/// Required parameters (set before calling):
///   model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
///   model.SetBoolParam("constraints/countsols/collect", true);
///   model.SetLongParam("constraints/countsols/sollimit", 100000);
/// </summary>
public SolveStatus Count()
{
    // Include the countsols constraint handler if not already included
    // Note: In SCIP 9.0+, this IS included by SCIPincludeDefaultPlugins(),
    // so we need to check if it exists before including it to avoid InvalidData error
    IntPtr conshdlr = ScipNativeMethods.SCIPfindConshdlr(_scipHandle, "countsols");
    if (conshdlr == IntPtr.Zero)
    {
        ReturnCode includeRet = ScipNativeMethods.SCIPincludeConshdlrCountsols(_scipHandle);
        ErrorHandler.CheckReturnCode(includeRet, "Failed to include countsols constraint handler");
    }

    // Set safe counting parameters (including disabling restarts)
    ReturnCode ret = ScipNativeMethods.SCIPsetParamsCountsols(_scipHandle);
    ErrorHandler.CheckReturnCode(ret, "Failed to set counting parameters");

    // Start the counting process
    ret = ScipNativeMethods.SCIPcount(_scipHandle);
    ErrorHandler.CheckReturnCode(ret, "Failed to count solutions");

    return ScipNativeMethods.SCIPgetStatus(_scipHandle);
}

/// <summary>
/// 获取计数的解数量
/// </summary>
public long GetCountedSolutionsCount()
{
    IntPtr valid;
    long count = ScipNativeMethods.SCIPgetNCountedSols(_scipHandle, out valid);
    return count;
}

/// <summary>
/// Get raw counted sparse solutions (relative to active variables)
/// Note: SCIPgetCountedSparseSols returns void, so no error code to check
/// </summary>
public (IntPtr vars, int nvars, IntPtr sols, int nsols) GetCountedSparseSolutions()
{
    ScipNativeMethods.SCIPgetCountedSparseSols(
        _scipHandle,
        out IntPtr vars,
        out int nvars,
        out IntPtr sols,
        out int nsols);

    return (vars, nvars, sols, nsols);
}

/// <summary>
/// Check if sparse solutions are available after calling Count()
/// Note: No need to free the returned arrays - they are managed internally by SCIP
/// </summary>
public bool AreSparseSolutionsAvailable()
{
    try
    {
        var (_, _, sols, nsols) = GetCountedSparseSolutions();
        bool available = (nsols > 0 && sols != IntPtr.Zero);
        // No need to free - arrays are managed internally by SCIP
        return available;
    }
    catch
    {
        return false;
    }
}

/// <summary>
/// Get all sparse solutions unrolled into concrete solutions with variable values.
///
/// This follows the official SCIP approach:
/// 1. Get sparse solutions via SCIPgetCountedSparseSols
/// 2. For each sparse solution, iterate through concrete solutions
///    using SCIPsparseSolGetFirstSol / SCIPsparseSolGetNextSol
/// 3. Map variable pointers back to Variable objects
///
/// MUST be called after Count() with constraints/countsols/collect = true.
/// </summary>
public List<Dictionary<Variable, double>> GetSparseSolutionsWithVariables()
{
    var solutions = new List<Dictionary<Variable, double>>();

    // Get sparse solutions
    var (vars, nvars, sols, nsols) = GetCountedSparseSolutions();

    if (nsols == 0 || sols == IntPtr.Zero)
    {
        // No need to free - arrays are managed internally by SCIP
        return solutions;
    }

    // Read array of SCIP_SPARSESOL* pointers
    IntPtr[] sparseSolPtrs = new IntPtr[nsols];
    for (int i = 0; i < nsols; i++)
    {
        sparseSolPtrs[i] = Marshal.ReadIntPtr(sols, i * IntPtr.Size);
    }

    // Iterate each sparse solution and unroll into concrete solutions
    for (int s = 0; s < nsols; s++)
    {
        IntPtr sparsesol = sparseSolPtrs[s];

        // Get variable info for this sparse solution
        IntPtr solVars = ScipNativeMethods.SCIPsparseSolGetVars(sparsesol);
        int solNVars = ScipNativeMethods.SCIPsparseSolGetNVars(sparsesol);

        if (solVars == IntPtr.Zero || solNVars == 0)
            continue;

        // Allocate buffer for concrete solution values (SCIP_Longint[] = int64[])
        IntPtr concreteSolBuf = Marshal.AllocHGlobal(solNVars * sizeof(long));

        try
        {
            // Get first concrete solution (void return in SCIP API)
            ScipNativeMethods.SCIPsparseSolGetFirstSol(sparsesol, concreteSolBuf, solNVars);

                // Iterate through all concrete solutions in this sparse solution
                int concreteCount = 0;
                do
                {
                    var solutionValues = new Dictionary<Variable, double>();

                    for (int v = 0; v < solNVars; v++)
                    {
                        IntPtr varPtr = Marshal.ReadIntPtr(solVars, v * IntPtr.Size);
                        long value = Marshal.ReadInt64(concreteSolBuf, v * sizeof(long));

                        // Try to get variable by name (since pointers may not match after SCIP transformation)
                        IntPtr varNamePtr = ScipNativeMethods.SCIPvarGetName(varPtr);
                        string varName = Marshal.PtrToStringAnsi(varNamePtr) ?? "";

                        // Try direct name match first
                        if (_varNameToVarMap.TryGetValue(varName, out Variable? variable))
                        {
                            solutionValues[variable] = (double)value;
                        }
                        // If not found, try stripping common prefixes (like "t_")
                        else if (varName.StartsWith("t_"))
                        {
                            string originalName = varName.Substring(2); // Remove "t_" prefix
                            if (_varNameToVarMap.TryGetValue(originalName, out variable))
                            {
                                solutionValues[variable] = (double)value;
                            }
                        }
                    }

                    solutions.Add(solutionValues);
                    concreteCount++;

                } while (ScipNativeMethods.SCIPsparseSolGetNextSol(sparsesol, concreteSolBuf, solNVars));
        }
        finally
        {
            Marshal.FreeHGlobal(concreteSolBuf);
        }
    }
    // Note: No need to free sparse solutions arrays - they are managed internally by SCIP

    return solutions;
}

/// <summary>
/// 获取最优解
/// </summary>
    public Solution? GetBestSolution()
    {
        IntPtr solPtr = ScipNativeMethods.SCIPgetBestSol(_scipHandle);
        if (solPtr == IntPtr.Zero)
        {
            return null;
        }

        return new Solution(this, solPtr);
    }

    /// <summary>
    /// 获取解池中的所有解
    /// </summary>
    public IReadOnlyList<Solution> GetSolutions()
    {
        int nsols = ScipNativeMethods.SCIPgetNSols(_scipHandle);
        if (nsols == 0)
        {
            return Array.Empty<Solution>();
        }

        IntPtr solsPtr = ScipNativeMethods.SCIPgetSols(_scipHandle);
        if (solsPtr == IntPtr.Zero)
        {
            return Array.Empty<Solution>();
        }

        var solutions = new Solution[nsols];
        for (int i = 0; i < nsols; i++)
        {
            IntPtr solPtr = Marshal.ReadIntPtr(solsPtr, i * IntPtr.Size);
            double objVal = ScipNativeMethods.SCIPgetSolOrigObj(_scipHandle, solPtr);
            solutions[i] = new Solution(this, solPtr, objVal);
        }

        return solutions;
    }

    /// <summary>
    /// 获取解池中的解数量
    /// </summary>
    public int SolutionCount => ScipNativeMethods.SCIPgetNSols(_scipHandle);

    /// <summary>
    /// 设置布尔参数
    /// </summary>
    public void SetBoolParam(string name, bool value)
    {
        ReturnCode ret = ScipNativeMethods.SCIPsetBoolParam(_scipHandle, name, value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to set bool param '{name}'");
    }

/// <summary>
/// 设置整数参数
/// </summary>
public void SetIntParam(string name, int value)
{
    ReturnCode ret = ScipNativeMethods.SCIPsetIntParam(_scipHandle, name, value);
    ErrorHandler.CheckReturnCode(ret, $"Failed to set int param '{name}'");
}

/// <summary>
/// 设置长整型参数
/// </summary>
public void SetLongParam(string name, long value)
{
    ReturnCode ret = ScipNativeMethods.SCIPsetLongintParam(_scipHandle, name, value);
    ErrorHandler.CheckReturnCode(ret, $"Failed to set longint param '{name}'");
}

/// <summary>
/// 设置实数参数
/// </summary>
    public void SetRealParam(string name, double value)
    {
        ReturnCode ret = ScipNativeMethods.SCIPsetRealParam(_scipHandle, name, value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to set real param '{name}'");
    }

    /// <summary>
    /// 设置字符串参数
    /// </summary>
    public void SetStringParam(string name, string value)
    {
        ReturnCode ret = ScipNativeMethods.SCIPsetStringParam(_scipHandle, name, value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to set string param '{name}'");
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public Statistics GetStatistics()
    {
        return new Statistics(this);
    }

    /// <summary>
    /// 获取整数参数值
    /// </summary>
    public int GetIntParam(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPgetIntParam(_scipHandle, name, out int value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to get int param '{name}'");
        return value;
    }

    /// <summary>
    /// 获取实数参数值
    /// </summary>
    public double GetRealParam(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPgetRealParam(_scipHandle, name, out double value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to get real param '{name}'");
        return value;
    }

    /// <summary>
    /// 获取布尔参数值
    /// </summary>
    public bool GetBoolParam(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPgetBoolParam(_scipHandle, name, out bool value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to get bool param '{name}'");
        return value;
    }

/// <summary>
/// 获取字符串参数值
/// </summary>
public string? GetStringParam(string name)
{
    ReturnCode ret = ScipNativeMethods.SCIPgetStringParam(_scipHandle, name, out IntPtr value);
    ErrorHandler.CheckReturnCode(ret, $"Failed to get string param '{name}'");
    if (value == IntPtr.Zero)
    {
        return null;
    }
    return Marshal.PtrToStringAnsi(value);
}

/// <summary>
/// 设置参数强调模式
/// </summary>
/// <param name="paramEmphasis">参数强调模式</param>
/// <param name="quiet">是否静默设置（不输出信息）</param>
public void SetEmphasis(ParamEmphasis paramEmphasis, bool quiet = false)
{
    ReturnCode ret = ScipNativeMethods.SCIPsetEmphasis(_scipHandle, paramEmphasis, quiet);
    ErrorHandler.CheckReturnCode(ret, $"Failed to set emphasis to {paramEmphasis}");
}

/// <summary>
/// 释放资源
/// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // 释放所有变量
            foreach (var kvp in _variables)
            {
                IntPtr varPtr = kvp.Value.VarPtr;
                if (varPtr != IntPtr.Zero)
                {
                    ReturnCode ret = ScipNativeMethods.SCIPreleaseVar(_scipHandle, ref varPtr);
                    if (ret != ReturnCode.Okay)
                    {
                        // 忽略释放失败，继续释放其他资源
                    }
                    // varPtr 现在应该为 IntPtr.Zero（由 SCIPreleaseVar 设置）
                }
            }

            // 释放所有约束
            foreach (var kvp in _constraints)
            {
                IntPtr consPtr = kvp.Value.ConsPtr;
                if (consPtr != IntPtr.Zero)
                {
                    ReturnCode ret = ScipNativeMethods.SCIPreleaseCons(_scipHandle, ref consPtr);
                    if (ret != ReturnCode.Okay)
                    {
                        // 忽略释放失败，继续释放其他资源
                    }
                    // consPtr 现在应该为 IntPtr.Zero（由 SCIPreleaseCons 设置）
                }
            }

            // 释放SCIP实例
            _scipHandle?.Dispose();
            _disposed = true;
        }
    }

    private void IncludeDefaultPlugins()
    {
        ReturnCode ret = ScipNativeMethods.SCIPincludeDefaultPlugins(_scipHandle);
        ErrorHandler.CheckReturnCode(ret, "Failed to include default plugins");
    }

    private void CreateProblem(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPcreateProbBasic(_scipHandle, name);
        ErrorHandler.CheckReturnCode(ret, $"Failed to create problem {name}");
    }

    internal ScipHandle ScipHandle => _scipHandle;

    public override string ToString()
    {
        return $"Model '{Name}' with {_variables.Count} variables and {_constraints.Count} constraints";
    }
}
