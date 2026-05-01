using System.Runtime.InteropServices;
using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Constraint base class
/// </summary>
// 约束基类
public abstract class Constraint
{
    private readonly string _name;
    private Model? _model;

/// <summary>
/// Gets the constraint name
/// </summary>
    // 获取约束名称
    public string Name => _name;

/// <summary>
/// Gets the associated model
/// </summary>
    // 获取关联的模型
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
/// Gets the constraint pointer
/// </summary>
    // 获取约束指针
    public IntPtr ConsPtr { get; private set; }

    protected void SetConsPtr(IntPtr consPtr)
    {
        ConsPtr = consPtr;
    }

/// <summary>
/// Internally sets the constraint pointer (used for constraints created directly by Model)
/// </summary>
    // 内部设置约束指针（用于Model直接创建的约束）
    internal void SetConsPtrInternal(IntPtr consPtr)
    {
        SetConsPtr(consPtr);
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
/// Represents a linear constraint
/// </summary>
// 表示线性约束
public sealed class LinearConstraint : Constraint
{
    private readonly LinearExpression _expression;
    private readonly Sense _sense;
    private readonly double _rhs;

    /// <summary>
    /// Gets the expression
    /// </summary>
    // 获取表达式
    public LinearExpression Expression => _expression;

/// <summary>
/// Gets the constraint direction
/// </summary>
    // 获取约束方向
    public Sense Sense => _sense;

/// <summary>
/// Gets the right-hand side value
/// </summary>
    // 获取右侧值
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

        // Allocate variable and coefficient arrays
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
                // Use BitConverter to convert double to long, then write
                long bits = BitConverter.DoubleToInt64Bits(kvp.Value);
                Marshal.WriteInt64(valsPtr + i * sizeof(double), bits);
                i++;
            }

            // Adjust rhs by subtracting the expression's constant term
            // expression + constant <= rhs  →  expression <= rhs - constant
            double constant = _expression.GetConstant();
            double lhs, rhs;
            switch (_sense)
            {
                case Sense.LessThanOrEqual:
                    lhs = double.NegativeInfinity;
                    rhs = _rhs - constant;
                    break;
                case Sense.GreaterThanOrEqual:
                    lhs = _rhs - constant;
                    rhs = double.PositiveInfinity;
                    break;
                case Sense.Equal:
                    lhs = _rhs - constant;
                    rhs = _rhs - constant;
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
/// Represents a range constraint
/// </summary>
// 表示范围约束
public sealed class RangeConstraint : Constraint
{
    private readonly LinearExpression _expression;
    private readonly double _lb;
    private readonly double _ub;

    /// <summary>
    /// Gets the expression
    /// </summary>
    // 获取表达式
    public LinearExpression Expression => _expression;

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

        // Allocate variable and coefficient arrays
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
                // Use BitConverter to convert double to long, then write
                long bits = BitConverter.DoubleToInt64Bits(kvp.Value);
                Marshal.WriteInt64(valsPtr + i * sizeof(double), bits);
                i++;
            }

            // Adjust bounds by subtracting the expression's constant term
            // lb <= expression + constant <= ub  →  lb - constant <= expression <= ub - constant
            double constant = _expression.GetConstant();
            ReturnCode ret = ScipNativeMethods.SCIPcreateConsBasicLinear(
                Model.ScipHandle,
                out IntPtr consPtr,
                Name,
                nvars,
                varsPtr,
                valsPtr,
                _lb - constant,
                _ub - constant);

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
