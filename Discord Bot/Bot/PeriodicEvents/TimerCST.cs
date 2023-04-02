﻿using Bot.EventHandlers;
using Discord;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
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

        async void DailyEvents(object? c)
        {
            await RefreshCurrencyAwardLimit();
            SetRoleMessageAndSurveyRepeatForToday();
        }

        void SetRoleMessageAndSurveyRepeatForToday()
        {
            var context = DBContextFactory.GetNewContext();
            var repeated = context.RoleMessagesAndSurveysRepeated.AsNoTracking();
            var repeats = context.RoleMessageAndSurveyRepeats.AsNoTracking();
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
                        await user.SendMessageAsync(roleMessage.Message);
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

    }
}
