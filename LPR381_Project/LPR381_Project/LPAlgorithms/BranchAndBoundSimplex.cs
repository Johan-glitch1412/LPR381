using LPR381_Project;
using LPR381_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381_Project.LPAlgorithms
{
    public class BranchAndBoundAlgorithm
    {
        private LinearProgrammingModel _model;
        private PrimalSimplex _simplexAlgorithm;

        public BranchAndBoundAlgorithm(LinearProgrammingModel model)
        {
            _model = model;
            _simplexAlgorithm = new PrimalSimplex(model);
        }

        public void RunBranchAndBound()
        {
            var solutions = new List<(double[] Solution, double ObjectiveValue)>();
            var nodes = new Queue<LinearProgrammingModel>();
            nodes.Enqueue(_model);

            while (nodes.Count > 0)
            {
                var currentModel = nodes.Dequeue();
                Console.WriteLine("\nProcessing Node:");
                DisplayModel(currentModel);

                // Solve the LP relaxation for the current node
                var (solution, optimal) = _simplexAlgorithm.Solve();
                if (!optimal)
                {
                    Console.WriteLine("The problem is unbounded or infeasible.");
                    continue;
                }

                // Display the solution
                double objectiveValue = ComputeObjectiveValue(solution);
                Console.WriteLine("Solution:");
                for (int i = 0; i < solution.Length; i++)
                {
                    Console.WriteLine($"x{i + 1} = {solution[i]:F2}");
                }
                Console.WriteLine($"Objective Value: {objectiveValue:F2}");

                // Check if the solution is integer
                if (IsIntegerSolution(solution))
                {
                    Console.WriteLine("Integer solution found.");
                    solutions.Add((solution, objectiveValue));
                }
                else
                {
                    // Generate and add branches
                    Console.WriteLine("Solution is not integer. Branching...");
                    GenerateBranches(solution, nodes);
                }
            }

            // Display all integer solutions found
            Console.WriteLine("\nAll Integer Solutions Found:");
            foreach (var (sol, objVal) in solutions)
            {
                Console.WriteLine("Solution:");
                for (int i = 0; i < sol.Length; i++)
                {
                    Console.WriteLine($"x{i + 1} = {sol[i]:F2}");
                }
                Console.WriteLine($"Objective Value: {objVal:F2}");
            }
        }

        private void DisplayModel(LinearProgrammingModel model)
        {
            // Initialize the tableau with the current model
            var initializer = new InitializeTable(model);
            initializer.InitializeTableau();
            Console.WriteLine("Simplex Tableau:");
            initializer.PrintTableau();
        }

        private bool IsIntegerSolution(double[] solution)
        {
            return solution.All(val => Math.Abs(val - Math.Round(val)) < 1e-5);
        }

        private double ComputeObjectiveValue(double[] solution)
        {
            // Compute the objective value for the given solution
            return solution.Zip(_model.Objective.Coefficients, (x, c) => x * c).Sum();
        }

        private void GenerateBranches(double[] solution, Queue<LinearProgrammingModel> nodes)
        {
            // Generate two branches by adding constraints to fix fractional variables
            for (int i = 0; i < solution.Length; i++)
            {
                if (Math.Abs(solution[i] - Math.Round(solution[i])) >= 1e-5)
                {
                    // Create two branches: one with the variable <= floor(solution[i])
                    // and one with the variable >= ceil(solution[i])
                    double floorValue = Math.Floor(solution[i]);
                    double ceilValue = Math.Ceiling(solution[i]);

                    // Branch 1: x_i <= floor(solution[i])
                    var model1 = CreateSubproblemWithConstraint(i, floorValue);
                    nodes.Enqueue(model1);
                    Console.WriteLine($"Branching with x{i + 1} <= {floorValue}");

                    // Branch 2: x_i >= ceil(solution[i])
                    var model2 = CreateSubproblemWithConstraint(i, ceilValue);
                    nodes.Enqueue(model2);
                    Console.WriteLine($"Branching with x{i + 1} >= {ceilValue}");

                    break; // Branch on the first non-integer variable found
                }
            }
        }

        private LinearProgrammingModel CreateSubproblemWithConstraint(int variableIndex, double bound)
        {
            var newModel = new LinearProgrammingModel
            {
                Objective = _model.Objective,
                Constraints = new List<Constraint>(_model.Constraints),
                SignRestrictions = new List<string>(_model.SignRestrictions)
            };

            // Add new constraint
            var constraint = new Constraint
            {
                Coefficients = Enumerable.Range(0, variableIndex).Select(_ => 0.0).Concat(new[] { 1.0 }).Concat(Enumerable.Range(variableIndex + 1, newModel.NumVariables - variableIndex - 1).Select(_ => 0.0)).ToList(),
                Relation = "<=",
                RHS = bound
            };
            newModel.AddConstraint(constraint);

            return newModel;
        }
    }
}


