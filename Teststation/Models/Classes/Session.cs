﻿using System;

namespace Teststation.Models
{
    public sealed class Session
    {
        public long Id { get; set; }
        public long TestId { get; set; }
        public Test Test { get; set; }
        public string CandidateId { get; set; }
        public User Candidate { get; set; }
        public bool Completed { get; set; }
        public TimeSpan Duration { get; set; }

    }
}
