using System.Collections.Generic;
using System.Runtime.InteropServices;
using ScipNet.Native;

namespace ScipNet.Core;

/// <summary>
/// Represents a SCIP optimization problem model
/// </summary>
public sealed class Model : IDisposable
{
    private readonly ScipHandle _scipHandle;
    private readonly Dictionary<string, Variable> _variables;
    private readonly Dictionary<string, Constraint> _constraints;
    private readonly Dictionary<IntPtr, Variable> _varPtrToVarMap; // Mapping from var pointer to Variable object
    private readonly Dictionary<string, Variable> _varNameToVarMap; // Mapping from var name to Variable object
    private bool _disposed;
    private LinearExpression? _linearObjective; // Store linear objective for evaluation when Count() doesn't return __objvar__

    /// <summary>
    /// Gets the model name
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the objective sense
    /// </summary>
    public ObjectiveSense ObjectiveSense { get; private set; }

    /// <summary>
    /// Gets all variables
    /// </summary>
    public IReadOnlyCollection<Variable> Variables => _variables.Values;

    /// <summary>
    /// Gets all constraints
    /// </summary>
    public IReadOnlyCollection<Constraint> Constraints => _constraints.Values;

    /// <summary>
    /// Creates a new SCIP model
    /// </summary>
    /// <param name="name">Model name</param>
    /// <param name="includeDefaultPlugins">Whether to include default plugins</param>
    public Model(string name = "model", bool includeDefaultPlugins = true)
    {
        ReturnCode ret = ScipNativeMethods.SCIPcreate(out IntPtr scipPtr);
        ErrorHandler.CheckReturnCode(ret, "Failed to create SCIP instance");

        _scipHandle = new ScipHandle(scipPtr, true);
        _variables = new Dictionary<string, Variable>();
        _constraints = new Dictionary<string, Constraint>();
        _varPtrToVarMap = new Dictionary<IntPtr, Variable>();
        _varNameToVarMap = new Dictionary<string, Variable>(); // For name-based variable lookup
        Name = name;
        ObjectiveSense = ObjectiveSense.Minimize;

        if (includeDefaultPlugins)
        {
            IncludeDefaultPlugins();
        }

        CreateProblem(name);
    }

    /// <summary>
    /// Adds a variable to the model
    /// </summary>
    public Variable AddVariable(
        string name,
        double lowerBound,
        double upperBound,
        VariableType type)
    {
        ReturnCode ret = ScipNativeMethods.SCIPcreateVarBasic(
            _scipHandle,
            out IntPtr varPtr,
            name,
            lowerBound,
            upperBound,
            0.0,
            type);
        ErrorHandler.CheckReturnCode(ret, $"Failed to create variable {name}");

        ret = ScipNativeMethods.SCIPaddVar(_scipHandle, varPtr);
        ErrorHandler.CheckReturnCode(ret, $"Failed to add variable {name}");

        var variable = new Variable(this, name, varPtr, type, lowerBound, upperBound);
        _variables[name] = variable;
        _varPtrToVarMap[varPtr] = variable; // Add to pointer-to-variable mapping
        _varNameToVarMap[name] = variable; // Add to name-to-variable mapping
        return variable;
    }

    /// <summary>
    /// Adds a constraint to the model
    /// </summary>
    public T AddConstraint<T>(T constraint) where T : Constraint
    {
        // Set constraint model reference
        constraint.Model = this;

        IntPtr consPtr = constraint.AddToModel();
        _constraints[constraint.Name] = constraint;
        return constraint;
    }

    /// <summary>
    /// Sets the objective sense
    /// </summary>
    public void SetObjectiveSense(ObjectiveSense sense)
    {
        ObjectiveSense = sense;
        ReturnCode ret = ScipNativeMethods.SCIPsetObjsense(_scipHandle, sense);
        ErrorHandler.CheckReturnCode(ret, "Failed to set objective sense");
    }

