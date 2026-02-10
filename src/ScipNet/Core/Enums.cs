namespace ScipNet.Core;

/// <summary>
/// 变量类型，对应 SCIP_VARTYPE
/// </summary>
public enum VariableType
{
    /// <summary>
    /// 二元变量：x ∈ {0, 1}
    /// </summary>
    Binary = 0,

    /// <summary>
    /// 整数变量：x ∈ {lb, ..., ub}
    /// </summary>
    Integer = 1,

    /// <summary>
    /// 连续变量：lb ≤ x ≤ ub
    /// </summary>
    Continuous = 3
}

/// <summary>
/// 目标函数方向，对应 SCIP_OBJSENSE
/// </summary>
public enum ObjectiveSense
{
    /// <summary>
    /// 最大化
    /// </summary>
    Maximize = -1,

    /// <summary>
    /// 最小化（默认）
    /// </summary>
    Minimize = 1
}

/// <summary>
/// 求解状态，对应 SCIP_STATUS
/// </summary>
public enum SolveStatus
{
    /// <summary>
    /// 求解状态未知
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 问题已求解至最优，最优解可用
    /// </summary>
    Optimal = 1,

    /// <summary>
    /// 问题被证明不可行
    /// </summary>
    Infeasible = 2,

    /// <summary>
    /// 问题被证明无界
    /// </summary>
    Unbounded = 3,

    /// <summary>
    /// 问题被证明不可行或无界
    /// </summary>
    InfeasibleOrUnbounded = 4,

    /// <summary>
    /// 用户中断求解过程（SIGINT 或 SCIPinterruptSolve()）
    /// </summary>
    UserInterrupt = 10,

    /// <summary>
    /// 进程收到 SIGTERM 信号
    /// </summary>
    Terminate = 11,

    /// <summary>
    /// 因达到节点限制而中断
    /// </summary>
    NodeLimit = 20,

    /// <summary>
    /// 因达到总节点限制而中断（包括重启）
    /// </summary>
    TotalNodeLimit = 21,

    /// <summary>
    /// 因达到停滞节点限制而中断（无原始界改进）
    /// </summary>
    StallNodeLimit = 22,

    /// <summary>
    /// 因达到时间限制而中断
    /// </summary>
    TimeLimit = 23,

    /// <summary>
    /// 因达到内存限制而中断
    /// </summary>
    MemoryLimit = 24,

    /// <summary>
    /// 因达到间隙限制而中断
    /// </summary>
    GapLimit = 25,

    /// <summary>
    /// 因达到原始界限制而中断
    /// </summary>
    PrimalLimit = 26,

    /// <summary>
    /// 因达到对偶界限制而中断
    /// </summary>
    DualLimit = 27,

    /// <summary>
    /// 因达到解限制而中断
    /// </summary>
    SolutionLimit = 28,

    /// <summary>
    /// 因达到解改进限制而中断
    /// </summary>
    BestSolutionLimit = 29,

    /// <summary>
    /// 因达到重启限制而中断
    /// </summary>
    RestartLimit = 30
}

/// <summary>
/// SCIP 方法返回码，对应 SCIP_RETCODE
/// </summary>
public enum ReturnCode
{
    /// <summary>
    /// 正常终止
    /// </summary>
    Okay = 1,

    /// <summary>
    /// 未指定错误
    /// </summary>
    Error = 0,

    /// <summary>
    /// 内存不足错误
    /// </summary>
    NoMemory = -1,

    /// <summary>
    /// 读取错误
    /// </summary>
    ReadError = -2,

    /// <summary>
    /// 写入错误
    /// </summary>
    WriteError = -3,

    /// <summary>
    /// 文件未找到错误
    /// </summary>
    NoFile = -4,

    /// <summary>
    /// 无法创建文件
    /// </summary>
    FileCreateError = -5,

    /// <summary>
    /// LP 求解器错误
    /// </summary>
    LpError = -6,

    /// <summary>
    /// 不存在问题
    /// </summary>
    NoProblem = -7,

    /// <summary>
    /// 此时无法调用该方法
    /// </summary>
    InvalidCall = -8,

    /// <summary>
    /// 输入数据错误
    /// </summary>
    InvalidData = -9,

    /// <summary>
    /// 方法返回无效结果码
    /// </summary>
    InvalidResult = -10,

    /// <summary>
    /// 未找到所需插件
    /// </summary>
    PluginNotFound = -11,

    /// <summary>
    /// 未找到指定名称的参数
    /// </summary>
    ParameterUnknown = -12,

    /// <summary>
    /// 参数类型不正确
    /// </summary>
    ParameterWrongType = -13,

    /// <summary>
    /// 参数值无效
    /// </summary>
    ParameterWrongValue = -14,

    /// <summary>
    /// 给定的键已存在于表中
    /// </summary>
    KeyAlreadyExisting = -15,

    /// <summary>
    /// 超过最大分支深度级别
    /// </summary>
    MaxDepthLevel = -16,

    /// <summary>
    /// 无法创建分支
    /// </summary>
    BranchError = -17,

    /// <summary>
    /// 函数未实现
    /// </summary>
    NotImplemented = -18
}

/// <summary>
/// SCIP 回调方法结果码，对应 SCIP_RESULT
/// </summary>
public enum ResultCode
{
    /// <summary>
    /// 方法未执行
    /// </summary>
    DidNotRun = 1,

    /// <summary>
    /// 方法未执行，但应稍后再次调用
    /// </summary>
    Delayed = 2,

    /// <summary>
    /// 方法已执行，但未找到任何内容
    /// </summary>
    DidNotFind = 3,

    /// <summary>
    /// 未发现不可行性
    /// </summary>
    Feasible = 4,

    /// <summary>
    /// 检测到不可行性
    /// </summary>
    Infeasible = 5,

    /// <summary>
    /// 检测到无界性
    /// </summary>
    Unbounded = 6,

    /// <summary>
    /// 当前节点不可行且可被剪枝
    /// </summary>
    Cutoff = 7,

    /// <summary>
    /// 方法添加了割平面
    /// </summary>
    Separated = 8,

    /// <summary>
    /// 方法添加了割平面，应立即开始新的分离轮次
    /// </summary>
    NewRound = 9,

    /// <summary>
    /// 方法缩减了变量的域
    /// </summary>
    ReducedDomain = 10,

    /// <summary>
    /// 方法添加了约束
    /// </summary>
    ConstraintAdded = 11,

    /// <summary>
    /// 方法修改了约束
    /// </summary>
    ConstraintChanged = 12,

    /// <summary>
    /// 方法创建了分支
    /// </summary>
    Branched = 13,

    /// <summary>
    /// 必须求解当前节点的 LP
    /// </summary>
    SolveLp = 14,

    /// <summary>
    /// 方法找到了可行的原始解
    /// </summary>
    FoundSolution = 15,

    /// <summary>
    /// 方法中断了执行，但需要时可以继续
    /// </summary>
    Suspended = 16,

    /// <summary>
    /// 方法成功执行
    /// </summary>
    Success = 17,

    /// <summary>
    /// 分支定界节点的处理应停止并稍后继续
    /// </summary>
    DelayNode = 18
}

/// <summary>
/// 约束方向
/// </summary>
public enum Sense
{
    /// <summary>
    /// 小于等于
    /// </summary>
    LessThanOrEqual = -1,

    /// <summary>
    /// 等于
    /// </summary>
    Equal = 0,

    /// <summary>
    /// 大于等于
    /// </summary>
    GreaterThanOrEqual = 1
}
