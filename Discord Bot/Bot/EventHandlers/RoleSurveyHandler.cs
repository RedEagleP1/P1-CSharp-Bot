
using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.HelperClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.EventHandlers
{
    public class RoleSurveyHandler : IEventHandler
    {
        private readonly DiscordSocketClient _client;
        public RoleSurveyHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public void Subscribe()
        {
            _client.ButtonExecuted += OnButtonExecuted;
        }

        async Task OnButtonExecuted(SocketMessageComponent component)
        {
            //await component.DeferAsync(ephemeral: true);

            _ = Task.Run(async () =>
            {
                try
                {
                    var interceptor = new RequestInterceptor(component);
                    await interceptor.Process();

                    if (!interceptor.IsRoleSurveyInteraction)
                    {
                        return;
                    }

                    var request = interceptor.Request;
                    var (fieldNumber, surveyId, selectedOptions) = RoleSurveyHelper.GetCurrentRoleSurveyEmbedInfo(request.Embed);
                    var context = DBContextFactory.GetNewContext();
                    var survey = await context.RolesSurvey.AsNoTracking().FirstOrDefaultAsync(rs => rs.Id == surveyId);
                    var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == survey.RoleId);
                    var incomingRoleSurvey_HM = await QueryHelper.GetRoleSurvey_HM(context, survey, role);
                    var builder = request.Embed.ToEmbedBuilder();

                    var selectedOptionsField = builder.Fields.FirstOrDefault(f => f.Name == $"Selected Options {fieldNumber}");
                    var newSelectedOptionsValue = selectedOptionsField.Value.ToString();
                    if (newSelectedOptionsValue == "None")
                    {
                        newSelectedOptionsValue = "";
                    }

                    if(request.IncomingValue != "Done")
                    {
                        newSelectedOptionsValue += "\n" + request.IncomingValue;
                        selectedOptionsField.Value = newSelectedOptionsValue;
                    }

                    if (survey.AllowOptionsMultiSelect)
                    {
                        if (request.IncomingValue != "Done")
                        {
                            var optionsLeft = GetOptionsLeft(selectedOptionsField.Value.ToString(), incomingRoleSurvey_HM.Options.Select(o => o.MainInstance.Text).ToArray());
                            optionsLeft.Add("Done");
                            var messagComponent = MessageComponentAndEmbedHelper.CreateButtons(optionsLeft.ToArray());

                            var roleId = incomingRoleSurvey_HM.Options.FirstOrDefault(o => o.MainInstance.Text == request.IncomingValue).MainInstance.RoleId;
                            if(roleId != null)
                            {
                                await GiveRoleAsync(request.User.Id, roleId.Value, context);
                            }                           

                            await request.UpdateOriginalMessageAsync(survey.InitialMessage, messagComponent, builder.Build());
                            return;
                        }

                            //do stuff
                        var optionsSelectedAsArray = GetOptionsSelectedAsArray(selectedOptions, incomingRoleSurvey_HM.Options.Select(o => o.MainInstance.Text).ToArray());
                        var nextRoleSurvey_HM = await GetNextRoleSurvey_HM(incomingRoleSurvey_HM, optionsSelectedAsArray , request.Embed, context);
                        if(nextRoleSurvey_HM == null)
                        {
                            var endMessage = survey.EndMessage ?? "Survey Ended";
                            await request.UpdateOriginalMessageAsync(endMessage, null, builder.Build());
                            return;
                        }

                        await RoleSurveyHelper.SendNextRoleSurvey(nextRoleSurvey_HM, interceptor.Request, builder);
                        return;                        
                    }

                    var optionsSelectedAsArray2 = GetOptionsSelectedAsArray(selectedOptions, incomingRoleSurvey_HM.Options.Select(o => o.MainInstance.Text).ToArray());
                    var nextRoleSurvey_HM2 = await GetNextRoleSurvey_HM(incomingRoleSurvey_HM, optionsSelectedAsArray2, request.Embed, context);
                    if (nextRoleSurvey_HM2 == null)
                    {
                        var roleId = incomingRoleSurvey_HM.Options.FirstOrDefault(o => o.MainInstance.Text == request.IncomingValue).MainInstance.RoleId;
                        if (roleId != null)
                        {
                            await GiveRoleAsync(request.User.Id, roleId.Value, context);
                        }
                        await request.UpdateOriginalMessageAsync(survey.EndMessage, null, builder.Build());
                        return;
                    }

                    await RoleSurveyHelper.SendNextRoleSurvey(nextRoleSurvey_HM2, interceptor.Request, builder);
                }
                catch(Exception exception)
                {
                    Console.WriteLine(exception);
                }
                
            });            
        }

        static List<string> GetOptionsLeft(string selectedOptions, string[] allOptions)
        {
            List<string> optionsLeft = new();
            foreach(var option in allOptions)
            {
                if(!selectedOptions.Contains(option))
                {
                    optionsLeft.Add(option);
                }
            }

            return optionsLeft;
        }

        static string[] GetOptionsSelectedAsArray(string selectedOptions, string[] allOptions)
        {
            List<string> options = new();
            foreach(var option in allOptions)
            {
                if(selectedOptions.Contains(option))
                {
                    options.Add(option);
                }
            }

            return options.ToArray();
        }

        static async Task<RoleSurvey_HM?> GetNextRoleSurvey_HM(RoleSurvey_HM current, string[] optionsSelected, IEmbed embed, ApplicationDbContext context)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == current.MainInstance.RoleId);
            var childSurveys = context.RolesSurvey.Where(rs => rs.ParentSurveyId == current.MainInstance.Id).OrderBy(rs => rs.Index).ToList();

            foreach (var cs in childSurveys)
            {
                var childSurvey_HM = await QueryHelper.GetRoleSurvey_HM(context, cs, role);
                var triggers = childSurvey_HM.Triggers.Select(t => t.MainInstance.Text);
                if (cs.HasConditionalTrigger)
                {
                    if (cs.AllTriggersShouldBeTrue && triggers.Except(optionsSelected).Any())
                    {
                        continue;
                    }

                    if (optionsSelected.Except(triggers).Count() == optionsSelected.Count())
                    {
                        continue;
                    }
                }

                return childSurvey_HM;
            }

            var parentSurveyId = current.MainInstance.ParentSurveyId;
            var currentSurveyIndex = current.MainInstance.Index;
            while(parentSurveyId != null)
            {
                var parentSurveyAllOptions = context.RoleSurveyOptions.Where(o => o.RoleSurveyId == parentSurveyId).Select(o => o.Text).ToArray();
                var parentSurveySelectedOptions = GetOptionsSelectedAsArray(RoleSurveyHelper.GetSelectedOptions(embed, parentSurveyId.Value), parentSurveyAllOptions);
                var siblingSurveys = context.RolesSurvey.Where(rs => rs.ParentSurveyId == parentSurveyId && rs.RoleId == role.Id).OrderBy(rs => rs.Index).ToList();

                for (int i = currentSurveyIndex + 1; i < siblingSurveys.Count; i++)
                {
                    var siblingSurvey_HM = await QueryHelper.GetRoleSurvey_HM(context, siblingSurveys[i], role);
                    var triggers = siblingSurvey_HM.Triggers.Select(t => t.MainInstance.Text);
                    if (siblingSurveys[i].HasConditionalTrigger)
                    {
                        if (siblingSurveys[i].AllTriggersShouldBeTrue && triggers.Except(parentSurveySelectedOptions).Any())
                        {
                            continue;
                        }

                        if (parentSurveySelectedOptions.Except(triggers).Count() == parentSurveySelectedOptions.Count())
                        {
                            continue;
                        }
                    }

                    return siblingSurvey_HM;
                }

                var parentSurvey = await context.RolesSurvey.FirstOrDefaultAsync(rs => rs.Id == parentSurveyId);
                if(parentSurvey == null)
                {
                    break;
                }

                parentSurveyId = parentSurvey.ParentSurveyId;
                currentSurveyIndex = parentSurvey.Index;
            }

            var rootSiblingSurveys = context.RolesSurvey.Where(rs => rs.ParentSurveyId == null && rs.RoleId == role.Id).OrderBy(rs => rs.Index).ToList();

            for (int i = currentSurveyIndex + 1; i < rootSiblingSurveys.Count; i++)
            {
                var siblingSurvey_HM = await QueryHelper.GetRoleSurvey_HM(context, rootSiblingSurveys[i], role);
                return siblingSurvey_HM;
            }

            return null;
        }

        static async Task GiveRoleAsync(ulong userId, ulong roleId, ApplicationDbContext context)
        {
            var roleToGive = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
            var socketGuildUser = await DiscordQueryHelper.GetSocketGuildUserAsync(roleToGive.GuildId, userId);
            if (socketGuildUser.Roles.Any(r => r.Id == roleId))
            {
                return;
            }

            await DBReadWrite.LockReadWrite();
            try
            {                
                var roleCostAndReward = await context.RolesCostAndReward.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (roleCostAndReward != null && roleCostAndReward.Cost > 0)
                {
                    var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == roleCostAndReward.CostCurrencyId && co.OwnerId == userId);
                    if (currencyOwned == null || currencyOwned.Amount < roleCostAndReward.Cost)
                    {
                        return;
                    }

                    currencyOwned.Amount -= roleCostAndReward.Cost;
                    await context.SaveChangesAsync();
                }

                if(roleCostAndReward != null && roleCostAndReward.Reward > 0 && roleCostAndReward.RewardCurrencyId != null)
                {
                    var currencyOwned = await context.CurrenciesOwned.FirstOrDefaultAsync(co => co.CurrencyId == roleCostAndReward.RewardCurrencyId && co.OwnerId == userId);
                    if(currencyOwned == null)
                    {
                        currencyOwned = new CurrencyOwned() { CurrencyId = roleCostAndReward.RewardCurrencyId.Value, OwnerId = userId, Amount = roleCostAndReward.Reward };
                        context.CurrenciesOwned.Add(currencyOwned);
                    }

                    await context.SaveChangesAsync();
                }
                
                await socketGuildUser.AddRoleAsync(roleId);
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }            
        }

    }
}
