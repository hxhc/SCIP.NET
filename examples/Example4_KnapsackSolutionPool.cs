using System;
using System.Collections.Generic;
using System.Linq;
using ScipNet;
using ScipNet.Core;

/// <summary>
/// Knapsack Problem with Solution Pool - Enumerate all feasible solutions
///
/// Problem: Given a knapsack with maximum weight capacity of 8kg,
/// enumerate all feasible ways to select fruits without exceeding the weight limit,
/// while maximizing the total price.
///
/// Items (Fruits):
///   1. Chestnut:          4kg, $4500
///   2. Apple:             5kg, $5700
///   3. Orange:            2kg, $2250
///   4. Strawberry:        1kg, $1100
///   5. Melon:             6kg, $6700
///
/// This example demonstrates:
/// 1. How to model a 0-1 knapsack problem with an objective function
/// 2. How to use Solution Pool to enumerate all feasible solutions (not just the optimal one)
/// 3. How to use Model.EvaluateObjective() to calculate the objective value for each solution
/// 4. How to analyze and display all feasible solutions
/// 5. How to identify the optimal solution from enumerated solutions
///
/// Note: Although we set a maximization objective, Count() will enumerate ALL feasible solutions,
///       not just the optimal one. The objective is useful for identifying which solution is best
///       after enumeration using Model.EvaluateObjective().
///
/// Usage of Model.EvaluateObjective():
///   After retrieving solutions with GetSparseSolutionsWithVariables(), you can calculate
///   the objective value for any solution dictionary:
///     double objValue = model.EvaluateObjective(solution);
///   where 'solution' is a Dictionary<Variable, double> mapping variables to their values.
/// </summary>
public class Example4_KnapsackSolutionPool
{
    // Item data structure
    private class Item
    {
        public int Id { get; }
        public string Name { get; }
        public double Weight { get; }
        public int Price { get; }

        public Item(int id, string name, double weight, int price)
        {
            Id = id;
            Name = name;
            Weight = weight;
            Price = price;
        }
    }

    public static void Run()
    {
        Console.WriteLine("=== Knapsack Problem - Solution Pool Example ===\n");

        // Define items
        var items = new Item[]
        {
            new Item(1, "Chestnut", 4, 4500),
            new Item(2, "Apple", 5, 5700),
            new Item(3, "Orange", 2, 2250),
            new Item(4, "Strawberry", 1, 1100),
            new Item(5, "Melon", 6, 6700)
        };

        double maxWeight = 8.0;  // Maximum weight capacity in kg
        int maxTime = 10;        // Time limit in seconds

        // Display problem information
        Console.WriteLine($"Problem: Knapsack with maximum weight {maxWeight} kg");
        Console.WriteLine($"\nItems available:");
        Console.WriteLine($"  ID  Name        Weight   Price");
        Console.WriteLine($"  {new string('-', 40)}");
        foreach (var item in items)
        {
            Console.WriteLine($"  {item.Id,-2}  {item.Name,-8}  {item.Weight,4}kg   ${item.Price,5}");
        }

        using var model = new Model("knapsack_solution_pool");

        // Step 1: Create binary variables for each item
        // x[i] = 1 if item i is selected, 0 otherwise
        var x = new Variable[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            x[i] = model.AddVariable($"x_{items[i].Name}", 0, 1, VariableType.Binary);
        }

        // Step 2: Add weight constraint
        // Sum(weight[i] * x[i]) <= maxWeight
        var weightExpr = new LinearExpression();
        for (int i = 0; i < items.Length; i++)
        {
            weightExpr = weightExpr + items[i].Weight * x[i];
        }
        model.AddConstraint(weightExpr.Leq(maxWeight));

        // Step 2.5: Set objective function - Maximize total price
        // Maximize: Sum(price[i] * x[i])
        var priceExpr = new LinearExpression();
        for (int i = 0; i < items.Length; i++)
        {
            priceExpr = priceExpr + items[i].Price * x[i];
        }
        model.SetObjective(priceExpr, ObjectiveSense.Maximize);
        Console.WriteLine($"Objective: Maximize total price (objective function set, but Count() will enumerate ALL feasible solutions)");

        // Step 3: Configure for counting
        // - Counter emphasis: optimized for enumeration
        // - countsols/collect: store solutions for later retrieval
        // - presolving/maxrounds: disable presolving to avoid variable transformation issues
        model.SetEmphasis(ParamEmphasis.Counter, quiet: true);
        model.SetBoolParam("constraints/countsols/collect", true);
        model.SetLongParam("constraints/countsols/sollimit", 100000);
        model.SetRealParam("limits/time", maxTime);
        model.SetIntParam("display/verblevel", 0);
        model.SetIntParam("presolving/maxrounds", 0); // Disable presolving to keep original variables

        // Step 4: Count all feasible solutions
        Console.WriteLine("\nEnumerating all feasible solutions...");
        var status = model.Count();
        Console.WriteLine($"Solve status: {status}");

        // Step 5: Get the total count
        long totalCount = model.GetCountedSolutionsCount();
        Console.WriteLine($"\nTotal feasible solutions found: {totalCount}");

        // Step 6: Retrieve all solutions
        Console.WriteLine("\nRetrieving solutions...");
        var solutions = model.GetSparseSolutionsWithVariables();
        Console.WriteLine($"Retrieved {solutions.Count} concrete solutions");

        if (solutions.Count == 0)
        {
            Console.WriteLine("No solutions retrieved.");
            return;
        }

        // Step 7: Analyze and display all solutions
        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"  ALL {solutions.Count} FEASIBLE SOLUTIONS");
        Console.WriteLine($"{new string('=', 80)}");

