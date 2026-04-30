using System.Runtime.InteropServices;
using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents a nonlinear expression (expression tree)
///
/// Supported operations:
/// - Arithmetic: +, -, *, /
/// - Power: Pow(x, exponent)
/// - Exponential: Exp(x)
/// - Logarithm: Log(x) - natural logarithm
/// - Square root: Sqrt(x) - equivalent to Pow(x, 0.5)
/// - Absolute value: Abs(x)
/// - Trigonometric: Sin(x), Cos(x) - input in radians
///
/// Usage:
///   var x = model.AddVariable("x", 0, 10, VariableType.Continuous);
///   var expr1 = NonlinearExpression.Sin(x);                    // sin(x)
///   var expr2 = NonlinearExpression.Pow(x, 2.0);                // x^2
///   var expr3 = NonlinearExpression.Exp(x) + NonlinearExpression.Log(x);  // e^x + ln(x)
///
/// Note: This is an abstract class - you cannot instantiate it directly.
/// Use the static methods (Sin, Cos, Pow, etc.) or implicit conversions
/// from Variable, double, or LinearExpression to create expressions.
/// </summary>
public abstract class NonlinearExpression
{
    /// <summary>
    /// Implicit conversion from variable to nonlinear expression
    /// </summary>
    public static implicit operator NonlinearExpression(Variable var) => new VarExpr(var);

    /// <summary>
    /// Implicit conversion from double to nonlinear expression
    /// </summary>
    public static implicit operator NonlinearExpression(double value) => new ConstExpr(value);

    /// <summary>
    /// Implicit conversion from LinearExpression to nonlinear expression
    /// </summary>
    public static implicit operator NonlinearExpression(LinearExpression linear) => FromLinear(linear);

    // ===== Arithmetic Operators =====

    public static NonlinearExpression operator +(NonlinearExpression left, NonlinearExpression right)
        => new SumExpr(new[] { left, right }, new[] { 1.0, 1.0 }, 0.0);

    public static NonlinearExpression operator -(NonlinearExpression left, NonlinearExpression right)
        => new SumExpr(new[] { left, right }, new[] { 1.0, -1.0 }, 0.0);

    public static NonlinearExpression operator *(NonlinearExpression left, NonlinearExpression right)
        => new ProductExpr(new[] { left, right }, 1.0);

    public static NonlinearExpression operator /(NonlinearExpression left, NonlinearExpression right)
        => left * new PowExpr(right, -1.0);

    public static NonlinearExpression operator -(NonlinearExpression expr)
        => new ProductExpr(new[] { expr }, -1.0);

    public static NonlinearExpression operator +(NonlinearExpression expr) => expr;

    // ===== Mathematical Functions =====

    /// <summary>
    /// Exponential function exp(x)
    /// </summary>
    public static NonlinearExpression Exp(NonlinearExpression arg) => new ExpExpr(arg);

    /// <summary>
    /// Natural logarithm log(x)
    /// </summary>
    public static NonlinearExpression Log(NonlinearExpression arg) => new LogExpr(arg);

    /// <summary>
    /// Square root sqrt(x)
    /// </summary>
    public static NonlinearExpression Sqrt(NonlinearExpression arg) => new PowExpr(arg, 0.5);

    /// <summary>
    /// Absolute value abs(x)
    /// </summary>
    public static NonlinearExpression Abs(NonlinearExpression arg) => new AbsExpr(arg);

    /// <summary>
    /// Power function x^n
    /// </summary>
    public static NonlinearExpression Pow(NonlinearExpression baseExpr, double exponent) => new PowExpr(baseExpr, exponent);

    /// <summary>
    /// Sine function sin(x)
    /// Note: Input should be in radians
    /// </summary>
    public static NonlinearExpression Sin(NonlinearExpression arg) => new SinExpr(arg);

    /// <summary>
    /// Cosine function cos(x)
    /// Note: Input should be in radians
    /// </summary>
    public static NonlinearExpression Cos(NonlinearExpression arg) => new CosExpr(arg);

    // ===== Constraint Creation =====

    public NonlinearConstraint Leq(double rhs) => new NonlinearConstraint(this, double.NegativeInfinity, rhs);
    public NonlinearConstraint Geq(double rhs) => new NonlinearConstraint(this, rhs, double.PositiveInfinity);
    public NonlinearConstraint Eq(double rhs) => new NonlinearConstraint(this, rhs, rhs);
    public NonlinearConstraint Between(double lhs, double rhs) => new NonlinearConstraint(this, lhs, rhs);

    // ===== Build Native Expression =====

    internal abstract IntPtr BuildExpr(ScipHandle scip);

    // ===== Conversion from LinearExpression =====

    private static NonlinearExpression FromLinear(LinearExpression linear)
    {
        NonlinearExpression result = new ConstExpr(linear.Constant);
        foreach (var kvp in linear.Coefficients)
        {
            result = new SumExpr(new[] { result, new VarExpr(kvp.Key) }, new[] { 1.0, kvp.Value }, 0.0);
        }
        return result;
    }

