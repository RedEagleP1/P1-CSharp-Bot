using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.HelperClasses;
using WebApp.Pages.Partials;

namespace WebApp.Pages.Modules.RoleSurveys
{
    [Authorize(Policy = "Allowed")]
    public class SelectedRoleModel : PageModel
    {
        public Guild Guild { get; set; }
        public Role Role { get; set; }
        public List<RoleSurvey_HM> RoleSurvey_HMs { get; set; }
        public bool IsEditMode { get; set; }
        public int RoleSurveyEditId { get; set; }
        public bool ShouldFocusOnParticularRoleSurvey { get; set; }
        public int RoleSurveyFocusId { get; set; }
        public List<Role> AllRoles { get; set; }
        private readonly ApplicationDbContext _db;
        public SelectedRoleModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task OnGet(ulong roleId)
        {
            Role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
            Guild = await _db.Guilds.AsNoTracking().FirstOrDefaultAsync(g => g.Id == Role.GuildId);
            AllRoles = _db.Roles.AsNoTracking().Where(r => r.GuildId == Guild.Id).ToList();
            var roleSurvey_HMs = new List<RoleSurvey_HM>();
            var allRoleSurveys = _db.RolesSurvey.Where(rs => rs.RoleId == roleId).AsNoTracking().ToList();
            foreach (var roleSurvey in allRoleSurveys)
            {
                roleSurvey_HMs.Add(await QueryHelper.GetRoleSurvey_HM(_db, roleSurvey, Role));
            }
            RoleSurvey_HMs = roleSurvey_HMs;
        }
        public async Task OnGetWithFocus(int roleSurveyId)
        {
            var roleSurvey = await _db.RolesSurvey.AsNoTracking().FirstOrDefaultAsync(rs => rs.Id == roleSurveyId);
            await OnGet(roleSurvey.RoleId);
            ShouldFocusOnParticularRoleSurvey = true;
            RoleSurveyFocusId = roleSurveyId;
        }

        public async Task OnGetWithFocusWithAlert(int roleSurveyId, string message)
        {
            await OnGetWithFocus(roleSurveyId);
            ViewData["Message"] = message;
        }

        public async Task OnGetEdit(int roleSurveyId)
        {
            var roleSurvey = await _db.RolesSurvey.AsNoTracking().FirstOrDefaultAsync(rs => rs.Id == roleSurveyId);
            await OnGet(roleSurvey.RoleId);
            IsEditMode = true;
            RoleSurveyEditId = roleSurveyId;
            ShouldFocusOnParticularRoleSurvey = true;
            RoleSurveyFocusId = roleSurveyId;
        }

