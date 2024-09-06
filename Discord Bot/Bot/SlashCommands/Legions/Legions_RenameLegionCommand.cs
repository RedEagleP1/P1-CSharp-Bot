using Bot.SlashCommands.DbUtils;
using Bot.SlashCommands.Legions;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Security.Cryptography;

namespace Bot.SlashCommands.Organizations
{
    /// <summary>
    /// This command renames the legion of the legion leader who invoked it.
    /// Only the legion leader may use this command.
    /// </summary>
    internal class Legions_RenameLegionCommand : ISlashCommand
    {
        const string name = "rename_legion";
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


            // Try to get the name option.
            SocketSlashCommandDataOption? nameOption = command.Data.Options.FirstOrDefault(x => x.Name == "name");
            if (nameOption == null)
                return "Please provide a new name for the legion.";

            // Check if the name option contains a valid value.
            string? newLegionName = nameOption.Value.ToString();
            if (newLegionName == null || newLegionName.Length < Legion.DEFAULT_MIN_NAME_LENGTH)
            {
                return $"Please provide a new name that is at least {Legion.DEFAULT_MIN_NAME_LENGTH} characters long for the legion.";
            }
            newLegionName = newLegionName.Trim();


            await DBReadWrite.LockReadWrite();
            try
            {
                using var context = DBContextFactory.GetNewContext();

                // Check if the user who invoked this command is a legion leader.
                Legion? legion = await UserDataUtils.CheckIfUserIsALegionLeader(command.User.Id, context);
                if (legion == null)
                    return "You are not a legion leader.";


                // Check if there is already an legion with this name.
                Legion? existingLegion = context.Legions.Count() > 0 ? await context.Legions.AsNoTracking().FirstOrDefaultAsync(x => x.Name == newLegionName)
                                                                              : null;
                if (existingLegion != null)
                    return "A legion with this name already exists.";


                // Change the legion's name.
                string oldName = legion.Name;
                legion.Name = newLegionName;
                context.Legions.Update(legion);

                // Save changes to database.
                await context.SaveChangesAsync();
               
                hadError = false;
                return $"The legion \"{oldName}\" has been renamed to \"{newLegionName}\".";

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
                .WithDescription("Allows the team lead to rename the organization.")
                .AddOption("name", ApplicationCommandOptionType.String, "The new name of the organization", true)
                .Build();
        }
    }
}
