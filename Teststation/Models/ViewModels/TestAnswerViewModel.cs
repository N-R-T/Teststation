using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class TestAnswerViewModel
    {
        public long UserId { get; set; }
        public long TestId { get; set; }

        [Display(Name = "Themengebiet")]
        public string Topic { get; set; }
        public List<QuestionAnswerViewModel> Questions { get; set; }
        public bool Completed { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
