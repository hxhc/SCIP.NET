namespace ScipNet.Core;

/// <summary>
/// Variable type, corresponds to SCIP_VARTYPE
/// </summary>
// 变量类型，对应 SCIP_VARTYPE
public enum VariableType
{
    /// <summary>
    /// Binary variable: x ∈ {0, 1}
    /// </summary>
    // 二元变量：x ∈ {0, 1}
    Binary = 0,

    /// <summary>
    /// Integer variable: x ∈ {lb, ..., ub}
    /// </summary>
    // 整数变量：x ∈ {lb, ..., ub}
    Integer = 1,

    /// <summary>
    /// Continuous variable: lb ≤ x ≤ ub
    /// </summary>
    // 连续变量：lb ≤ x ≤ ub
    Continuous = 3
}

/// <summary>
/// Objective function direction, corresponds to SCIP_OBJSENSE
/// </summary>
// 目标函数方向，对应 SCIP_OBJSENSE
public enum ObjectiveSense
{
    /// <summary>
    /// Maximize
    /// </summary>
    // 最大化
    Maximize = -1,

    /// <summary>
    /// Minimize (default)
    /// </summary>
    // 最小化（默认）
    Minimize = 1
}

/// <summary>
/// Solve status, corresponds to SCIP_STATUS
/// </summary>
// 求解状态，对应 SCIP_STATUS
public enum SolveStatus
{
    /// <summary>
    /// Unknown solve status
    /// </summary>
    // 未知求解状态
    Unknown = 0,

    /// <summary>
    /// Problem solved to optimality, optimal solution available
    /// </summary>
    // 问题已最优求解，最优解可用
    Optimal = 1,

    /// <summary>
    /// Problem proven infeasible
    /// </summary>
    // 问题被证明不可行
    Infeasible = 2,

    /// <summary>
    /// Problem proven unbounded
    /// </summary>
    // 问题被证明无界
    Unbounded = 3,

    /// <summary>
    /// Problem proven infeasible or unbounded
    /// </summary>
    // 问题被证明不可行或无界
    InfeasibleOrUnbounded = 4,

    /// <summary>
    /// User interrupted solving process (SIGINT or SCIPinterruptSolve())
    /// </summary>
    // 用户中断求解过程
    UserInterrupt = 10,

    /// <summary>
    /// Process received SIGTERM signal
    /// </summary>
    // 进程收到 SIGTERM 信号
    Terminate = 11,

    /// <summary>
    /// Interrupted due to node limit
    /// </summary>
    // 因节点限制而中断
    NodeLimit = 20,

    /// <summary>
    /// Interrupted due to total node limit (including restarts)
    /// </summary>
    // 因总节点限制而中断（含重启）
    TotalNodeLimit = 21,

    /// <summary>
    /// Interrupted due to stall node limit (no primal bound improvement)
    /// </summary>
    // 因停滞节点限制而中断
    StallNodeLimit = 22,

    /// <summary>
    /// Interrupted due to time limit
    /// </summary>
    // 因时间限制而中断
    TimeLimit = 23,

    /// <summary>
    /// Interrupted due to memory limit
    /// </summary>
    // 因内存限制而中断
    MemoryLimit = 24,

    /// <summary>
    /// Interrupted due to gap limit
    /// </summary>
    // 因间隙限制而中断
    GapLimit = 25,

    /// <summary>
    /// Interrupted due to primal bound limit
    /// </summary>
    // 因原始界限制而中断
    PrimalLimit = 26,

    /// <summary>
    /// Interrupted due to dual bound limit
    /// </summary>
    // 因对偶界限制而中断
    DualLimit = 27,

    /// <summary>
    /// Interrupted due to solution limit
    /// </summary>
    // 因解的数量限制而中断
    SolutionLimit = 28,

    /// <summary>
    /// Interrupted due to best solution limit
    /// </summary>
    // 因最优解限制而中断
    BestSolutionLimit = 29,

