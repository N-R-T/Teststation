using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models.Classes
{
    public sealed class CircuitPart
    {
        public long Id { get; set; }
        public double NeededResistance { get; set; }
        public int Position { get; set; }
        public long QuestionId { get; set; }
        public CircuitQuestion Question { get; set; }
        public List<Resistor> Resistors { get; set; }

    }
}
