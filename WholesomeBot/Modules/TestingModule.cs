using Discord.Commands;
using WholesomeBot.Services;

namespace WholesomeBot.Modules;

// Modules must be public and inherit from an IModuleBase
public class TestingModule : ModuleBase<SocketCommandContext>
{
    // Dependency Injection will fill this value 
    public VRChatApiService VRChatApiService { get; set; } = null!;

    [Command("debugerror")]
    public Task DebugError([Remainder] string text)
    {
        throw new Exception(text);
    }

    [Command("joinvrinstance")]
    public Task JoinVrInstance([Remainder] string text)
    {
        _ = VRChatApiService.RequestInvite(text);
        return Task.CompletedTask;
    }

    [Command("test")]
    public Task test([Remainder] string text)
    {
        var players = text.Split(";");
        _ = VRChatApiService.GetInviteForPlayer(players[0], players[1]);
        return Task.CompletedTask;
    }
}