    /// <summary>
    /// Sets the objective function (linear)
    /// </summary>
    /// <param name="expression">Objective function expression</param>
    /// <param name="sense">Objective sense</param>
    /// <remarks>
    /// Linear objectives use SCIP's native method (via SCIPchgVarObj), not epigraph transformation.
    /// This is for compatibility with Count() functionality, because Count() only returns original
    /// decision variables, not auxiliary variables. Therefore, we store the expression to manually
    /// calculate the objective value in EvaluateObjective().
    ///
    /// Nonlinear objective functions still use epigraph transformation, as SCIP requires it.
    /// </remarks>
    public void SetObjective(LinearExpression expression, ObjectiveSense sense)
    {
        // Store the linear objective for later evaluation
        // This is needed because Count() doesn't return auxiliary variables like __objvar__
        _linearObjective = expression;

        // Set objective sense
        SetObjectiveSense(sense);

        // Set objective coefficients (SCIP native method)
        foreach (var kvp in expression.Coefficients)
        {
            ReturnCode ret = ScipNativeMethods.SCIPchgVarObj(_scipHandle, kvp.Key.VarPtr, kvp.Value);
            ErrorHandler.CheckReturnCode(ret, $"Failed to set objective coefficient for variable {kvp.Key.Name}");
        }
    }

    /// <summary>
    /// Calculates the objective value for a given solution
    /// </summary>
    /// <param name="solution">Solution dictionary with variables as keys and their values.
    /// Typically from the solution list returned by GetSparseSolutionsWithVariables()</param>
    /// <returns>Objective function value. Returns 0.0 if no objective is set</returns>
    /// <remarks>
    /// This method handles evaluation for both linear and nonlinear objective functions:
    ///
    /// 1. Linear objective functions: Use the stored expression to calculate directly
    ///    - Reason: SCIP's Count() functionality only returns original decision variables, not auxiliary variables
    ///    - Therefore, we store the linear expression to manually calculate the objective value
    ///
    /// 2. Nonlinear objective functions: Look for the auxiliary variable named "__objvar__"
    ///    - Nonlinear objectives are transformed via epigraph: maximize f(x) → maximize objvar, subject to f(x) >= objvar
    ///    - The objective value is obtained by looking for __objvar__'s value in the solution
    ///
    /// Reason for this design difference:
    /// - Linear objectives can use SCIP's native method (SCIPchgVarObj) to keep the problem simple
    /// - Nonlinear objectives must go through epigraph transformation; SCIP's native solver doesn't directly support nonlinear objectives
    /// - Count() only returns original variables, so linear objectives need to store expressions for evaluation
    ///
    /// Usage example:
    /// <code>
    /// var solutions = model.GetSparseSolutionsWithVariables();
    /// foreach (var solution in solutions)
    /// {
    ///     double objValue = model.EvaluateObjective(solution);
    ///     Console.WriteLine($"Objective: {objValue}");
    /// }
    /// </code>
    /// </remarks>
    public double EvaluateObjective(Dictionary<Variable, double> solution)
    {
        // First check if __objvar__ exists (for nonlinear objectives)
        if (_varNameToVarMap.TryGetValue("__objvar__", out Variable? objVar) && solution.TryGetValue(objVar, out double objVal))
        {
            return objVal;
        }

        // If no __objvar__, try using the stored linear expression to calculate
        if (_linearObjective != null)
        {
            double objectiveValue = _linearObjective.Constant;
            foreach (var kvp in _linearObjective.Coefficients)
            {
                if (solution.TryGetValue(kvp.Key, out double varValue))
                {
                    objectiveValue += kvp.Value * varValue;
                }
            }
            return objectiveValue;
        }

        // No objective function set
        return 0.0;
    }

