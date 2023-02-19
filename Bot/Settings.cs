using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public class Settings
    {
        public string ConnectionString { get; private set; }
        public string DiscordBotToken { get; private set; }
        public ulong P1RepublicGuildId { get; private set; }
        public ulong P1OCGuildId { get; private set; }
        public ulong AccountsChannelId { get; private set; }

        public Settings()
        {
            IConfiguration config = new ConfigurationBuilder()
                                    .AddJsonFile("appsettings.json", optional: false)
                                    .Build();

            ConnectionString = config.GetConnectionString("DefaultConnection");
            DiscordBotToken = config.GetSection("Discord")["botToken"];
            P1RepublicGuildId = ulong.Parse(config.GetSection("Discord:P1Republic")["guildID"]);
            var p1oc = config.GetSection("Discord:P1OC");
            P1OCGuildId = ulong.Parse(p1oc["guildID"]);
            AccountsChannelId = ulong.Parse(p1oc["AccountsChannelId"]);
        }
    }
}
