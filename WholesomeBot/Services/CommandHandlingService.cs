using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace WholesomeBot.Services;

public class CommandHandlingService
{
    private const string PrefixStr = "#fuckkoza!";
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly LoggingService _logger;
    private readonly IServiceProvider _services;

    private int _argPos;

    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _logger = services.GetRequiredService<LoggingService>();
        _services = services;

        _commands.CommandExecuted += CommandExecutedAsync;
        _discord.MessageReceived += MessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        // Register modules that are public and inherit ModuleBase<T>.
        foreach (var module in await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services))
            _ = _logger.LogAsync($"{module.Name} Loaded");
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        if (rawMessage is not SocketUserMessage {Source: MessageSource.User} message)
            return;
        if (!message.HasStringPrefix(PrefixStr, ref _argPos))
            return;

        var context = new SocketCommandContext(_discord, message);
        await _commands.ExecuteAsync(context, _argPos, _services);
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        // command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;

        // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
            return;

        // the command failed, let's notify the user that something happened.
        await context.Channel.SendMessageAsync($"{result}");
    }
}