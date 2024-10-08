﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RoleSurvey
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public bool HasConditionalTrigger { get; set; }
        public bool AllTriggersShouldBeTrue { get; set; }
        public string InitialMessage { get; set; }
        public bool AllowOptionsMultiSelect { get; set; }
        public string? EndMessage { get; set; }
        public ulong RoleId { get; set; }
        public int? ParentSurveyId { get; set; }
    }
}
