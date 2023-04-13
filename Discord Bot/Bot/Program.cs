using Bot;
using Microsoft.Extensions.Configuration;
using Models;
using Microsoft.EntityFrameworkCore;
using Bot.PeriodicEvents;

// Get values from the config given their key and their target type.
Settings.Init();
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseMySql(Settings.ConnectionString, ServerVersion.AutoDetect(Settings.ConnectionString))    
    .Options;

DBContextFactory.Init(options);
DiscordBot bot = new DiscordBot();
await bot.StartBot();

await Task.Delay(-1);