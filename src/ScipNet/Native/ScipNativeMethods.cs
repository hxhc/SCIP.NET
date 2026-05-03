using System.Runtime.InteropServices;
using ScipNet.Core;

namespace ScipNet.Native;

/// <summary>
/// SCIP_DECL_EVENTEXEC delegate: called when an event is triggered
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate ReturnCode EventExecCallback(
    IntPtr scip,
    IntPtr eventhdlr,
    IntPtr event_,
    IntPtr eventdata);

/// <summary>
/// SCIP_DECL_EVENTINIT delegate: called when event handler is initialized
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate ReturnCode EventInitCallback(
    IntPtr scip,
    IntPtr eventhdlr);

/// <summary>
/// SCIP_DECL_EVENTEXIT delegate: called when event handler is exited
/// </summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate ReturnCode EventExitCallback(
    IntPtr scip,
    IntPtr eventhdlr);

/// <summary>
/// SCIP Native method declarations
/// </summary>
internal static class ScipNativeMethods
{
    private const string DllName = "libscip";

    /// <summary>
    /// Creates a SCIP instance
    /// </summary>
    // 创建 SCIP 实例
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreate(
        out IntPtr scip);

    /// <summary>
    /// Releases a SCIP instance
    /// </summary>
    // 释放 SCIP 实例
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPfree(
        ref IntPtr scip);

    /// <summary>
    /// Creates a problem
    /// </summary>
    // 创建问题
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPcreateProbBasic(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    /// <summary>
    /// Includes default plugins
    /// </summary>
    // 包含默认插件
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPincludeDefaultPlugins(
        IntPtr scip);

    /// <summary>
    /// Solves the problem
    /// </summary>
    // 求解问题
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPsolve(
        IntPtr scip);

    /// <summary>
    /// Gets the solution status
    /// </summary>
    // 获取求解状态
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SolveStatus SCIPgetStatus(
        IntPtr scip);

    /// <summary>
    /// Creates a variable
    /// </summary>
    // 创建变量
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
    // 添加变量到问题
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddVar(
        IntPtr scip,
        IntPtr var);

    /// <summary>
    /// Creates a linear constraint
    /// </summary>
    // 创建线性约束
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
    // 向线性约束添加系数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddCoefLinear(
        IntPtr scip,
        IntPtr cons,
        IntPtr var,
        double val);

    /// <summary>
    /// Adds a constraint to the problem
    /// </summary>
    // 添加约束到问题
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddCons(
        IntPtr scip,
        IntPtr cons);

    /// <summary>
    /// Sets the objective function sense
    /// </summary>
    // 设置目标函数方向
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPsetObjsense(
        IntPtr scip,
        ObjectiveSense objsense);

