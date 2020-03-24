using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class ResistorCreationViewModel
    {
        public long Id { get; set; }
        public double CorrectResistance { get; set; }
        public bool Visible { get; set; }
        public long CircuitPartId { get; set; }
    }
}
