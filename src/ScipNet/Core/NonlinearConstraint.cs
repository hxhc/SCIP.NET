using ScipNet.Native;

namespace ScipNet.Core;

    /// <summary>
    /// Represents a nonlinear constraint
    /// </summary>
public sealed class NonlinearConstraint : Constraint
{
    private readonly NonlinearExpression _expression;
    private readonly double _lhs;
    private readonly double _rhs;

    /// <summary>
    /// Gets the expression
    /// </summary>
    public NonlinearExpression Expression => _expression;

    /// <summary>
    /// Gets the lower bound
    /// </summary>
    public double LowerBound => _lhs;

    /// <summary>
    /// Gets the upper bound
    /// </summary>
    public double UpperBound => _rhs;

    public NonlinearConstraint(
        NonlinearExpression expression,
        double lhs,
        double rhs,
        string? name = null)
        : base(name ?? $"nlcons_{Guid.NewGuid():N}")
    {
        _expression = expression;
        _lhs = lhs;
        _rhs = rhs;
    }

    internal override IntPtr AddToModel()
    {
        // Build native expression tree
        IntPtr exprPtr = _expression.BuildExpr(Model.ScipHandle);

        try
        {
            // Create nonlinear constraint
            ReturnCode ret = ScipNativeMethods.SCIPcreateConsBasicNonlinear(
                Model.ScipHandle,
                out IntPtr consPtr,
                Name,
                exprPtr,
                _lhs,
                _rhs);
            ErrorHandler.CheckReturnCode(ret, $"Failed to create nonlinear constraint {Name}");

            // Add constraint to model
            ret = ScipNativeMethods.SCIPaddCons(Model.ScipHandle, consPtr);
            ErrorHandler.CheckReturnCode(ret, $"Failed to add nonlinear constraint {Name}");

            SetConsPtr(consPtr);
            return consPtr;
        }
        finally
        {
            // Always release the expression (constraint has captured/copied it)
            ScipNativeMethods.SCIPreleaseExpr(Model.ScipHandle, ref exprPtr);
        }
    }

    public override string ToString()
    {
        if (double.IsNegativeInfinity(_lhs) && double.IsPositiveInfinity(_rhs))
            return _expression.ToString() ?? "";
        if (double.IsNegativeInfinity(_lhs))
            return $"{_expression} <= {_rhs}";
        if (double.IsPositiveInfinity(_rhs))
            return $"{_expression} >= {_lhs}";
        if (_lhs == _rhs)
            return $"{_expression} == {_lhs}";
        return $"{_lhs} <= {_expression} <= {_rhs}";
    }
}
