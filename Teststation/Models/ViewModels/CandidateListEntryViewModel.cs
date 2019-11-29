using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Teststation.Models.ViewModels
{
    public class CandidateListEntryViewModel
    {        
        public string Name { get; set; }
        public string UserId { get; set; }
        public UserInformation UserInformation { get; set; }
        
    }
}