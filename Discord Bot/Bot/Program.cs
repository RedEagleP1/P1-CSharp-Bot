// See https://aka.ms/new-console-template for more information
using Bot;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using BotInfrastructure.HttpClients;
using Bot.Commands;

// Create a new instance of a host
using IHost host = Host.CreateApplicationBuilder(args).Build();

// Create a new instance of a service collection
var services = new ServiceCollection();

// Add the required services for the application here
services.AddSingleton<DiscordSocketClient>();
services.AddSingleton<IDiscordBot, DiscordBot>();

services.AddTransient<IHttpClient, BotHttpClient>();
services.AddTransient<ICreateDynamicCommands, CreateDynamicCommands>();

// Configuration
// Make sure you follow the conventions for naming env variables to be read by the configuration properly. <section>__<key>
var config = new ConfigurationBuilder()
.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
.AddEnvironmentVariables()
.Build();

// Add the configuration to the service collection
//todo This is not working
services.Configure<BotConfigurationModel>(config.GetSection("Configuration"));

// build the service provider
var serviceProvider = services.BuildServiceProvider();

var bot = serviceProvider.GetRequiredService<DiscordBot>();

await bot.Start();

