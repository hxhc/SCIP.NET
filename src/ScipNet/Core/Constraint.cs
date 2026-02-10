using System.Runtime.InteropServices;
using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// 约束基类
/// </summary>
public abstract class Constraint
{
    private readonly string _name;
    private Model? _model;

    /// <summary>
    /// 获取约束名称
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// 获取关联的模型
    /// </summary>
    public Model Model
    {
        get
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Constraint has not been added to a model yet");
            }
            return _model;
        }
        internal set => _model = value;
    }

    /// <summary>
    /// 获取约束指针
    /// </summary>
    public IntPtr ConsPtr { get; private set; }

    protected void SetConsPtr(IntPtr consPtr)
    {
        ConsPtr = consPtr;
    }

    protected Constraint(string name)
    {
        _name = name;
    }

    internal abstract IntPtr AddToModel();

    public override string ToString()
    {
        return $"{_name}";
    }
}

/// <summary>
/// 代表线性约束
/// </summary>
public sealed class LinearConstraint : Constraint
{
    private readonly LinearExpression _expression;
    private readonly Sense _sense;
    private readonly double _rhs;

    /// <summary>
    /// 获取表达式
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

    public LinearConstraint(
        LinearExpression expression,
        Sense sense,
        double rhs,
        string? name = null)
        : base(name ?? $"cons_{Guid.NewGuid():N}")
    {
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
            throw new InvalidOperationException("Constraint must have at least one variable");
        }

        // 分配变量和系数数组
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
                // 使用 BitConverter 将 double 转换为 long，然后写入
                long bits = BitConverter.DoubleToInt64Bits(kvp.Value);
                Marshal.WriteInt64(valsPtr + i * sizeof(double), bits);
                i++;
            }

            double lhs, rhs;
            switch (_sense)
            {
                case Sense.LessThanOrEqual:
                    lhs = double.NegativeInfinity;
                    rhs = _rhs;
                    break;
                case Sense.GreaterThanOrEqual:
                    lhs = _rhs;
                    rhs = double.PositiveInfinity;
                    break;
                case Sense.Equal:
                    lhs = _rhs;
                    rhs = _rhs;
                    break;
                default:
                    throw new ArgumentException($"Unknown sense: {_sense}");
            }

            ReturnCode ret = ScipNativeMethods.SCIPcreateConsBasicLinear(
                Model.ScipHandle,
                out IntPtr consPtr,
                Name,
                nvars,
                varsPtr,
                valsPtr,
                lhs,
                rhs);

            ErrorHandler.CheckReturnCode(ret, $"Failed to create constraint {Name}");

            ret = ScipNativeMethods.SCIPaddCons(Model.ScipHandle, consPtr);
            ErrorHandler.CheckReturnCode(ret, $"Failed to add constraint {Name}");

            SetConsPtr(consPtr);
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
        return $"{_expression} {op} {_rhs}";
    }
}

/// <summary>
/// 代表范围约束
/// </summary>
public sealed class RangeConstraint : Constraint
{
    private readonly LinearExpression _expression;
    private readonly double _lb;
    private readonly double _ub;

    /// <summary>
    /// 获取表达式
    /// </summary>
    public LinearExpression Expression => _expression;

    /// <summary>
    /// 获取下界
    /// </summary>
    public double LowerBound => _lb;

    /// <summary>
    /// 获取上界
    /// </summary>
    public double UpperBound => _ub;

    public RangeConstraint(
        LinearExpression expression,
        double lb,
        double ub,
        string? name = null)
        : base(name ?? $"cons_{Guid.NewGuid():N}")
    {
        _expression = expression;
        _lb = lb;
        _ub = ub;
    }

    internal override IntPtr AddToModel()
    {
        var coefficients = _expression.GetCoefficients();
        var nvars = coefficients.Count;

        if (nvars == 0)
        {
            throw new InvalidOperationException("Constraint must have at least one variable");
        }

        // 分配变量和系数数组
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
                // 使用 BitConverter 将 double 转换为 long，然后写入
                long bits = BitConverter.DoubleToInt64Bits(kvp.Value);
                Marshal.WriteInt64(valsPtr + i * sizeof(double), bits);
                i++;
            }

            ReturnCode ret = ScipNativeMethods.SCIPcreateConsBasicLinear(
                Model.ScipHandle,
                out IntPtr consPtr,
                Name,
                nvars,
                varsPtr,
                valsPtr,
                _lb,
                _ub);

            ErrorHandler.CheckReturnCode(ret, $"Failed to create constraint {Name}");

            ret = ScipNativeMethods.SCIPaddCons(Model.ScipHandle, consPtr);
            ErrorHandler.CheckReturnCode(ret, $"Failed to add constraint {Name}");

            SetConsPtr(consPtr);
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
        return $"{_lb} <= {_expression} <= {_ub}";
    }
}
