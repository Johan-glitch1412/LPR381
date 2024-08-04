using LPR_commandLine.FileHandeling;
using LPR_commandLine.LPAlgorithms;
using LPR_commandLine.Models;
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
            public static string inputFile = @"C:\Users\Johan.Schoeman\Downloads\LPR_commandLine\LP.txt";
        static void Main(string[] args)
        {

            LinearProgrammingModel model = FileParser.ParseInputFile(inputFile);
            while (true)
            {
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Load Model from File");
                Console.WriteLine("2. PrimalSimplex");
                Console.WriteLine("3. DualSimplex for Testing");
                Console.WriteLine("4. Perform Sensitivity Analysis");
                Console.WriteLine("5. Exit");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        LoadModel();
                        Console.WriteLine();
                        DisplayTable();
                        Console.WriteLine("InitializeTable");
                        DiplayIntialTable();
                        break;
                    case "2":
                        Console.Clear();
                        PrimalSimplex simplex = new PrimalSimplex(model);
                        simplex.RunSimplex();

                        break;
                    case "3":
                        DualSimplex dsimplex = new DualSimplex(model);
                        dsimplex.RunDualSimplex();
                        // Call sensitivity analysis methods here
                        break;
                    case "4":
                        Console.Write("Enter the path to the output file: ");
                        var outputFile = Console.ReadLine();
                        // Call export method here
                        break;
                    case "5":
                        return;
                    default:
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
                var model = FileParser.ParseInputFile(inputFile);
                Console.WriteLine("Model loaded successfully.");

                // Display the parsed data

                Console.WriteLine("Objective: " + model.Objective.Type);
                Console.Write("z = ");
                int coefficientCount = model.Objective.Coefficients.Count;
                for (int i = 0; i < coefficientCount; i++)
                {
                    // Print coefficient with appropriate formatting
                    if (i > 0)
                    {
                        Console.Write(" + ");
                    }
                    string varName = $"x{i + 1}";
                    Console.Write($"{model.Objective.Coefficients[i]}{varName}");
                }
                Console.WriteLine();

                Console.WriteLine($"Constraints:");
                foreach (var constraint in model.Constraints)
                {
                    // Build the constraint expression
                    string constraintExpression = "";
                    int numCoefficients = constraint.Coefficients.Count;
                    for (int i = 0; i < numCoefficients; i++)
                    {
                        if (i > 0)
                        {
                            constraintExpression += " + ";
                        }
                        constraintExpression += $"{constraint.Coefficients[i]}x{i + 1}";
                    }

                    // Print the formatted constraint
                    Console.WriteLine($"  {constraintExpression} {constraint.Relation} {constraint.RHS}");
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
            if (File.Exists(inputFile))
            {
                var model = FileParser.ParseInputFile(inputFile);
                Console.WriteLine("Model loaded successfully.");

                // Load and display the primal table
                var primalTable = LoadTable(model);
                DisplayTable(primalTable);
            }
            else
            {
                Console.WriteLine("File not found.");
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
            // Define column width for alignment
            const int colWidth = 8;

            // Display headers
            Console.Write("Primal Table:\n");
            Console.Write("".PadRight(colWidth)); // Space for the row headers
            foreach (var varName in primalTable.VariableNames)
            {
                Console.Write($"{varName.PadRight(colWidth)}");
            }
            Console.WriteLine($"{"|".PadLeft(3)} {"RHS".PadRight(colWidth)}");

            // Display objective function
            Console.Write("z".PadRight(colWidth));
            foreach (var coef in primalTable.ObjectiveCoefficients)
            {
                Console.Write($"{coef.ToString().PadRight(colWidth)}");
            }
            Console.WriteLine();

            // Display constraints
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
            // Display sign restrictions
            Console.WriteLine();
            for (int i = 0; i < primalTable.SignRestrictions.Count; i++)
            {
                Console.WriteLine($"{primalTable.VariableNames[i].PadRight(colWidth)}= {primalTable.SignRestrictions[i]}");
            }

        }
        private static void DiplayIntialTable()
        {
            if (File.Exists(inputFile))
            {
                var model = FileParser.ParseInputFile(inputFile);
                Console.WriteLine("Model loaded successfully.");

                // Initialize and print the tableau
                var simplex = new InitializeTable(model);
                simplex.InitializeTableau();
                simplex.PrintTableau();
            }
            else
            {
                Console.WriteLine("File not found.");
            }
        }
    }

}