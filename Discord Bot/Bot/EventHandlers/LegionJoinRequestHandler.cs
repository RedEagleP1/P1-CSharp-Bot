using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Models;

namespace Bot.EventHandlers
{
    internal class LegionJoinRequestHandler : IEventHandler
    {
        private readonly DiscordSocketClient _client;


        public LegionJoinRequestHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public void Subscribe()
        {
            _client.ButtonExecuted += OnButtonExecuted;
        }

        public async Task OnButtonExecuted(SocketMessageComponent component)
        {
            // Extract the Id of the user requesting to join their organization into a legion from the content string.
            // The component.Message.Author object was giving me the Id of the Discord bot instead of the
            // user who triggered this request. That is most likely just because the message to the
            // team lead was indeed sent by the bot.
            LegionDataUtils.ExtractSenderIdFromMessageContent(component.Message.Content, out ulong? senderId);
            if (senderId == null)
            {
                Console.WriteLine("ERROR: Sender Id is null!");
                return;
            }
            

            SocketUser legionLeader = component.User;
            SocketUser sender = _client.GetUser((ulong) senderId);


            Legion? legion = null;
            Organization? org = null;

            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();
                
                legion = await LegionDataUtils.GetLegionFromLeaderId(component.User.Id, context);
                org = await OrgDataUtils.GetOrgFromLeaderId(sender.Id, context);

                if (legion == null)
                {
                    Console.WriteLine("ERROR: LegionJoinRequestHandler failed. The legion is null!");
                    return;
                }    
                else if (org == null)
                {
                    Console.WriteLine("ERROR: LegionJoinRequestHandler failed. The organization is null!");
                    return;
                }


                await HideAcceptAndDenyButtons(component);

                Console.WriteLine($"{senderId}    {legionLeader.Id}");

                // We can now check for our custom id
                switch (component.Data.CustomId)
                {
                    // Check if the accept button was clicked.
                    case "accept_join_legion":
                        // Make the requester's organization a new member of the legion.
                        bool result = await AddOrgToLegion(org, legion, context);
                        if (result)
                        {
                            // Send a direct message saying their join request has been accepted.
                            await sender.SendMessageAsync($"Legion leader <@{legionLeader.Id}> has confirmed your join request. You are now a member of the \"{legion.Name}\" legion!");
                        }
                        else
                        {
                            // Show an error message.
                            await sender.SendMessageAsync($"Sorry, an error occurred while trying to add you to the \"{legion.Name}\" legion. Contact the legion leader, {legionLeader.Username}.");
                        }
                        break;

                    // Check if the deny button was clicked.
                    case "deny_join_legion":
                        // Send a direct message saying their join request has been denied.
                        await sender.SendMessageAsync($"Sorry, legion leader <@{legionLeader.Id}> has denied your request to join the \"{legion.Name}\" legion.");

                        break;

                } // end switch


            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while trying to handle the response to this request to join the \"{legion.Name}\" legion! JoinRequester={sender.Username}  LegionLeader={legionLeader.Username}:\n\"{ex.Message}\"\nInner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
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
        /// Adds the specified organization as a new member of the legion.
        /// </summary>
        /// <param name="org">The organization to add to the legion.</param>
        /// <param name="legion">The legion.</param>
        /// <param name="context">The database context.</param>
        /// <returns>True if the organization was added successfully, false otherwise.</returns>
        private async Task<bool> AddOrgToLegion(Organization org, Legion legion, ApplicationDbContext context)
        {
            try
            {
                // Add the user as a new member of the organization.
                context.LegionMembers.Add(new LegionMember()
                {                    
                    OrganizationId = org.Id,
                    LegionId = legion.Id
                });

                // Save changes to database.
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: An error occurred while trying to add a the \"{org.Name}\" organization to the \"{legion.Name}\" legion.\nException: \"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                return false;
            }


            return true;
        }

    }
}
