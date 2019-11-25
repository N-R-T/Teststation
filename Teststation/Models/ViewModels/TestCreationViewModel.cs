using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class TestCreationViewModel
    {
        public long Id { get; set; }
        public string Topic { get; set; }

        public List<QuestionCreationViewModel> Questions { get; set; }
    }
}
