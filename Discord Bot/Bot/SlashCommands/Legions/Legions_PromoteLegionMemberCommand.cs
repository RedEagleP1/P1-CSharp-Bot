using Bot.SlashCommands.DbUtils;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command allows a legion leader to promote someone else to be the legion leader.
    /// </summary>
    internal class Legions_PromoteLegionMemberCommand : ISlashCommand
    {
        const string name = "promote_legion_member";
        readonly SlashCommandProperties properties = CreateNewProperties();

        public string Name => name;
        public SlashCommandProperties Properties => properties;

        private bool hadError = false;


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


                // Check if the user that invoked this command is a legion leader.
                Legion? legion = await UserDataUtils.CheckIfUserIsALegionLeader(command.User.Id, context);
                if (legion == null)
                    return "You are not a legion leader.";

                
                // Try to get the specified user.
                SocketUser? targetUser = null;
                try
                {
                    var memberObject = command.Data.Options.FirstOrDefault(option => option.Name == "member");
                    if (memberObject == null || memberObject.Value.GetType() != typeof(SocketGuildUser))
                    {
                        return "Failed to get the target user info.";
                    }
                    targetUser = memberObject.Value as SocketGuildUser;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: An error occurred:\n\"{ex.Message}\"\n    Inner Exception: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
                    return "An error occurred while finding the target user.";
                }


                // Check if the specified user is a member of this legion.
                LegionMember? tempLegionMember = await UserDataUtils.CheckIfUserIsInALegion(targetUser.Id, context);
                if (tempLegionMember == null)
                    return $"The specified user is not a member of the \"{legion.Name}\" legion.";


                if (targetUser.Id == legion.LeaderID)
                    return $"You can't promote yourself. You already are the leader of the \"{legion.Name}\" legion.";


                // Promote the target user.
                legion.LeaderID = targetUser.Id;
                context.Legions.Update(legion);

                // Save changes to database.
                await context.SaveChangesAsync();


                // Return the succes message.
                hadError = false;
                return $"<@{targetUser.Id}> has been promoted to be the leader of the \"{legion.Name}\" legion!";
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
                .WithDescription("Allows a legion leader to promote someone else to be the new legion leader.")
                .AddOption("member", ApplicationCommandOptionType.User, "The member who will be promoted", isRequired: true)
                .Build();
        }
    }
}
