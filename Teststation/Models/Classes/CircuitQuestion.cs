using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public sealed class CircuitQuestion : Question
    {
        public CircuitType CircuitType { get; set; }
        public double InitialCurrent { get; set; }
        public double Amperage { get; set; }
        public List<CircuitPart> Parts { get; set; }
    }
}
