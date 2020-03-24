using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public sealed class CircuitAnswer : Answer
    {
        public double GivenResistance { get; set; }
        public long ResistorId { get; set; }
        public Resistor Resistor { get; set; }
        public override Question GetQuestion()
        {
            return Resistor.CircuitPart.Question;
        }
        public override bool IsCorrect()
        {
            return Resistor.CorrectResistance == GivenResistance;
        }
    }
}
