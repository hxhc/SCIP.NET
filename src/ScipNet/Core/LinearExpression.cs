using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ScipNet.Core;

/// <summary>
/// Represents a linear expression
/// </summary>
public sealed class LinearExpression
{
    private readonly Dictionary<Variable, double> _coefficients;
    private double _constant;

    /// <summary>
    /// Gets the constant term
    /// </summary>
    public double Constant => _constant;

    /// <summary>
    /// Gets the coefficient dictionary
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
    /// Adds a term to the expression
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
    /// Adds a constant term
    /// </summary>
    public LinearExpression AddConstant(double value)
    {
        _constant += value;
        return this;
    }

    /// <summary>
    /// Addition operator
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
    /// Subtraction operator
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
    /// Multiplication operator
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
    /// Calculates the value of the expression
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
    /// Creates a less than or equal to constraint
    /// </summary>
    public LinearConstraint Leq(double rhs)
    {
        return new LinearConstraint(this, Sense.LessThanOrEqual, rhs);
    }

    /// <summary>
    /// Creates a greater than or equal to constraint
    /// </summary>
    public LinearConstraint Geq(double rhs)
    {
        return new LinearConstraint(this, Sense.GreaterThanOrEqual, rhs);
    }

    /// <summary>
    /// Creates an equal to constraint
    /// </summary>
    public LinearConstraint Eq(double rhs)
    {
        return new LinearConstraint(this, Sense.Equal, rhs);
    }

    /// <summary>
    /// Creates a range constraint
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
