using Bot.SlashCommands.DbUtils;
using Bot.SlashCommands.Legions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Text;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// Pings all members in the organization of the user who invoked this command.
    /// </summary>
    internal class Organizations_PingOrgCommand : ISlashCommand
    {
        const string name = "ping_org";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        private bool hadError = false;


        public Organizations_PingOrgCommand(DiscordSocketClient client)
        {
            this.client = client;
        }

        public async Task HandleCommand(SocketSlashCommand command)
        {
            await command.DeferAsync();
            _ = Task.Run(async () =>
            {
                string message = await GetMessage(command);

                await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Content = message; 
                    response.Flags = hadError ? MessageFlags.Ephemeral : MessageFlags.None;
                });
            });
        }


        async Task<string> GetMessage(SocketSlashCommand command)
        {
            hadError = true;

            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();


                // Check if the user is a moderator.
                var user = client.GetGuild(Settings.P1RepublicGuildId)?.GetUser(command.User.Id);
                if (user == null)
                    return "Could not find user info.";

                bool isAdmin = (user.Roles.FirstOrDefault(x => x.Name == Organization.MODERATOR_ROLE) == null);


                // Check if the user that invoked this command is a member of a organization.
                OrganizationMember? member = await UserDataUtils.CheckIfUserIsInAnOrg(command.User.Id, context);
                if (member == null)
                    return "You cannot ping everyone in your organization since you are not a member of any organization.";


                // Try to get the id option.
                SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
                ulong orgId = 0;
                if (idOption == null)
                {
                    orgId = 0;
                }
                else
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    orgId = (ulong)(long)idOption.Value;
                }


                // If the user supplied an Id that isn't their own legion, then check if they have permission to ping any legion they want.
                if (idOption != null && orgId != member.OrganizationId && !isAdmin)
                {
                    return "You do not have permission to ping any organization you want by Id.";
                }


                // Find the organization.
                Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstOrDefaultAsync(o => o.Id == member.OrganizationId)
                                                                      : null;
                if (org == null)
                    return "$Could not find an organization with Id {org.Id}.";


                // Get a list of all the organization's members.
                List<OrganizationMember>? members = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.Where(m => m.OrganizationId == org.Id).ToListAsync()
                                                                                            : null;
                if (members == null)
                    return "Failed to get a list of all members of your organization.";
                else if (members.Count < 1)
                    return "There are no members in this organization. There is an error in the database since this should not be possible.";


                // Build and return the succes message.
                hadError = false;

                // Fill out the EmbedBuilder.
                // NOTE: If the optional third parameter of GetMemberPingsList() is set to true below, then the
                //       person pinging the organization is excluded from the list. That means it will show an empty
                //       ping list if you run this command while being the only member of your organization.
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithAuthor(command.User.Username, command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                    .WithTitle($"{org.Name} Members:")
                    .WithDescription(OrgDataUtils.GetMemberPingsList(command.User.Id, client, members, true)) // This adds the members list into the embed.
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp();

                // Insert the embed into the response
                await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Embed = embedBuilder.Build();
                    response.Flags = MessageFlags.Ephemeral;
                });

                // Return the message content.
                return $"<@{command.User.Id}> pinged the \"{org.Name}\" organization.";
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
                .WithDescription("Allows a user to ping their organization. Admins can provide an id to ping any org they want.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the organization to ping", false)
                .Build();
        }
    }
}