        public async Task<IActionResult> OnPostDelete(int roleSurveyId)
        {
            if(await _db.RolesSurvey.AsNoTracking().FirstOrDefaultAsync(rs => rs.ParentSurveyId == roleSurveyId) != null)
            {
                var message = "Role Survey was not deleted. You can't delete a survey that has children. Delete the children first.";
                return RedirectToPage("SelectedRole", "WithFocusWithAlert", new { roleSurveyId = roleSurveyId, message = message });
            }

            var roleSurvey = await _db.RolesSurvey.FirstOrDefaultAsync(rs => rs.Id == roleSurveyId);
            var siblingRoleSurveys = _db.RolesSurvey.Where(rs => rs.ParentSurveyId == roleSurvey.ParentSurveyId && rs.RoleId == roleSurvey.RoleId).OrderBy(rs => rs.Index).ToList();
            siblingRoleSurveys.Remove(roleSurvey);
            for(int i=0;i<siblingRoleSurveys.Count;i++)
            {
                siblingRoleSurveys[i].Index = i;
            }
            _db.RolesSurvey.Remove(roleSurvey);         
            
            await _db.SaveChangesAsync();
            return RedirectToPage(new { roleId = roleSurvey.RoleId });
        }
        public async Task<IActionResult> OnPostSave(RoleSurvey_HM roleSurvey_HM)
        {
            var allOptions = _db.RoleSurveyOptions.AsNoTracking().Where(o => o.RoleSurveyId == roleSurvey_HM.MainInstance.Id).ToList();
            var optionsToRemove = new List<RoleSurveyOption>();
            foreach (RoleSurveyOption option in allOptions)
            {
                if (roleSurvey_HM.Options.FirstOrDefault(o => o.MainInstance.Id == option.Id) == null)
                {
                    optionsToRemove.Add(option);
                }
            }

            if (_db.RoleSurveyRoleSurveyTriggers.AsNoTracking().AsEnumerable().Where(rsrst => optionsToRemove.Any(o => o.Id == rsrst.RoleSurveyOptionId)).Any())
            {
                var message = "Changes were not saved. You can't delete an option that is being used as a conditional trigger. Remove the trigger in children first.";
                return RedirectToPage("SelectedRole", "WithFocusWithAlert", new { roleSurveyId = roleSurvey_HM.MainInstance.Id, message = message });
            }

            _db.RemoveRange(optionsToRemove);
            _db.RoleSurveyOptions.UpdateRange(roleSurvey_HM.Options.Select(hm => hm.MainInstance));
            _db.RolesSurvey.Update(roleSurvey_HM.MainInstance);

            var allTriggers = _db.RoleSurveyRoleSurveyTriggers.Where(rsrst => rsrst.RoleSurveyId == roleSurvey_HM.MainInstance.Id).ToList();
            var triggersToRemove = new List<RoleSurveyRoleSurveyTrigger>();
            foreach (var trigger in allTriggers)
            {
                if (roleSurvey_HM.Triggers.FirstOrDefault(t => t.MainInstance.Id == trigger.RoleSurveyOptionId) == null)
                {
                    triggersToRemove.Add(trigger);
                }
            }

            var triggersToAdd = new List<RoleSurveyRoleSurveyTrigger>();
            foreach (var triggers_HM in roleSurvey_HM.Triggers)
            {
                if (allTriggers.FirstOrDefault(t => t.RoleSurveyOptionId == triggers_HM.MainInstance.Id) == null)
                {
                    triggersToAdd.Add(new RoleSurveyRoleSurveyTrigger()
                    {
                        RoleSurveyId = roleSurvey_HM.MainInstance.Id,
                        RoleSurveyOptionId = triggers_HM.MainInstance.Id
                    });
                }
            }

            _db.RoleSurveyRoleSurveyTriggers.RemoveRange(triggersToRemove);
            _db.RoleSurveyRoleSurveyTriggers.AddRange(triggersToAdd);
            await _db.SaveChangesAsync();
            return RedirectToPage("SelectedRole", "WithFocus", new { roleSurveyId = roleSurvey_HM.MainInstance.Id });
        }

