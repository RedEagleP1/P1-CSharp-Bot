using Bot_Application.Commands;
using Discord;
using Discord_Client;
using Discord_Client.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiscordClient.Controllers;

[ApiController]
[Route("[controller]")]
public class BotManagementController : ControllerBase
{
    LoggingService _loggingService;
    IDiscordBot _discordBot;
    

    public BotManagementController(LoggingService loggingService, IDiscordBot botService)
    {
        _loggingService = loggingService;
        _discordBot = botService;
    }


    [HttpGet]
    public async Task<IActionResult> ReloadBotCommands([FromBody] List<DiscordCommandOption> commandOptions)
    {
        await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "Discord_Client", "Reloading bot commands..."));

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> StartBot()
    {
        await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "Discord_Client", "Starting bot..."));
        await _discordBot.StartAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> RestartBot()
    {
        await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "Discord_Client", "Restarting bot..."));
        await _discordBot.RestartAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> CloseBot()
    {
        await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "Discord_Client", "Closing bot..."));
        await _discordBot.CloseAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ReloadReponseCache()    
    {
        await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "Discord_Client", "Reloading response cache..."));

        return Ok();
    }

}