    /// <summary>
    /// Interrupted due to restart limit
    /// </summary>
    // 因重启限制而中断
    RestartLimit = 30
}

/// <summary>
/// SCIP method return code, corresponds to SCIP_RETCODE
/// </summary>
// SCIP 方法返回码，对应 SCIP_RETCODE
public enum ReturnCode
{
    /// <summary>
    /// Normal termination
    /// </summary>
    // 正常终止
    Okay = 1,

    /// <summary>
    /// Unspecified error
    /// </summary>
    // 未指定错误
    Error = 0,

    /// <summary>
    /// Out of memory error
    /// </summary>
    // 内存不足错误
    NoMemory = -1,

    /// <summary>
    /// Read error
    /// </summary>
    // 读取错误
    ReadError = -2,

    /// <summary>
    /// Write error
    /// </summary>
    // 写入错误
    WriteError = -3,

    /// <summary>
    /// File not found error
    /// </summary>
    // 文件未找到错误
    NoFile = -4,

    /// <summary>
    /// Unable to create file
    /// </summary>
    // 无法创建文件
    FileCreateError = -5,

    /// <summary>
    /// LP solver error
    /// </summary>
    // LP 求解器错误
    LpError = -6,

    /// <summary>
    /// No problem exists
    /// </summary>
    // 不存在问题
    NoProblem = -7,

    /// <summary>
    /// Method cannot be called at this time
    /// </summary>
    // 此时不能调用该方法
    InvalidCall = -8,

    /// <summary>
    /// Input data error
    /// </summary>
    // 输入数据错误
    InvalidData = -9,

    /// <summary>
    /// Method returned invalid result code
    /// </summary>
    // 方法返回了无效的结果码
    InvalidResult = -10,

    /// <summary>
    /// Required plugin not found
    /// </summary>
    // 未找到所需的插件
    PluginNotFound = -11,

    /// <summary>
    /// Parameter with specified name not found
    /// </summary>
    // 未找到指定名称的参数
    ParameterUnknown = -12,

    /// <summary>
    /// Parameter type incorrect
    /// </summary>
    // 参数类型不正确
    ParameterWrongType = -13,

    /// <summary>
    /// Parameter value invalid
    /// </summary>
    // 参数值无效
    ParameterWrongValue = -14,

    /// <summary>
    /// Given key already exists in table
    /// </summary>
    // 给定的键已存在于表中
    KeyAlreadyExisting = -15,

    /// <summary>
    /// Maximum branch depth level exceeded
    /// </summary>
    // 超过最大分支深度级别
    MaxDepthLevel = -16,

    /// <summary>
    /// Unable to create branch
    /// </summary>
    // 无法创建分支
    BranchError = -17,

    /// <summary>
    /// Function not implemented
    /// </summary>
    // 函数未实现
    NotImplemented = -18
}

/// <summary>
/// SCIP callback method result code, corresponds to SCIP_RESULT
/// </summary>
// SCIP 回调方法结果码，对应 SCIP_RESULT
public enum ResultCode
{
    /// <summary>
    /// Method did not run
    /// </summary>
    // 方法未运行
    DidNotRun = 1,

    /// <summary>
    /// Method did not run, but should be called again later
    /// </summary>
    // 方法未运行，但应稍后再次调用
    Delayed = 2,

    /// <summary>
    /// Method executed, but found nothing
    /// </summary>
    // 方法已执行，但未发现任何内容
    DidNotFind = 3,

    /// <summary>
    /// No infeasibility found
    /// </summary>
    // 未发现不可行性
    Feasible = 4,

    /// <summary>
    /// Infeasibility detected
    /// </summary>
    // 检测到不可行性
    Infeasible = 5,

    /// <summary>
    /// Unboundedness detected
    /// </summary>
    // 检测到无界性
    Unbounded = 6,

    /// <summary>
    /// Current node infeasible and can be pruned
    /// </summary>
    // 当前节点不可行，可被剪枝
    Cutoff = 7,

