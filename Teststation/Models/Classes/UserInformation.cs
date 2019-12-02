using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Teststation.Models
{
    public class UserInformation
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public UserRole Role { get; set; }
        public List<Session> Sessions { get; set; }
        public DateTime DayOfLastActivity { get; set; }
    }
}
