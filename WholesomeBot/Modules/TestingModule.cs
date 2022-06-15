using Discord.Commands;
using Discord.WebSocket;
using WholesomeBot.Services;

namespace WholesomeBot.Modules;

// Modules must be public and inherit from an IModuleBase
public class TestingModule : ModuleBase<SocketCommandContext>
{
    // Dependency Injection will fill this value 
    public VRChatApiService VRChatApiService { get; set; } = null!;
    public VerificationService VerificationService { get; set; } = null!;

    [Command("debugerror")]
    public Task DebugError([Remainder] string text)
    {
        throw new Exception(text);
    }

    [Command("verify")]
    public async Task Verify([Remainder] string text)
    {
        var user = await VRChatApiService.GetUserByDisplayname(text);
        if (user == null) return;
        VerificationService.AddVerifiedUser(user.Id, Context.User.Id);
    }

    [Command("test")]
    public Task test([Remainder] string text)
    {
        var players = text.Split(";");
        _ = VRChatApiService.GetInviteForPlayer(players[0], players[1]);
        return Task.CompletedTask;
    }
}
