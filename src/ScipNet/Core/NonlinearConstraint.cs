using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// 代表非线性约束
/// </summary>
public sealed class NonlinearConstraint : Constraint
{
    private readonly NonlinearExpression _expression;
    private readonly double _lhs;
    private readonly double _rhs;

    /// <summary>
    /// 获取表达式
    /// </summary>
    public NonlinearExpression Expression => _expression;

    /// <summary>
    /// 获取下界
    /// </summary>
    public double LowerBound => _lhs;

    /// <summary>
    /// 获取上界
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
        // 构建原生表达式树
        IntPtr exprPtr = _expression.BuildExpr(Model.ScipHandle);

        try
        {
            // 创建非线性约束
            ReturnCode ret = ScipNativeMethods.SCIPcreateConsBasicNonlinear(
                Model.ScipHandle,
                out IntPtr consPtr,
                Name,
                exprPtr,
                _lhs,
                _rhs);
            ErrorHandler.CheckReturnCode(ret, $"Failed to create nonlinear constraint {Name}");

            // 添加约束到模型
            ret = ScipNativeMethods.SCIPaddCons(Model.ScipHandle, consPtr);
            ErrorHandler.CheckReturnCode(ret, $"Failed to add nonlinear constraint {Name}");

            SetConsPtr(consPtr);
            return consPtr;
        }
        finally
        {
            // 始终释放表达式（约束已捕获/复制它）
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
