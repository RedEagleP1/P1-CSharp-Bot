using System.Collections.Generic;
using Discord;
using DiscordBot.BotApplication.Commands;

public static class TestData
{
    public static List<CreateCommandModel> GetTestCommands()
    {
        return new List<CreateCommandModel>
        {
            new CreateCommandModel
            {
                Name = "greet",
                Description = "Send a greeting message",
                Options = new List<DiscordCommandOption>
                {
                    new DiscordCommandOption
                    {
                        Name = "user",
                        Description = "The user to greet",
                        Type = ApplicationCommandOptionType.User,
                        Required = true
                    },
                    new DiscordCommandOption
                    {
                        Name = "message",
                        Description = "The greeting message",
                        Type = ApplicationCommandOptionType.String,
                        Required = false
                    }
                }
            },
            new CreateCommandModel
            {
                Name = "Admin",
                Description = "Anouncement",
                Options = new List<DiscordCommandOption>
                {
                    new DiscordCommandOption
                    {
                        Name = "user",
                        Description = "The user to anounce to",
                        Type = ApplicationCommandOptionType.User,
                        Required = true
                    },
                    new DiscordCommandOption
                    {
                        Name = "reason",
                        Description = "The reason for the anouncement",
                        Type = ApplicationCommandOptionType.String,
                        Required = false
                    }
                }
            }
        };
    }
}