using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class RoleSurveyOption
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public ulong RoleId { get; set; }
        public Role Role { get; set; }
        public int RoleSurveyId { get; set; }
        public RoleSurvey RoleSurvey { get; set; }
        public List<RoleSurvey> RoleSurveysThatHaveThisOptionAsTrigger { get; set; }
    }
}
