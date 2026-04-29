using System.Runtime.InteropServices;
using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// 代表 Indicator 约束：当二元变量为 1 时，线性约束必须成立
/// </summary>
public sealed class IndicatorConstraint : Constraint
{
    private readonly Variable _binaryVar;
    private readonly LinearExpression _expression;
    private readonly Sense _sense;
    private readonly double _rhs;

    /// <summary>
    /// 获取二元指示变量
    /// </summary>
    public Variable BinaryVariable => _binaryVar;

    /// <summary>
    /// 获取线性表达式
    /// </summary>
    public LinearExpression Expression => _expression;

    /// <summary>
    /// 获取约束方向
    /// </summary>
    public Sense Sense => _sense;

    /// <summary>
    /// 获取右侧值
    /// </summary>
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

        // SCIPcreateConsBasicIndicator 只支持 sum(vals*vars) <= rhs 形式
        // 对于 >= 和 == 需要转换：
        //   a^T x >= b  →  -a^T x <= -b
        //   a^T x == b  →  a^T x <= b AND -a^T x <= -b（拆为两个 indicator 约束）

        bool negate = _sense == Sense.GreaterThanOrEqual;
        double rhs = negate ? -_rhs : _rhs;

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

            // 对于 == 约束，需要添加第二个 indicator 约束：a^T x >= rhs
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
                        -_rhs);

                    ErrorHandler.CheckReturnCode(ret, $"Failed to create second indicator constraint {secondName}");

                    ret = ScipNativeMethods.SCIPaddCons(Model.ScipHandle, consPtr2);
                    ErrorHandler.CheckReturnCode(ret, $"Failed to add second indicator constraint {secondName}");

                    // 释放第二个约束（由 Model 管理，但这里不需要跟踪）
                    ret = ScipNativeMethods.SCIPreleaseCons(Model.ScipHandle, ref consPtr2);
                    if (ret != ReturnCode.Okay)
                    {
                        // 释放失败不影响功能
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
