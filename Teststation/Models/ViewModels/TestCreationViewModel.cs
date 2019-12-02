using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
