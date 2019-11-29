using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public abstract class Answer
    {
        public long Id { get; set; }
        public long CandidateId { get; set; }
        public UserInformation Candidate { get; set; }


        public virtual bool IsCorrect()
        {
            return true;
        }

        public virtual Question GetQuestion()
        {
            return null;
        }
    }
}
