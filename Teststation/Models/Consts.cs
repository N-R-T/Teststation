using System.Collections.Generic;

namespace Teststation.Models
{
    public static class Consts
    {
        public const string goodGrade = "Bestanden";
        public const string badGrade = "Nicht bestanden";
        public const int neededPercentage = 60;
        public const long backUpTestId = 10;
        public const string fillerNameForNewTest = "Neuer Test";
        public const double resultIfEvaluationHasErrors = 666.66;
        public const int minimalPasswordLength = 5;
        public const string quoteUserName = "@@@";

        public static List<(double Resistance, string Label)> standardResistances = new List<(double, string)>()
        {
            (1, "1 Ω"),
            (2, "2 Ω"),
            (3, "3 Ω"),
            (4, "4 Ω"),
            (5, "5 Ω"),
            (6, "6 Ω"),
            (7, "7 Ω"),
            (8, "8 Ω"),
            (9, "9 Ω"),
            (10, "10 Ω"),
        };
    }
}
