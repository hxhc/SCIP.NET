using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// 代表优化问题中的决策变量
/// </summary>
public sealed class Variable
{
    private readonly Model _model;
    private readonly string _name;
    private readonly VariableType _type;
    private readonly IntPtr _varPtr;
    private double _lb;
    private double _ub;
    private double _obj;

    /// <summary>
    /// 获取变量名称
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 获取变量类型
    /// </summary>
    public VariableType Type => _type;

    /// <summary>
    /// 获取下界
    /// </summary>
    public double LowerBound => _lb;

    /// <summary>
    /// 获取上界
    /// </summary>
    public double UpperBound => _ub;

    /// <summary>
    /// 获取目标函数系数
    /// </summary>
    public double Objective => _obj;

    internal Variable(
        Model model,
        string name,
        IntPtr varPtr,
        VariableType type,
        double lowerBound,
        double upperBound,
        double objective)
    {
        _model = model;
        _name = name;
        _varPtr = varPtr;
        _type = type;
        _lb = lowerBound;
        _ub = upperBound;
        _obj = objective;
    }

    internal IntPtr VarPtr => _varPtr;

    /// <summary>
    /// 获取变量在指定解中的值
    /// </summary>
    public double GetSolValue(Solution solution)
    {
        return ScipNativeMethods.SCIPgetSolVal(_model.ScipHandle, solution.SolPtr, _varPtr);
    }

    /// <summary>
    /// 创建线性表达式（乘法）
    /// </summary>
    public static LinearExpression operator *(Variable variable, double coefficient)
    {
        return new LinearExpression().AddTerm(variable, coefficient);
    }

    public static LinearExpression operator *(double coefficient, Variable variable)
    {
        return variable * coefficient;
    }

    /// <summary>
    /// 创建线性表达式（加法）
    /// </summary>
    public static LinearExpression operator +(Variable variable, double value)
    {
        return new LinearExpression().AddTerm(variable, 1.0).AddConstant(value);
    }

    public static LinearExpression operator +(double value, Variable variable)
    {
        return variable + value;
    }

    public static LinearExpression operator +(Variable left, Variable right)
    {
        return new LinearExpression().AddTerm(left, 1.0).AddTerm(right, 1.0);
    }

    /// <summary>
    /// 创建线性表达式（减法）
    /// </summary>
    public static LinearExpression operator -(Variable variable, double value)
    {
        return new LinearExpression().AddTerm(variable, 1.0).AddConstant(-value);
    }

    public static LinearExpression operator -(Variable left, Variable right)
    {
        return new LinearExpression().AddTerm(left, 1.0).AddTerm(right, -1.0);
    }

    public override string ToString()
    {
        return $"{_name} [{_type}, {_lb}, {_ub}, obj={_obj}]";
    }
}
