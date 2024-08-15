using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.ComponentModel;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a user to request to join the specified organization.
    /// </summary>
    internal class Organizations_JoinOrgCommand : ISlashCommand
    {
        const string name = "join_org";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        private bool requestSent = false;


        public Organizations_JoinOrgCommand(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync(true);
            _ = Task.Run(async () =>
            {
                string message = await GetMessage(command);

                if (requestSent)
                {
                    await command.DeleteOriginalResponseAsync();
                }
                else
                {

                    await command.ModifyOriginalResponseAsync(response =>
                    {
                        response.Content = message;
                        response.Flags = MessageFlags.Ephemeral;
                    });
                }

            });
        }


        async Task<string> GetMessage(SocketSlashCommand command)
        {
            // First, check if the user has permission to use this command.
            var user = client.GetGuild(Settings.P1RepublicGuildId)?.GetUser(command.User.Id);
            if (user == null)
                return "Could not find user info.";


            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();


                // Check if the user that invoked this command is already a member of an organization.
                OrganizationMember? member = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.FirstOrDefaultAsync(x => x.UserId == command.User.Id)
                                                                                     : null;
                if (member != null)
                    return "You are already in an organization so you cannot join another.";


                // Try to get the id option.
                SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
                ulong orgId = 0;
                if (idOption == null)
                {
                    return "Please provide the Id of the organization you wish to join.";
                }
                else
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    orgId = (ulong)(long)idOption.Value;
                }


                // Check if there is an organization with this Id.
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(x => x.Id == orgId)
                                                                      : null;
                if (org == null)
                    return "There is no organization with this Id.";


                // Check if the organization has room for a new member
                List<OrganizationMember>? members = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.Where(x => x.OrganizationId == orgId).ToListAsync()
                                                                                            : null;
                if (members == null || members.Count == 0)
                    return "This organization has no members. There must be an error in the database as this is normally not possible.";
                if (members.Count >= org.MaxMembers)
                    return "Sorry, you cannot join as this organization is already full.";


                // Find the organization leader
                SocketUser leader = client.GetUser(org.LeaderID);
                if (leader == null)
                    return $"Could not find the user object for the leader of the \"{org.Name}\" organization.";

                // Send a DM to the organization's team lead so they can accept or deny this join request.
                try
                {
                    await leader.SendMessageAsync($"<@{command.User.Id}> has requested to join your organization.", components: CreateJoinRequest());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: An error occurred while trying to send a direct message to the leader of the organization, leader.Name:\n" +
                                $"\"{ex.Message}\"\n" +
                                $"Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");

                    return $"An error occurred while trying to send a direct message to the leader of the organization, <@{leader.Id}>.";
                }


                // This causes the message content to be set to null. We don't need it since we are using an embed for the content.
                return $"A request notification has been sent to the \"{org.Name}\" organization's team lead, <@{leader.Id}>. You will receive a direct message once they accept or deny your join request.";
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

        /// <summary>
        /// This function creates the join request we will send to the organization's team lead.
        /// </summary>
        /// <returns>The join request message to send to the team lead.</returns>
        private static MessageComponent CreateJoinRequest()
        {
            // Build the join request we will send to the team lead
            ActionRowBuilder row1Builder = new ActionRowBuilder()
                .WithButton("Accept", "accept_join_org", ButtonStyle.Primary)
                .WithButton("Deny", "deny_join_org", ButtonStyle.Secondary);                

            ComponentBuilder components = new ComponentBuilder()
                .WithRows(new List<ActionRowBuilder>() { row1Builder });

            return components.Build();
        }

        static SlashCommandProperties CreateNewProperties()
        {
            return new SlashCommandBuilder()
                .WithName(name)
                .WithDescription("Sends a request to join the specified information. The team lead will accept or deny it soon after.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the organization to join", true)
                .Build();
        }

        
    }
}
