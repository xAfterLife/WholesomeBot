using System.Runtime.CompilerServices;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace WholesomeBot.Services;

public class LoggingService
{
    private readonly IServiceProvider _services;

    public LoggingService(IServiceProvider services)
    {
        _services = services;
    }

    public Task InitializeAsync()
    {
        _services.GetRequiredService<CommandService>().Log += LogAsync;
        _services.GetRequiredService<DiscordSocketClient>().Log += LogAsync;
        return Task.CompletedTask;
    }

    public Task LogAsync(string message, Exception? ex = null, [CallerMemberName] string caller = "", [CallerFilePath] string file = "")
    {
        Console.ForegroundColor = ex != null ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"{DateTime.Now.ToLongTimeString()} [{Path.GetFileName(file)} -> {caller}] ");
        Console.ForegroundColor = ConsoleColor.White;

        if ( !string.IsNullOrEmpty(message) )
            Console.WriteLine($"{message}");
        if ( ex != null )
            Console.WriteLine(ex.ToString());

        return Task.CompletedTask;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.ForegroundColor = log.Exception != null ? ConsoleColor.Red : ConsoleColor.DarkBlue;
        Console.Write($"{DateTime.Now.ToLongTimeString()} [{log.Source}] ");
        Console.ForegroundColor = ConsoleColor.White;

        if ( !string.IsNullOrEmpty(log.Message) )
            Console.WriteLine($"{log.Message}");
        if ( log.Exception != null )
            Console.WriteLine(log.Exception.ToString());

        return Task.CompletedTask;
    }
}
