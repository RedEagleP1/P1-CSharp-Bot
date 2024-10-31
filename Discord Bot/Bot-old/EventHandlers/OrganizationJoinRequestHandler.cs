using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.EventHandlers
{
    internal class OrganizationJoinRequestHandler : IEventHandler
    {
        private readonly DiscordSocketClient _client;
        public OrganizationJoinRequestHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public void Subscribe()
        {
            _client.ButtonExecuted += OnButtonExecuted;
        }

        public async Task OnButtonExecuted(SocketMessageComponent component)
        {
            // Extract the Id of the user requesting to join the organization from the content string.
            // The component.Message.Author object was giving me the Id of the Discord bot instead of the
            // user who triggered this request. That is most likely just because the message to the
            // team lead was indeed sent by the bot.
            OrgDataUtils.ExtractSenderIdFromMessageContent(component.Message.Content, out ulong? senderId);
            if (senderId == null)
            {
                Console.WriteLine("ERROR: Sender Id is null!");
                return;
            }
            

            SocketUser teamLead = component.User;
            SocketUser sender = _client.GetUser((ulong) senderId);

            using var context = DBContextFactory.GetNewContext();


            Organization? org = null;

            await DBReadWrite.LockReadWrite();
            try
            {
                org = await OrgDataUtils.GetOrgFromLeaderId(component.User.Id, context);
                if (org == null)
                {
                    Console.WriteLine("ERROR: OrganizationJoinRequestHandler failed. The organization is null!");
                    return;
                }


                await HideAcceptAndDenyButtons(component);


                // We can now check for our custom id
                switch (component.Data.CustomId)
                {
                    // Check if the accept button was clicked.
                    case "accept_join_org":
                        // Make the requester a new member of the organization.
                        bool result = await AddUserToOrganization(sender.Id, org, context);
                        if (result)
                        {
                            // Lets sending a direct message saying their join request has been accepted.
                            await sender.SendMessageAsync($"Team lead <@{teamLead.Id}> has confirmed your join request. You are now a member of the \"{org.Name}\" organization!");
                        }
                        else
                        {
                            // Lets sending a direct message saying their join request has been denied.
                            await sender.SendMessageAsync($"Sorry, an error occurred while trying to add you to the \"{org.Name}\" organization. Contact the team lead, {teamLead.Username}.");
                        }
                        break;

                    // Check if the deny button was clicked.
                    case "deny_join_org":
                        // Lets sending a direct message saying their join request has been denied.
                        await sender.SendMessageAsync($"Sorry, team lead <@{component.User.Id}> has denied your request to join the \"{org.Name}\" organization.");

                        break;

                } // end switch


            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while trying to handle the response to this request to join the \"{org.Name}\" organization! JoinRequester={sender.Username}  TeamLead={teamLead.Username}:\n\"{ex.Message}\"\nInner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
            }
            finally
            {
                DBReadWrite.ReleaseLock();
            }

        }

        private async Task HideAcceptAndDenyButtons(SocketMessageComponent component)
        {
            // Make the buttons disappear now that the team lead has clicked on one of them.
            await component.DeferAsync();
            await component.ModifyOriginalResponseAsync(msg =>
            {
                msg.Content = component.Message.Content;
                msg.Components = null; // Just remove all components from the message, thus removing the buttons.
            });
        }

        /// <summary>
        /// Adds the specified user as a new member of the organization.
        /// </summary>
        /// <param name="userId">The Id of the user to add.</param>
        /// <param name="org">The organization.</param>
        /// <param name="context">The database context.</param>
        /// <returns>True if the user was added successfully, false otherwise.</returns>
        private async Task<bool> AddUserToOrganization(ulong userId, Organization org, ApplicationDbContext context)
        {
            try
            {
                // Add the user as a new member of the organization.
                context.OrganizationMembers.Add(new OrganizationMember()
                {
                    UserId = userId,
                    OrganizationId = org.Id,
                });

                // Save changes to database.
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while trying to add <@{userId}> as a new member of the \"{org.Name}\" organization.\nException: \"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                return false;
            }


            return true;
        }

    }
}
