using LPR_commandLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR_commandLine.LPAlgorithms
{
    internal class PrimalSimplex
    {
        private InitializeTable _initializer;
        public PrimalSimplex(LinearProgrammingModel model)
        {
            _initializer = new InitializeTable(model);
        }

        public void RunSimplex()
        {
            _initializer.InitializeTableau();
            _initializer.PrintTableau();

            double[,] tableau = _initializer.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            while (true)
            {
                // Check if the solution is optimal
                bool optimal = true;
                int pivotColumn = -1;
                for (int j = 0; j < numCols - 1; j++)
                {
                    if (tableau[0, j] < 0) // For maximization, we look for negative values in the objective row
                    {
                        optimal = false;
                        pivotColumn = j;
                        break;
                    }
                }

                if (optimal)
                {
                    Console.WriteLine("Optimal solution found.");
                    break;
                }

                // Find the pivot row
                double minRatio = double.PositiveInfinity;
                int pivotRow = -1;
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

                // Perform the pivot operation
                Pivot(tableau, pivotRow, pivotColumn);
                _initializer.PrintTableau();
            }
        }
        private void Pivot(double[,] tableau, int pivotRow, int pivotColumn)
        {
            int numCols = tableau.GetLength(1);
            int numRows = tableau.GetLength(0);

            // Normalize the pivot row
            double pivotElement = tableau[pivotRow, pivotColumn];
            for (int j = 0; j < numCols; j++)
            {
                tableau[pivotRow, j] /= pivotElement;
            }

            // Update the other rows
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

    }
}