    /// <summary>
    /// Method added cutting planes
    /// </summary>
    // 方法添加了割平面
    Separated = 8,

    /// <summary>
    /// Method added cutting planes, should immediately start new separation round
    /// </summary>
    // 方法添加了割平面，应立即开始新的分离轮次
    NewRound = 9,

    /// <summary>
    /// Method reduced variable domain
    /// </summary>
    // 方法缩减了变量域
    ReducedDomain = 10,

    /// <summary>
    /// Method added constraints
    /// </summary>
    // 方法添加了约束
    ConstraintAdded = 11,

    /// <summary>
    /// Method modified constraints
    /// </summary>
    // 方法修改了约束
    ConstraintChanged = 12,

    /// <summary>
    /// Method created branches
    /// </summary>
    // 方法创建了分支
    Branched = 13,

    /// <summary>
    /// Must solve LP of current node
    /// </summary>
    // 必须求解当前节点的 LP
    SolveLp = 14,

    /// <summary>
    /// Method found feasible primal solution
    /// </summary>
    // 方法找到了可行的原始解
    FoundSolution = 15,

    /// <summary>
    /// Method suspended execution, but can continue if needed
    /// </summary>
    // 方法暂停执行，但可在需要时继续
    Suspended = 16,

    /// <summary>
    /// Method executed successfully
    /// </summary>
    // 方法执行成功
    Success = 17,

    /// <summary>
    /// Branch and bound node processing should stop and continue later
    /// </summary>
    // 分支定界节点处理应停止并稍后继续
    DelayNode = 18
}

/// <summary>
/// Constraint direction
/// </summary>
// 约束方向
public enum Sense
{
    /// <summary>
    /// Less than or equal
    /// </summary>
    // 小于或等于
    LessThanOrEqual = -1,

    /// <summary>
    /// Equal
    /// </summary>
    // 等于
    Equal = 0,

    /// <summary>
    /// Greater than or equal
    /// </summary>
    // 大于或等于
    GreaterThanOrEqual = 1
}

/// <summary>
/// SCIP parameter emphasis mode, corresponds to SCIP_PARAMEMPHASIS
/// </summary>
// SCIP 参数强调模式，对应 SCIP_PARAMEMPHASIS
public enum ParamEmphasis
{
    /// <summary>
    /// Default parameter settings
    /// </summary>
    // 默认参数设置
    Default = 0,

    /// <summary>
    /// CP solver mode (e.g., without LP relaxation)
    /// </summary>
    // CP 求解器模式（如无 LP 松弛）
    CPSolver = 1,

    /// <summary>
    /// Solve easy problems quickly
    /// </summary>
    // 快速求解简单问题
    EasyCIP = 2,

    /// <summary>
    /// Detect feasibility quickly
    /// </summary>
    // 快速检测可行性
    Feasibility = 3,

    /// <summary>
    /// Handle difficult LP
    /// </summary>
    // 处理困难的 LP
    HardLP = 4,

    /// <summary>
    /// Prove optimality quickly
    /// </summary>
    // 快速证明最优性
    Optimality = 5,

    /// <summary>
    /// Counting process (obtain feasible and "fast" counts)
    /// </summary>
    // 计数过程（获取可行和"快速"计数）
    Counter = 6,

    /// <summary>
    /// Feasibility phase of three-phase solving process
    /// </summary>
    // 三阶段求解过程的可行性阶段
    PhaseFeas = 7,

    /// <summary>
    /// Improvement phase of three-phase solving process
    /// </summary>
    // 三阶段求解过程的改进阶段
    PhaseImprove = 8,

    /// <summary>
    /// Proof phase of three-phase solving process
    /// </summary>
    // 三阶段求解过程的证明阶段
    PhaseProof = 9,

    /// <summary>
    /// Solve numerical problems
    /// </summary>
    // 求解数值问题
    Numerics = 10,

    /// <summary>
    /// Benchmark mode
    /// </summary>
    // 基准测试模式
    Benchmark = 11
}
