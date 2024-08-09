using LPR381_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace LPR381_Project.LPAlgorithms
{
    public class InitializeTable
    {
        private LinearProgrammingModel _model;
        public double[,] Tableau { get; private set; }

        public InitializeTable(LinearProgrammingModel model)
        {
            _model = model;
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

            int tableauColumns = numVariables + slackExcessVariables + 1; // Including RHS
            Tableau = new double[numConstraints + 1, tableauColumns];

            // Fill the tableau with the objective function
            for (int i = 0; i < numVariables; i++)
            {
                Tableau[0, i] = -_model.Objective.Coefficients[i]; // Negate coefficients for maximization
            }

            // Fill the tableau with constraints and add slack/excess variables
            int slackExcessIndex = numVariables;
            for (int i = 0; i < numConstraints; i++)
            {
                for (int j = 0; j < numVariables; j++)
                {
                    Tableau[i + 1, j] = _model.Constraints[i].Coefficients[j];
                }

                if (_model.Constraints[i].Relation == "<=")
                {
                    // Add slack variable
                    Tableau[i + 1, slackExcessIndex++] = 1;
                }
                else if (_model.Constraints[i].Relation == ">=")
                {
                    // Add excess variable and negate the row
                    Tableau[i + 1, slackExcessIndex++] = -1;
                    // Negate the entire row for >= constraints
                    for (int j = 0; j < tableauColumns; j++)
                    {
                        Tableau[i + 1, j] *= -1;
                    }
                    // Negate the RHS value as well
                    Tableau[i + 1, tableauColumns - 1] = -_model.Constraints[i].RHS;
                }
                else if (_model.Constraints[i].Relation == "=")
                {
                    // For equality constraints, add both slack and excess variables
                    Tableau[i + 1, slackExcessIndex] = 1; // Slack variable
                    Tableau[i + 1, slackExcessIndex + 1] = -1; // Excess variable
                    slackExcessIndex += 2; // Move to next available index for slack/excess variables
                }

                // Right-hand side
                if (_model.Constraints[i].Relation != ">=") // RHS already negated for >= constraints
                {
                    Tableau[i + 1, Tableau.GetLength(1) - 1] = _model.Constraints[i].RHS;
                }
            }
        }

        public void PrintTableau()
        {
            if (Tableau == null)
            {
                Console.WriteLine("Tableau not initialized.");
                return;
            }

            Console.WriteLine("Simplex Tableau:");

            int numCols = Tableau.GetLength(1);
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
            for (int i = 0; i < Tableau.GetLength(0); i++)
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
                    Console.Write($"{Tableau[i, j],8:F2} ");
                }
                Console.WriteLine();
            }
        }
    }
}