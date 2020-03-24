using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public sealed class CircuitPart
    {
        public long Id { get; set; }
        public double Resistance { get; set; }
        public double NeededCurrent { get; set; }
        public int Position { get; set; }
        public long QuestionId { get; set; }
        public CircuitQuestion Question { get; set; }

        [NotMapped]
        public Resistor Resistor1 { get; set; }

        [ForeignKey("Resistor1")]
        public long Resistor1Id { get; set; }

        [NotMapped]
        public Resistor Resistor2 { get; set; }

        [ForeignKey("Resistor2")]
        public long Resistor2Id { get; set; }
        
        [NotMapped]
        public Resistor Resistor3 { get; set; }
        [ForeignKey("Resistor3")]
        public long Resistor3Id { get; set; }

        public List<Resistor> Resistors()
        {
            return new List<Resistor> { Resistor1, Resistor2, Resistor3 };
        }
    }
}
