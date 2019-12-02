using System;

namespace Teststation.Models
{
    public class Session
    {
        public long Id { get; set; }
        public long TestId { get; set; }
        public Test Test { get; set; }
        public long CandidateId { get; set; }
        public UserInformation Candidate { get; set; }
        public bool Completed { get; set; }
        public TimeSpan Duration { get; set; }

    }
}
