using Bot;
using Microsoft.Extensions.Configuration;
using Models;
using Microsoft.EntityFrameworkCore;
using Bot.PeriodicEvents;

// Get values from the config given their key and their target type.
var settings = new Settings();
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseMySql(settings.ConnectionString, ServerVersion.AutoDetect(settings.ConnectionString))    
    .Options;

var dbContextFactory = new DBContextFactory(options);
DiscordBot bot = new DiscordBot(settings, dbContextFactory);
await bot.StartBot();

var timerCST = new TimerCST(dbContextFactory);
timerCST.EnablePeriodicEvents();

await Task.Delay(-1);