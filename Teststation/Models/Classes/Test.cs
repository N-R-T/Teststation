using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Teststation.Models
{
    public class Test
    {
        public long Id { get; set; }

        [Display(Name = "Themengebiet")]
        public string Topic { get; set; }

        [Display(Name = "Veröffentlichungsstatus")]
        public TestStatus ReleaseStatus { get; set; }
        public List<Question> Questions { get; set; }
        public List<Session> Sessions { get; set; }

        public Test()
        {

        }

        public Test(Test original)
        {
            Id = Consts.backUpTestId;
            Topic = original.Topic;
            Questions = new List<Question>();
        }

        public List<Question> GetQuestions()
        {
            return new List<Question>();
        }

        public int GetAllPoints()
        {
            var maxPoints = 0;
            foreach (var question in Questions)
            {
                maxPoints += question.Points;
            }
            return maxPoints;
        }
    }
}
