﻿using PensionContributionManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PensionContributionManagementSystem.Core.Dtos
{
    public class ContributionResponseDto
    {     
            public string Id {  get; set; }
            public string MemberId { get; set; }
            public string ContributionType { get; set; } 
            public decimal Amount { get; set; }
            public DateTime ContributionDate { get; set; }
            public string ReferenceNumber { get; set; }
        
    }
}
