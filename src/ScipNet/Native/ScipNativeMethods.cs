using System.Runtime.InteropServices;
using ScipNet.Core;

namespace ScipNet.Native;

/// <summary>
/// SCIP Native 方法声明
/// </summary>
internal static class ScipNativeMethods
{
    private const string DllName = "libscip";

    /// <summary>
    /// 创建 SCIP 实例
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreate(
        out IntPtr scip);

    /// <summary>
    /// 释放 SCIP 实例
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPfree(
        ref IntPtr scip);

    /// <summary>
    /// 创建问题
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPcreateProbBasic(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    /// <summary>
    /// 包含默认插件
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPincludeDefaultPlugins(
        IntPtr scip);

    /// <summary>
    /// 求解
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPsolve(
        IntPtr scip);

    /// <summary>
    /// 获取求解状态
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern SolveStatus SCIPgetStatus(
        IntPtr scip);

    /// <summary>
    /// 创建变量
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
    /// 添加变量到问题
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddVar(
        IntPtr scip,
        IntPtr var);

    /// <summary>
    /// 创建线性约束
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
    /// 添加系数到线性约束
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddCoefLinear(
        IntPtr scip,
        IntPtr cons,
        IntPtr var,
        double val);

    /// <summary>
    /// 添加约束到问题
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddCons(
        IntPtr scip,
        IntPtr cons);

    /// <summary>
    /// 设置目标函数方向
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPsetObjsense(
        IntPtr scip,
        ObjectiveSense objsense);

    /// <summary>
    /// 更改变量的目标函数系数
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPchgVarObj(
        IntPtr scip,
        IntPtr var,
        double newobj);

    /// <summary>
    /// 获取变量在解中的值
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetSolVal(
        IntPtr scip,
        IntPtr sol,
        IntPtr var);

    /// <summary>
    /// 获取最佳解
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr SCIPgetBestSol(
        IntPtr scip);

    /// <summary>
    /// 获取原始界
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetPrimalbound(
        IntPtr scip);

    /// <summary>
    /// 获取对偶界
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetDualbound(
        IntPtr scip);

    /// <summary>
    /// 获取间隙
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetGap(
        IntPtr scip);

    /// <summary>
    /// 获取总节点数
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCIPgetNNodes")]
    public static extern long SCIPgetNNodes(
        IntPtr scip);

    /// <summary>
    /// 获取剩余节点数（开放节点数）
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SCIPgetNNodesLeft")]
    public static extern int SCIPgetNNodesLeft(
        IntPtr scip);

    /// <summary>
    /// 获取求解时间
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern double SCIPgetSolvingTime(
        IntPtr scip);

    /// <summary>
    /// 获取 LP 迭代次数
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern long SCIPgetNLPIterations(
        IntPtr scip);

    /// <summary>
    /// 获取找到的解数量
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SCIPgetNSols(
        IntPtr scip);

    /// <summary>
    /// 释放变量
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseVar(
        IntPtr scip,
        ref IntPtr var);

    /// <summary>
    /// 释放约束
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseCons(
        IntPtr scip,
        ref IntPtr cons);

    /// <summary>
    /// 设置布尔参数
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetBoolParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.I1)] bool value);

    /// <summary>
    /// 设置整数参数
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetIntParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        int value);

    /// <summary>
    /// 设置实数参数
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetRealParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        double value);

    /// <summary>
    /// 设置字符串参数
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern ReturnCode SCIPsetStringParam(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name,
        [MarshalAs(UnmanagedType.LPStr)] string value);

    /// <summary>
    /// 获取参数数量
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int SCIPgetNParams(
        IntPtr scip);

    /// <summary>
    /// 获取参数名称
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr SCIPgetParamName(
        IntPtr scip,
        int idx);

    /// <summary>
    /// 获取参数类型
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int SCIPgetParamType(
        IntPtr scip,
        [MarshalAs(UnmanagedType.LPStr)] string name);

    // ===== 非线性表达式创建 =====

    /// <summary>
    /// 创建变量表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprVar(
        IntPtr scip,
        out IntPtr expr,
        IntPtr var,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// 创建常量值表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprValue(
        IntPtr scip,
        out IntPtr expr,
        double value,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// 创建求和表达式
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
    /// 创建乘积表达式
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
    /// 创建幂运算表达式
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
    /// 创建指数表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprExp(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// 创建对数表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprLog(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// 创建绝对值表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprAbs(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// 创建正弦表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprSin(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// 创建余弦表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPcreateExprCos(
        IntPtr scip,
        out IntPtr expr,
        IntPtr child,
        IntPtr ownercreate,
        IntPtr ownercreatedata);

    /// <summary>
    /// 释放表达式
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPreleaseExpr(
        IntPtr scip,
        ref IntPtr expr);

    // ===== 非线性约束 =====

    /// <summary>
    /// 创建基本非线性约束
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
    /// 向非线性约束添加线性变量
    /// </summary>
    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern ReturnCode SCIPaddLinearVarNonlinear(
        IntPtr scip,
        IntPtr cons,
        IntPtr var,
        double coef);

    // ===== 解池 =====