    /// <summary>
    /// Sets the objective function (nonlinear, automatically converted via epigraph reformulation)
    /// </summary>
    /// <param name="expression">Nonlinear objective function expression</param>
    /// <param name="sense">Objective sense</param>
    public void SetObjective(NonlinearExpression expression, ObjectiveSense sense)
    {
        // Set objective sense
        SetObjectiveSense(sense);

        // Create auxiliary continuous variable objvar (objective coefficient = 1.0), register to _variables to ensure release on Dispose
        ReturnCode ret = ScipNativeMethods.SCIPcreateVarBasic(
            _scipHandle,
            out IntPtr objVarPtr,
            "__objvar__",
            double.NegativeInfinity,
            double.PositiveInfinity,
            1.0,
            VariableType.Continuous);
        ErrorHandler.CheckReturnCode(ret, "Failed to create objective variable");

        ret = ScipNativeMethods.SCIPaddVar(_scipHandle, objVarPtr);
        ErrorHandler.CheckReturnCode(ret, "Failed to add objective variable");

        var objVar = new Variable(this, "__objvar__", objVarPtr, VariableType.Continuous,
            double.NegativeInfinity, double.PositiveInfinity);
        _variables[objVar.Name] = objVar;

        // Build native expression tree
        IntPtr exprPtr = expression.BuildExpr(_scipHandle);
        try
        {
            // epigraph reformulation:
            //   minimize f(x)  →  constraint: f(x) - objvar <= 0,  i.e., f(x) <= objvar
            //   maximize f(x)  →  constraint: f(x) - objvar >= 0,  i.e., f(x) >= objvar
            double lhs = sense == ObjectiveSense.Minimize ? double.NegativeInfinity : 0.0;
            double rhs = sense == ObjectiveSense.Minimize ? 0.0 : double.PositiveInfinity;

            ret = ScipNativeMethods.SCIPcreateConsBasicNonlinear(
                _scipHandle,
                out IntPtr consPtr,
                "__objcons__",
                exprPtr,
                lhs,
                rhs);
            ErrorHandler.CheckReturnCode(ret, "Failed to create objective constraint");

            // Add objvar with coefficient -1.0 to the linear part of the constraint
            // Constraint actual expression: f(x) - 1.0*objvar <= 0 (min) or >= 0 (max)
            ret = ScipNativeMethods.SCIPaddLinearVarNonlinear(_scipHandle, consPtr, objVarPtr, -1.0);
            ErrorHandler.CheckReturnCode(ret, "Failed to add linear var to objective constraint");

            // Add constraint
            ret = ScipNativeMethods.SCIPaddCons(_scipHandle, consPtr);
            ErrorHandler.CheckReturnCode(ret, "Failed to add objective constraint");

            // Record constraint (managed and released by Model)
            var objCons = new NonlinearConstraint(expression, lhs, rhs, "__objcons__");
            objCons.Model = this;
            objCons.SetConsPtrInternal(consPtr);
            _constraints[objCons.Name] = objCons;
        }
        finally
        {
            // Release expression (already captured by constraint)
            ScipNativeMethods.SCIPreleaseExpr(_scipHandle, ref exprPtr);
        }
    }

