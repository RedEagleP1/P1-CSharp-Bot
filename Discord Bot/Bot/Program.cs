// See https://aka.ms/new-console-template for more information
using Bot;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Bot_Infrastructure.HttpClients;
using Discord.Interactions;
using Bot.Config;
using Bot_Application.Commands;
using Discord;
using Bot.Services;


// Create a new instance of a host
using IHost host = Host.CreateApplicationBuilder(args).Build();

// Create a new instance of a service collection
var services = new ServiceCollection();

services.AddSingleton<LoggingService>();

// Configuration
// Make sure you follow the conventions for naming env variables to be read by the configuration properly. <section>__<key>
var config = new ConfigurationBuilder()
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.AddEnvironmentVariables()
.Build();

// Add the configuration to the service collection
services.Configure<Configuration>(config.GetSection("Configuration"));
services.Configure<BackendApiConfiguration>(config.GetSection("BackendApiConfiguration"));
services.Configure<DiscordSocketClientConfiguration>(config.GetSection("DiscordSocketClientConfiguration"));

// Add the required services for the application here
//todo write an extension method for the application layer services.
// services.AddSingleton<DiscordSocketClient>();
services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig(){
    LogLevel = LogSeverity.Debug,
    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
    MessageCacheSize = 100
}));
services.AddSingleton<ICreateDynamicCommands, CreateDynamicCommands>();
services.AddSingleton<IDiscordBot, DiscordBot>();

services.AddHttpClient();
services.AddSingleton<IHttpClient, BotHttpClient>();
services.AddSingleton<CommandContextContainer>();

// build the service provider
var serviceProvider = services.BuildServiceProvider();

var bot = serviceProvider.GetRequiredService<IDiscordBot>();

await bot.StartAsync();
