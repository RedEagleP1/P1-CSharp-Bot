using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.HelperClasses
{
    public static class QueryHelper
    {
        public static async Task<RoleSurvey_HM> GetRoleSurvey_HM(ApplicationDbContext context, RoleSurvey roleSurvey, Role role)
        {
            return new RoleSurvey_HM()
            {
                MainInstance = roleSurvey,
                RoleName = role.Name,
                Options = await GetOption_HMs(context, roleSurvey),
                Triggers = await GetTrigger_HMs(context, roleSurvey)
            };
        }

        static async Task<List<RoleSurveyOption_HM>> GetOption_HMs(ApplicationDbContext context, RoleSurvey roleSurvey)
        {
            var roleSurveyOption_HMs = new List<RoleSurveyOption_HM>();
            var options = context.RoleSurveyOptions.Where(o => o.RoleSurveyId == roleSurvey.Id).AsNoTracking().ToList();
            foreach (var option in options)
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == option.RoleId);
                var roleSurveyOption_HM = new RoleSurveyOption_HM()
                {
                    MainInstance = option,
                    RoleName = role == null ? "None" : role.Name
                };
                roleSurveyOption_HMs.Add(roleSurveyOption_HM);
            }

            return roleSurveyOption_HMs;
        }
        static async Task<List<RoleSurveyOption_HM>> GetTrigger_HMs(ApplicationDbContext context, RoleSurvey roleSurvey)
        {
            var triggerSurveyOption_HMs = new List<RoleSurveyOption_HM>();
            var triggerOptionsIds = context.RoleSurveyRoleSurveyTriggers.Where(rsrst => rsrst.RoleSurveyId == roleSurvey.Id).AsNoTracking().Select(rsrst => rsrst.RoleSurveyOptionId).ToList();
            var triggers = context.RoleSurveyOptions.AsNoTracking().AsEnumerable().Where(o => triggerOptionsIds.Contains(o.Id)).ToList();
            foreach (var option in triggers)
            {
                var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == option.RoleId);
                var triggerSurveyOption_HM = new RoleSurveyOption_HM()
                {
                    MainInstance = option,
                    RoleName = role == null ? "None" : role.Name
                };
                triggerSurveyOption_HMs.Add(triggerSurveyOption_HM);
            }

            return triggerSurveyOption_HMs;
        }
    }
}
