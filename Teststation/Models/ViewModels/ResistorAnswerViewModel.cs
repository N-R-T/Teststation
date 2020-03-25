using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class ResistorAnswerViewModel
    {
        public long Id { get; set; }
        public double CorrectResistance { get; set; }
        public bool Visible { get; set; }
        public long CircuitPartId { get; set; }

        public List<double> FillerValues(double givenAnswer)
        {
            List<double> list = new List<double>();
            Random rand = new Random();
            int minVal = (int)CorrectResistance - 10;
            int maxVal = (int)CorrectResistance + 11;
            if (minVal < 0)
            {
                minVal = 0;
            }
            list.Add(CorrectResistance);
            if (!list.Contains(givenAnswer))
            {
                list.Add(givenAnswer);
            }            

            while (list.Count < 5)
            {
                double value = rand.Next(minVal, maxVal);
                if (rand.Next(0, 101) >= 50)
                {
                    value += rand.NextDouble();
                }

                if (!list.Contains(value) ||
                    (value - 5 >= CorrectResistance && value + 5 <= CorrectResistance))
                {
                    list.Add(value);
                }
            }

            list = list.OrderBy(x => x).ToList();
            return list;
        } 
    }
}
