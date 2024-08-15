using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command deletes the specified legion from the database.
    /// It also removes all member entries for that legion from the LegionMembers table.
    /// </summary>
    internal class Legions_DeleteLegionCommand : ISlashCommand
    {
        const string name = "delete_legion";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;


        public Legions_DeleteLegionCommand(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                string message = await GetMessage(command);

                await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Content = message;
                    response.Flags = MessageFlags.Ephemeral;
                });

            });
        }

        
        async Task<string> GetMessage(SocketSlashCommand command)
        {
            // First, check if the user has permission to use this command.
            var user = client.GetGuild(Settings.P1RepublicGuildId)?.GetUser(command.User.Id);
            if (user == null)
                return "Could not find user info.";
            else if (user.Roles.FirstOrDefault(x => x.Name == LegionConstants.MODERATOR_ROLE) == null)
                return "You do not have permission to use this command.";


            // Try to get the id option.
            SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
            ulong legionId = 0;
            if (idOption == null)
            {
                return "Please provide the Id of legion you want to delete.";
            }
            else
            {
                // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                legionId = (ulong)(long)idOption.Value;
            }


            await DBReadWrite.LockReadWrite();
            try
            {
                // Check if there is an legion with this Id.
                using var context = DBContextFactory.GetNewContext();


                // Find the legion
                Legion? legion = context.Legions.Count() > 0 ? await context.Legions.FirstAsync(x => x.Id == legionId)
                                                             : null;
                if (legion == null)
                    return "There is no legion with this Id.";


                // First, find all member entries for this legion.
                if (context.LegionMembers.Count() > 0)
                {
                    var result = context.LegionMembers.Where(x => x.LegionId == legionId);

                    if (result != null && result.Count() > 0)
                    {
                        // And delete them.
                        foreach (LegionMember member in result)
                        {
                            context.LegionMembers.Remove(member);
                        }
                    }
                }


                // Now simply delete the legion.
                context.Legions.Remove(legion);

                // Finally, save the changes.
                await context.SaveChangesAsync();

                return $"Removed the legion \"{legion.Name}\" from the database.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while trying to write to the database:\n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                return $"An error occurred while trying to access the database.";
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }
        }
        
        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Deletes the legion with the specified Id.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the legion to delete", true)
                .Build();
        }
    }
}
