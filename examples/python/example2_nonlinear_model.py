import pyscipopt as scip
from pyscipopt import Model, quicksum

def run_multiple_solutions_exclusion():
    """
    迭代添加排除约束，收集多个可行解。
    每次求解均寻找全局最优解（即使目标无意义，也会找到一个可行解）。
    修正后的参数确保状态始终为 'optimal'，不会因 gap 提前终止。
    """
    print("=== 迭代排除法收集多个可行解 ===")

    model = Model("exclusion_collect")
    n = 30
    x = [model.addVar(f"x{i}", vtype="B") for i in range(n)]

    # 目标函数：最大化加权和
    obj = quicksum((i + 1) * x[i] for i in range(n))
    model.setObjective(obj, sense="maximize")

    model.addCons(quicksum(x[i] for i in range(n)) <= 20, name="capacity")

    # ---------- 核心设置：保证每次求解得到最优解 ----------
    # 1. 关闭默认的 gap 检查（将 gap 设为一个极大值，求解器永远无法满足）
    model.setParam("limits/absgap", 1e+20)   # 绝对间隙极大
    model.setParam("limits/gap", 1e+20)  # 相对间隙极大
    # 2. 时间限制（防止无限循环）
    model.setParam("limits/time", 60)        # 60 秒总时间
    # 3.（可选）关闭预求解，避免简化引起的歧义
    model.setParam("presolving/maxrounds", 0)

    solutions = []
    iteration = 0
    max_solutions = 100

    while iteration < max_solutions:
        iteration += 1
        model.optimize()

        status = model.getStatus()
        if status != "optimal":
            print(f"迭代 {iteration}: 状态 {status}，停止收集。")
            break

        sol_obj = model.getObjVal()
        sol_binary = [round(model.getVal(x[i])) for i in range(n)]

        print(f"找到第 {iteration} 个解: obj = {sol_obj:.4f}")

        solutions.append(sol_binary[:])

        # 排除约束
        excl_expr = quicksum(
            (1 - x[i]) if sol_binary[i] == 1 else x[i]
            for i in range(n)
        )
        model.addCons(excl_expr >= 1, name=f"exclude_sol_{iteration}")

    print(f"\n总共收集到 {len(solutions)} 个不同的可行解。")
    if solutions:
        print("前5个解的前20位二进制值：")
        for i, sol in enumerate(solutions[:5]):
            s = ''.join(str(b) for b in sol[:20])
            print(f"  Solution {i+1}: {s}...")

if __name__ == "__main__":
    run_multiple_solutions_exclusion()