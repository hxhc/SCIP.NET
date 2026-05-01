using System.Runtime.InteropServices;
using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents an Indicator constraint: when the binary variable is 1, the linear constraint must hold
/// </summary>
// 指示约束：当二进制变量为1时，线性约束必须成立
public sealed class IndicatorConstraint : Constraint
{
    private readonly Variable _binaryVar;
    private readonly LinearExpression _expression;
    private readonly Sense _sense;
    private readonly double _rhs;

/// <summary>
/// Gets the binary indicator variable
/// </summary>
    // 获取二进制指示变量
    public Variable BinaryVariable => _binaryVar;

/// <summary>
/// Gets the linear expression
/// </summary>
    // 获取线性表达式
    public LinearExpression Expression => _expression;

/// <summary>
/// Gets the constraint sense
/// </summary>
    // 获取约束方向
    public Sense Sense => _sense;

/// <summary>
/// Gets the right-hand side value
/// </summary>
    // 获取右侧值
    public double RightHandSide => _rhs;

    public IndicatorConstraint(
        Variable binaryVar,
        LinearExpression expression,
        Sense sense,
        double rhs,
        string? name = null)
        : base(name ?? $"ind_{Guid.NewGuid():N}")
    {
        if (binaryVar.Type != VariableType.Binary)
        {
            throw new ArgumentException($"Indicator variable must be Binary, got {binaryVar.Type}", nameof(binaryVar));
        }

        _binaryVar = binaryVar;
        _expression = expression;
        _sense = sense;
        _rhs = rhs;
    }

    internal override IntPtr AddToModel()
    {
        var coefficients = _expression.GetCoefficients();
        var nvars = coefficients.Count;

        if (nvars == 0)
        {
            throw new InvalidOperationException("Indicator constraint expression must have at least one variable");
        }

        // SCIPcreateConsBasicIndicator only supports the form sum(vals*vars) <= rhs
        // For >= and ==, conversion is needed:
        //   a^T x >= b  →  -a^T x <= -b
        //   a^T x == b  →  a^T x <= b AND -a^T x <= -b (split into two indicator constraints)
        //
        // The constant term from the expression must also be accounted for:
        //   expression + constant <= rhs  →  expression <= rhs - constant
        //   expression + constant >= rhs  →  -expression <= -(rhs - constant)

        double constant = _expression.GetConstant();
        double adjustedRhs = _rhs - constant;
        bool negate = _sense == Sense.GreaterThanOrEqual;
        double rhs = negate ? -adjustedRhs : adjustedRhs;

        IntPtr varsPtr = IntPtr.Zero;
        IntPtr valsPtr = IntPtr.Zero;

        try
        {
            varsPtr = Marshal.AllocHGlobal(nvars * IntPtr.Size);
            valsPtr = Marshal.AllocHGlobal(nvars * sizeof(double));

            int i = 0;
            foreach (var kvp in coefficients)
            {
                Marshal.WriteIntPtr(varsPtr, i * IntPtr.Size, kvp.Key.VarPtr);
                double val = negate ? -kvp.Value : kvp.Value;
                long bits = BitConverter.DoubleToInt64Bits(val);
                Marshal.WriteInt64(valsPtr + i * sizeof(double), bits);
                i++;
            }

            ReturnCode ret = ScipNativeMethods.SCIPcreateConsBasicIndicator(
                Model.ScipHandle,
                out IntPtr consPtr,
                Name,
                _binaryVar.VarPtr,
                nvars,
                varsPtr,
                valsPtr,
                rhs);

            ErrorHandler.CheckReturnCode(ret, $"Failed to create indicator constraint {Name}");

            ret = ScipNativeMethods.SCIPaddCons(Model.ScipHandle, consPtr);
            ErrorHandler.CheckReturnCode(ret, $"Failed to add indicator constraint {Name}");

            SetConsPtr(consPtr);

            // For == constraints, need to add a second indicator constraint: a^T x >= rhs
            if (_sense == Sense.Equal)
            {
                IntPtr varsPtr2 = Marshal.AllocHGlobal(nvars * IntPtr.Size);
                IntPtr valsPtr2 = Marshal.AllocHGlobal(nvars * sizeof(double));

                try
                {
                    int j = 0;
                    foreach (var kvp in coefficients)
                    {
                        Marshal.WriteIntPtr(varsPtr2, j * IntPtr.Size, kvp.Key.VarPtr);
                        long bits = BitConverter.DoubleToInt64Bits(-kvp.Value);
                        Marshal.WriteInt64(valsPtr2 + j * sizeof(double), bits);
                        j++;
                    }

                    string secondName = $"{Name}_eq2";
                    ret = ScipNativeMethods.SCIPcreateConsBasicIndicator(
                        Model.ScipHandle,
                        out IntPtr consPtr2,
                        secondName,
                        _binaryVar.VarPtr,
                        nvars,
                        varsPtr2,
                        valsPtr2,
                        -adjustedRhs);

                    ErrorHandler.CheckReturnCode(ret, $"Failed to create second indicator constraint {secondName}");

                    ret = ScipNativeMethods.SCIPaddCons(Model.ScipHandle, consPtr2);
                    ErrorHandler.CheckReturnCode(ret, $"Failed to add second indicator constraint {secondName}");

                    // Release the second constraint (managed by Model, but no need to track here)
                    ret = ScipNativeMethods.SCIPreleaseCons(Model.ScipHandle, ref consPtr2);
                    if (ret != ReturnCode.Okay)
                    {
                        // Release failure does not affect functionality
                    }
                }
                finally
                {
                    if (varsPtr2 != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(varsPtr2);
                    }
                    if (valsPtr2 != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(valsPtr2);
                    }
                }
            }

            return consPtr;
        }
        finally
        {
            if (varsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(varsPtr);
            }
            if (valsPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(valsPtr);
            }
        }
    }

    public override string ToString()
    {
        var op = _sense switch
        {
            Sense.LessThanOrEqual => "<=",
            Sense.GreaterThanOrEqual => ">=",
            Sense.Equal => "==",
            _ => "?"
        };
        return $"{_binaryVar.Name} = 1 -> {_expression} {op} {_rhs}";
    }
}
