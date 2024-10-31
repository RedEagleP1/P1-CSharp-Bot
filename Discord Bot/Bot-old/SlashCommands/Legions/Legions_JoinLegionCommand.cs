using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.ComponentModel;

namespace Bot.SlashCommands.Legions
{
    /// <summary>
    /// This command allows a team lead to request to join the specified legion.
    /// </summary>
    internal class Legions_JoinLegionCommand : ISlashCommand
    {
        const string name = "join_legion";
        readonly SlashCommandProperties properties = CreateNewProperties();

        private DiscordSocketClient client;

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        private bool requestSent = false;


        public Legions_JoinLegionCommand(DiscordSocketClient client)
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


                // Check if the user that invoked this command is an organization leader.
                Organization? org = await UserDataUtils.CheckIfUserIsAnOrgLeader(command.User.Id, context);
                if (org == null)
                    return "You are not a team lead so you cannot join your organization to a legion.";


                // Try to get the id option.
                SocketSlashCommandDataOption? idOption = command.Data.Options.FirstOrDefault(x => x.Name == "id");
                ulong legionId = 0;
                if (idOption == null)
                {
                    return "Please provide the Id of the legion you wish to join.";
                }
                else
                {
                    // This double cast looks silly, but when I casted directly to ulong it kept crashing with an invalid cast error for some reason.
                    legionId = (ulong)(long)idOption.Value;
                }


                // Check if there is an legion with this Id.
                Legion? legion = context.Legions.Count() > 0 ? await context.Legions.FirstOrDefaultAsync(x => x.Id == legionId)
                                                                      : null;
                if (legion == null)
                    return "There is no legion with this Id.";



				// Get the relevant team settings record.
				TeamSettings? teamSettings = TeamSettingsUtils.GetTeamSettingsForGuild(org.GuildId, context);
				if (teamSettings == null)
				{
					teamSettings = TeamSettings.CreateDefault(org.GuildId);

					// Add the new team settings record into the database.
					context.TeamSettings.Add(teamSettings);
					await context.SaveChangesAsync();
				}


				// Check if the legion has room for a new member
				List<LegionMember>? members = context.LegionMembers.Count() > 0 ? await context.LegionMembers.Where(x => x.LegionId == legionId).ToListAsync()
                                                                                : null;
                if (members != null && members.Count >= teamSettings.MaxOrgsPerLegion)
                    return "Sorry, you cannot join as this legion is already full.";


                // Find the legion leader
                SocketUser leader = client.GetUser(legion.LeaderID);
                if (leader == null)
                    return $"Could not find the user object for the leader of the \"{legion.Name}\" legion.";

                // Send a DM to the legion's leader so they can accept or deny this join request.
                try
                {
                    await leader.SendMessageAsync($"<@{command.User.Id}> has requested to join their organization ({org.Name}) with your legion.", components: LegionDataUtils.CreateJoinRequest());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: An error occurred while trying to send a direct message to the leader of the legion, {leader.Username}:\n" +
                                $"\"{ex.Message}\"\n" +
                                $"Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");

                    return $"An error occurred while trying to send a direct message to the leader of the legion, <@{leader.Id}>.";
                }


                // This causes the message content to be set to null. We don't need it since we are using an embed for the content.
                return $"A join request notification has been sent to the \"{legion.Name}\" legion's leader, <@{leader.Id}>. You will receive a direct message once they accept or deny your join request.";
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
                .WithDescription("Sends a request to join the specified legion. The legion leader will accept or deny it soon after.")
                .AddOption("id", ApplicationCommandOptionType.Integer, "The Id of the legion to join", true)
                .Build();
        }

        
    }
}
