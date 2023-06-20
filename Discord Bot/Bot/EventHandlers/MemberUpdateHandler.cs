using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;

namespace Bot.EventHandlers
{
    public class MemberUpdateHandler : IEventHandler
    {
        readonly DiscordSocketClient client;
        public MemberUpdateHandler(DiscordSocketClient client)
        {
            this.client = client;
        }

        public void Subscribe()
        {
            client.GuildMemberUpdated += OnGuildMemberUpdate;
        }
        Task OnGuildMemberUpdate(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser after)
        {
            _ = Task.Run(async () =>
            {
                SocketGuildUser b = await before.GetOrDownloadAsync();
                await SendRoleMessage(b, after);
            });

            return Task.CompletedTask;
        }
        async Task SendRoleMessage(SocketGuildUser before, SocketGuildUser after)
        {
            if (before.Roles.Count >= after.Roles.Count)
                return;

            using var context = DBContextFactory.GetNewContext();
            var addedRoles = after.Roles.Except(before.Roles);
            foreach(var role in addedRoles)
            {
                var roleMessage = await context.RoleMessages.AsNoTracking().FirstOrDefaultAsync(rm => rm.RoleId == role.Id);
                if (roleMessage != null)
                {
                    try
                    {
                        await after.SendMessageAsync(roleMessage.Message);
                    }
                    catch (Discord.Net.HttpException exc)
                    {
                        if (exc.DiscordCode != DiscordErrorCode.CannotSendMessageToUser)
                        {
                            Console.WriteLine(exc.ToString());
                        }
                    }                    
                }

                var roleSurvey = await context.RolesSurvey.AsNoTracking().FirstOrDefaultAsync(rs => rs.RoleId == role.Id && rs.ParentSurveyId == null && rs.Index == 0);
                if (roleSurvey != null)
                {
                    await RoleSurveyHelper.SendRoleSurvey(roleSurvey, after, context);
                }
            }          
        }
    }
}
