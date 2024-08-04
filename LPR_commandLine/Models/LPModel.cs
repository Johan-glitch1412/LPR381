using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LPR_commandLine.Models
{
    public class LinearProgrammingModel
    {
        public ObjectiveFunction Objective { get; set; }
        public List<Constraint> Constraints { get; set; }
        public List<string> SignRestrictions { get; set; }
        public double[,] Tableau { get; set; }
        public int NumVariables => Objective.Coefficients.Count;
        public int NumConstraints => Constraints.Count;

        public LinearProgrammingModel()
        {
            Constraints = new List<Constraint>();
            SignRestrictions = new List<string>();

        }
    }

    public class ObjectiveFunction
    {
        public string Type { get; set; }
        public List<double> Coefficients { get; set; }

        public ObjectiveFunction()
        {
            Coefficients = new List<double>();
        }
    }

    public class Constraint
    {
        public List<double> Coefficients { get; set; }
        public string Relation { get; set; }
        public double RHS { get; set; }

        public Constraint()
        {
            Coefficients = new List<double>();
        }
    }

    public class Table
    {
        public List<string> VariableNames { get; set; } = new List<string>();
        public List<double> ObjectiveCoefficients { get; set; } = new List<double>();
        public List<List<double>> ConstraintCoefficients { get; set; } = new List<List<double>>();
        public List<string> Relations { get; set; } = new List<string>();
        public List<double> RHS { get; set; } = new List<double>();
        public List<string> SignRestrictions { get; set; } = new List<string>();

        public Table()
        {
            VariableNames = new List<string>();
            ObjectiveCoefficients = new List<double>();
            ConstraintCoefficients = new List<List<double>>();
            Relations = new List<string>();
            RHS = new List<double>();
            SignRestrictions = new List<string>();
        }
    }
}
