using Discord;
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
    public SharedInstanceService SharedInstanceService { get; set; } = null!;

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

    [Command("createinstance")]
    public async Task CreateInstance()
    {
        var guid = await SharedInstanceService.CreateInstance(Context.User.Id);
        if (guid == null)
        {
            await ReplyAsync($"{Context.User.Mention} something went wrong.. sad\n[Guid not found]");
            return;
        }

        var instance = await SharedInstanceService.GetInstance(guid.Value);
        if (instance == null)
        {
            await ReplyAsync($"{Context.User.Mention} something went wrong.. sad\n[Instance not found]");
            return;
        }

        var verifiedUser = VerificationService.GetVerifiedUser(Context.User.Id);
        if (verifiedUser == null)
        {
            await ReplyAsync($"{Context.User.Mention} something went wrong.. sad\n[verifiedUser not found]");
            return;
        }

        var user = await VRChatApiService.GetUserById(verifiedUser.VrcId);

        var componentBuilder = new ComponentBuilder();
        componentBuilder.WithButton("Invite me", (guid).ToString());

        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithColor(VRChatApiService.GetTrustColor(user));

        embedBuilder.WithAuthor($"{user.DisplayName}", user.CurrentAvatarImageUrl, @"https://vrchat.com/home/user/" + user.Id);
        embedBuilder.WithDescription(instance.WorldName);

        embedBuilder.WithImageUrl(await VRChatApiService.GetWorldImageUrl(instance.WorldName));
        embedBuilder.WithFooter("Find out more about user with \'findvruser <UserID>\'");

        await ReplyAsync(embed: embedBuilder.Build(), components: componentBuilder.Build());
    }

    [Command("verify")]
    public async Task Verify([Remainder] string text)
    {
        var user = await VRChatApiService.GetUserByDisplayname(text);

        if (user == null)
        {
            await ReplyAsync("Can't find you");
            return;
        }

        if (VerificationService.AddVerifiedUser(user.Id, Context.User.Id))
            await ReplyAsync("Yes yes done");
        else
            await ReplyAsync("Nope won't do");
    }

    [Command("test")]
    public Task test([Remainder] string text)
    {
        var players = text.Split(";");
        _ = VRChatApiService.GetInviteForPlayer(players[0], players[1]);
        return Task.CompletedTask;
    }
}