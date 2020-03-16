using System.Collections.Generic;

namespace Teststation.Models
{
    public sealed class MultipleChoiceQuestion : Question
    {
        public List<Choice> Choices { get; set; }
    }
}
