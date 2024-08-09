using LPR_commandLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR_commandLine.LPAlgorithms
{
    public class CuttingPlaneAlgorithm
    {
        private LinearProgrammingModel _model;
        private InitializeTable _initialTable;
        private List<double[]> _cuts;
        private PrimalSimplex _simplexAlgorithm;

        public CuttingPlaneAlgorithm(LinearProgrammingModel model)
        {
            _model = model;
            _initialTable = new InitializeTable(model);
            _cuts = new List<double[]>();
            _simplexAlgorithm = new PrimalSimplex(model);
        }

        public void RunCuttingPlane()
        {
            bool integerSolutionFound = false;
            int iteration = 0;

            while (!integerSolutionFound)
            {
                iteration++;
                Console.WriteLine($"\nIteration {iteration}:");

                // Initialize and display the tableau
                _initialTable.InitializeTableau();
                PrintTableau("Current Tableau:");

                // Solve the LP relaxation
                var (solution, optimal) = _simplexAlgorithm.Solve();

                if (!optimal)
                {
                    Console.WriteLine("The problem is unbounded or infeasible.");
                    return;
                }

                // Display the solution
                Console.WriteLine("Current Solution:");
                for (int i = 0; i < solution.Length; i++)
                {
                    Console.WriteLine($"x{i + 1} = {solution[i]:F2}");
                }

                // Check if the solution is integer
                integerSolutionFound = IsIntegerSolution(solution);

                if (!integerSolutionFound)
                {
                    Console.WriteLine("Solution is not integer. Adding cutting planes...");

                    // Generate and add cutting plane
                    var cut = GenerateCut(solution);
                    _cuts.Add(cut);
                    AddCutToModel(cut);

                    // Reinitialize the tableau with the new cut
                    _initialTable = new InitializeTable(_model);
                    _simplexAlgorithm = new PrimalSimplex(_model); // Use updated model with cuts
                }
                else
                {
                    Console.WriteLine("Integer solution found!");
                    PrintTableau("Final Optimal Tableau:");
                    Console.WriteLine("Final Optimal Solution:");
                    for (int i = 0; i < solution.Length; i++)
                    {
                        Console.WriteLine($"x{i + 1} = {solution[i]:F2}");
                    }
                }
            }
        }

        private void PrintTableau(string message)
        {
            Console.WriteLine(message);
            _initialTable.PrintTableau();
        }

        private bool IsIntegerSolution(double[] solution)
        {
            return solution.All(val => Math.Abs(val - Math.Round(val)) < 1e-5);
        }

        private double[] GenerateCut(double[] solution)
        {
            // Generate a cutting plane based on the current non-integer solution
            double[] cut = new double[solution.Length + 1]; // +1 for the RHS term
            for (int i = 0; i < solution.Length; i++)
            {
                cut[i] = Math.Floor(solution[i]) - solution[i];
            }
            cut[solution.Length] = -1; // Right-hand side of the cut
            return cut;
        }

        private void AddCutToModel(double[] cut)
        {
            // Add the generated cut to the model constraints
            Console.WriteLine("Adding cut to the model:");
            string cutString = string.Join(" ", cut.Take(cut.Length - 1)) + " <= " + cut.Last();
            Console.WriteLine(cutString);
        }
    }
}