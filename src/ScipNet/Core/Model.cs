using System.Collections.Generic;
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
        double objective,
        VariableType type)
    {
        ReturnCode ret = ScipNativeMethods.SCIPcreateVarBasic(
            _scipHandle,
            out IntPtr varPtr,
            name,
            lowerBound,
            upperBound,
            objective,
            type);
        ErrorHandler.CheckReturnCode(ret, $"Failed to create variable {name}");

        ret = ScipNativeMethods.SCIPaddVar(_scipHandle, varPtr);
        ErrorHandler.CheckReturnCode(ret, $"Failed to add variable {name}");

        var variable = new Variable(this, name, varPtr, type, lowerBound, upperBound, objective);
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
    /// 设置目标函数
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
