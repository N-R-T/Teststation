using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Teststation.Models
{
    public class TestCandidateViewModel
    {        
        public Test Test { get; set; }
        public double Result { get; set; }
        public bool Completed { get; set; }
        public bool IsStarted { get; set; }
    }
}
