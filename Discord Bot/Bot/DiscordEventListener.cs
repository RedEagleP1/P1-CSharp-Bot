using Bot.Commands;
using Discord.Commands;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Bot
{
    public class DiscordEventListener
    {
        private readonly CancellationToken _cancellationToken;

        private readonly DiscordSocketClient _client;
        private readonly IServiceScopeFactory _serviceScope;
        private readonly CommandContextContainer _commandContextContainer;

        public DiscordEventListener(DiscordSocketClient client, IServiceScopeFactory serviceScope, CommandContextContainer container)
        {
            _client = client;
            _serviceScope = serviceScope;
            _cancellationToken = new CancellationTokenSource().Token;
            _commandContextContainer = container;
        }

        private IMediator Mediator
        {
            get
            {
                var scope = _serviceScope.CreateScope();
                return scope.ServiceProvider.GetRequiredService<IMediator>();
            }
        }

        public Task StartAsync()
        {
            _client.SlashCommandExecuted += OnSlashCommandExecuted;

            return Task.CompletedTask;
        }

        public Task OnSlashCommandExecuted(SocketSlashCommand command)
        {
            foreach (var context in _commandContextContainer.CommandContexts)
            {
                if (command.Data.Name == context)
                {
                    return Mediator.Send(Activator.CreateInstance<IDiscordCommandMediatrRegisteredTypeWithHandler>());
                }
            }
            // could use swtitch on the command name
            // var ourCommand = new OurSlashCommand();
            // return Mediator.Send(ourCommand);
        }
    }

    public interface IDiscordCommandMediatrRegisteredTypeWithHandler
    {
        Task<Unit> Handle(SocketSlashCommand command);
    }

}