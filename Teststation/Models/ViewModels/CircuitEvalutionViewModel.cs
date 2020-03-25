using System.Collections.Generic;

namespace Teststation.Models
{
    public class CircuitEvalutionViewModel
    {
        public CircuitQuestion Question { get; set; }
        public List<CircuitAnswer> GivenAnswers { get; set; }
    }
}
