using System;

namespace Teststation.Models
{
    public class TestCandidateViewModel
    {
        public Test Test { get; set; }
        public double Result { get; set; }
        public bool Completed { get; set; }
        public bool IsStarted { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
