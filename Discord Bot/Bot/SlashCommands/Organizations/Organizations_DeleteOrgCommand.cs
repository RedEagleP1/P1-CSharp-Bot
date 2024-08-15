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
    /// This command removes the specified organization from the database.
    /// It also removes all member entries for that organization from the OrganizationMembers table.
    /// </summary>
    internal class Organizations_DeleteOrgCommand : ISlashCommand
    {
        const string name = "delete_org";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;


        public Organizations_DeleteOrgCommand(DiscordSocketClient client)
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
            else if (user.Roles.FirstOrDefault(x => x.Name == OrganizationConstants.MODERATOR_ROLE) == null)
                return "You do not have permission to use this command.";


            // Try to get the id option.
            SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
            ulong orgId = 0;
            if (idOption == null)
            {
                return "Please provide the Id of organization you want to remove.";
            }
            else
            {
                // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                orgId = (ulong)(long)idOption.Value;
            }


            await DBReadWrite.LockReadWrite();
            try
            {
                // Check if there is an organization with this Id.
                using var context = DBContextFactory.GetNewContext();


                // Find the organization
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstAsync(x => x.Id == orgId)
                                                                      : null;
                if (org == null)
                    return "There is no organization with this Id.";


                // First, find all member entries for this organization.
                if (context.OrganizationMembers.Count() > 0)
                {
                    var result = context.OrganizationMembers.Where(x => x.OrganizationId == orgId);

                    if (result != null && result.Count() > 0)
                    {
                        // And delete them.
                        foreach (OrganizationMember member in result)
                        {
                            context.OrganizationMembers.Remove(member);
                        }
                    }
                }


                // Now simply delete the organization.
                context.Organizations.Remove(org);

                // Finally, save the changes.
                await context.SaveChangesAsync();

                return $"Removed the organization \"{org.Name}\" from the database.";
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
                .WithDescription("Removes the organization with the specified Id.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the organization to remove", true)
                .Build();
        }
    }
}
