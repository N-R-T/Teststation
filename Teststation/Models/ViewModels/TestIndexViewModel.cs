using System.Collections.Generic;

namespace Teststation.Models
{
    public class TestEntryViewModel
    {
        public Test Test { get; set; }
        public List<string> Errors { get; set; }
    }
}