        // Store solution details for analysis
        var solutionDetails = new List<(int Index, List<Item> Items, double TotalWeight, double TotalPrice)>();

        for (int i = 0; i < solutions.Count; i++)
        {
            var sol = solutions[i];

            // Get selected items and calculate totals
            var selectedItems = new List<Item>();
            double totalWeight = 0;

            for (int j = 0; j < items.Length; j++)
            {
                if (sol[x[j]] > 0.5)  // Variable is selected (value ≈ 1)
                {
                    selectedItems.Add(items[j]);
                    totalWeight += items[j].Weight;
                }
            }

            // Calculate objective value using Model.EvaluateObjective
            double totalPrice = model.EvaluateObjective(sol);

            solutionDetails.Add((i + 1, selectedItems, totalWeight, (int)totalPrice));

            // Display solution
            Console.WriteLine($"\nSolution #{i + 1}:");
            Console.WriteLine($"  Selected Items ({selectedItems.Count}):");
            foreach (var item in selectedItems)
            {
                Console.WriteLine($"    - {item.Name} ({item.Weight}kg, ${item.Price})");
            }
            Console.WriteLine($"  Total Weight: {totalWeight}kg (Limit: {maxWeight}kg)");
            Console.WriteLine($"  Total Price:  ${totalPrice:F2}");
            Console.WriteLine($"  Objective Value: ${totalPrice:F2} (from Model.EvaluateObjective)");
        }

