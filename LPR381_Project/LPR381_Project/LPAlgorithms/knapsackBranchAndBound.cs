using LPR381_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381_Project.LPAlgorithms
{
    public class knapsackBranchAndBound
    {

        public List<KeyValuePair<int, double>> RankVariablesWithRatio(LinearProgrammingModel model)
        {
            if (model.Constraints.Count == 0 || model.Objective.Coefficients.Count == 0)
            {
                throw new ArgumentException("Model must have at least one constraint and objective function coefficients.");
            }

            int numVariables = model.Objective.Coefficients.Count;
            if (model.Constraints[0].Coefficients.Count != numVariables)
            {
                throw new ArgumentException("Objective function and first constraint must have the same number of variables.");
            }

            List<KeyValuePair<int, double>> ratios = new List<KeyValuePair<int, double>>();
            for (int i = 0; i < numVariables; i++)
            {
                double objectiveCoef = model.Objective.Coefficients[i];
                double constraintCoef = model.Constraints[0].Coefficients[i];

                if (constraintCoef != 0)
                {
                    ratios.Add(new KeyValuePair<int, double>(i + 1, objectiveCoef / constraintCoef));
                }
            }

            ratios.Sort((x, y) => y.Value.CompareTo(x.Value));

            Console.WriteLine("Ranking:");
            int rank = 1;
            foreach (var ratio in ratios)
            {
                Console.WriteLine($"x{ratio.Key}: Ratio = {ratio.Value}, Rank = {rank}");
                rank++;
            }

            return ratios;
        }

        public void SolveKnapsackBranchAndBound(LinearProgrammingModel model)
        {
            var rankedVariables = RankVariablesWithRatio(model);
            double rhs = model.Constraints[0].RHS;
            SolveSubproblem(rankedVariables, rhs, new Dictionary<int, double>(), model);
        }

        private void SolveSubproblem(List<KeyValuePair<int, double>> rankedVariables, double rhs, Dictionary<int, double> fixedValues, LinearProgrammingModel model)
        {
            double remainingRHS = rhs;
            Dictionary<int, double> currentSolution = new Dictionary<int, double>(fixedValues);

            foreach (var variable in rankedVariables)
            {
                int varIndex = variable.Key;
                double constraintCoef = model.Constraints[0].Coefficients[varIndex - 1];

                if (currentSolution.ContainsKey(varIndex))
                {
                    continue;
                }

                if (constraintCoef <= remainingRHS)
                {
                    currentSolution[varIndex] = 1;
                    remainingRHS -= constraintCoef;
                }
                else
                {
                    currentSolution[varIndex] = remainingRHS / constraintCoef;
                    remainingRHS = 0;
                    break;
                }
            }

            PrintSolution(currentSolution, remainingRHS);

            foreach (var variable in currentSolution)
            {
                if (variable.Value != 0 && variable.Value != 1)
                {
                    Console.WriteLine($"Generating Subproblems for x{variable.Key}");
                    Dictionary<int, double> subProblem1FixedValues = new Dictionary<int, double>(currentSolution);
                    subProblem1FixedValues[variable.Key] = 0;
                    SolveSubproblem(rankedVariables, rhs, subProblem1FixedValues, model);

                    Dictionary<int, double> subProblem2FixedValues = new Dictionary<int, double>(currentSolution);
                    subProblem2FixedValues[variable.Key] = 1;
                    double newRHS = rhs - (subProblem2FixedValues[variable.Key] * model.Constraints[0].Coefficients[variable.Key - 1]);
                    SolveSubproblem(rankedVariables, newRHS, subProblem2FixedValues, model);

                    return;
                }
            }
        }

        private void PrintSolution(Dictionary<int, double> solution, double remainingRHS)
        {
            Console.WriteLine("Current Solution:");
            foreach (var variable in solution)
            {
                Console.WriteLine($"x{variable.Key} = {variable.Value}");
            }
            Console.WriteLine($"Remaining RHS: {remainingRHS}");
        }
    }
}