    /// <summary>
    /// Changes the objective coefficient of a variable
    /// </summary>
    // 修改变量的目标系数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPchgVarObj(
        IntPtr scip,
        IntPtr var,
        double newobj);

    /// <summary>
    /// Gets the value of a variable in a solution
    /// </summary>
    // 获取变量在解中的值
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetSolVal(
        IntPtr scip,
        IntPtr sol,
        IntPtr var);

    /// <summary>
    /// Gets the best solution
    /// </summary>
    // 获取最优解
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SCIPgetBestSol(
        IntPtr scip);

    /// <summary>
    /// Gets the primal bound
    /// </summary>
    // 获取原问题界
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetPrimalbound(
        IntPtr scip);

    /// <summary>
    /// Gets the dual bound
    /// </summary>
    // 获取对偶界
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetDualbound(
        IntPtr scip);

    /// <summary>
    /// Gets the gap
    /// </summary>
    // 获取间隙
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetGap(
        IntPtr scip);

    /// <summary>
    /// Gets the total number of nodes
    /// </summary>
    // 获取节点总数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCIPgetNNodes")]
    public static extern long SCIPgetNNodes(
        IntPtr scip);

    /// <summary>
    /// Gets the number of remaining nodes (open nodes)
    /// </summary>
    // 获取剩余节点数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCIPgetNNodesLeft")]
    public static extern int SCIPgetNNodesLeft(
        IntPtr scip);

    /// <summary>
    /// Gets the solving time
    /// </summary>
    // 获取求解时间
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetSolvingTime(
        IntPtr scip);

    /// <summary>
    /// Gets the number of LP iterations
    /// </summary>
    // 获取 LP 迭代次数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SCIPgetNLPIterations(
        IntPtr scip);

    /// <summary>
    /// Gets the number of solutions found
    /// </summary>
    // 获取找到的解数量
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SCIPgetNSols(
        IntPtr scip);

    /// <summary>
    /// Releases a variable
    /// </summary>
    // 释放变量
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseVar(
        IntPtr scip,
        ref IntPtr var);

    /// <summary>
    /// Releases a constraint
    /// </summary>
    // 释放约束
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseCons(
        IntPtr scip,
        ref IntPtr cons);

    /// <summary>
    /// Sets a boolean parameter
    /// </summary>
    // 设置布尔参数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetBoolParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.I1)] bool value);

    /// <summary>
    /// Sets an integer parameter
    /// </summary>
    // 设置整数参数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetIntParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        int value);

    /// <summary>
    /// Sets a real parameter
    /// </summary>
    // 设置实数参数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetRealParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        double value);

    /// <summary>
    /// Sets a string parameter
    /// </summary>
    // 设置字符串参数
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetStringParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value);

    /// <summary>
    /// Gets the number of parameters
    /// </summary>
    // 获取参数数量
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SCIPgetNParams(
        IntPtr scip);

    /// <summary>
    /// Gets a parameter name
    /// </summary>
    // 获取参数名称
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr SCIPgetParamName(
        IntPtr scip,
        int idx);

    /// <summary>
    /// Gets a parameter type
    /// </summary>
    // 获取参数类型
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int SCIPgetParamType(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    // ===== Nonlinear Expression Creation =====
    // ===== 非线性表达式创建 =====

    /// <summary>
    /// Creates a variable expression
    /// </summary>
    // 创建变量表达式
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
    // 创建常量表达式
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
    // 创建求和表达式
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
    // 创建乘积表达式
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
    // 创建幂表达式
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
    // 创建指数表达式
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
    // 创建对数表达式
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
    // 创建绝对值表达式
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
    // 创建正弦表达式
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
    // 创建余弦表达式
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
    // 释放表达式
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseExpr(
        IntPtr scip,
        ref IntPtr expr);

    // ===== Nonlinear Constraints =====
    // ===== 非线性约束 =====

    /// <summary>
    /// Creates a basic nonlinear constraint
    /// </summary>
    // 创建基本非线性约束
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
    // 向非线性约束添加线性变量
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddLinearVarNonlinear(
        IntPtr scip,
        IntPtr cons,
        IntPtr var,
        double coef);

    // ===== Solution Pool =====
    // ===== 解池 =====

/// <summary>
/// Gets all solutions in the solution pool
/// </summary>
// 获取解池中的所有解
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPgetSols(
    IntPtr scip);

/// <summary>
/// Gets the original objective value of a specific solution
/// </summary>
// 获取特定解的原始目标值
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern double SCIPgetSolOrigObj(
    IntPtr scip,
    IntPtr sol);

// ===== Count/Enumerate All Feasible Solutions =====
// ===== 计数/枚举所有可行解 =====

/// <summary>
/// Sets counting parameters (including safe settings like disabling restarts)
/// </summary>
// 设置计数参数（包括禁用重启等安全设置）
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetParamsCountsols(
    IntPtr scip);

/// <summary>
/// Counts/enumerates all feasible solutions (instead of only solving for the optimal solution)
/// </summary>
// 计数/枚举所有可行解（而非仅求解最优解）
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPcount(
    IntPtr scip);

/// <summary>
/// Gets the number of counted solutions
/// </summary>
// 获取已计数解的数量
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern long SCIPgetNCountedSols(
    IntPtr scip,
    out IntPtr valid);

/// <summary>
/// Gets the collected sparse solutions (these solutions are relative to active variables)
/// Note: Returns void in SCIP API, not ReturnCode
/// The returned arrays are managed internally by SCIP and don't need to be freed by the caller
/// </summary>
// 获取收集的稀疏解（相对于活跃变量）
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
// 查找约束处理器
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern IntPtr SCIPfindConshdlr(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name);

/// <summary>
/// Gets the variable name
/// </summary>
// 获取变量名称
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern IntPtr SCIPvarGetName(
    IntPtr var);

/// <summary>
/// Gets the variable type
/// </summary>
// 获取变量类型
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern VariableType SCIPvarGetType(
    IntPtr var);

/// <summary>
/// Explicitly includes the countsols constraint handler
/// Note: This IS included by SCIPincludeDefaultPlugins() in SCIP 9.0+
/// so we need to check if it's already included before calling this
/// </summary>
// 显式包含计数解约束处理器
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPincludeConshdlrCountsols(
    IntPtr scip);

/// <summary>
/// Gets the variable array in a sparse solution
/// </summary>
// 获取稀疏解中的变量数组
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPsparseSolGetVars(
    IntPtr sparsesol);

/// <summary>
/// Gets the number of variables in a sparse solution
/// </summary>
// 获取稀疏解中的变量数量
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern int SCIPsparseSolGetNVars(
    IntPtr sparsesol);

/// <summary>
/// Gets the first concrete solution in a sparse solution
/// Note: Returns void in SCIP API, not ReturnCode
/// </summary>
// 获取稀疏解中的第一个具体解
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void SCIPsparseSolGetFirstSol(
    IntPtr sparsesol,
    IntPtr sol,
    int nvars);

/// <summary>
/// Gets the next concrete solution in a sparse solution
/// Returns true if a next solution was found, false if no more solutions
/// </summary>
// 获取稀疏解中的下一个具体解
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public static extern bool SCIPsparseSolGetNextSol(
    IntPtr sparsesol,
    IntPtr sol,
    int nvars);

/// <summary>
/// Converts original variables to active variable representation (used to convert sparse solutions from active variable space to original variable space)
/// </summary>
// 将原始变量转换为活跃变量表示
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
// 释放由 SCIPgetProbvarLinearSum 分配的内存
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void SCIPfreeBufferArray(
    IntPtr scip,
    ref IntPtr ptr);

// ===== Indicator Constraints =====
// ===== 指示约束 =====

    /// <summary>
    /// Creates a basic Indicator constraint: when binvar = 1, sum(vals[i]*vars[i]) &lt;= rhs holds
    /// </summary>
    // 创建基本指示约束：当 binvar = 1 时，sum(vals[i]*vars[i]) <= rhs 成立
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
// ===== 获取参数值 =====

/// <summary>
/// Gets an integer parameter value
/// </summary>
// 获取整数参数值
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetIntParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out int value);

