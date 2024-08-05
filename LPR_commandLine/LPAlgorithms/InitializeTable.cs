using LPR_commandLine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace LPR_commandLine.LPAlgorithms
{
    internal class InitializeTable
    {
        private LinearProgrammingModel _model;
        private List<List<double>> _tableau; // Use List for dynamic sizing

        public InitializeTable(LinearProgrammingModel model)
        {
            _model = model;
            _tableau = new List<List<double>>(); // Initialize the list
        }

        public void InitializeTableau()
        {
            int numVariables = _model.Objective.Coefficients.Count;
            int numConstraints = _model.Constraints.Count;

            // Calculate the number of slack/excess variables
            int slackExcessVariables = 0;
            foreach (var constraint in _model.Constraints)
            {
                if (constraint.Relation == "<=" || constraint.Relation == "=")
                {
                    slackExcessVariables++;
                }
                if (constraint.Relation == ">=" || constraint.Relation == "=")
                {
                    slackExcessVariables++;
                }
            }

            // Calculate the number of binary constraints
            int binConstraints = _model.SignRestrictions.Count(r => r == "bin");

            // Calculate the number of rows needed for the binary constraints
            int additionalRows = binConstraints * 2;

            // Initialize the tableau with the required number of rows and columns
            int tableauColumns = numVariables + slackExcessVariables + 1; // Including RHS
            for (int i = 0; i < numConstraints + 1 + additionalRows; i++)
            {
                _tableau.Add(new List<double>(new double[tableauColumns]));
            }

            // Fill the tableau with the objective function
            for (int i = 0; i < numVariables; i++)
            {
                _tableau[0][i] = -_model.Objective.Coefficients[i]; // Negate coefficients for maximization
            }

            // Fill the tableau with constraints and add slack/excess variables
            int slackExcessIndex = numVariables;
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numVariables; j++)
                {
                    _tableau[i + 1][j] = _model.Constraints[i].Coefficients[j];
                }

                if (_model.Constraints[i].Relation == "<=")
                {
                    // Add slack variable
                    _tableau[i + 1][slackExcessIndex++] = 1;
                }
                else if (_model.Constraints[i].Relation == ">=")
                {
                    // Add excess variable and negate the row
                    _tableau[i + 1][slackExcessIndex++] = -1;
                    // Negate the entire row for >= constraints
                    for (int j = 0; j < tableauColumns; j++)
                    {
                        _tableau[i + 1][j] *= -1;
                    }
                    // Negate the RHS value as well
                    _tableau[i + 1][tableauColumns - 1] = -_model.Constraints[i].RHS;
                }
                else if (_model.Constraints[i].Relation == "=")
                {
                    // For equality constraints, add both slack and excess variables
                    _tableau[i + 1][slackExcessIndex] = 1; // Slack variable
                    _tableau[i + 1][slackExcessIndex + 1] = -1; // Excess variable
                    slackExcessIndex += 2; // Move to next available index for slack/excess variables
                }

                // Right-hand side
                if (_model.Constraints[i].Relation != ">=") // RHS already negated for >= constraints
                {
                    _tableau[i + 1][_tableau[i + 1].Count - 1] = _model.Constraints[i].RHS;
                }
            }

            // Add binary and integer constraints if needed
            //AddBinAndIntConstraint();
        }

        public void AddBinAndIntConstraint()
        {
            int numVariables = _model.Objective.Coefficients.Count;
            int numConstraints = _model.Constraints.Count;

            // Add binary constraints
            int newRow = numConstraints + 1;
            for (int i = 0; i < numVariables; i++)
            {
                if (_model.SignRestrictions[i] == "bin")
                {
                    // Ensure we do not exceed the row bounds
                    if (newRow >= _tableau.Count)
                    {
                        throw new IndexOutOfRangeException("new row exceeded the number of allocated rows.");
                    }

                    // Binary variable must be <= 1
                    _tableau[newRow][i] = 1;
                    _tableau[newRow][_tableau[newRow].Count - 1] = 1;
                    newRow++;

                    // Ensure we do not exceed the row bounds
                    if (newRow >= _tableau.Count)
                    {
                        throw new IndexOutOfRangeException("new row exceeded the number of allocated rows.");
                    }

                    // Binary variable must be >= 0
                    _tableau[newRow][i] = -1;
                    _tableau[newRow][_tableau[newRow].Count - 1] = 0;
                    newRow++;
                }
            }
        }

        public void PrintTableau()
        {
            if (_tableau == null || _tableau.Count == 0)
            {
                Console.WriteLine("Tableau not initialized.");
                return;
            }

            Console.WriteLine("Simplex Tableau:");

            int numCols = _tableau[0].Count;
            int numVariables = _model.Objective.Coefficients.Count;

            // Print column headings
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

            // Print the tableau
            for (int i = 0; i < _tableau.Count; i++)
            {
                // Print row heading
                if (i == 0)
                {
                    Console.Write(" z     ");
                }
                else
                {
                    Console.Write($" c{i}   ");
                }

                // Print row values
                for (int j = 0; j < numCols; j++)
                {
                    Console.Write($"{_tableau[i][j],8:F2} ");
                }
                Console.WriteLine();
            }
        }
    }
}