using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace WholesomeBot.Services;

public class ComponentHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly LoggingService _logger;
    private readonly IServiceProvider _services;

    public ComponentHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _logger = services.GetRequiredService<LoggingService>();
        _services = services;
    }

    public Task InitializeAsync()
    {
        _discord.ButtonExecuted += DiscordButtonExecuted;
        _discord.SelectMenuExecuted += SelectMenuExecuted;
        return Task.CompletedTask;
    }

    /// <summary>
    ///     TODO: ERROR WARUM??
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    private Task SelectMenuExecuted(SocketMessageComponent component)
    {
        return component.RespondAsync($"{component.User.Mention} has chosen an option!");
    }

    /// <summary>
    ///     TODO: ERROR WARUM??
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    private Task DiscordButtonExecuted(SocketMessageComponent component)
    {
        return component.RespondAsync($"{component.User.Mention} has clicked the button!");
    }
}
