using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bot.OneTimeRegister
{
    public static class SlashCommandsRegister
    {        
        public static async Task RegisterCommand(SocketGuild guild, SlashCommandProperties properties)
        {                
            try
            {
                await guild.CreateApplicationCommandAsync(properties);
            }
            catch (HttpException e)
            {                
                var json = JsonConvert.SerializeObject(e.Errors.FirstOrDefault(), Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public static async Task DeleteCommand(SocketGuild guild, string name)
        {
            foreach(var command in await guild.GetApplicationCommandsAsync())
            {
                if(command.Name == name)
                {
                    await command.DeleteAsync();
                    break;
                }

            }
        }
    }
}
