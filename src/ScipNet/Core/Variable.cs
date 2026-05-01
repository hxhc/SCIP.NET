using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents a decision variable in the optimization problem
/// </summary>
// 决策变量
public sealed class Variable
{
    private readonly Model _model;
    private readonly string _name;
    private readonly VariableType _type;
    private readonly IntPtr _varPtr;
    private double _lb;
    private double _ub;

/// <summary>
/// Gets the variable name
/// </summary>
// 获取变量名
public string Name => _name;

/// <summary>
/// Gets the variable type
/// </summary>
// 获取变量类型
public VariableType Type => _type;

/// <summary>
/// Gets the lower bound
/// </summary>
// 获取下界
public double LowerBound => _lb;

/// <summary>
/// Gets the upper bound
/// </summary>
// 获取上界
public double UpperBound => _ub;

    internal Variable(
        Model model,
        string name,
        IntPtr varPtr,
        VariableType type,
        double lowerBound,
        double upperBound)
    {
        _model = model;
        _name = name;
        _varPtr = varPtr;
        _type = type;
        _lb = lowerBound;
        _ub = upperBound;
    }

    internal IntPtr VarPtr => _varPtr;

/// <summary>
/// Gets the value of the variable in the specified solution
/// </summary>
// 获取变量在指定解中的值
public double GetSolValue(Solution solution)
    {
        return ScipNativeMethods.SCIPgetSolVal(_model.ScipHandle, solution.SolPtr, _varPtr);
    }

/// <summary>
/// Creates an Indicator constraint: when this binary variable is 1, the given linear constraint holds
/// </summary>
// 创建指示约束：当此二元变量为1时，给定线性约束成立
public IndicatorConstraint Implies(LinearConstraint constraint, string? name = null)
    {
        return new IndicatorConstraint(this, constraint.Expression, constraint.Sense, constraint.RightHandSide, name);
    }

/// <summary>
/// Creates a linear expression (multiplication)
/// </summary>
// 创建线性表达式（乘法）
public static LinearExpression operator *(Variable variable, double coefficient)
    {
        return new LinearExpression().AddTerm(variable, coefficient);
    }

    public static LinearExpression operator *(double coefficient, Variable variable)
    {
        return variable * coefficient;
    }

/// <summary>
/// Creates a linear expression (addition)
/// </summary>
// 创建线性表达式（加法）
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
/// Creates a linear expression (subtraction)
/// </summary>
// 创建线性表达式（减法）
    public static LinearExpression operator -(Variable variable, double value)
    {
        return new LinearExpression().AddTerm(variable, 1.0).AddConstant(-value);
    }

    public static LinearExpression operator -(double value, Variable variable)
    {
        return new LinearExpression().AddTerm(variable, -1.0).AddConstant(value);
    }

    public static LinearExpression operator -(Variable left, Variable right)
    {
        return new LinearExpression().AddTerm(left, 1.0).AddTerm(right, -1.0);
    }

    public override string ToString()
    {
        return $"{_name} [{_type}, {_lb}, {_ub}]";
    }
}
