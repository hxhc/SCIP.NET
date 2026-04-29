"""
SCIP.NET Advanced Model Example — pyscipopt version

Demonstrates nonlinear constraints, nonlinear objectives,
solution pool, and indicator constraints using pyscipopt.
"""

import pyscipopt as scip
from pyscipopt import Model, quicksum


def run_division_constraint():
    """Example 1: Nonlinear Constraint (a <= x1/x2 <= b)"""
    print("=== Example 1: Nonlinear Constraint (a <= x1/x2 <= b) ===")

    model = Model("division_constraint")

    # Continuous variables x1 in [1, 10], x2 in [1, 10]
    x1 = model.addVar("x1", lb=1.0, ub=10.0, vtype="C")
    x2 = model.addVar("x2", lb=1.0, ub=10.0, vtype="C")

    # Linear objective: maximize x1 + x2
    model.setObjective(x1 + x2, sense="maximize")

    # Nonlinear constraint: 0.5 <= x1/x2 <= 2.0
    # In pyscipopt, use expr >= value and expr <= value directly
    model.addCons(x1 / x2 >= 0.5, name="lower_bound")
    model.addCons(x1 / x2 <= 2.0, name="upper_bound")

    print(f"Constraints: {model.getNCons()}")
    for c in model.getConss():
        print(f"  {c.name}")

    print("Solving...")
    model.optimize()

    status = model.getStatus()
    print(f"Status: {status}")

    if status == "optimal":
        # Method 1: Use model.getVal() to get values from best solution
        x1_val = model.getVal(x1)
        x2_val = model.getVal(x2)
        print(f"Optimal value: {model.getObjVal():.4f}")
        print(f"x1 = {x1_val:.4f}")
        print(f"x2 = {x2_val:.4f}")
        print(f"x1/x2 = {x1_val / x2_val:.4f}")


def run_nonlinear_objective():
    """Example 2: Nonlinear Objective (minimize x^2 + y^2)"""
    print("=== Example 2: Nonlinear Objective (minimize x^2 + y^2) ===")

    model = Model("nonlinear_objective")

    x = model.addVar("x", lb=0.0, ub=10.0, vtype="C")
    y = model.addVar("y", lb=0.0, ub=10.0, vtype="C")

    # Linear constraint: x + y >= 3
    model.addCons(x + y >= 3.0, name="sum_constraint")

    # Nonlinear objective: minimize x^2 + y^2
    # In pyscipopt, use ** for power
    model.setObjective(x**2 + y**2, sense="minimize")

    print("Solving...")
    model.optimize()

    status = model.getStatus()
    print(f"Status: {status}")

    if status == "optimal":
        x_val = model.getVal(x)
        y_val = model.getVal(y)
        print(f"Optimal value: {model.getObjVal():.4f}")
        print(f"x = {x_val:.4f}")
        print(f"y = {y_val:.4f}")
        print(f"x^2 + y^2 = {x_val**2 + y_val**2:.4f}")


def run_solution_pool():
    """Example 3: Solution Pool — get multiple solutions."""
    print("=== Example 3: Solution Pool ===")

    model = Model("solution_pool")

    # Solution pool parameters — MUST be set before optimize()
    model.setParam("limits/solutions", 100)  # Stop after 100 solutions
    model.setParam("limits/maxsol", 100)  # Pool capacity
    model.setParam("limits/maxorigsol", 100)  # Original problem sol pool
    model.setParam("limits/gap", 0.0)  # Allow non-optimal exploration
    model.setBoolParam("constraints/countsols/collect", True)

    # Additional heuristics to find more diverse solutions
    model.setParam("heuristics/diving/freq", 1)
    model.setParam("heuristics/coeffdiving/freq", 1)
    model.setParam("heuristics/fracdiving/freq", 1)
    model.setParam("heuristics/guideddiving/freq", 1)
    model.setParam("heuristics/pscostdiving/freq", 1)
    model.setParam("heuristics/linesearchdiving/freq", 1)
    model.setParam("heuristics/distdiving/freq", 1)
    model.setParam("heuristics/rens/freq", 1)
    model.setParam("heuristics/mutation/freq", 1)

    # 30 binary variables — large feasible space
    n = 30
    x = [model.addVar(f"x{i}", lb=0, ub=1, vtype="B") for i in range(n)]

    # Objective: maximize sum((i+1) * x_i)
    obj = quicksum((i + 1) * x[i] for i in range(n))
    model.setObjective(obj, sense="maximize")

    # Capacity constraint: sum(x_i) <= 20
    model.addCons(quicksum(x[i] for i in range(n)) <= 20, name="capacity")

    print(f"Variables: {n}, Domain: [0,1], Capacity: <= 20")

    print("Solving...")
    model.optimize()

    status = model.getStatus()
    print(f"Status: {status}")

    # Get all solutions from the pool
    nsols = model.getNSols()
    print(f"Number of solutions in pool: {nsols}")

    if nsols > 0:
        sols = model.getSols()
        print(f"Retrieved {len(sols)} solutions")

        # Show first 10 solutions
        for i, sol in enumerate(sols[:10]):
            selected = "".join(
                "1" if model.getSolVal(sol, x[i]) > 0.5 else "0" for i in range(n)
            )
            objval = model.getSolObjVal(sol)
            print(f"  Solution {i + 1}: obj={objval:.2f}, selected={selected}")

        if nsols > 10:
            print(f"  ... and {nsols - 10} more solutions")
    else:
        print("No solutions collected. Try adjusting parameters.")
        print(f"Best solution obj: {model.getObjVal():.4f}")


def run_indicator_constraint():
    """Example 4: Indicator Constraint (if z=1 then y <= 5)"""
    print("=== Example 4: Indicator Constraint (if z=1 then y <= 5) ===")

    model = Model("indicator_example")

    # z: binary — factory open?
    z = model.addVar("z", lb=0, ub=1, vtype="B")
    # x: product A output
    x = model.addVar("x", lb=0.0, ub=20.0, vtype="C")
    # y: product B output
    y = model.addVar("y", lb=0.0, ub=20.0, vtype="C")

    # Indicator constraint: z = 1 -> y <= 5
    # In pyscipopt, use addConsIndicator(expr, binvar=binvar, name=name)
    # The expr should be a constraint expression (like y <= 5)
    model.addConsIndicator(y <= 5.0, binvar=z, name="indicator_y")

    # Indicator constraint: z = 1 -> x <= 10
    model.addConsIndicator(x <= 10.0, binvar=z, name="indicator_x")

    # Regular constraint: x + y <= 12
    model.addCons(x + y <= 12.0, name="capacity")

    # Objective: maximize x + 2*y
    model.setObjective(x + 2 * y, sense="maximize")

    print("Solving...")
    model.optimize()

    status = model.getStatus()
    print(f"Status: {status}")

    if status == "optimal":
        print(f"Optimal value: {model.getObjVal():.4f}")
        print(f"z (factory open) = {model.getVal(z):.4f}")
        print(f"x (product A)    = {model.getVal(x):.4f}")
        print(f"y (product B)    = {model.getVal(y):.4f}")


if __name__ == "__main__":
    print("pyscipopt Advanced Model Example\n")

    run_division_constraint()
    print()

    run_nonlinear_objective()
    print()

    run_solution_pool()
    print()

    run_indicator_constraint()