    /// <summary>
    /// Optimizes the model
    /// </summary>
    public SolveStatus Optimize()
{
    ReturnCode ret = ScipNativeMethods.SCIPsolve(_scipHandle);
    ErrorHandler.CheckReturnCode(ret, "Failed to solve");

    return ScipNativeMethods.SCIPgetStatus(_scipHandle);
}

/// <summary>
/// Count/enumerate all feasible solutions (instead of just solving for optimal)
/// After calling this method, the status will be Infeasible, which is expected behavior.
///
/// IMPORTANT: This method must be called BEFORE GetSparseSolutionsWithVariables().
/// The countsols constraint handler is automatically included by this method.
///
/// Required parameters (set before calling):
///   model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
///   model.SetBoolParam("constraints/countsols/collect", true);
///   model.SetLongParam("constraints/countsols/sollimit", 100000);
/// </summary>
public SolveStatus Count()
{
    // Include the countsols constraint handler if not already included
    // Note: In SCIP 9.0+, this IS included by SCIPincludeDefaultPlugins(),
    // so we need to check if it exists before including it to avoid InvalidData error
    IntPtr conshdlr = ScipNativeMethods.SCIPfindConshdlr(_scipHandle, "countsols");
    if (conshdlr == IntPtr.Zero)
    {
        ReturnCode includeRet = ScipNativeMethods.SCIPincludeConshdlrCountsols(_scipHandle);
        ErrorHandler.CheckReturnCode(includeRet, "Failed to include countsols constraint handler");
    }

    // Set safe counting parameters (including disabling restarts)
    ReturnCode ret = ScipNativeMethods.SCIPsetParamsCountsols(_scipHandle);
    ErrorHandler.CheckReturnCode(ret, "Failed to set counting parameters");

    // Start the counting process
    ret = ScipNativeMethods.SCIPcount(_scipHandle);
    ErrorHandler.CheckReturnCode(ret, "Failed to count solutions");

    return ScipNativeMethods.SCIPgetStatus(_scipHandle);
}

/// <summary>
/// Gets the number of counted solutions
/// </summary>
public long GetCountedSolutionsCount()
{
    IntPtr valid;
    long count = ScipNativeMethods.SCIPgetNCountedSols(_scipHandle, out valid);
    return count;
}

/// <summary>
/// Get raw counted sparse solutions (relative to active variables)
/// Note: SCIPgetCountedSparseSols returns void, so no error code to check
/// </summary>
public (IntPtr vars, int nvars, IntPtr sols, int nsols) GetCountedSparseSolutions()
{
    ScipNativeMethods.SCIPgetCountedSparseSols(
        _scipHandle,
        out IntPtr vars,
        out int nvars,
        out IntPtr sols,
        out int nsols);

    return (vars, nvars, sols, nsols);
}

/// <summary>
/// Check if sparse solutions are available after calling Count()
/// Note: No need to free the returned arrays - they are managed internally by SCIP
/// </summary>
public bool AreSparseSolutionsAvailable()
{
    try
    {
        var (_, _, sols, nsols) = GetCountedSparseSolutions();
        bool available = (nsols > 0 && sols != IntPtr.Zero);
        // No need to free - arrays are managed internally by SCIP
        return available;
    }
    catch
    {
        return false;
    }
}

/// <summary>
/// Get all sparse solutions unrolled into concrete solutions with variable values.
///
/// This follows the official SCIP approach:
/// 1. Get sparse solutions via SCIPgetCountedSparseSols
/// 2. For each sparse solution, iterate through concrete solutions
///    using SCIPsparseSolGetFirstSol / SCIPsparseSolGetNextSol
/// 3. Map variable pointers back to Variable objects
///
/// MUST be called after Count() with constraints/countsols/collect = true.
/// </summary>
public List<Dictionary<Variable, double>> GetSparseSolutionsWithVariables()
{
    var solutions = new List<Dictionary<Variable, double>>();

    // Get sparse solutions
    var (vars, nvars, sols, nsols) = GetCountedSparseSolutions();

    if (nsols == 0 || sols == IntPtr.Zero)
    {
        // No need to free - arrays are managed internally by SCIP
        return solutions;
    }

    // Read array of SCIP_SPARSESOL* pointers
    IntPtr[] sparseSolPtrs = new IntPtr[nsols];
    for (int i = 0; i < nsols; i++)
    {
        sparseSolPtrs[i] = Marshal.ReadIntPtr(sols, i * IntPtr.Size);
    }

    // Iterate each sparse solution and unroll into concrete solutions
    for (int s = 0; s < nsols; s++)
    {
        IntPtr sparsesol = sparseSolPtrs[s];

        // Get variable info for this sparse solution
        IntPtr solVars = ScipNativeMethods.SCIPsparseSolGetVars(sparsesol);
        int solNVars = ScipNativeMethods.SCIPsparseSolGetNVars(sparsesol);

        if (solVars == IntPtr.Zero || solNVars == 0)
            continue;

        // Allocate buffer for concrete solution values (SCIP_Longint[] = int64[])
        IntPtr concreteSolBuf = Marshal.AllocHGlobal(solNVars * sizeof(long));

        try
        {
            // Get first concrete solution (void return in SCIP API)
            ScipNativeMethods.SCIPsparseSolGetFirstSol(sparsesol, concreteSolBuf, solNVars);

                // Iterate through all concrete solutions in this sparse solution
                int concreteCount = 0;
                do
                {
                    var solutionValues = new Dictionary<Variable, double>();

                    for (int v = 0; v < solNVars; v++)
                    {
                        IntPtr varPtr = Marshal.ReadIntPtr(solVars, v * IntPtr.Size);
                        long value = Marshal.ReadInt64(concreteSolBuf, v * sizeof(long));

                        // Try to get variable by name (since pointers may not match after SCIP transformation)
                        IntPtr varNamePtr = ScipNativeMethods.SCIPvarGetName(varPtr);
                        string varName = Marshal.PtrToStringAnsi(varNamePtr) ?? "";

                        // Try direct name match first
                        if (_varNameToVarMap.TryGetValue(varName, out Variable? variable))
                        {
                            solutionValues[variable] = (double)value;
                        }
                        // If not found, try stripping common prefixes (like "t_")
                        else if (varName.StartsWith("t_"))
                        {
                            string originalName = varName.Substring(2); // Remove "t_" prefix
                            if (_varNameToVarMap.TryGetValue(originalName, out variable))
                            {
                                solutionValues[variable] = (double)value;
                            }
                        }
                    }

                    solutions.Add(solutionValues);
                    concreteCount++;

                } while (ScipNativeMethods.SCIPsparseSolGetNextSol(sparsesol, concreteSolBuf, solNVars));
        }
        finally
        {
            Marshal.FreeHGlobal(concreteSolBuf);
        }
    }
    // Note: No need to free sparse solutions arrays - they are managed internally by SCIP

    return solutions;
}

/// <summary>
/// Gets the best (optimal) solution
/// </summary>
    public Solution? GetBestSolution()
    {
        IntPtr solPtr = ScipNativeMethods.SCIPgetBestSol(_scipHandle);
        if (solPtr == IntPtr.Zero)
        {
            return null;
        }

        return new Solution(this, solPtr);
    }