    // ===== Private Nested Node Types =====

    /// <summary>
    /// Variable expression node
    /// </summary>
    private sealed class VarExpr : NonlinearExpression
    {
        private readonly Variable _var;

        public VarExpr(Variable var)
        {
            _var = var;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            var ret = ScipNativeMethods.SCIPcreateExprVar(scip, out IntPtr expr, _var.VarPtr, IntPtr.Zero, IntPtr.Zero);
            ErrorHandler.CheckReturnCode(ret, $"Failed to create var expression for {_var.Name}");
            return expr;
        }

        public override string ToString() => _var.Name;
    }

    /// <summary>
    /// Constant expression node
    /// </summary>
    private sealed class ConstExpr : NonlinearExpression
    {
        private readonly double _value;

        public ConstExpr(double value)
        {
            _value = value;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            var ret = ScipNativeMethods.SCIPcreateExprValue(scip, out IntPtr expr, _value, IntPtr.Zero, IntPtr.Zero);
            ErrorHandler.CheckReturnCode(ret, "Failed to create value expression");
            return expr;
        }

        public override string ToString() => _value.ToString("G");
    }

    /// <summary>
    /// Sum expression node
    /// </summary>
    private sealed class SumExpr : NonlinearExpression
    {
        private readonly NonlinearExpression[] _children;
        private readonly double[] _coefficients;
        private readonly double _constant;

        public SumExpr(NonlinearExpression[] children, double[] coefficients, double constant)
        {
            _children = children;
            _coefficients = coefficients;
            _constant = constant;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            var nchildren = _children.Length;
            var childPtrs = new IntPtr[nchildren];

            try
            {
                // Recursively build all child expressions
                for (int i = 0; i < nchildren; i++)
                {
                    childPtrs[i] = _children[i].BuildExpr(scip);
                }

                // Allocate coefficient array
                IntPtr coefficientsPtr = Marshal.AllocHGlobal(nchildren * sizeof(double));
                try
                {
                    for (int i = 0; i < nchildren; i++)
                    {
                        long bits = BitConverter.DoubleToInt64Bits(_coefficients[i]);
                        Marshal.WriteInt64(coefficientsPtr + i * sizeof(double), bits);
                    }

                    // Allocate child expression pointer array
                    IntPtr childrenPtr = Marshal.AllocHGlobal(nchildren * IntPtr.Size);
                    try
                    {
                        for (int i = 0; i < nchildren; i++)
                        {
                            Marshal.WriteIntPtr(childrenPtr, i * IntPtr.Size, childPtrs[i]);
                        }

                        var ret = ScipNativeMethods.SCIPcreateExprSum(
                            scip, out IntPtr expr,
                            nchildren, childrenPtr, coefficientsPtr, _constant,
                            IntPtr.Zero, IntPtr.Zero);
                        ErrorHandler.CheckReturnCode(ret, "Failed to create sum expression");
                        return expr;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(childrenPtr);
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(coefficientsPtr);
                }
            }
            finally
            {
                // Parent expression has captured child nodes, release child node references
                for (int i = 0; i < childPtrs.Length; i++)
                {
                    if (childPtrs[i] != IntPtr.Zero)
                    {
                        ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtrs[i]);
                    }
                }
            }
        }

        public override string ToString()
        {
            var parts = new List<string>();
            for (int i = 0; i < _children.Length; i++)
            {
                var coef = _coefficients[i];
                var child = _children[i].ToString() ?? "";
                if (coef == 1.0)
                    parts.Add(child);
                else if (coef == -1.0)
                    parts.Add($"-{child}");
                else
                    parts.Add($"{coef}*{child}");
            }
            if (_constant != 0.0)
                parts.Add(_constant.ToString("G"));
            return $"({string.Join(" + ", parts)})";
        }
    }

    /// <summary>
    /// Product expression node
    /// </summary>
    private sealed class ProductExpr : NonlinearExpression
    {
        private readonly NonlinearExpression[] _children;
        private readonly double _coefficient;

        public ProductExpr(NonlinearExpression[] children, double coefficient)
        {
            _children = children;
            _coefficient = coefficient;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            var nchildren = _children.Length;
            var childPtrs = new IntPtr[nchildren];

            try
            {
                for (int i = 0; i < nchildren; i++)
                {
                    childPtrs[i] = _children[i].BuildExpr(scip);
                }

                IntPtr childrenPtr = Marshal.AllocHGlobal(nchildren * IntPtr.Size);
                try
                {
                    for (int i = 0; i < nchildren; i++)
                    {
                        Marshal.WriteIntPtr(childrenPtr, i * IntPtr.Size, childPtrs[i]);
                    }

                    var ret = ScipNativeMethods.SCIPcreateExprProduct(
                        scip, out IntPtr expr,
                        nchildren, childrenPtr, _coefficient,
                        IntPtr.Zero, IntPtr.Zero);
                    ErrorHandler.CheckReturnCode(ret, "Failed to create product expression");
                    return expr;
                }
                finally
                {
                    Marshal.FreeHGlobal(childrenPtr);
                }
            }
            finally
            {
                for (int i = 0; i < childPtrs.Length; i++)
                {
                    if (childPtrs[i] != IntPtr.Zero)
                    {
                        ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtrs[i]);
                    }
                }
            }
        }

