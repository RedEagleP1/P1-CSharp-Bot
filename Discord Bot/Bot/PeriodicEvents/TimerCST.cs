using Bot.EventHandlers;
using Discord;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.PeriodicEvents
{
    internal class TimerCST
    {
        Timer? timer;
        public void EnablePeriodicEvents()
        {
            var currentTimeCST = DateTime.UtcNow.AddHours(-6);
            var after1Day = currentTimeCST.AddDays(1);
            var startOfNextDay = new DateTime(after1Day.Year, after1Day.Month, after1Day.Day, 0, 0, 0);
            var timeLeftBeforeNewDay = startOfNextDay - currentTimeCST;
            timer = new Timer(DailyEvents, null, timeLeftBeforeNewDay, TimeSpan.FromHours(24));
        }

        void DailyEvents(object? c)
        {
            _ = Task.Run(async () =>
            {
                await RefreshCurrencyAwardLimit();
                SetRoleMessageAndSurveyRepeatForToday();
                ResetCurrencyLogic();
            });            
        }

        async void SetRoleMessageAndSurveyRepeatForToday()
        {
            var context = DBContextFactory.GetNewContext();
            var repeated = await context.RoleMessagesAndSurveysRepeated.AsNoTracking().ToListAsync();
            var repeats = await context.RoleMessageAndSurveyRepeats.AsNoTracking().ToListAsync();
            foreach(var repeat in repeats)
            {
                var lastRepeated = repeated.FirstOrDefault(r => r.RoleId == repeat.RoleId);
                if(lastRepeated == null || (lastRepeated.LastRepeated - DateTime.UtcNow.AddHours(-6)).TotalDays >= repeat.RepeatAfterEvery_InDays)
                {
                    RepeatRoleMessageAndSurveyAfterTime(repeat.RoleId, (int)repeat.RepeatTime.TotalMilliseconds);
                }
            }
        }

        static async void RepeatRoleMessageAndSurveyAfterTime(ulong roleId, int milliseconds)
        {
            await Task.Delay(milliseconds);

            try
            {
                var context = DBContextFactory.GetNewContext();

                var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
                var users = await DiscordQueryHelper.GetAllUsersWithRoleAsync(role.GuildId, roleId);
                var roleMessage = await context.RoleMessages.FirstOrDefaultAsync(rm => rm.RoleId == roleId);

                if(roleMessage != null)
                {                    
                    foreach(var user in users)
                    {
                        try
                        {
                            await user.SendMessageAsync(roleMessage.Message);
                        }
                        catch(Discord.Net.HttpException exc)
                        {
                            if(exc.DiscordCode != DiscordErrorCode.CannotSendMessageToUser)
                            {
                                Console.WriteLine(exc.ToString());
                            }
                        }
                        
                    }
                }

                var roleSurvey = await context.RolesSurvey.FirstOrDefaultAsync(rs => rs.Index == 0 && rs.RoleId == roleId && rs.ParentSurveyId == null);
                if(roleSurvey != null)
                {
                    foreach(var user in users)
                    {
                        await RoleSurveyHelper.SendRoleSurvey(roleSurvey, user, context);
                    }
                }

                await DBReadWrite.LockReadWrite();
                try
                {
                    var repeated = await context.RoleMessagesAndSurveysRepeated.FirstOrDefaultAsync(r => r.RoleId == roleId);
                    if (repeated == null)
                    {
                        repeated = new RoleMessageAndSurveyRepeated() { RoleId = roleId, LastRepeated = DateTime.UtcNow.AddHours(-6) };
                        context.RoleMessagesAndSurveysRepeated.Add(repeated);
                    }

                    repeated.LastRepeated = DateTime.UtcNow.AddHours(-6);
                    await context.SaveChangesAsync();
                }
                finally
                {
                    DBReadWrite.ReleaseLock();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }            
        }

        static async Task RefreshCurrencyAwardLimit()
        {
            using var context = DBContextFactory.GetNewContext();
            foreach(var awardLimit in context.CurrencyAwardLimits)
            {
                context.CurrencyAwardLimits.Remove(awardLimit);
            }

            await context.SaveChangesAsync();
        }

        public static async Task ResetCurrencyLogic()
        {
            using var context = DBContextFactory.GetNewContext();
            var currencyIds = new List<int?>();
            foreach (var reset in context.CurrencyResets)
            {
                reset.DaysLeft--;
                if (reset.DaysLeft <= 0)
                {
                    if (reset.Auto == false)
                    {
                        reset.DaysLeft = reset.DaysBetween;
                    }
                    else
                    {
                        var today = DateTime.UtcNow.AddHours(-6); // CST conversion
                        var firstDayOfNextMonth = new DateTime(today.Year, today.Month, 1).AddMonths(1);
                        reset.DaysLeft = (firstDayOfNextMonth - today).Days;
                    }
                    currencyIds.Add(reset.CurrencyId);
                }
            }
            //This threw a bug inside of the foreeach loop, so I moved it outside and it now works.
            foreach (var currency in context.CurrenciesOwned)
            {
                if (currency != null)
                {
                    if (currencyIds.Contains(currency.CurrencyId))
                    {
                        currency.Amount = 0;
                    }
                }
            }

            await context.SaveChangesAsync();
        }

    }
}
