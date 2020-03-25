using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public sealed class Resistor
    {
        public long Id { get; set; }
        public double CorrectResistance { get; set; }
        public bool Visible { get; set; }

        [NotMapped]
        public CircuitPart CircuitPart { get; set; }

    }
}
