using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class CircuitPartCreationViewModel
    {
        public long Id { get; set; }
        public double NeededResistance { get; set; }
        public int Position { get; set; }
        public long QuestionId { get; set; }
        public List<ResistorCreationViewModel> Resistors { get; set; }
    }
}
