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
    private readonly SharedInstanceService _sharedInstance;

    public ComponentHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _logger = services.GetRequiredService<LoggingService>();
        _sharedInstance = services.GetRequiredService<SharedInstanceService>();
        _services = services;
    }

    public Task InitializeAsync()
    {
        _discord.ButtonExecuted += DiscordButtonExecuted;
        _discord.SelectMenuExecuted += SelectMenuExecuted;
        _discord.ModalSubmitted += ModalSubmitted;
        return Task.CompletedTask;
    }


    /// <summary>
    ///     TODO: Implement xD
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private Task ModalSubmitted(SocketModal component)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     TODO: ERROR WENN ZU SCHNELL REAGIERT WIRD
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    private Task SelectMenuExecuted(SocketMessageComponent component)
    {
        return component.RespondAsync($"{component.User.Mention} has chosen an option!");
    }

    /// <summary>
    ///     TODO: ERROR WENN ZU SCHNELL REAGIERT WIRD
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    private Task DiscordButtonExecuted(SocketMessageComponent component)
    {
        if (!_sharedInstance.InstanceActive(component.Data.CustomId))
            return component.RespondAsync($"{component.User.Mention} Instance not active");

        if (!_sharedInstance.HasPermission(component.Data.CustomId, component.User as SocketGuildUser))
            return component.RespondAsync($"{component.User.Mention} Instance doesn't permit you to join");

        return component.RespondAsync(
            _sharedInstance.InvitePlayer(component.Data.CustomId, component.User as SocketGuildUser).Result
                ? $"{component.User.Mention} Invite has been sent"
                : $"{component.User.Mention} something went wrong.. sad");
    }
}