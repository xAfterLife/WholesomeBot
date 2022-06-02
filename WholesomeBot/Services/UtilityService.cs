using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace WholesomeBot.Services;

public class UtilityService
{
    private readonly IServiceProvider _services;
    private readonly LoggingService _logger;

    public UtilityService(IServiceProvider services)
    {
        _services = services;
        _logger = _services.GetRequiredService<LoggingService>();
    }

    public ValueTask<T> DeserializeJsonAsync<T>(ReadOnlySpan<char> jsonString)
    {
        try
        {
            return JsonSerializer.DeserializeAsync<T>(new MemoryStream(Encoding.UTF8.GetBytes(jsonString.ToArray())))!;
        }
        catch (Exception e)
        {
            _ = _logger.LogAsync("", e);
            return default;
        }
    }

    public static Task EmbedAddEmptyField(EmbedBuilder embedBuilder)
    {
        embedBuilder.AddField("" + '\u200B', '\u200B', true);
        return Task.CompletedTask;
    }
}
