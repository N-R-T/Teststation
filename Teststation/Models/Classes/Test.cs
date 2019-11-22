using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class Test
    {
        public long Id { get; set; }
        public string Topic { get; set; }
        public TestStatus ReleaseStatus { get; set; }
        public List<Question> Questions { get; set; }
        public List<Session> Sessions { get; set; }

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
