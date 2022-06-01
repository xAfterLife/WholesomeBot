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
    public async Task JoinVrInstance([Remainder] string text)
    {
        await VRChatApiService.RequestInvite(text);
    }

    [Command("invitevruser")]
    public Task InviteVrUser([Remainder] string text)
    {
        _ = VRChatApiService.Invite(text);
        return Task.CompletedTask;
    }
}
