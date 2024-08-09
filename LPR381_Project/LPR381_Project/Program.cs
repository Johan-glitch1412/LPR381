using LPR381_Project;
using LPR381_Project.FileHandeling;
using LPR381_Project.Models;
using LPR381_Project.LPAlgorithms;
using static LPR381_Project.Models.Constraint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LPR381_Project
{

    internal class Program
    {
        public static string inputFile = @"C:\Users\walte\OneDrive - belgiumcampus.ac.za\STUDIES\3ThirdYear\LPR381 LinearPrograming\LPR381\LPR381_Project\LPR381_Project\LP.txt";
        public static LinearProgrammingModel model;
        private static SensitivityAnalysis sensitivityAnalysis;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Load Model from File");
                Console.WriteLine("2. PrimalSimplex/DualSimplex");
                Console.WriteLine("3. Perform Sensitivity Analysis");
                Console.WriteLine("4. Branch and Bound");
                Console.WriteLine("5. Knapsack Branch and Bound");
                Console.WriteLine("6. Cutting Plane Algorithm");
                Console.WriteLine("7. Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        LoadModel();
                        DisplayTable();
                        DisplayInitialTable();
                        break;
                    case "2":
                        Console.Clear();
                        RunSimplexBasedOnModel();
                        break;
                    case "3":
                        Console.Clear();
                        PerformSensitivityAnalysis();
                        break;
                    case "4":
                        Console.Clear();
                        RunBranchAndBound();
                        break;
                    case "5":
                        Console.Clear();
                        RunKnapsackBranchAndBound();
                        break;
                    case "6":
                        Console.Clear();
                        RunCuttingPlaneAlgorithm();
                        break;
                    case "7":
                        return;
                    default:
                        Console.Clear();
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
        }

        private static void LoadModel()
        {
            Console.Clear();
            if (File.Exists(inputFile))
            {
                model = FileParser.ParseInputFile(inputFile);
                Console.WriteLine("Model loaded successfully.");
                sensitivityAnalysis = new SensitivityAnalysis(model);

                // Display the parsed data
                Console.WriteLine("Objective: " + model.Objective.Type);
                Console.Write("z = ");
                for (int i = 0; i < model.Objective.Coefficients.Count; i++)
                {
                    if (i > 0)
                    {
                        Console.Write(" + ");
                    }
                    Console.Write($"{model.Objective.Coefficients[i]}x{i + 1}");
                }
                Console.WriteLine();

                Console.WriteLine("Constraints:");
                foreach (var constraint in model.Constraints)
                {
                    string constraintExpression = "";
                    for (int i = 0; i < constraint.Coefficients.Count; i++)
                    {
                        if (i > 0)
                        {
                            constraintExpression += " + ";
                        }
                        constraintExpression += $"{constraint.Coefficients[i]}x{i + 1}";
                    }
                    Console.WriteLine($"{constraintExpression} {constraint.Relation} {constraint.RHS}");
                }

                Console.WriteLine("Sign Restrictions: " + string.Join(", ", model.SignRestrictions));
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("File not found.");
            }
        }

        private static void DisplayTable()
        {
            if (model != null)
            {
                var primalTable = LoadTable(model);
                DisplayTable(primalTable);
            }
            else
            {
                Console.WriteLine("Model not loaded.");
            }
            Console.WriteLine();
        }

        private static Table LoadTable(LinearProgrammingModel model)
        {
            Table primalTable = new Table();
            int numVariables = model.Objective.Coefficients.Count;

            // Generate variable names
            for (int i = 0; i < numVariables; i++)
            {
                primalTable.VariableNames.Add($"x{i + 1}");
            }

            // Add objective function coefficients
            primalTable.ObjectiveCoefficients.AddRange(model.Objective.Coefficients);

            // Add constraints
            foreach (var constraint in model.Constraints)
            {
                primalTable.ConstraintCoefficients.Add(constraint.Coefficients);
                primalTable.Relations.Add(constraint.Relation);
                primalTable.RHS.Add(constraint.RHS);
            }
            primalTable.SignRestrictions.AddRange(model.SignRestrictions);

            return primalTable;
        }

        private static void DisplayTable(Table primalTable)
        {
            const int colWidth = 8;

            Console.WriteLine("Primal Table:");
            Console.Write("".PadRight(colWidth));
            foreach (var varName in primalTable.VariableNames)
            {
                Console.Write($"{varName.PadRight(colWidth)}");
            }
            Console.WriteLine($"{"|".PadLeft(3)} {"RHS".PadRight(colWidth)}");

            Console.Write("z".PadRight(colWidth));
            foreach (var coef in primalTable.ObjectiveCoefficients)
            {
                Console.Write($"{coef.ToString().PadRight(colWidth)}");
            }
            Console.WriteLine();

            for (int i = 0; i < primalTable.ConstraintCoefficients.Count; i++)
            {
                Console.Write($"c{i + 1}".PadRight(colWidth));
                var constraintCoefficients = primalTable.ConstraintCoefficients[i];
                foreach (var coef in constraintCoefficients)
                {
                    Console.Write($"{coef.ToString().PadRight(colWidth)}");
                }
                Console.WriteLine($"{"|".PadLeft(3)} {primalTable.RHS[i].ToString().PadRight(colWidth)}");
            }

            Console.WriteLine();
            for (int i = 0; i < primalTable.SignRestrictions.Count; i++)
            {
                Console.WriteLine($"{primalTable.VariableNames[i].PadRight(colWidth)}= {primalTable.SignRestrictions[i]}");
            }
        }

        private static void DisplayInitialTable()
        {
            if (model != null)
            {
                var simplex = new InitializeTable(model);
                simplex.InitializeTableau();
                simplex.PrintTableau();
            }
            else
            {
                Console.WriteLine("Model not loaded.");
            }
        }
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        private static void RunSimplexBasedOnModel()
        {
            if (model != null)
            {
                // Initialize the tableau
                InitializeTable simplexTable = new InitializeTable(model);
                simplexTable.InitializeTableau();

                // Debugging: Print the entire tableau to ensure correct setup
                Console.WriteLine("Initial Tableau:");
                simplexTable.PrintTableau();

                // Extract the RHS column for checking
                var rhsValues = new List<double>();
                for (int i = 0; i < simplexTable.Tableau.GetLength(0); i++)
                {
                    rhsValues.Add(simplexTable.Tableau[i, simplexTable.Tableau.GetLength(1) - 1]);
                }

                // Debugging: Print RHS values
                Console.WriteLine("RHS Values:");
                foreach (var value in rhsValues)
                {
                    Console.WriteLine(value);
                }

                // Check the RHS values to determine if Dual Simplex is needed
                bool useDualSimplex = rhsValues.Any(value => value < 0);

                // Debugging: Print whether Dual Simplex will be used
                Console.WriteLine($"Use Dual Simplex: {useDualSimplex}");

                if (useDualSimplex)
                {
                    Console.WriteLine("Running Dual Simplex...");
                    DualSimplex dsimplex = new DualSimplex(model);
                    dsimplex.RunDualSimplex();
                }
                else
                {
                    Console.WriteLine("Running Primal Simplex...");
                    PrimalSimplex simplex = new PrimalSimplex(model);
                    simplex.Solve();
                }
            }
            else
            {
                Console.WriteLine("Model not loaded.");
            }
        }
        private static void RunBranchAndBound()
        {
            if (model != null)
            {
                Console.WriteLine("Running Branch and Bound...");
                BranchAndBoundAlgorithm branchAndBound = new BranchAndBoundAlgorithm(model);
                branchAndBound.RunBranchAndBound();
            }
            else
            {
                Console.WriteLine("Model not loaded.");
            }
        }

        private static void RunKnapsackBranchAndBound()
        {
            if (model != null)
            {
                Console.WriteLine("Running Knapsack Branch and Bound...");

            }
            else
            {
                Console.WriteLine("Model not loaded.");
            }
        }

        private static void RunCuttingPlaneAlgorithm()
        {
            if (model != null)
            {
                Console.WriteLine("Running Cutting Plane Algorithm...");
                CuttingPlaneAlgorithm cuttingPlane = new CuttingPlaneAlgorithm(model);
                cuttingPlane.RunCuttingPlane();
            }
            else
            {
                Console.WriteLine("Model not loaded.");
            }
        }
        private static void PerformSensitivityAnalysis()
        {
            if (model != null)
            {
                sensitivityAnalysis = new SensitivityAnalysis(model); // Ensure sensitivityAnalysis is initialized

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Performing Sensitivity Analysis...");
                    Console.WriteLine("Sensitivity Analysis Menu:");
                    Console.WriteLine("1. Display Range of a Non-Basic Variable");
                    Console.WriteLine("2. Apply Change to a Non-Basic Variable");
                    Console.WriteLine("3. Display Range of a Basic Variable");
                    Console.WriteLine("4. Apply Change to a Basic Variable");
                    Console.WriteLine("5. Display Range of a Constraint RHS");
                    Console.WriteLine("6. Apply Change to a Constraint RHS");
                    Console.WriteLine("7. Display Range of a Variable in a Non-Basic Column");
                    Console.WriteLine("8. Apply Change to a Variable in a Non-Basic Column");
                    Console.WriteLine("9. Add New Activity");
                    Console.WriteLine("10. Add New Constraint");
                    Console.WriteLine("11. Display Shadow Prices");
                    Console.WriteLine("12. Apply Duality");
                    Console.WriteLine("13. Solve Dual Programming Model");
                    Console.WriteLine("14. Verify Strong or Weak Duality");
                    Console.WriteLine("15. Return to Main Menu");

                    var choice = Console.ReadLine();

                    switch (choice)
                    {
                        case "1":
                            DisplayRangeOfNonBasicVariable();
                            break;
                        case "2":
                            ApplyChangeToNonBasicVariable();
                            break;
                        case "3":
                            DisplayRangeOfBasicVariable();
                            break;
                        case "4":
                            ApplyChangeToBasicVariable();
                            break;
                        case "5":
                            DisplayRangeOfConstraintRHS();
                            break;
                        case "6":
                            ApplyChangeToConstraintRHS();
                            break;
                        case "7":
                            DisplayRangeOfVariableInNonBasicColumn();
                            break;
                        case "8":
                            ApplyChangeToVariableInNonBasicColumn();
                            break;
                        case "9":
                            AddNewActivity();
                            break;
                        case "10":
                            AddNewConstraint();
                            break;
                        case "11":
                            DisplayShadowPrices();
                            break;
                        case "12":
                            ApplyDuality();
                            break;
                        case "13":
                            SolveDualProgrammingModel();
                            break;
                        case "14":
                            VerifyDuality();
                            break;
                        case "15":
                            return; // Return to the main menu
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }

                    Console.WriteLine("Press any key to return to the Sensitivity Analysis Menu...");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("Model not loaded.");
            }
        }


    private static void DisplayRangeOfNonBasicVariable()
        {
            Console.WriteLine("Enter the index of the non-basic variable:");
            int index = int.Parse(Console.ReadLine());
            sensitivityAnalysis.DisplayRangeNonBasicVariable(index);
        }

        private static void ApplyChangeToNonBasicVariable()
        {
            Console.WriteLine("Enter the index of the non-basic variable:");
            int index = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the new value:");
            double newValue = double.Parse(Console.ReadLine());
            sensitivityAnalysis.ApplyChangeNonBasicVariable(index, newValue);
        }

        private static void DisplayRangeOfBasicVariable()
        {
            Console.WriteLine("Enter the index of the basic variable:");
            int index = int.Parse(Console.ReadLine());
            sensitivityAnalysis.DisplayRangeBasicVariable(index);
        }

        private static void ApplyChangeToBasicVariable()
        {
            Console.WriteLine("Enter the index of the basic variable:");
            int index = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the new value:");
            double newValue = double.Parse(Console.ReadLine());
            sensitivityAnalysis.ApplyChangeBasicVariable(index, newValue);
        }

        private static void DisplayRangeOfConstraintRHS()
        {
            Console.WriteLine("Enter the index of the constraint:");
            int index = int.Parse(Console.ReadLine());
            sensitivityAnalysis.DisplayRangeConstraintRHS(index);
        }

        private static void ApplyChangeToConstraintRHS()
        {
            Console.WriteLine("Enter the index of the constraint:");
            int index = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the new RHS value:");
            double newValue = double.Parse(Console.ReadLine());
            sensitivityAnalysis.ApplyChangeConstraintRHS(index, newValue);
        }

        private static void DisplayRangeOfVariableInNonBasicColumn()
        {
            Console.WriteLine("Enter the index of the non-basic column:");
            int index = int.Parse(Console.ReadLine());
            sensitivityAnalysis.DisplayRangeVariableInNonBasicColumn(index);
        }

        private static void ApplyChangeToVariableInNonBasicColumn()
        {
            Console.WriteLine("Enter the index of the non-basic column:");
            int index = int.Parse(Console.ReadLine());
            Console.WriteLine("Enter the new value:");
            double newValue = double.Parse(Console.ReadLine());
            sensitivityAnalysis.ApplyChangeVariableInNonBasicColumn(index, newValue);
        }

        private static void AddNewActivity()
        {
            Console.WriteLine("Enter the coefficients of the new activity (comma separated):");
            var coefficients = Console.ReadLine().Split(',').Select(double.Parse).ToArray();
            sensitivityAnalysis.AddNewActivity(coefficients);
        }

        private static void AddNewConstraint()
        {
            Console.WriteLine("Enter the coefficients of the new constraint (comma separated):");
            var coefficients = Console.ReadLine().Split(',').Select(double.Parse).ToList();
            Console.WriteLine("Enter the relation (<=, =, >=):");
            string relation = Console.ReadLine();
            Console.WriteLine("Enter the RHS value:");
            double rhs = double.Parse(Console.ReadLine());
            var newConstraint = new Constraint(coefficients, relation, rhs);
            sensitivityAnalysis.AddNewConstraint(newConstraint);
        }

        private static void DisplayShadowPrices()
        {
            sensitivityAnalysis.DisplayShadowPrices();
        }

        private static void ApplyDuality()
        {
            sensitivityAnalysis.ApplyDuality();
        }

        private static void SolveDualProgrammingModel()
        {
            sensitivityAnalysis.SolveDualProgrammingModel();
        }

        private static void VerifyDuality()
        {
            sensitivityAnalysis.VerifyDuality();
        }
    }
}