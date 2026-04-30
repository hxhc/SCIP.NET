using System.Runtime.InteropServices;
using ScipNet.Core;

namespace ScipNet.Native;

/// <summary>
/// SCIP Native method declarations
/// </summary>
internal static class ScipNativeMethods
{
    private const string DllName = "libscip";

    /// <summary>
    /// Creates a SCIP instance
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreate(
        out IntPtr scip);

    /// <summary>
    /// Releases a SCIP instance
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPfree(
        ref IntPtr scip);

    /// <summary>
    /// Creates a problem
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPcreateProbBasic(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    /// <summary>
    /// Includes default plugins
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPincludeDefaultPlugins(
        IntPtr scip);

    /// <summary>
    /// Solves the problem
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPsolve(
        IntPtr scip);

    /// <summary>
    /// Gets the solution status
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SolveStatus SCIPgetStatus(
        IntPtr scip);

    /// <summary>
    /// Creates a variable
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPcreateVarBasic(
        IntPtr scip,
        out IntPtr var,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        double lb,
        double ub,
        double obj,
        VariableType vartype);

    /// <summary>
    /// Adds a variable to the problem
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddVar(
        IntPtr scip,
        IntPtr var);

    /// <summary>
    /// Creates a linear constraint
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPcreateConsBasicLinear(
        IntPtr scip,
        out IntPtr cons,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        int nvars,
        IntPtr vars,
        IntPtr vals,
        double lhs,
        double rhs);

    /// <summary>
    /// Adds a coefficient to a linear constraint
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddCoefLinear(
        IntPtr scip,
        IntPtr cons,
        IntPtr var,
        double val);

    /// <summary>
    /// Adds a constraint to the problem
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddCons(
        IntPtr scip,
        IntPtr cons);

    /// <summary>
    /// Sets the objective function sense
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPsetObjsense(
        IntPtr scip,
        ObjectiveSense objsense);

    /// <summary>
    /// Changes the objective coefficient of a variable
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPchgVarObj(
        IntPtr scip,
        IntPtr var,
        double newobj);

    /// <summary>
    /// Gets the value of a variable in a solution
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetSolVal(
        IntPtr scip,
        IntPtr sol,
        IntPtr var);

    /// <summary>
    /// Gets the best solution
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SCIPgetBestSol(
        IntPtr scip);

    /// <summary>
    /// Gets the primal bound
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetPrimalbound(
        IntPtr scip);

    /// <summary>
    /// Gets the dual bound
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetDualbound(
        IntPtr scip);

    /// <summary>
    /// Gets the gap
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetGap(
        IntPtr scip);

    /// <summary>
    /// Gets the total number of nodes
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCIPgetNNodes")]
    public static extern long SCIPgetNNodes(
        IntPtr scip);

    /// <summary>
    /// Gets the number of remaining nodes (open nodes)
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCIPgetNNodesLeft")]
    public static extern int SCIPgetNNodesLeft(
        IntPtr scip);

    /// <summary>
    /// Gets the solving time
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetSolvingTime(
        IntPtr scip);

    /// <summary>
    /// Gets the number of LP iterations
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SCIPgetNLPIterations(
        IntPtr scip);

    /// <summary>
    /// Gets the number of solutions found
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SCIPgetNSols(
        IntPtr scip);

    /// <summary>
    /// Releases a variable
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseVar(
        IntPtr scip,
        ref IntPtr var);

    /// <summary>
    /// Releases a constraint
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseCons(
        IntPtr scip,
        ref IntPtr cons);

    /// <summary>
    /// Sets a boolean parameter
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetBoolParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.I1)] bool value);

    /// <summary>
    /// Sets an integer parameter
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetIntParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        int value);

    /// <summary>
    /// Sets a real parameter
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetRealParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        double value);

    /// <summary>
    /// Sets a string parameter
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetStringParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value);

    /// <summary>
    /// Gets the number of parameters
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SCIPgetNParams(
        IntPtr scip);

    /// <summary>
    /// Gets a parameter name
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr SCIPgetParamName(
        IntPtr scip,
        int idx);

    /// <summary>
    /// Gets a parameter type
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int SCIPgetParamType(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    // ===== Nonlinear Expression Creation =====

    /// <summary>
    /// Creates a variable expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprVar(
        IntPtr scip,
        out IntPtr expr,
        IntPtr var,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates a constant value expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprValue(
        IntPtr scip,
        out IntPtr expr,
        double value,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates a sum expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprSum(
        IntPtr scip,
        out IntPtr expr,
        int nchildren,
        IntPtr children,
        IntPtr coefficients,
        double constant,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates a product expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprProduct(
        IntPtr scip,
        out IntPtr expr,
        int nchildren,
        IntPtr children,
        double coefficient,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates a power expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprPow(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        double exponent,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates an exponential expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprExp(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates a logarithm expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprLog(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates an absolute value expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprAbs(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates a sine expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprSin(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Creates a cosine expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprCos(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// Releases an expression
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseExpr(
        IntPtr scip,
        ref IntPtr expr);

    // ===== Nonlinear Constraints =====

    /// <summary>
    /// Creates a basic nonlinear constraint
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPcreateConsBasicNonlinear(
        IntPtr scip,
        out IntPtr cons,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        IntPtr expr,
        double lhs,
        double rhs);

    /// <summary>
    /// Adds a linear variable to a nonlinear constraint
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddLinearVarNonlinear(
        IntPtr scip,
        IntPtr cons,
        IntPtr var,
        double coef);

    // ===== Solution Pool =====

/// <summary>
/// Gets all solutions in the solution pool
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPgetSols(
    IntPtr scip);

/// <summary>
/// Gets the original objective value of a specific solution
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern double SCIPgetSolOrigObj(
    IntPtr scip,
    IntPtr sol);

// ===== Count/Enumerate All Feasible Solutions =====

/// <summary>
/// Sets counting parameters (including safe settings like disabling restarts)
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetParamsCountsols(
    IntPtr scip);

/// <summary>
/// Counts/enumerates all feasible solutions (instead of only solving for the optimal solution)
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPcount(
    IntPtr scip);

/// <summary>
/// Gets the number of counted solutions
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern long SCIPgetNCountedSols(
    IntPtr scip,
    out IntPtr valid);

/// <summary>
/// Gets the collected sparse solutions (these solutions are relative to active variables)
/// Note: Returns void in SCIP API, not ReturnCode
/// The returned arrays are managed internally by SCIP and don't need to be freed by the caller
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void SCIPgetCountedSparseSols(
    IntPtr scip,
    out IntPtr vars,
    out int nvars,
    out IntPtr sols,
    out int nsols);

/// <summary>
/// Finds a constraint handler
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern IntPtr SCIPfindConshdlr(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name);

/// <summary>
/// Gets the variable name
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern IntPtr SCIPvarGetName(
    IntPtr var);

/// <summary>
/// Explicitly includes the countsols constraint handler
/// Note: This IS included by SCIPincludeDefaultPlugins() in SCIP 9.0+
/// so we need to check if it's already included before calling this
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPincludeConshdlrCountsols(
    IntPtr scip);

/// <summary>
/// Gets the variable array in a sparse solution
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPsparseSolGetVars(
    IntPtr sparsesol);

/// <summary>
/// Gets the number of variables in a sparse solution
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern int SCIPsparseSolGetNVars(
    IntPtr sparsesol);

/// <summary>
/// Gets the first concrete solution in a sparse solution
/// Note: Returns void in SCIP API, not ReturnCode
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void SCIPsparseSolGetFirstSol(
    IntPtr sparsesol,
    IntPtr sol,
    int nvars);

/// <summary>
/// Gets the next concrete solution in a sparse solution
/// Returns true if a next solution was found, false if no more solutions
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public static extern bool SCIPsparseSolGetNextSol(
    IntPtr sparsesol,
    IntPtr sol,
    int nvars);

/// <summary>
/// Converts original variables to active variable representation (used to convert sparse solutions from active variable space to original variable space)
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPgetProbvarLinearSum(
    IntPtr scip,
    IntPtr var,
    ref double scalar,
    ref double constant,
    out IntPtr vars,
    out int nvars,
    IntPtr scalars);

/// <summary>
/// Frees memory allocated by SCIPgetProbvarLinearSum
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void SCIPfreeBufferArray(
    IntPtr scip,
    ref IntPtr ptr);

// ===== Indicator Constraints =====

    /// <summary>
    /// Creates a basic Indicator constraint: when binvar = 1, sum(vals[i]*vars[i]) &lt;= rhs holds
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPcreateConsBasicIndicator(
        IntPtr scip,
        out IntPtr cons,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        IntPtr binvar,
        int nvars,
        IntPtr vars,
        IntPtr vals,
        double rhs);


// ===== Get Parameter Values =====

/// <summary>
/// Gets an integer parameter value
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetIntParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out int value);

/// <summary>
/// Gets a real parameter value
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetRealParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out double value);

/// <summary>
/// Gets a boolean parameter value
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetBoolParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    [MarshalAs(UnmanagedType.I1)] out bool value);

/// <summary>
/// Gets a string parameter value (caller must free the returned string)
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetStringParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out IntPtr value);

/// <summary>
/// Sets a long integer parameter
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPsetLongintParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    long value);

/// <summary>
/// Gets a long integer parameter value
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetLongintParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out long value);

/// <summary>
/// Sets the parameter emphasis mode
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetEmphasis(
    IntPtr scip,
    ParamEmphasis paramemphasis,
    [MarshalAs(UnmanagedType.I1)] bool quiet);
}