/// <summary>
/// 获取解池中的所有解
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPgetSols(
    IntPtr scip);

/// <summary>
/// 获取特定解的原始目标值
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern double SCIPgetSolOrigObj(
    IntPtr scip,
    IntPtr sol);

// ===== 计数/枚举所有可行解 =====

/// <summary>
/// 设置计数参数（包括禁用 restarts 等安全设置）
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetParamsCountsols(
    IntPtr scip);

/// <summary>
/// 计数/枚举所有可行解（而不是只求解最优解）
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPcount(
    IntPtr scip);

/// <summary>
/// 获取计数的解数量
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern long SCIPgetNCountedSols(
    IntPtr scip,
    out IntPtr valid);

/// <summary>
/// 获取收集的稀疏解（这些解是相对于 active variables 的）
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
/// 查找约束处理器
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern IntPtr SCIPfindConshdlr(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name);

/// <summary>
/// 获取变量名称
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern IntPtr SCIPvarGetName(
    IntPtr var);

/// <summary>
/// 显式包含 countsols 约束处理器
/// Note: This IS included by SCIPincludeDefaultPlugins() in SCIP 9.0+
/// so we need to check if it's already included before calling this
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPincludeConshdlrCountsols(
    IntPtr scip);

/// <summary>
/// 获取稀疏解中的变量数组
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern IntPtr SCIPsparseSolGetVars(
    IntPtr sparsesol);

/// <summary>
/// 获取稀疏解中的变量数量
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern int SCIPsparseSolGetNVars(
    IntPtr sparsesol);

/// <summary>
/// 获取稀疏解中的第一个具体解
/// Note: Returns void in SCIP API, not ReturnCode
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void SCIPsparseSolGetFirstSol(
    IntPtr sparsesol,
    IntPtr sol,
    int nvars);

/// <summary>
/// 获取稀疏解中的下一个具体解
/// Returns true if a next solution was found, false if no more solutions
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
[return: MarshalAs(UnmanagedType.U1)]
public static extern bool SCIPsparseSolGetNextSol(
    IntPtr sparsesol,
    IntPtr sol,
    int nvars);

/// <summary>
/// 将原始变量转换为活动变量的表示（用于将稀疏解从活动变量空间转换到原始变量空间）
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
/// 释放由SCIPgetProbvarLinearSum分配的内存
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern void SCIPfreeBufferArray(
    IntPtr scip,
    ref IntPtr ptr);

// ===== Indicator 约束 =====

    /// <summary>
    /// 创建基本 Indicator 约束：binvar = 1 时，sum(vals[i]*vars[i]) &lt;= rhs 成立
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


// ===== 获取参数值 =====

/// <summary>
/// 获取整数参数值
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetIntParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out int value);

/// <summary>
/// 获取实数参数值
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetRealParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out double value);

/// <summary>
/// 获取布尔参数值
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetBoolParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    [MarshalAs(UnmanagedType.I1)] out bool value);

/// <summary>
/// 获取字符串参数值（调用者需释放返回的字符串）
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetStringParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out IntPtr value);

/// <summary>
/// 设置长整型参数
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPsetLongintParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    long value);

/// <summary>
/// 获取长整型参数值
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern ReturnCode SCIPgetLongintParam(
    IntPtr scip,
    [MarshalAs(UnmanagedType.LPStr)] string name,
    out long value);

/// <summary>
/// 设置参数强调模式
/// </summary>
[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
public static extern ReturnCode SCIPsetEmphasis(
    IntPtr scip,
    ParamEmphasis paramemphasis,
    [MarshalAs(UnmanagedType.I1)] bool quiet);
}
