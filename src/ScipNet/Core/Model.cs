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
