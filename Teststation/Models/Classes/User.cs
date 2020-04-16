using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Teststation.Models
{
    public sealed class User : IdentityUser
    {
        public bool IsDeleted { get; set; }
        public List<Session> Sessions { get; set; }
        public DateTime DayOfLastActivity { get; set; }
    }
}
