using System.Collections.Generic;

namespace Teststation.Models
{
    public class CandidateSessionViewModel
    {
        public List<TestCandidateViewModel> Tests { get; set; }
        public UserInformation UserInformation { get; set; }
    }
}
