using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class CircuitPartAnswerViewModel
    {
        public long Id { get; set; }
        public double Resistance { get; set; }
        public double NeededCurrent { get; set; }
        public int Position { get; set; }
        public long QuestionId { get; set; }

        public double GivenResistance { get; set; }

        public ResistorAnswerViewModel Resistor1 { get; set; }
        public ResistorAnswerViewModel Resistor2 { get; set; }
        public ResistorAnswerViewModel Resistor3 { get; set; }
        public List<ResistorAnswerViewModel> Resistors()
        {
            return new List<ResistorAnswerViewModel> { Resistor1, Resistor2, Resistor3 };
        }
    }
}
