namespace ScipNet.Core;

/// <summary>
/// Variable type, corresponds to SCIP_VARTYPE
/// </summary>
public enum VariableType
{
    /// <summary>
    /// Binary variable: x ∈ {0, 1}
    /// </summary>
    Binary = 0,

    /// <summary>
    /// Integer variable: x ∈ {lb, ..., ub}
    /// </summary>
    Integer = 1,

    /// <summary>
    /// Continuous variable: lb ≤ x ≤ ub
    /// </summary>
    Continuous = 3
}

/// <summary>
/// Objective function direction, corresponds to SCIP_OBJSENSE
/// </summary>
public enum ObjectiveSense
{
    /// <summary>
    /// Maximize
    /// </summary>
    Maximize = -1,

    /// <summary>
    /// Minimize (default)
    /// </summary>
    Minimize = 1
}

/// <summary>
/// Solve status, corresponds to SCIP_STATUS
/// </summary>
public enum SolveStatus
{
    /// <summary>
    /// Unknown solve status
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Problem solved to optimality, optimal solution available
    /// </summary>
    Optimal = 1,

    /// <summary>
    /// Problem proven infeasible
    /// </summary>
    Infeasible = 2,

    /// <summary>
    /// Problem proven unbounded
    /// </summary>
    Unbounded = 3,

    /// <summary>
    /// Problem proven infeasible or unbounded
    /// </summary>
    InfeasibleOrUnbounded = 4,

    /// <summary>
    /// User interrupted solving process (SIGINT or SCIPinterruptSolve())
    /// </summary>
    UserInterrupt = 10,

    /// <summary>
    /// Process received SIGTERM signal
    /// </summary>
    Terminate = 11,

    /// <summary>
    /// Interrupted due to node limit
    /// </summary>
    NodeLimit = 20,

    /// <summary>
    /// Interrupted due to total node limit (including restarts)
    /// </summary>
    TotalNodeLimit = 21,

    /// <summary>
    /// Interrupted due to stall node limit (no primal bound improvement)
    /// </summary>
    StallNodeLimit = 22,

    /// <summary>
    /// Interrupted due to time limit
    /// </summary>
    TimeLimit = 23,

    /// <summary>
    /// Interrupted due to memory limit
    /// </summary>
    MemoryLimit = 24,

    /// <summary>
    /// Interrupted due to gap limit
    /// </summary>
    GapLimit = 25,

    /// <summary>
    /// Interrupted due to primal bound limit
    /// </summary>
    PrimalLimit = 26,

    /// <summary>
    /// Interrupted due to dual bound limit
    /// </summary>
    DualLimit = 27,

    /// <summary>
    /// Interrupted due to solution limit
    /// </summary>
    SolutionLimit = 28,

    /// <summary>
    /// Interrupted due to best solution limit
    /// </summary>
    BestSolutionLimit = 29,

    /// <summary>
    /// Interrupted due to restart limit
    /// </summary>
    RestartLimit = 30
}

/// <summary>
/// SCIP method return code, corresponds to SCIP_RETCODE
/// </summary>
public enum ReturnCode
{
    /// <summary>
    /// Normal termination
    /// </summary>
    Okay = 1,

    /// <summary>
    /// Unspecified error
    /// </summary>
    Error = 0,

    /// <summary>
    /// Out of memory error
    /// </summary>
    NoMemory = -1,

    /// <summary>
    /// Read error
    /// </summary>
    ReadError = -2,

    /// <summary>
    /// Write error
    /// </summary>
    WriteError = -3,

    /// <summary>
    /// File not found error
    /// </summary>
    NoFile = -4,

    /// <summary>
    /// Unable to create file
    /// </summary>
    FileCreateError = -5,

    /// <summary>
    /// LP solver error
    /// </summary>
    LpError = -6,

    /// <summary>
    /// No problem exists
    /// </summary>
    NoProblem = -7,

    /// <summary>
    /// Method cannot be called at this time
    /// </summary>
    InvalidCall = -8,

    /// <summary>
    /// Input data error
    /// </summary>
    InvalidData = -9,

    /// <summary>
    /// Method returned invalid result code
    /// </summary>
    InvalidResult = -10,

    /// <summary>
    /// Required plugin not found
    /// </summary>
    PluginNotFound = -11,

    /// <summary>
    /// Parameter with specified name not found
    /// </summary>
    ParameterUnknown = -12,

