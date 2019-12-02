using System.Collections.Generic;

namespace Teststation.Models
{
    public class MultipleChoiceEvalutionViewModel
    {
        public MultipleChoiceQuestion Question { get; set; }
        public List<MultipleChoiceAnswer> GivenAnswers { get; set; }
    }
}
