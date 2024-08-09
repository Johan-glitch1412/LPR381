using LPR_commandLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR_commandLine.LPAlgorithms
{

    public class DualSimplex
    {
        private InitializeTable _initializer;
        private LinearProgrammingModel _model;

        public DualSimplex(LinearProgrammingModel model)
        {
            _model = model;
            _initializer = new InitializeTable(model);
        }

        public void RunDualSimplex()
        {
            _initializer.InitializeTableau();
            PrintTableau();

            double[,] tableau = _initializer.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            while (true)
            {
                bool optimal = true;
                int pivotRow = FindPivotRow(tableau, ref optimal);

                if (optimal)
                {
                    Console.WriteLine("Dual Simplex phase completed.");
                    // Switch to Primal Simplex to find the optimal solution
                    RunPrimalSimplex(tableau);
                    break;
                }

                if (pivotRow == -1)
                {
                    Console.WriteLine("No valid pivot row found. The problem might be infeasible.");
                    break;
                }

                int pivotColumn = FindPivotColumn(tableau, pivotRow);

                if (pivotColumn == -1)
                {
                    Console.WriteLine("The problem is infeasible.");
                    break;
                }

                Console.WriteLine($"Pivot Column: x{pivotColumn + 1}");
                Console.WriteLine($"Pivot Row: {pivotRow}");

                Pivot(tableau, pivotRow, pivotColumn);
                PrintTableau();
            }
        }

        private void RunPrimalSimplex(double[,] tableau)
        {
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            while (true)
            {
                int pivotColumn = -1;
                double minValue = 0;

                // Find the pivot column
                for (int j = 0; j < numCols - 1; j++)
                {
                    if (tableau[0, j] < minValue)
                    {
                        minValue = tableau[0, j];
                        pivotColumn = j;
                    }
                }

                if (pivotColumn == -1)
                {
                    Console.WriteLine("Optimal solution found.");
                    PrintSolution(tableau);
                    break;
                }

                double minRatio = double.PositiveInfinity;
                int pivotRow = -1;

                // Find the pivot row
                for (int i = 1; i < numRows; i++)
                {
                    if (tableau[i, pivotColumn] > 0)
                    {
                        double ratio = tableau[i, numCols - 1] / tableau[i, pivotColumn];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1)
                {
                    Console.WriteLine("The problem is unbounded.");
                    break;
                }

                Console.WriteLine($"Pivot Column: x{pivotColumn + 1}");
                Console.WriteLine($"Pivot Row: {pivotRow}");

                Pivot(tableau, pivotRow, pivotColumn);
                PrintTableau();
            }
        }

        private int FindPivotRow(double[,] tableau, ref bool optimal)
        {
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);
            int pivotRow = -1;
            double maxRatio = double.NegativeInfinity;

            for (int i = 1; i < numRows; i++)
            {
                if (tableau[i, numCols - 1] < 0)
                {
                    optimal = false;
                    double ratio = tableau[i, numCols - 1] / tableau[i, numCols - 2];
                    if (ratio > maxRatio)
                    {
                        maxRatio = ratio;
                        pivotRow = i;
                    }
                }
            }

            if (pivotRow < 0 || pivotRow >= numRows)
            {
                Console.WriteLine($"Invalid pivotRow: {pivotRow}");
            }

            return pivotRow;
        }

        private int FindPivotColumn(double[,] tableau, int pivotRow)
        {
            int numCols = tableau.GetLength(1);
            int pivotColumn = -1;
            double minRatio = double.PositiveInfinity;

            for (int j = 0; j < numCols - 1; j++)
            {
                if (tableau[pivotRow, j] < 0)
                {
                    double ratio = tableau[0, j] / tableau[pivotRow, j];
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotColumn = j;
                    }
                }
            }

            if (pivotColumn < 0 || pivotColumn >= numCols)
            {
                Console.WriteLine($"Invalid pivotColumn: {pivotColumn}");
            }

            return pivotColumn;
        }

        private void Pivot(double[,] tableau, int pivotRow, int pivotColumn)
        {
            int numCols = tableau.GetLength(1);
            int numRows = tableau.GetLength(0);

            double pivotElement = tableau[pivotRow, pivotColumn];
            for (int j = 0; j < numCols; j++)
            {
                tableau[pivotRow, j] /= pivotElement;
            }

            for (int i = 0; i < numRows; i++)
            {
                if (i != pivotRow)
                {
                    double factor = tableau[i, pivotColumn];
                    for (int j = 0; j < numCols; j++)
                    {
                        tableau[i, j] -= factor * tableau[pivotRow, j];
                    }
                }
            }
        }

        private void PrintTableau()
        {
            if (_initializer.Tableau == null)
            {
                Console.WriteLine("Tableau not initialized.");
                return;
            }

            Console.WriteLine("Dual Simplex Tableau:");

            int numCols = _initializer.Tableau.GetLength(1);
            int numVariables = _model.Objective.Coefficients.Count;

            Console.Write("        ");
            for (int i = 0; i < numVariables; i++)
            {
                Console.Write($"   x{i + 1}    ");
            }
            for (int i = numVariables; i < numCols - 1; i++)
            {
                Console.Write($"   s/e{i - numVariables + 1}   ");
            }
            Console.WriteLine("   RHS");

            for (int i = 0; i < _initializer.Tableau.GetLength(0); i++)
            {
                Console.Write(i == 0 ? " z     " : $" c{i}   ");

                for (int j = 0; j < numCols; j++)
                {
                    Console.Write($"{_initializer.Tableau[i, j],8:F2} ");
                }
                Console.WriteLine();
            }
        }

        private void PrintSolution(double[,] tableau)
        {
            int numCols = tableau.GetLength(1);
            Console.WriteLine("Optimal Solution:");
            for (int j = 0; j < numCols - 1; j++)
            {
                if (tableau[0, j] == 0)
                {
                    Console.WriteLine($"x{j + 1} = 0");
                }
                else
                {
                    for (int i = 1; i < tableau.GetLength(0); i++)
                    {
                        if (tableau[i, j] == 1)
                        {
                            Console.WriteLine($"x{j + 1} = {tableau[i, numCols - 1]}");
                            break;
                        }
                    }
                }
            }
            Console.WriteLine($"Objective value: {tableau[0, numCols - 1]}");
        }
    }
}