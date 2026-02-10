using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ScipNet.Core;

/// <summary>
/// 代表线性表达式
/// </summary>
public sealed class LinearExpression
{
    private readonly Dictionary<Variable, double> _coefficients;
    private double _constant;

    /// <summary>
    /// 获取常数项
    /// </summary>
    public double Constant => _constant;

    /// <summary>
    /// 获取系数字典
    /// </summary>
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
    /// 添加项到表达式
    /// </summary>
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
    /// 添加常数项
    /// </summary>
    public LinearExpression AddConstant(double value)
    {
        _constant += value;
        return this;
    }

    /// <summary>
    /// 加法运算符
    /// </summary>
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
    /// 减法运算符
    /// </summary>
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

    /// <summary>
    /// 乘法运算符
    /// </summary>
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
    /// 计算表达式的值
    /// </summary>
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
    /// 创建小于等于约束
    /// </summary>
    public LinearConstraint Leq(double rhs)
    {
        return new LinearConstraint(this, Sense.LessThanOrEqual, rhs);
    }

    /// <summary>
    /// 创建大于等于约束
    /// </summary>
    public LinearConstraint Geq(double rhs)
    {
        return new LinearConstraint(this, Sense.GreaterThanOrEqual, rhs);
    }

    /// <summary>
    /// 创建等于约束
    /// </summary>
    public LinearConstraint Eq(double rhs)
    {
        return new LinearConstraint(this, Sense.Equal, rhs);
    }

    /// <summary>
    /// 创建范围约束
    /// </summary>
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
