// See https://aka.ms/new-console-template for more information

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using BotInfrastructure.HttpClients;

var services = new ServiceCollection()
    .AddHttpClient()
    .BuildServiceProvider();

var discordSocket = new DiscordSocketClient(new DiscordSocketConfig()
{
    GatewayIntents = GatewayIntents.All,
});


await discordSocket.LoginAsync(TokenType.Bot, "YOUR_BOT_TOKEN");
await discordSocket.StartAsync();

await Task.Delay(-1);
