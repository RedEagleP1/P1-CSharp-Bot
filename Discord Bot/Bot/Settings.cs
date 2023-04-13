using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot
{
    public static class Settings
    {
        public static string ConnectionString { get; private set; }
        public static string DiscordBotToken { get; private set; }
        public static ulong P1RepublicGuildId { get; private set; }
        public static ulong P1OCGuildId { get; private set; }
        public static ulong AccountsChannelId { get; private set; }
        public static ulong ReviewChannelId { get; private set; }
        public static AccountCommandSettings AccountCommandSettings { get; private set; }
        public static ReviewCommandSettings ReviewCommandSettings { get; private set; }

        public static void Init()
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
            ReviewChannelId = ulong.Parse(p1oc["ReviewsChannelId"]);

            var accountCommandAppSettings = config.GetSection("Discord:accountCommand");
            AccountCommandSettings = new AccountCommandSettings
            {
                Cost = float.Parse(accountCommandAppSettings["Cost"]),
                Reward = float.Parse(accountCommandAppSettings["Reward"])
            };

            var reviewCommandAppSettings = config.GetSection("Discord:reviewCommand");
            ReviewCommandSettings = new ReviewCommandSettings
            {
                Cost = float.Parse(reviewCommandAppSettings["Cost"]),
                Reward = float.Parse(reviewCommandAppSettings["Reward"]),
                AcademyTaskRatingRewards = new RatingRewards
                {
                    Three = float.Parse(reviewCommandAppSettings["AcademyTaskRatingRewards:three"]),
                    Four = float.Parse(reviewCommandAppSettings["AcademyTaskRatingRewards:four"]),
                    Five = float.Parse(reviewCommandAppSettings["AcademyTaskRatingRewards:five"])
                },
                NonAcademyTaskRatingRewards = new RatingRewards
                {
                    Three = float.Parse(reviewCommandAppSettings["NonAcademyTaskRatingRewards:three"]),
                    Four = float.Parse(reviewCommandAppSettings["NonAcademyTaskRatingRewards:four"]),
                    Five = float.Parse(reviewCommandAppSettings["NonAcademyTaskRatingRewards:five"])
                }
            };
        }
    }

    public struct AccountCommandSettings
    {
        public float Cost { get; set; }
        public float Reward { get; set; }
    }
    public struct ReviewCommandSettings
    {
        public float Cost { get; set; }
        public float Reward { get; set; }
        public RatingRewards AcademyTaskRatingRewards { get; set; }
        public RatingRewards NonAcademyTaskRatingRewards { get; set; }
    }

    public struct RatingRewards
    {
        public float Three { get; set; }
        public float Four { get; set; }
        public float Five { get; set; }
    }
}
