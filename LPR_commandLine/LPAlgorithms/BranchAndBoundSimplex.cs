using LPR_commandLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR_commandLine.LPAlgorithms
{
    public class BranchAndBoundAlgorithm
    {
        private LinearProgrammingModel _model;

        public BranchAndBoundAlgorithm(LinearProgrammingModel model)
        {
            _model = model;
        }

        public void RunBranchAndBound()
        {
            Console.WriteLine("Running Branch and Bound Algorithm...\n");

            // Call the initial simplex algorithm to find an optimal solution for the LP relaxation
            var simplexAlgorithm = new PrimalSimplex(_model);
            var (initialSolution, isOptimal) = simplexAlgorithm.Solve();

            if (!isOptimal)
            {
                Console.WriteLine("The LP relaxation is unbounded or infeasible.");
                return;
            }

            // Check if the solution is integer
            if (IsIntegerSolution(initialSolution))
            {
                Console.WriteLine("Integer solution found.");
                DisplaySolution(initialSolution);
                return;
            }

            // If not integer, create branches
            foreach (var variableIndex in GetFractionalVariables(initialSolution))
            {
                var fractionalValue = initialSolution[variableIndex];
                CreateBranches(variableIndex, fractionalValue);
            }
        }

        private bool IsIntegerSolution(double[] solution)
        {
            return solution.All(val => Math.Abs(val - Math.Round(val)) < 1e-5);
        }

        private IEnumerable<int> GetFractionalVariables(double[] solution)
        {
            // Return indices of variables with fractional parts
            return solution.Select((value, index) => new { value, index })
                            .Where(v => Math.Abs(v.value - Math.Round(v.value)) >= 1e-5)
                            .Select(v => v.index);
        }

        private void CreateBranches(int variableIndex, double fractionalValue)
        {
            Console.WriteLine($"Branching on variable x{variableIndex + 1} with fractional value {fractionalValue}...\n");

            // Branch 1: x[variableIndex] <= floor(fractionalValue)
            var model1 = CloneModel(_model);
            AddConstraintToModel(model1, variableIndex, Math.Floor(fractionalValue), "<=");
            ProcessBranch(model1, "Branch 1");

            // Branch 2: x[variableIndex] >= ceil(fractionalValue)
            var model2 = CloneModel(_model);
            AddConstraintToModel(model2, variableIndex, Math.Ceiling(fractionalValue), ">=");
            ProcessBranch(model2, "Branch 2");
        }

        private void AddConstraintToModel(LinearProgrammingModel model, int variableIndex, double value, string relation)
        {
            var newConstraint = new Constraint
            {
                Coefficients = Enumerable.Range(0, model.NumVariables)
                                            .Select(i => i == variableIndex ? 1.0 : 0.0)
                                            .ToList(),
                RHS = value,
                Relation = relation
            };
            model.Constraints.Add(newConstraint);
        }

        private void ProcessBranch(LinearProgrammingModel model, string branchName)
        {
            Console.WriteLine($"Processing {branchName}...\n");

            var simplex = new PrimalSimplex(model);
            var (solution, isOptimal) = simplex.Solve();

            if (!isOptimal)
            {
                Console.WriteLine($"{branchName} resulted in an infeasible or unbounded solution.");
                return;
            }

            Console.WriteLine($"{branchName} Solution:");
            DisplaySolution(solution);

            // Optionally add additional logic for pruning or further branching
        }

        private void DisplaySolution(double[] solution)
        {
            for (int i = 0; i < solution.Length; i++)
            {
                Console.WriteLine($"x{i + 1} = {solution[i]:F2}");
            }
        }

        private LinearProgrammingModel CloneModel(LinearProgrammingModel model)
        {
            var clonedModel = new LinearProgrammingModel
            {
                Objective = new ObjectiveFunction
                {
                    Type = model.Objective.Type,
                    Coefficients = new List<double>(model.Objective.Coefficients)
                },
                Constraints = model.Constraints.Select(c => new Constraint
                {
                    Coefficients = new List<double>(c.Coefficients),
                    Relation = c.Relation,
                    RHS = c.RHS
                }).ToList(),
                SignRestrictions = new List<string>(model.SignRestrictions),
                Tableau = (double[,])model.Tableau.Clone()
            };
            return clonedModel;
        }
    }
}


