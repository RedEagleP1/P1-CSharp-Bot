using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class DiscordQueryHelper
    {
        static DiscordSocketClient client;
        static SocketTextChannel accountPostChannel;
        static SocketTextChannel reviewPostChannel;
        public static SocketTextChannel AccountPostChannel { get => accountPostChannel; }
        public static SocketTextChannel ReviewPostChannel { get => reviewPostChannel; }
        public static void Init(DiscordSocketClient client)
        {
            DiscordQueryHelper.client = client;
        }

        public static void SetAccountPostChannel(SocketTextChannel postChannel)
        {
            accountPostChannel = postChannel;
        }

        public static void SetReviewPostChannel(SocketTextChannel postChannel)
        {
            reviewPostChannel = postChannel;
        }

        public static async Task<IUser> GetUserAsync(ulong userId)
        {
            return await client.GetUserAsync(userId);
        }

        public static async Task<SocketGuildUser> GetSocketGuildUserAsync(ulong guildId, ulong userId)
        {
            var guild = client.GetGuild(guildId);
            await guild.DownloadUsersAsync();
            return guild.GetUser(userId);            
        }

        public static async Task<List<SocketGuildUser>> GetAllUsersWithRoleAsync(ulong guildId, ulong roleId)
        {
            var guild = client.GetGuild(guildId);
            await guild.DownloadUsersAsync();
            return guild.Users.Where(u => u.Roles.Any(r => r.Id == roleId)).ToList();
        }

    }
}
