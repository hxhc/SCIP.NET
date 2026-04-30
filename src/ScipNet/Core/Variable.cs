using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents a decision variable in the optimization problem
/// </summary>
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
    public string Name => _name;

    /// <summary>
    /// Gets the variable type
    /// </summary>
    public VariableType Type => _type;

    /// <summary>
    /// Gets the lower bound
    /// </summary>
    public double LowerBound => _lb;

    /// <summary>
    /// Gets the upper bound
    /// </summary>
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
    public double GetSolValue(Solution solution)
    {
        return ScipNativeMethods.SCIPgetSolVal(_model.ScipHandle, solution.SolPtr, _varPtr);
    }

    /// <summary>
    /// Creates an Indicator constraint: when this binary variable is 1, the given linear constraint holds
    /// </summary>
    public IndicatorConstraint Implies(LinearConstraint constraint, string? name = null)
    {
        return new IndicatorConstraint(this, constraint.Expression, constraint.Sense, constraint.RightHandSide, name);
    }

    /// <summary>
    /// Creates a linear expression (multiplication)
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
    /// Creates a linear expression (addition)
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
    /// Creates a linear expression (subtraction)
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
        return $"{_name} [{_type}, {_lb}, {_ub}]";
    }
}
