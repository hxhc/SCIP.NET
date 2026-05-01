using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ScipNet.Core;

/// <summary>
/// Represents a linear expression
/// </summary>
// 表示一个线性表达式
public sealed class LinearExpression
{
    private readonly Dictionary<Variable, double> _coefficients;
    private double _constant;

    /// <summary>
    /// Gets the constant term
    /// </summary>
    // 获取常数项
    public double Constant => _constant;

    /// <summary>
    /// Gets the coefficient dictionary
    /// </summary>
    // 获取系数字典
    public IReadOnlyDictionary<Variable, double> Coefficients => _coefficients;

    public LinearExpression()
    {
        _coefficients = new Dictionary<Variable, double>();
        _constant = 0.0;
    }

    private LinearExpression(Dictionary<Variable, double> coefficients, double constant)
    {
        _coefficients = coefficients;
        _constant = constant;
    }

    /// <summary>
    /// Adds a term to the expression
    /// </summary>
    // 向表达式添加项
    public LinearExpression AddTerm(Variable variable, double coefficient)
    {
        if (_coefficients.TryGetValue(variable, out double existing))
        {
            _coefficients[variable] = existing + coefficient;
        }
        else
        {
            _coefficients[variable] = coefficient;
        }
        return this;
    }

    /// <summary>
    /// Adds a constant term
    /// </summary>
    // 添加常数项
    public LinearExpression AddConstant(double value)
    {
        _constant += value;
        return this;
    }

    /// <summary>
    /// Addition operator
    /// </summary>
    // 加法运算符
    public static LinearExpression operator +(LinearExpression left, LinearExpression right)
    {
        var result = new Dictionary<Variable, double>();

        foreach (var kvp in left._coefficients)
        {
            result[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in right._coefficients)
        {
            if (result.TryGetValue(kvp.Key, out double existing))
            {
                result[kvp.Key] = existing + kvp.Value;
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }

        return new LinearExpression(result, left._constant + right._constant);
    }

    public static LinearExpression operator +(LinearExpression expr, double value)
    {
        var result = new Dictionary<Variable, double>(expr._coefficients);
        return new LinearExpression(result, expr._constant + value);
    }

    public static LinearExpression operator +(double value, LinearExpression expr)
    {
        return expr + value;
    }

    public static LinearExpression operator +(LinearExpression expr, Variable variable)
    {
        var result = new Dictionary<Variable, double>(expr._coefficients);
        if (result.TryGetValue(variable, out double existing))
        {
            result[variable] = existing + 1.0;
        }
        else
        {
            result[variable] = 1.0;
        }
        return new LinearExpression(result, expr._constant);
    }

    public static LinearExpression operator +(Variable variable, LinearExpression expr)
    {
        return expr + variable;
    }

    /// <summary>
    /// Subtraction operator
    /// </summary>
    // 减法运算符
    public static LinearExpression operator -(LinearExpression left, LinearExpression right)
    {
        var result = new Dictionary<Variable, double>();

        foreach (var kvp in left._coefficients)
        {
            result[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in right._coefficients)
        {
            if (result.TryGetValue(kvp.Key, out double existing))
            {
                result[kvp.Key] = existing - kvp.Value;
            }
            else
            {
                result[kvp.Key] = -kvp.Value;
            }
        }

        return new LinearExpression(result, left._constant - right._constant);
    }

    public static LinearExpression operator -(LinearExpression expr, double value)
    {
        var result = new Dictionary<Variable, double>(expr._coefficients);
        return new LinearExpression(result, expr._constant - value);
    }

    public static LinearExpression operator -(LinearExpression expr, Variable variable)
    {
        var result = new Dictionary<Variable, double>(expr._coefficients);
        if (result.TryGetValue(variable, out double existing))
        {
            result[variable] = existing - 1.0;
        }
        else
        {
            result[variable] = -1.0;
        }
        return new LinearExpression(result, expr._constant);
    }

    public static LinearExpression operator -(Variable variable, LinearExpression expr)
    {
        var result = new Dictionary<Variable, double>(expr._coefficients);
        foreach (var key in new List<Variable>(result.Keys))
        {
            result[key] = -result[key];
        }
        if (result.TryGetValue(variable, out double existing))
        {
            result[variable] = existing + 1.0;
        }
        else
        {
            result[variable] = 1.0;
        }
        return new LinearExpression(result, -expr._constant);
    }

    /// <summary>
    /// Subtraction operator (symmetric form: double - LinearExpression)
    /// </summary>
    // 减法运算符（对称形式：double - LinearExpression）
    public static LinearExpression operator -(double value, LinearExpression expr)
    {
        var result = new Dictionary<Variable, double>();
        foreach (var kvp in expr._coefficients)
        {
            result[kvp.Key] = -kvp.Value;
        }
        return new LinearExpression(result, value - expr._constant);
    }

    /// <summary>
    /// Multiplication operator
    /// </summary>
    // 乘法运算符
    public static LinearExpression operator *(LinearExpression expr, double scalar)
    {
        var result = new Dictionary<Variable, double>();
        foreach (var kvp in expr._coefficients)
        {
            result[kvp.Key] = kvp.Value * scalar;
        }
        return new LinearExpression(result, expr._constant * scalar);
    }

    public static LinearExpression operator *(double scalar, LinearExpression expr)
    {
        return expr * scalar;
    }

    /// <summary>
    /// Calculates the value of the expression
    /// </summary>
    // 计算表达式的值
    public double Evaluate(Solution solution)
    {
        double value = _constant;
        foreach (var kvp in _coefficients)
        {
            value += kvp.Value * kvp.Key.GetSolValue(solution);
        }
        return value;
    }

    /// <summary>
    /// Creates a less than or equal to constraint
    /// </summary>
    // 创建小于等于约束
    public LinearConstraint Leq(double rhs)
    {
        return new LinearConstraint(this, Sense.LessThanOrEqual, rhs);
    }

    /// <summary>
    /// Creates a greater than or equal to constraint
    /// </summary>
    // 创建大于等于约束
    public LinearConstraint Geq(double rhs)
    {
        return new LinearConstraint(this, Sense.GreaterThanOrEqual, rhs);
    }

    /// <summary>
    /// Creates an equal to constraint
    /// </summary>
    // 创建等于约束
    public LinearConstraint Eq(double rhs)
    {
        return new LinearConstraint(this, Sense.Equal, rhs);
    }

    /// <summary>
    /// Creates a range constraint
    /// </summary>
    // 创建范围约束
    public RangeConstraint Between(double lb, double ub)
    {
        return new RangeConstraint(this, lb, ub);
    }

    public override string ToString()
    {
        var parts = new List<string>();

        foreach (var kvp in _coefficients)
        {
            if (kvp.Value == 1.0)
            {
                parts.Add(kvp.Key.Name);
            }
            else if (kvp.Value == -1.0)
            {
                parts.Add($"-{kvp.Key.Name}");
            }
            else
            {
                parts.Add($"{kvp.Value}*{kvp.Key.Name}");
            }
        }

        if (_constant != 0.0)
        {
            parts.Add(_constant.ToString());
        }

        return string.Join(" + ", parts);
    }

    internal Dictionary<Variable, double> GetCoefficients()
    {
        return _coefficients;
    }

    internal double GetConstant()
    {
        return _constant;
    }
}
