using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class MultipleChoiceEvalutionViewModel
    {
        public MultipleChoiceQuestion Question { get; set; }
        public List<MultipleChoiceAnswer> GivenAnswers { get; set; }
    }
}
