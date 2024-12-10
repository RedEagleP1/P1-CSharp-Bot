using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord_Client.Services
{
    public class LoggingService
    {
        public LoggingService(DiscordSocketClient client)
        {
            client.Log += LogAsync;
        }

        public Task LogAsync(LogMessage logMessage)
        {
            if (logMessage.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{logMessage.Severity}] {cmdException.Command.Aliases.First()}"+
                    $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine("Reason: \"{cmdException}\"");
            }
            else
            {
                Console.WriteLine($"[General/{logMessage.Severity}] {logMessage}");
            }


            return Task.CompletedTask;
        }
    }
}