        // Step 8: Statistical Analysis
        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"  STATISTICAL ANALYSIS");
        Console.WriteLine($"{new string('=', 80)}");

        // Find best solution by price
        var bestSolution = solutionDetails.OrderByDescending(s => s.TotalPrice).First();
        Console.WriteLine($"\n💰 Best Solution (Maximum Value):");
        Console.WriteLine($"  Solution #{bestSolution.Index}: {string.Join(", ", bestSolution.Items.Select(i => i.Name))}");
        Console.WriteLine($"  Total Weight: {bestSolution.TotalWeight}kg");
        Console.WriteLine($"  Total Price:  ${bestSolution.TotalPrice}");

        // Find most efficient solution (price per kg)
        var efficientSolutions = solutionDetails
            .Select(s => new
            {
                s.Index,
                s.Items,
                s.TotalWeight,
                s.TotalPrice,
                Efficiency = s.TotalPrice / s.TotalWeight
            })
            .OrderByDescending(s => s.Efficiency)
            .First();
        Console.WriteLine($"\n📊 Most Efficient Solution (Price per kg):");
        Console.WriteLine($"  Solution #{efficientSolutions.Index}: {string.Join(", ", efficientSolutions.Items.Select(i => i.Name))}");
        Console.WriteLine($"  Total Weight: {efficientSolutions.TotalWeight}kg");
        Console.WriteLine($"  Total Price:  ${efficientSolutions.TotalPrice}");
        Console.WriteLine($"  Efficiency:   ${efficientSolutions.Efficiency:F2}/kg");

        // Find solution with most items
        var mostItems = solutionDetails.OrderByDescending(s => s.Items.Count).First();
        Console.WriteLine($"\n📦 Solution with Most Items:");
        Console.WriteLine($"  Solution #{mostItems.Index}: {string.Join(", ", mostItems.Items.Select(i => i.Name))}");
        Console.WriteLine($"  Items:        {mostItems.Items.Count}");
        Console.WriteLine($"  Total Weight: {mostItems.TotalWeight}kg");
        Console.WriteLine($"  Total Price:  ${mostItems.TotalPrice}");

        // Find lightest solution
        var lightest = solutionDetails.OrderBy(s => s.TotalWeight).First();
        Console.WriteLine($"\n⚖️  Lightest Solution:");
        Console.WriteLine($"  Solution #{lightest.Index}: {string.Join(", ", lightest.Items.Select(i => i.Name))}");
        Console.WriteLine($"  Total Weight: {lightest.TotalWeight}kg");
        Console.WriteLine($"  Total Price:  ${lightest.TotalPrice}");

        // Find heaviest solution
        var heaviest = solutionDetails.OrderByDescending(s => s.TotalWeight).First();
        Console.WriteLine($"\n🪨 Heaviest Solution:");
        Console.WriteLine($"  Solution #{heaviest.Index}: {string.Join(", ", heaviest.Items.Select(i => i.Name))}");
        Console.WriteLine($"  Total Weight: {heaviest.TotalWeight}kg (Limit: {maxWeight}kg)");
        Console.WriteLine($"  Total Price:  ${heaviest.TotalPrice}");

        // Step 9: Item Selection Frequency
        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"  ITEM SELECTION FREQUENCY");
        Console.WriteLine($"{new string('=', 80)}\n");

        var frequency = new Dictionary<string, int>();
        foreach (var sol in solutions)
        {
            foreach (var item in items)
            {
                if (sol[x[item.Id - 1]] > 0.5)
                {
                    string itemName = item.Name;
                    frequency.TryGetValue(itemName, out int count);
                    frequency[itemName] = count + 1;
                }
            }
        }

        Console.WriteLine($"  {"Item",-10}  {"Count",-8}  {"Percentage",-12}  {"Bar"}");
        Console.WriteLine($"  {new string('-', 50)}");
        foreach (var item in items.OrderBy(i => i.Id))
        {
            int count = frequency.GetValueOrDefault(item.Name, 0);
            double pct = (double)count / solutions.Count * 100;
            int barLength = (int)(pct / 2);
            string bar = new string('█', barLength);
            Console.WriteLine($"  {item.Name,-10}  {count,3}/{solutions.Count,-3}  {pct,7:F1}%     {bar}");
        }

        // Step 10: Solution Distribution by Number of Items
        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"  SOLUTION DISTRIBUTION BY NUMBER OF ITEMS");
        Console.WriteLine($"{new string('=', 80)}\n");

        var byCount = solutionDetails
            .GroupBy(s => s.Items.Count)
            .OrderBy(g => g.Key);

        foreach (var group in byCount)
        {
            int count = group.Count();
            double pct = (double)count / solutions.Count * 100;
            Console.WriteLine($"  {group.Key} item(s):  {count} solutions ({pct:F1}%)");
        }

        // Step 11: Summary
        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"  SUMMARY");
        Console.WriteLine($"{new string('=', 80)}");
        Console.WriteLine($"\n  Problem:");
        Console.WriteLine($"    - Knapsack capacity: {maxWeight}kg");
        Console.WriteLine($"    - Available items:   {items.Length}");
        Console.WriteLine($"    - Possible subsets:  {Math.Pow(2, items.Length)}");
        Console.WriteLine($"    - Objective:         Maximize total price");
        Console.WriteLine($"\n  Results:");
        Console.WriteLine($"    - Feasible solutions:  {solutions.Count}");
        Console.WriteLine($"    - Coverage:            {(double)solutions.Count / Math.Pow(2, items.Length) * 100:F1}%");
        Console.WriteLine($"    - Best value:          ${bestSolution.TotalPrice}");
        Console.WriteLine($"    - Optimal solution:    Solution #{bestSolution.Index}: {string.Join(", ", bestSolution.Items.Select(i => i.Name))}");
        Console.WriteLine($"    - Value range:         ${solutionDetails.Min(s => s.TotalPrice)} - ${solutionDetails.Max(s => s.TotalPrice)}");
        Console.WriteLine($"    - Weight range:        {solutionDetails.Min(s => s.TotalWeight):F1}kg - {solutionDetails.Max(s => s.TotalWeight):F1}kg");
        Console.WriteLine($"\n  Status: ✅ Successfully enumerated all feasible solutions!");
        Console.WriteLine($"  Note:   The optimal solution (maximizing price) is highlighted above");
    }
}
