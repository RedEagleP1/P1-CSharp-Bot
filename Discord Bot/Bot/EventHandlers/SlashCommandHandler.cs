using Bot.SlashCommands;
using Bot.SlashCommands.ResponseHelpers;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Models;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace Bot.EventHandlers
{
    public class SlashCommandHandler : IEventHandler
    {
        readonly DiscordSocketClient client;
        readonly List<ISlashCommand> slashCommands;
        readonly List<IRespondToButtonsAndModals> commandsThatRespondToButtonsAndModals = new();
        public SlashCommandHandler(DiscordSocketClient client, List<ISlashCommand> slashCommands)
        {
            this.client = client;
            this.slashCommands = slashCommands;
        }

        public void Subscribe()
        {
            client.SlashCommandExecuted += OnSlashCommandExecuted;

            foreach(var command in slashCommands)
            {
                if(command is INeedAwake awake)
                {
                    awake.Awake();
                }

                if(command is IRespondToButtonsAndModals respondToButtonsAndModals)
                {
                    commandsThatRespondToButtonsAndModals.Add(respondToButtonsAndModals);
                }
            }

            client.ButtonExecuted += OnButtonExecuted;
            client.ModalSubmitted += OnModalSubmitted;
        }

        private async Task OnSlashCommandExecuted(SocketSlashCommand command)
        {
            foreach (var slashCommand in slashCommands)
            {
                if (slashCommand.Name == command.Data.Name)
                {
                    await slashCommand.HandleCommand(command);
                    return;
                }
            }
        }

        private async Task OnButtonExecuted(SocketMessageComponent component)
        {
            if(await ModalLauncher.LaunchModalIfNeeded(component))
            {
                return;
            }


            try
            {
                await component.DeferAsync(ephemeral: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught:\n" +
                                  $"    EXCEPTION:\n    \"{ex.Message}\" +" +
                                  $"    INNER EXCEPTION: \"{(ex.InnerException != null ? ex.InnerException.Message : "")}\"");
            }


            _ = Task.Run(async () =>
            {
                try
                {
                    var interceptor = new RequestInterceptor(component);
                    await interceptor.Process();
                    foreach (var command in commandsThatRespondToButtonsAndModals)
                    {
                        if (command.Name == interceptor.TargetCommand)
                        {
                            await command.OnRequestReceived(interceptor.Request);
                            return;
                        }
                    }
                }
                catch(Exception exceptionToLog)
                {
                    Console.WriteLine(exceptionToLog);
                }
            });
        }

        private async Task OnModalSubmitted(SocketModal modal)
        {
            await modal.DeferAsync(ephemeral: true);

            _ = Task.Run(async () =>
            {
                try
                {
                    var interceptor = new RequestInterceptor(modal);
                    await interceptor.Process();
                    foreach (var command in commandsThatRespondToButtonsAndModals)
                    {
                        if (command.Name == interceptor.TargetCommand)
                        {
                            await command.OnRequestReceived(interceptor.Request);
                            return;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}
