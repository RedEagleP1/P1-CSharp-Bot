
using Bot_Application.Commands;
using Bot_Infrastructure.HttpClients;
using Discord;
using Discord.WebSocket;
using Discord_Client;
using Discord_Client.Config;
using Discord_Client.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var services = builder.Services;
services.Configure<Configuration>(builder.Configuration.GetSection("Configuration"));

services.AddControllers();
services.AddSingleton<LoggingService>();
services.AddSingleton<ResponseCache>();
services.AddSingleton(config => new DiscordSocketConfig(){
    LogLevel = LogSeverity.Debug,
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
    MessageCacheSize = 100
});
services.AddSingleton<DiscordSocketClient>();
services.AddSingleton<ICreateDynamicCommands, CreateDynamicCommands>();
services.AddSingleton<IDiscordBot, DiscordBot>();
services.AddHttpClient();
services.AddSingleton<IHttpClient, BotHttpClient>();
var app = builder.Build();

//todo Implement the discord bot first
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();
var discordBot = app.Services.GetRequiredService<IDiscordBot>();
await discordBot.StartAsync();
app.Run();


