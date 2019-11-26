using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class TestCreationViewModel
    {
        public long Id { get; set; }

        [Display(Name = "Themengebiet")]
        public string Topic { get; set; }

        public List<QuestionCreationViewModel> Questions { get; set; }
    }
}
