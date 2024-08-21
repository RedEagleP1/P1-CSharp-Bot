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
    /// Pings all members in the legion of the user who invoked this command.
    /// </summary>
    internal class Legions_PingLegionCommand : ISlashCommand
    {
        const string name = "ping_legion";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        private bool hadError = false;


        public Legions_PingLegionCommand(DiscordSocketClient client)
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
              
                bool isAdmin = (user.Roles.FirstOrDefault(x => x.Name == Legion.MODERATOR_ROLE) == null);

                // Check if the user that invoked this command is a member of a legion.
                LegionMember? legionMember = await UserDataUtils.CheckIfUserIsInALegion(command.User.Id, context);
                if (legionMember == null)
                    return "You cannot ping everyone in your legion since you are not a member of any legion.";


                // Try to get the id option.
                SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
                ulong legionId = 0;
                if (idOption == null)
                {
                    legionId = 0;
                }
                else
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    legionId = (ulong)(long)idOption.Value;
                }


                // If the user supplied an Id that isn't their own legion, then check if they have permission to ping any legion they want.
                if (idOption != null && legionId != legionMember.LegionId && !isAdmin)
                {
                    return "You do not have permission to ping any legion you want by Id.";
                }


                // Find the legion.
                Legion? legion = context.Legions.Count() > 0 ? await context.Legions.FirstOrDefaultAsync(o => o.Id == legionId)
                                                             : null;
                if (legion == null)
                    return $"Could not find a legion with Id {legionId}.";


                // Get a list of all the legion's orgs.
                List<LegionMember>? members = context.LegionMembers.Count() > 0 ? await context.LegionMembers.Where(m => m.LegionId == legion.Id).ToListAsync()
                                                                                : null;
                if (members == null)
                    return "Failed to get a list of all organizations in your legion.";
                else if (members.Count < 1)
                    return "There are no members in this legion. There is an error in the database since this should not be possible.";


                // Build and return the succes message.
                hadError = false;

                // Fill out the EmbedBuilder.
                // NOTE: If the optional third parameter of GetMemberPingsList() is set to true below, then the
                //       person pinging the organization is excluded from the list. That means it will show an empty
                //       ping list if you run this command while being the only member of your organization.
                EmbedBuilder embedBuilder = new EmbedBuilder()
                    .WithAuthor(command.User.Username, command.User.GetAvatarUrl() ?? command.User.GetDefaultAvatarUrl())
                    .WithTitle($"{legion.Name} Members:")                   
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp();

                foreach (LegionMember lMember in members)
                {
                    // Get the organization;
                    Organization? org = context.Organizations.Count() > 0 ? await context.Organizations.FirstAsync(x => x.Id == lMember.OrganizationId)
                                                                          : null;
                    if (org == null)
                    {
                        Console.WriteLine("ERROR: The Ping_Legion command encountered a null organization while trying to ping all members. Skipping it.");
                        continue;
                    }

                    // Get the members list for this organization.
                    List<OrganizationMember>? orgMembers = context.OrganizationMembers.Count() > 0 ? await context.OrganizationMembers.Where(m => m.OrganizationId == org.Id).ToListAsync()
                                                                                                   : null;
                    if (orgMembers == null)
                    {
                        Console.WriteLine($"ERROR: The Ping_Legion command could not find an organization with Id {org.Id}. Skipping it.");
                        continue;
                    }


                    // Add the members ping list for this organization to the embed.
                    embedBuilder.AddField($"Organization:  { org.Name}", OrgDataUtils.GetMemberPingsList(command.User.Id, client, orgMembers) + "_\n");

                } // end foreach


                // Insert the embed into the response
                await command.ModifyOriginalResponseAsync(response =>
                {
                    response.Embed = embedBuilder.Build();
                    response.Flags = MessageFlags.Ephemeral;
                });

                // Return the message content.
                return $"<@{command.User.Id}> pinged the \"{legion.Name}\" legion.";
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
                .WithDescription("Allows a user to ping their legion. Admins can provide an id to ping any legion.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the legion to ping", false)
                .Build();
        }
    }
}