/// <summary>
/// Gets a real parameter value
/// </summary>
// 获取实数参数值
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetRealParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out double value);

/// <summary>
/// Gets a boolean parameter value
/// </summary>
// 获取布尔参数值
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetBoolParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    [MarshalAs(UnmanagedType.I1)] out bool value);

/// <summary>
/// Gets a string parameter value (caller must free the returned string)
/// </summary>
// 获取字符串参数值（调用者必须释放返回的字符串）
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetStringParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out IntPtr value);

/// <summary>
/// Sets a long integer parameter
/// </summary>
// 设置长整数参数
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPsetLongintParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    long value);

/// <summary>
/// Gets a long integer parameter value
/// </summary>
// 获取长整数参数值
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetLongintParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out long value);

/// <summary>
/// Sets the parameter emphasis mode
/// </summary>
// 设置参数强调模式
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetEmphasis(
    IntPtr scip,
    ParamEmphasis paramemphasis,
    [MarshalAs(UnmanagedType.I1)] bool quiet);

// ===== Event Handling =====
// ===== 事件处理 =====

/// <summary>
/// Event type constant: a new feasible solution was found (includes both poor and best)
/// SCIP_EVENTTYPE is uint64_t in SCIP, so we use ulong here.
/// </summary>
public const ulong SCIP_EVENTTYPE_POORSOLFOUND = 0x002000000;
public const ulong SCIP_EVENTTYPE_BESTSOLFOUND = 0x004000000;
public const ulong SCIP_EVENTTYPE_SOLFOUND = SCIP_EVENTTYPE_POORSOLFOUND | SCIP_EVENTTYPE_BESTSOLFOUND;

/// <summary>
/// Includes a basic event handler (only exec callback)
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPincludeEventhdlrBasic(
    IntPtr scip,
    out IntPtr eventhdlrptr,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    [MarshalAs(UnmanagedType.LPStr)] string desc,
    IntPtr eventexec,
    IntPtr eventhdlrdata);

/// <summary>
/// Finds an event handler by name
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern IntPtr SCIPfindEventhdlr(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name);

/// <summary>
/// Catches (subscribes to) an event type
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPcatchEvent(
    IntPtr scip,
    ulong eventtype,
    IntPtr eventhdlr,
    IntPtr eventdata,
    out int filterpos);

/// <summary>
/// Drops (unsubscribes from) an event type
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPdropEvent(
    IntPtr scip,
    ulong eventtype,
    IntPtr eventhdlr,
    IntPtr eventdata,
    int filterpos);

/// <summary>
/// Gets the solution from a SOLFOUND event
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPeventGetSol(
    IntPtr event_);

/// <summary>
/// Gets the event type of an event
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ulong SCIPeventGetType(
    IntPtr event_);

/// <summary>
/// Gets the objective value of a solution
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern double SCIPgetSolObjVal(
    IntPtr scip,
    IntPtr sol);

/// <summary>
/// Gets all variables of the problem
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPgetVars(
    IntPtr scip);

/// <summary>
/// Gets the number of variables in the problem
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern int SCIPgetNVars(
    IntPtr scip);

/// <summary>
/// Sets the solving process initialization callback of the event handler
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetEventhdlrInitsol(
    IntPtr scip,
    IntPtr eventhdlr,
    IntPtr eventinitsol);

/// <summary>
/// Sets the solving process deinitialization callback of the event handler
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetEventhdlrExitsol(
    IntPtr scip,
    IntPtr eventhdlr,
    IntPtr eventexitsol);
}