    /// <summary>
    /// Parameter type incorrect
    /// </summary>
    ParameterWrongType = -13,

    /// <summary>
    /// Parameter value invalid
    /// </summary>
    ParameterWrongValue = -14,

    /// <summary>
    /// Given key already exists in table
    /// </summary>
    KeyAlreadyExisting = -15,

    /// <summary>
    /// Maximum branch depth level exceeded
    /// </summary>
    MaxDepthLevel = -16,

    /// <summary>
    /// Unable to create branch
    /// </summary>
    BranchError = -17,

    /// <summary>
    /// Function not implemented
    /// </summary>
    NotImplemented = -18
}

/// <summary>
/// SCIP callback method result code, corresponds to SCIP_RESULT
/// </summary>
public enum ResultCode
{
    /// <summary>
    /// Method did not run
    /// </summary>
    DidNotRun = 1,

    /// <summary>
    /// Method did not run, but should be called again later
    /// </summary>
    Delayed = 2,

    /// <summary>
    /// Method executed, but found nothing
    /// </summary>
    DidNotFind = 3,

    /// <summary>
    /// No infeasibility found
    /// </summary>
    Feasible = 4,

    /// <summary>
    /// Infeasibility detected
    /// </summary>
    Infeasible = 5,

    /// <summary>
    /// Unboundedness detected
    /// </summary>
    Unbounded = 6,

    /// <summary>
    /// Current node infeasible and can be pruned
    /// </summary>
    Cutoff = 7,

    /// <summary>
    /// Method added cutting planes
    /// </summary>
    Separated = 8,

    /// <summary>
    /// Method added cutting planes, should immediately start new separation round
    /// </summary>
    NewRound = 9,

    /// <summary>
    /// Method reduced variable domain
    /// </summary>
    ReducedDomain = 10,

    /// <summary>
    /// Method added constraints
    /// </summary>
    ConstraintAdded = 11,

    /// <summary>
    /// Method modified constraints
    /// </summary>
    ConstraintChanged = 12,

    /// <summary>
    /// Method created branches
    /// </summary>
    Branched = 13,

    /// <summary>
    /// Must solve LP of current node
    /// </summary>
    SolveLp = 14,

    /// <summary>
    /// Method found feasible primal solution
    /// </summary>
    FoundSolution = 15,

    /// <summary>
    /// Method suspended execution, but can continue if needed
    /// </summary>
    Suspended = 16,

    /// <summary>
    /// Method executed successfully
    /// </summary>
    Success = 17,

    /// <summary>
    /// Branch and bound node processing should stop and continue later
    /// </summary>
    DelayNode = 18
}

/// <summary>
/// Constraint direction
/// </summary>
public enum Sense
{
    /// <summary>
    /// Less than or equal
    /// </summary>
    LessThanOrEqual = -1,

    /// <summary>
    /// Equal
    /// </summary>
    Equal = 0,

    /// <summary>
    /// Greater than or equal
    /// </summary>
    GreaterThanOrEqual = 1
}

/// <summary>
/// SCIP parameter emphasis mode, corresponds to SCIP_PARAMEMPHASIS
/// </summary>
public enum ParamEmphasis
{
    /// <summary>
    /// Default parameter settings
    /// </summary>
    Default = 0,

    /// <summary>
    /// CP solver mode (e.g., without LP relaxation)
    /// </summary>
    CPSolver = 1,

    /// <summary>
    /// Solve easy problems quickly
    /// </summary>
    EasyCIP = 2,

    /// <summary>
    /// Detect feasibility quickly
    /// </summary>
    Feasibility = 3,

    /// <summary>
    /// Handle difficult LP
    /// </summary>
    HardLP = 4,

    /// <summary>
    /// Prove optimality quickly
    /// </summary>
    Optimality = 5,

    /// <summary>
    /// Counting process (obtain feasible and "fast" counts)
    /// </summary>
    Counter = 6,

    /// <summary>
    /// Feasibility phase of three-phase solving process
    /// </summary>
    PhaseFeas = 7,

    /// <summary>
    /// Improvement phase of three-phase solving process
    /// </summary>
    PhaseImprove = 8,

    /// <summary>
    /// Proof phase of three-phase solving process
    /// </summary>
    PhaseProof = 9,

    /// <summary>
    /// Solve numerical problems
    /// </summary>
    Numerics = 10,

    /// <summary>
    /// Benchmark mode
    /// </summary>
    Benchmark = 11
}
