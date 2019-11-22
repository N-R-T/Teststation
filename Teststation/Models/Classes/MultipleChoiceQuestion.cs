using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class MultipleChoiceQuestion : Question
    {
        public List<Choice> Choices { get; set; }
    }
}