    /// <summary>
    /// Gets all solutions from the solution pool
    /// </summary>
    public IReadOnlyList<Solution> GetSolutions()
    {
        int nsols = ScipNativeMethods.SCIPgetNSols(_scipHandle);
        if (nsols == 0)
        {
            return Array.Empty<Solution>();
        }

        IntPtr solsPtr = ScipNativeMethods.SCIPgetSols(_scipHandle);
        if (solsPtr == IntPtr.Zero)
        {
            return Array.Empty<Solution>();
        }

        var solutions = new Solution[nsols];
        for (int i = 0; i < nsols; i++)
        {
            IntPtr solPtr = Marshal.ReadIntPtr(solsPtr, i * IntPtr.Size);
            double objVal = ScipNativeMethods.SCIPgetSolOrigObj(_scipHandle, solPtr);
            solutions[i] = new Solution(this, solPtr, objVal);
        }

        return solutions;
    }

    /// <summary>
    /// Gets the number of solutions in the solution pool
    /// </summary>
    public int SolutionCount => ScipNativeMethods.SCIPgetNSols(_scipHandle);

    /// <summary>
    /// Sets a boolean parameter
    /// </summary>
    public void SetBoolParam(string name, bool value)
    {
        ReturnCode ret = ScipNativeMethods.SCIPsetBoolParam(_scipHandle, name, value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to set bool param '{name}'");
    }

/// <summary>
/// Sets an integer parameter
/// </summary>
public void SetIntParam(string name, int value)
{
    ReturnCode ret = ScipNativeMethods.SCIPsetIntParam(_scipHandle, name, value);
    ErrorHandler.CheckReturnCode(ret, $"Failed to set int param '{name}'");
}

/// <summary>
/// Sets a long integer parameter
/// </summary>
public void SetLongParam(string name, long value)
{
    ReturnCode ret = ScipNativeMethods.SCIPsetLongintParam(_scipHandle, name, value);
    ErrorHandler.CheckReturnCode(ret, $"Failed to set longint param '{name}'");
}

/// <summary>
/// Sets a real (floating-point) parameter
/// </summary>
    public void SetRealParam(string name, double value)
    {
        ReturnCode ret = ScipNativeMethods.SCIPsetRealParam(_scipHandle, name, value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to set real param '{name}'");
    }

    /// <summary>
    /// Sets a string parameter
    /// </summary>
    public void SetStringParam(string name, string value)
    {
        ReturnCode ret = ScipNativeMethods.SCIPsetStringParam(_scipHandle, name, value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to set string param '{name}'");
    }

    /// <summary>
    /// Gets statistics information
    /// </summary>
    public Statistics GetStatistics()
    {
        return new Statistics(this);
    }

    /// <summary>
    /// Gets an integer parameter value
    /// </summary>
    public int GetIntParam(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPgetIntParam(_scipHandle, name, out int value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to get int param '{name}'");
        return value;
    }

    /// <summary>
    /// Gets a real (floating-point) parameter value
    /// </summary>
    public double GetRealParam(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPgetRealParam(_scipHandle, name, out double value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to get real param '{name}'");
        return value;
    }

    /// <summary>
    /// Gets a boolean parameter value
    /// </summary>
    public bool GetBoolParam(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPgetBoolParam(_scipHandle, name, out bool value);
        ErrorHandler.CheckReturnCode(ret, $"Failed to get bool param '{name}'");
        return value;
    }

/// <summary>
/// Gets a string parameter value
/// </summary>
public string? GetStringParam(string name)
{
    ReturnCode ret = ScipNativeMethods.SCIPgetStringParam(_scipHandle, name, out IntPtr value);
    ErrorHandler.CheckReturnCode(ret, $"Failed to get string param '{name}'");
    if (value == IntPtr.Zero)
    {
        return null;
    }
    return Marshal.PtrToStringAnsi(value);
}

/// <summary>
/// Sets parameter emphasis mode
/// <param name="paramEmphasis">Parameter emphasis mode</param>
/// <param name="quiet">Whether to set quietly (no output)</param>
public void SetEmphasis(ParamEmphasis paramEmphasis, bool quiet = false)
{
    ReturnCode ret = ScipNativeMethods.SCIPsetEmphasis(_scipHandle, paramEmphasis, quiet);
    ErrorHandler.CheckReturnCode(ret, $"Failed to set emphasis to {paramEmphasis}");
}

/// <summary>
/// Releases resources
/// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            // Release all variables
            foreach (var kvp in _variables)
            {
                IntPtr varPtr = kvp.Value.VarPtr;
                if (varPtr != IntPtr.Zero)
                {
                    ReturnCode ret = ScipNativeMethods.SCIPreleaseVar(_scipHandle, ref varPtr);
                    if (ret != ReturnCode.Okay)
                    {
                        // Ignore release failure, continue releasing other resources
                    }
                    // varPtr should now be IntPtr.Zero (set by SCIPreleaseVar)
                }
            }

            // Release all constraints
            foreach (var kvp in _constraints)
            {
                IntPtr consPtr = kvp.Value.ConsPtr;
                if (consPtr != IntPtr.Zero)
                {
                    ReturnCode ret = ScipNativeMethods.SCIPreleaseCons(_scipHandle, ref consPtr);
                    if (ret != ReturnCode.Okay)
                    {
                        // Ignore release failure, continue releasing other resources
                    }
                    // consPtr should now be IntPtr.Zero (set by SCIPreleaseCons)
                }
            }

            // Release SCIP instance
            _scipHandle?.Dispose();
            _disposed = true;
        }
    }

    private void IncludeDefaultPlugins()
    {
        ReturnCode ret = ScipNativeMethods.SCIPincludeDefaultPlugins(_scipHandle);
        ErrorHandler.CheckReturnCode(ret, "Failed to include default plugins");
    }

    private void CreateProblem(string name)
    {
        ReturnCode ret = ScipNativeMethods.SCIPcreateProbBasic(_scipHandle, name);
        ErrorHandler.CheckReturnCode(ret, $"Failed to create problem {name}");
    }

    internal ScipHandle ScipHandle => _scipHandle;

    public override string ToString()
    {
        return $"Model '{Name}' with {_variables.Count} variables and {_constraints.Count} constraints";
    }
}
