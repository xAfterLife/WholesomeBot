using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using WholesomeBot.Services;

namespace WholesomeBot;

internal class Program
{
    private LoggingService _logger = null!;

    private static void Main()
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }

    public async Task MainAsync()
    {
        await using var services = ConfigureServices();
        _logger = services.GetRequiredService<LoggingService>();

        await _logger.LogAsync("Initializing Services");
        //Initialize
        await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
        await services.GetRequiredService<LoggingService>().InitializeAsync();

        await _logger.LogAsync("Logging into Discord");
        //Bot Login
        var client = services.GetRequiredService<DiscordSocketClient>();
        await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("token", EnvironmentVariableTarget.User));
        await client.StartAsync();

        await _logger.LogAsync("Bot started");
        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
               .AddSingleton<DiscordSocketClient>()
               .AddSingleton<CommandService>()
               .AddSingleton<CommandHandlingService>()
               .AddSingleton<VRChatApiService>()
               .AddSingleton<LoggingService>()
               .AddSingleton<UtilityService>()
               .AddSingleton<HttpClient>()
               .BuildServiceProvider();
    }
}
