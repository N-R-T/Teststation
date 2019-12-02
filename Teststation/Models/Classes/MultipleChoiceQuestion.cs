using System.Collections.Generic;

namespace Teststation.Models
{
    public class MultipleChoiceQuestion : Question
    {
        public List<Choice> Choices { get; set; }
    }
}
