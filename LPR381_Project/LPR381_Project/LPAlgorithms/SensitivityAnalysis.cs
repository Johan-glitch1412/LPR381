using LPR381_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR381_Project.LPAlgorithms
{
    public class SensitivityAnalysis
    {
        private PrimalSimplex _simplexAlgorithm;
        private LinearProgrammingModel _model;
        private LinearProgrammingModel _dualModel;
        private double[] _dualSolution;

        public SensitivityAnalysis(LinearProgrammingModel model)
        {
            _model = model;
            _simplexAlgorithm = new PrimalSimplex(model);
        }

        public void DisplayRangeNonBasicVariable(int variableIndex)
        {
            if (variableIndex < 0 || variableIndex >= _model.NumVariables)
            {
                Console.WriteLine("Invalid variable index. Example: Enter a number between 0 and " + (_model.NumVariables - 1));
                return;
            }

            // Get the initial tableau
            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            // Find the current coefficient of the non-basic variable in the objective function
            double originalCoefficient = tableau[0, variableIndex];

            // Determine the range by checking changes to the variable's coefficient
            double maxChange = double.PositiveInfinity;
            double minChange = double.NegativeInfinity;

            // Analyze each constraint row to determine the feasible range for the non-basic variable
            for (int i = 1; i < numRows; i++)
            {
                double coefficient = tableau[i, variableIndex];
                if (coefficient != 0)
                {
                    double rhs = tableau[i, numCols - 1];
                    double ratio = rhs / coefficient;

                    // Calculate the range of coefficients
                    if (coefficient > 0)
                    {
                        maxChange = Math.Min(maxChange, ratio);
                        minChange = Math.Max(minChange, -ratio);
                    }
                    else
                    {
                        maxChange = Math.Min(maxChange, -ratio);
                        minChange = Math.Max(minChange, ratio);
                    }
                }
            }

            // Display the results
            Console.WriteLine($"Variable x{variableIndex + 1} (Non-Basic):");
            Console.WriteLine($"Original Coefficient in Objective Function: {originalCoefficient}");
            Console.WriteLine($"Range of Coefficient Change:");
            Console.WriteLine($"  Minimum Change: {minChange}");
            Console.WriteLine($"  Maximum Change: {maxChange}");
        }

        public void ApplyChangeNonBasicVariable(int variableIndex, double newValue)
        {
            // Validate variableIndex
            if (variableIndex < 0 || variableIndex >= _model.NumVariables)
            {
                Console.WriteLine("Invalid variable index. Example: Enter a number between 0 and " + (_model.NumVariables - 1));
                return;
            }

            // Validate newValue
            if (double.IsNaN(newValue) || double.IsInfinity(newValue))
            {
                Console.WriteLine("Invalid value. Example: Enter a valid number.");
                return;
            }

            // Get the initial tableau
            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            // Modify the coefficient in the objective function
            tableau[0, variableIndex] = newValue;

            // Optionally, solve the LP again or update the tableau
            // For simplicity, this example does not solve again, but you can integrate with the simplex method
            _model.Tableau = tableau;

            // Display the updated tableau
            Console.WriteLine("Updated Tableau after changing non-basic variable:");
            PrintTableau();
        }

        private void PrintTableau()
        {
            // Implement this method to print the current state of the tableau
            // Example implementation:
            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            Console.WriteLine("Simplex Tableau:");
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    Console.Write($"{tableau[i, j],10:F2} ");
                }
                Console.WriteLine();
            }
        }

        public void DisplayRangeBasicVariable(int variableIndex)
        {
            // Validate variableIndex
            if (variableIndex < 0 || variableIndex >= _model.NumVariables)
            {
                Console.WriteLine("Invalid variable index. Example: Enter a number between 0 and " + (_model.NumVariables - 1));
                return;
            }


            // Get the initial tableau
            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            // Determine the range for the basic variable by solving the tableau
            // This requires checking how changes in the basic variable's coefficient impact feasibility

            double originalValue = tableau[variableIndex + 1, numCols - 1]; // RHS of the basic variable row

            // Calculate how the basic variable's coefficient range affects feasibility
            double maxChange = double.PositiveInfinity;
            double minChange = double.NegativeInfinity;

            for (int i = 1; i < numRows; i++)
            {
                if (i != variableIndex + 1) // Skip the basic variable's own row
                {
                    double ratio = tableau[i, numCols - 1] / tableau[i, variableIndex];
                    if (ratio > 0)
                    {
                        maxChange = Math.Min(maxChange, ratio);
                        minChange = Math.Max(minChange, -ratio);
                    }
                }
            }

            // Display the results
            Console.WriteLine($"Basic Variable x{variableIndex + 1}:");
            Console.WriteLine($"Original Value: {originalValue}");
            Console.WriteLine($"Range of Value Change:");
            Console.WriteLine($"  Minimum Change: {minChange}");
            Console.WriteLine($"  Maximum Change: {maxChange}");
        }

        public void ApplyChangeBasicVariable(int variableIndex, double newValue)
        {
            // Validate variableIndex
            if (variableIndex < 0 || variableIndex >= _model.NumVariables)
            {
                Console.WriteLine("Invalid variable index. Example: Enter a number between 0 and " + (_model.NumVariables - 1));
                return;
            }

            // Validate newValue
            if (double.IsNaN(newValue) || double.IsInfinity(newValue))
            {
                Console.WriteLine("Invalid value. Example: Enter a valid number.");
                return;
            }

            // Get the initial tableau
            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            // Modify the basic variable's value in the tableau
            tableau[variableIndex + 1, numCols - 1] = newValue; // Update RHS for the basic variable's row

            // Optionally, solve the LP again or update the tableau
            // For simplicity, this example does not solve again, but you can integrate with the simplex method
            _model.Tableau = tableau;

            // Display the updated tableau
            Console.WriteLine("Updated Tableau after changing basic variable:");
            PrintTableau();
        }


        public void DisplayRangeConstraintRHS(int constraintIndex)
        {
            // Validate constraintIndex
            if (constraintIndex < 0 || constraintIndex >= _model.NumConstraints)
            {
                Console.WriteLine("Invalid constraint index. Example: Enter a number between 0 and " + (_model.NumConstraints - 1));
                return;
            }

            // Get the initial tableau
            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            // Determine the range for the constraint RHS value
            double originalValue = tableau[constraintIndex + 1, numCols - 1]; // RHS of the constraint row

            double maxChange = double.PositiveInfinity;
            double minChange = double.NegativeInfinity;

            for (int i = 1; i < numRows; i++)
            {
                if (i != constraintIndex + 1) // Skip the constraint's own row
                {
                    double ratio = tableau[i, numCols - 1] / tableau[i, constraintIndex];
                    if (ratio > 0)
                    {
                        maxChange = Math.Min(maxChange, ratio);
                        minChange = Math.Max(minChange, -ratio);
                    }
                }
            }

            // Display the results
            Console.WriteLine($"Constraint {constraintIndex + 1}:");
            Console.WriteLine($"Original RHS Value: {originalValue}");
            Console.WriteLine($"Range of RHS Value Change:");
            Console.WriteLine($"  Minimum Change: {minChange}");
            Console.WriteLine($"  Maximum Change: {maxChange}");
        }


        public void ApplyChangeConstraintRHS(int constraintIndex, double newValue)
            {
                // Validate constraintIndex
                if (constraintIndex < 0 || constraintIndex >= _model.NumConstraints)
                {
                    Console.WriteLine("Invalid constraint index. Example: Enter a number between 0 and " + (_model.NumConstraints - 1));
                    return;
                }

                // Validate newValue
                if (double.IsNaN(newValue) || double.IsInfinity(newValue))
                {
                    Console.WriteLine("Invalid value. Example: Enter a valid number.");
                    return;
                }
            // Get the initial tableau
            double[,] tableau = _model.Tableau;
                int numCols = tableau.GetLength(1);

                // Modify the RHS value for the given constraint
                tableau[constraintIndex + 1, numCols - 1] = newValue; // Update RHS

                // Optionally, resolve the LP again or adjust the tableau
                // For simplicity, this example does not solve again but integrates with the simplex method
                _model.Tableau = tableau;

                // Display the updated tableau
                Console.WriteLine("Updated Tableau after changing constraint RHS:");
                PrintTableau();
            }


        public void DisplayRangeNonBasicVariable(int variableIndex)
        {
            if (variableIndex < 0 || variableIndex >= _model.NumVariables)
            {
                Console.WriteLine("Invalid variable index.");
                return;
            }

            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            Console.WriteLine($"Tableau Dimensions: {numRows}x{numCols}");
            Console.WriteLine($"Variable Index: {variableIndex}");

            double originalCoefficient = tableau[0, variableIndex];
            double maxChange = double.PositiveInfinity;
            double minChange = double.NegativeInfinity;

            for (int i = 1; i < numRows; i++)
            {
                double coefficient = tableau[i, variableIndex];
                if (coefficient != 0)
                {
                    double rhs = tableau[i, numCols - 1];
                    double ratio = rhs / coefficient;

                    if (coefficient > 0)
                    {
                        maxChange = Math.Min(maxChange, ratio);
                        minChange = Math.Max(minChange, -ratio);
                    }
                    else
                    {
                        maxChange = Math.Min(maxChange, -ratio);
                        minChange = Math.Max(minChange, ratio);
                    }
                }
            }

            Console.WriteLine($"Original Coefficient: {originalCoefficient}");
            Console.WriteLine($"Range of Coefficient Change: Min = {minChange}, Max = {maxChange}");
        }


        public void ApplyChangeVariableInNonBasicColumn(int columnIndex, double newValue)
        {
            // Validate columnIndex
            if (columnIndex < 0 || columnIndex >= _model.NumVariables)
            {
                Console.WriteLine("Invalid column index. Example: Enter a number between 0 and " + (_model.NumVariables - 1));
                return;
            }

            // Validate newValue
            if (double.IsNaN(newValue) || double.IsInfinity(newValue))
            {
                Console.WriteLine("Invalid value. Example: Enter a valid number.");
                return;
            }

            // Get the initial tableau
            double[,] tableau = _model.Tableau;
            int numRows = tableau.GetLength(0);
            int numCols = tableau.GetLength(1);

            // Apply the change to the variable in the specified column
            for (int i = 0; i < numRows; i++)
            {
                tableau[i, columnIndex] = newValue;
            }

            // Optionally, resolve the LP again or adjust the tableau
            // For simplicity, this example does not solve again but integrates with the simplex method
            _model.Tableau = tableau;

            // Display the updated tableau
            Console.WriteLine("Updated Tableau after changing variable in non-basic column:");
            PrintTableau();
        }

        public void AddNewActivity(double[] newActivityCoefficients)
        {
            // Validate newActivityCoefficients
            if (newActivityCoefficients.Length != _model.NumConstraints)
            {
                Console.WriteLine("Invalid number of coefficients. Example: Enter an array with " + _model.NumConstraints + " elements.");
                return;
            }

            if (newActivityCoefficients.Any(c => double.IsNaN(c) || double.IsInfinity(c)))
            {
                Console.WriteLine("Invalid coefficient value. Example: Enter valid numbers for all coefficients.");
                return;
            }

            // Extend the tableau and coefficients list
            int numRows = _model.Tableau.GetLength(0);
            int numCols = _model.Tableau.GetLength(1);

            // Create a new tableau with additional column
            double[,] newTableau = new double[numRows, numCols + 1];

            // Copy the existing tableau
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    newTableau[i, j] = _model.Tableau[i, j];
                }
            }

            // Add new activity coefficients to the new column
            for (int i = 0; i < numRows; i++)
            {
                newTableau[i, numCols] = (i < _model.NumConstraints) ? newActivityCoefficients[i] : 0;
            }

            // Update the model's tableau
            _model.Tableau = newTableau;

            // Update the objective function
            _model.Objective.Coefficients.Add(0); // Add 0 coefficient for the new activity

            // Update the constraints (new variable is initially not part of constraints)
            _model.Constraints.Add(item: new Constraint { Coefficients = new List<double>(newActivityCoefficients), Relation = "<=", RHS = 0 });

            // Display the updated tableau
            Console.WriteLine("Updated Tableau after adding new activity:");
            PrintTableau();
        }

        public void EnsureTableauSize(int numConstraints, int numVariables)
        {
            if (_model.Tableau == null ||
                _model.Tableau.GetLength(0) != numConstraints ||
                _model.Tableau.GetLength(1) != numVariables + 1) // +1 for RHS
            {
                // Create a new tableau with the new dimensions
                double[,] newTableau = new double[numConstraints, numVariables + 1];

                // Copy existing data if available
                if (_model.Tableau != null)
                {
                    int oldRows = _model.Tableau.GetLength(0);
                    int oldCols = _model.Tableau.GetLength(1);

                    for (int i = 0; i < Math.Min(numConstraints, oldRows); i++)
                    {
                        for (int j = 0; j < Math.Min(numVariables + 1, oldCols); j++)
                        {
                            newTableau[i, j] = _model.Tableau[i, j];
                        }
                    }
                }

                _model.Tableau = newTableau;
            }
        }
        public void AddNewConstraint(Constraint newConstraint)
        {
            // Ensure newConstraint coefficients match the number of variables
            if (newConstraint.Coefficients.Count != _model.NumVariables)
            {
                Console.WriteLine("Invalid number of coefficients for new constraint.");
                return;
            }

            // Ensure tableau size
            EnsureTableauSize(_model.NumConstraints + 1, _model.NumVariables);

            // Add the new constraint to the tableau
            int numRows = _model.Tableau.GetLength(0);
            int numCols = _model.Tableau.GetLength(1);

            // Create a new tableau with an additional row
            double[,] newTableau = new double[numRows + 1, numCols];

            // Copy the existing tableau
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    newTableau[i, j] = _model.Tableau[i, j];
                }
            }

            // Add the new constraint to the new row
            for (int j = 0; j < numCols - 1; j++)
            {
                newTableau[numRows, j] = newConstraint.Coefficients[j];
            }
            newTableau[numRows, numCols - 1] = newConstraint.RHS; // Add RHS value

            // Update the model's tableau
            _model.Tableau = newTableau;

            // Add the new constraint to the model's constraints list
            _model.Constraints.Add(newConstraint);

            // Update the model's sign restrictions
            _model.SignRestrictions.Add(newConstraint.Relation);

            // Display the updated tableau
            Console.WriteLine("Updated Tableau after adding new constraint:");
            PrintTableau();
        }



        public void DisplayShadowPrices()
        {
            var shadowPrices = _simplexAlgorithm.CalculateShadowPrices();
            Console.WriteLine("Shadow Prices:");
            for (int i = 0; i < shadowPrices.Length; i++)
            {
                Console.WriteLine($"Constraint {i + 1} shadow price = {shadowPrices[i]:F2}");
            }
        }

        public void ApplyDuality()
        {
            // Create a new LinearProgrammingModel for the dual problem
            LinearProgrammingModel dualModel = new LinearProgrammingModel();

            // Set the objective function type for the dual model
            dualModel.Objective.Type = _model.Objective.Type == "Maximize" ? "Minimize" : "Maximize";

            // Set the objective function coefficients for the dual model
            dualModel.Objective.Coefficients = _model.Constraints.Select(c => c.RHS).ToList();

            // Ensure the number of variables and constraints match
            if (_model.NumVariables != _model.Objective.Coefficients.Count)
            {
                Console.WriteLine("Mismatch between number of variables and objective coefficients.");
                return;
            }
            if (_model.NumVariables != _model.SignRestrictions.Count)
            {
                Console.WriteLine("Mismatch between number of variables and sign restrictions.");
                return;
            }

            // Set the constraints for the dual model
            for (int i = 0; i < _model.NumVariables; i++)
            {
                // Check if coefficients and RHS are in range
                if (i >= _model.Constraints.Count || i >= _model.Objective.Coefficients.Count)
                {
                    Console.WriteLine("Index out of range while setting dual constraints.");
                    return;
                }

                var constraint = new Constraint
                {
                    Coefficients = _model.Constraints.Select(c => c.Coefficients[i]).ToList(),
                    RHS = _model.Objective.Coefficients[i],
                    Relation = _model.SignRestrictions[i] == ">=" ? "<=" : ">="
                };
                dualModel.Constraints.Add(constraint);
            }

            // Set the sign restrictions for the dual model
            dualModel.SignRestrictions = _model.Constraints.Select(c => "<=").ToList();

            // Display or store the dual model
            Console.WriteLine("Dual problem formulated.");
            PrintModel(dualModel); // Helper function to display the model
        }


        private void PrintModel(LinearProgrammingModel model)
        {
            Console.WriteLine($"Objective: {model.Objective.Type}");
            for (int i = 0; i < model.Objective.Coefficients.Count; i++)
            {
                Console.Write($"x{i + 1}: {model.Objective.Coefficients[i]} ");
            }
            Console.WriteLine("\nConstraints:");
            for (int i = 0; i < model.Constraints.Count; i++)
            {
                Console.WriteLine($"{string.Join(" ", model.Constraints[i].Coefficients)} {model.Constraints[i].Relation} {model.Constraints[i].RHS}");
            }
        }


        public void SolveDualProgrammingModel()
        {
            // Apply duality to create the dual model
            ApplyDuality();

            // Now, use the simplex method or another LP solver to solve the dual problem
            PrimalSimplex dualSimplex = new PrimalSimplex(_dualModel); // Correctly pass the dual model
            var (solution, optimal) = dualSimplex.Solve();

            if (optimal)
            {
                _dualSolution = solution; // Store the solution for duality verification
                Console.WriteLine("Optimal solution for the dual problem:");
                for (int i = 0; i < solution.Length; i++)
                {
                    Console.WriteLine($"y{i + 1} = {solution[i]:F2}");
                }
            }
            else
            {
                Console.WriteLine("Dual problem is unbounded or infeasible.");
            }
        }

        public void VerifyDuality()
        {
            // Solve the primal problem
            var (primalSolution, primalOptimal) = _simplexAlgorithm.Solve();

            // Solve the dual problem and store the solution
            SolveDualProgrammingModel();

            // Compare the objective values
            double primalObjectiveValue = _model.Objective.Coefficients.Zip(primalSolution, (c, x) => c * x).Sum();
            double dualObjectiveValue = _dualModel.Objective.Coefficients.Zip(_dualSolution, (c, y) => c * y).Sum();

            Console.WriteLine($"Primal Objective Value: {primalObjectiveValue}");
            Console.WriteLine($"Dual Objective Value: {dualObjectiveValue}");

            if (Math.Abs(primalObjectiveValue - dualObjectiveValue) < 1e-5)
            {
                Console.WriteLine("Strong Duality holds. Primal and Dual have equal objective values.");
            }
            else if (primalObjectiveValue > dualObjectiveValue)
            {
                Console.WriteLine("Weak Duality holds. Primal objective is greater than Dual.");
            }
            else
            {
                Console.WriteLine("Duality condition violated.");
            }
        }

    }
}