        public override string ToString()
        {
            if (_coefficient == -1.0)
                return $"(-{string.Join(" * ", _children.Select(c => c.ToString()))})";
            var coefStr = _coefficient == 1.0 ? "" : $"{_coefficient}*";
            return $"({coefStr}{string.Join(" * ", _children.Select(c => c.ToString()))})";
        }
    }

    /// <summary>
    /// Power expression node
    /// </summary>
    private sealed class PowExpr : NonlinearExpression
    {
        private readonly NonlinearExpression _child;
        private readonly double _exponent;

        public PowExpr(NonlinearExpression child, double exponent)
        {
            _child = child;
            _exponent = exponent;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            IntPtr childPtr = _child.BuildExpr(scip);
            try
            {
                var ret = ScipNativeMethods.SCIPcreateExprPow(
                    scip, out IntPtr expr, childPtr, _exponent, IntPtr.Zero, IntPtr.Zero);
                ErrorHandler.CheckReturnCode(ret, "Failed to create power expression");
                return expr;
            }
            finally
            {
                ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtr);
            }
        }

        public override string ToString() => $"({_child}^{_exponent})";
    }

    /// <summary>
    /// Exponential expression node
    /// </summary>
    private sealed class ExpExpr : NonlinearExpression
    {
        private readonly NonlinearExpression _child;

        public ExpExpr(NonlinearExpression child)
        {
            _child = child;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            IntPtr childPtr = _child.BuildExpr(scip);
            try
            {
                var ret = ScipNativeMethods.SCIPcreateExprExp(
                    scip, out IntPtr expr, childPtr, IntPtr.Zero, IntPtr.Zero);
                ErrorHandler.CheckReturnCode(ret, "Failed to create exp expression");
                return expr;
            }
            finally
            {
                ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtr);
            }
        }

        public override string ToString() => $"exp({_child})";
    }

    /// <summary>
    /// Logarithm expression node
    /// </summary>
    private sealed class LogExpr : NonlinearExpression
    {
        private readonly NonlinearExpression _child;

        public LogExpr(NonlinearExpression child)
        {
            _child = child;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            IntPtr childPtr = _child.BuildExpr(scip);
            try
            {
                var ret = ScipNativeMethods.SCIPcreateExprLog(
                    scip, out IntPtr expr, childPtr, IntPtr.Zero, IntPtr.Zero);
                ErrorHandler.CheckReturnCode(ret, "Failed to create log expression");
                return expr;
            }
            finally
            {
                ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtr);
            }
        }

        public override string ToString() => $"log({_child})";
    }

    /// <summary>
    /// Absolute value expression node
    /// </summary>
    private sealed class AbsExpr : NonlinearExpression
    {
        private readonly NonlinearExpression _child;

        public AbsExpr(NonlinearExpression child)
        {
            _child = child;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            IntPtr childPtr = _child.BuildExpr(scip);
            try
            {
                var ret = ScipNativeMethods.SCIPcreateExprAbs(
                    scip, out IntPtr expr, childPtr, IntPtr.Zero, IntPtr.Zero);
                ErrorHandler.CheckReturnCode(ret, "Failed to create abs expression");
                return expr;
            }
            finally
            {
                ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtr);
            }
        }

        public override string ToString() => $"abs({_child})";
    }

    /// <summary>
    /// Sine expression node
    /// </summary>
    private sealed class SinExpr : NonlinearExpression
    {
        private readonly NonlinearExpression _child;

        public SinExpr(NonlinearExpression child)
        {
            _child = child;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            IntPtr childPtr = _child.BuildExpr(scip);
            try
            {
                var ret = ScipNativeMethods.SCIPcreateExprSin(
                    scip, out IntPtr expr, childPtr, IntPtr.Zero, IntPtr.Zero);
                ErrorHandler.CheckReturnCode(ret, "Failed to create sin expression");
                return expr;
            }
            finally
            {
                ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtr);
            }
        }

        public override string ToString() => $"sin({_child})";
    }

    /// <summary>
    /// Cosine expression node
    /// </summary>
    private sealed class CosExpr : NonlinearExpression
    {
        private readonly NonlinearExpression _child;

        public CosExpr(NonlinearExpression child)
        {
            _child = child;
        }

        internal override IntPtr BuildExpr(ScipHandle scip)
        {
            IntPtr childPtr = _child.BuildExpr(scip);
            try
            {
                var ret = ScipNativeMethods.SCIPcreateExprCos(
                    scip, out IntPtr expr, childPtr, IntPtr.Zero, IntPtr.Zero);
                ErrorHandler.CheckReturnCode(ret, "Failed to create cos expression");
                return expr;
            }
            finally
            {
                ScipNativeMethods.SCIPreleaseExpr(scip, ref childPtr);
            }
        }

        public override string ToString() => $"cos({_child})";
    }
}
