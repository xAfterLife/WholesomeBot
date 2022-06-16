using Discord;
using Discord.Commands;
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

    [Command("components")]
    public Task Components()
    {
        var menuBuilder = new SelectMenuBuilder()
                          .WithPlaceholder("Select an option")
                          .WithCustomId("menu-1")
                          .WithMinValues(1)
                          .WithMaxValues(1)
                          .AddOption("Shy Neko", "opt-a", "My sweetie")
                          .AddOption("Option B", "opt-b", "There is no Option B");

        var builder = new ComponentBuilder().WithSelectMenu(menuBuilder).WithButton("label", "custom-id");

        return ReplyAsync("Who's cuter?", components: builder.Build());
    }

    [Command("verify")]
    public async Task Verify([Remainder] string text)
    {
        var user = await VRChatApiService.GetUserByDisplayname(text);
        if ( user == null )
            return;
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
