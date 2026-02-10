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
}