        public async Task<IActionResult> OnPostAddNewSurvey(ulong roleId, int index)
        {
            var roleSurveys = _db.RolesSurvey.Where(rs => rs.RoleId == roleId && rs.ParentSurveyId == null).OrderBy(rs => rs.Index).ToList();
            var newRoleSurvey = new RoleSurvey()
            {
                Index = index,
                InitialMessage = "This is a new survey",
                RoleId = roleId
            };

            for(int i=index;i<roleSurveys.Count;i++)
            {
                roleSurveys[i].Index++;
            }

            _db.RolesSurvey.Add(newRoleSurvey);
            await _db.SaveChangesAsync();

            return RedirectToPage(new { roleId = roleId });
        }
        public async Task<IActionResult> OnPostAddChildSurvey(int parentSurveyId, int index)
        {
            var parentSurvey = await _db.RolesSurvey.AsNoTracking().FirstOrDefaultAsync(rs => rs.Id == parentSurveyId);
            var childRoleSurveys = _db.RolesSurvey.Where(rs => rs.ParentSurveyId == parentSurveyId).OrderBy(rs => rs.Index).ToList();
            var newRoleSurvey = new RoleSurvey()
            {
                Index = index,
                InitialMessage = "This is a new survey",
                RoleId = parentSurvey.RoleId,
                ParentSurveyId = parentSurveyId
            };

            for (int i = index; i < childRoleSurveys.Count; i++)
            {
                childRoleSurveys[i].Index++;
            }

            _db.RolesSurvey.Add(newRoleSurvey);
            await _db.SaveChangesAsync();

            return RedirectToPage(new { roleId = parentSurvey.RoleId });
        }
        public async Task<IActionResult> OnGetRoleSurveyAddOption(RoleSurvey_HM roleSurvey_HM)
        {
            roleSurvey_HM.Options.Add(new RoleSurveyOption_HM()
            {
                MainInstance = new RoleSurveyOption() { Text="", RoleId = null, RoleSurveyId = roleSurvey_HM.MainInstance.Id }
            });

            var model = await CreateRoleSurveyEditPartialModelFromIncomingProperties(roleSurvey_HM, _db);
            return Partial("/Pages/Partials/_RoleSurveyEdit.cshtml", model);            
        }
        public async Task<IActionResult> OnGetRoleSurveyAddTrigger(RoleSurvey_HM roleSurvey_HM, int triggerId)
        {
            if (roleSurvey_HM.Triggers.Count == 0)
            {
                roleSurvey_HM.MainInstance.HasConditionalTrigger = true;
            }
            var model = await CreateRoleSurveyEditPartialModelFromIncomingProperties(roleSurvey_HM, _db);
            var triggerToAdd = model.AvailableTriggerOptions.FirstOrDefault(t => t.Id == triggerId);

            model.RoleSurvey_HM.Triggers.Add(new RoleSurveyOption_HM()
            {
                MainInstance = triggerToAdd
            });
            model.AvailableTriggerOptions.Remove(triggerToAdd);
            return Partial("/Pages/Partials/_RoleSurveyEdit.cshtml", model);
        }
        public async Task<IActionResult> OnGetRoleSurveyRemoveTrigger(RoleSurvey_HM roleSurvey_HM, int triggerIndex)
        {
            if (roleSurvey_HM.Triggers.Count == 1)
            {
                roleSurvey_HM.MainInstance.HasConditionalTrigger = false;
            }
            var model = await CreateRoleSurveyEditPartialModelFromIncomingProperties(roleSurvey_HM, _db);
            var triggerToRemove = model.RoleSurvey_HM.Triggers[triggerIndex];

            model.RoleSurvey_HM.Triggers.Remove(triggerToRemove);
            model.AvailableTriggerOptions.Add(triggerToRemove.MainInstance);
            return Partial("/Pages/Partials/_RoleSurveyEdit.cshtml", model);
        }
        public async Task<IActionResult> OnGetRoleSurveyRemoveOption(RoleSurvey_HM roleSurvey_HM, int optionIndex)
        {
            roleSurvey_HM.Options.RemoveAt(optionIndex);
            var model = await CreateRoleSurveyEditPartialModelFromIncomingProperties(roleSurvey_HM, _db);
            return Partial("/Pages/Partials/_RoleSurveyEdit.cshtml", model);
        }
        static async Task<RoleSurveyEditPartialModel> CreateRoleSurveyEditPartialModelFromIncomingProperties(RoleSurvey_HM roleSurvey_HM, ApplicationDbContext _db)
        {
            foreach (var option_HM in roleSurvey_HM.Options)
            {
                var optionRole = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == option_HM.MainInstance.RoleId);
                option_HM.RoleName = optionRole == null ? "None" : optionRole.Name;
            }

            var role = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleSurvey_HM.MainInstance.RoleId);
            var allRoles = _db.Roles.AsNoTracking().Where(r => r.GuildId == role.GuildId).ToList();
            var parentSurvey = await _db.RolesSurvey.AsNoTracking().FirstOrDefaultAsync(rs => rs.Id == roleSurvey_HM.MainInstance.ParentSurveyId);
            var availableTriggerOptions = new List<RoleSurveyOption>();
            if (parentSurvey != null)
            {
                availableTriggerOptions = _db.RoleSurveyOptions.Where(o => o.RoleSurveyId == parentSurvey.Id).AsNoTracking().ToList();
                for (int i = availableTriggerOptions.Count - 1; i >= 0; i--)
                {
                    if (roleSurvey_HM.Triggers.FirstOrDefault(t => t.MainInstance.Id == availableTriggerOptions[i].Id) != null)
                    {
                        availableTriggerOptions.RemoveAt(i);
                    }
                }
            }

            return new RoleSurveyEditPartialModel() { RoleSurvey_HM = roleSurvey_HM, AllRoles = allRoles, AvailableTriggerOptions = availableTriggerOptions, CanHaveCondition = roleSurvey_HM.MainInstance.ParentSurveyId != null };
        }
    }
}
