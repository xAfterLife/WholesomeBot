using Discord.Commands;
using WholesomeBot.Services;

namespace WholesomeBot.Modules;

// Modules must be public and inherit from an IModuleBase
public class VRCModule : ModuleBase<SocketCommandContext>
{
    // Dependency Injection will fill this value 
    public VRChatApiService VRChatApiService { get; set; } = null!;

    [Command("findvruser")]
    public async Task FindVrUser([Remainder] string text)
    {
        var users = await VRChatApiService.SearchUsersEmbed(text);

        if (users.Count == 0)
        {
            _ = ReplyAsync("No Matches Found");
            return;
        }

        foreach (var embed in users)
            _ = ReplyAsync(embed: embed);
    }

    [Command("friendvruser")]
    public async Task FriendVrUser([Remainder] string text)
    {
        if (!await VRChatApiService.SendFriendRequest(text))
        {
            _ = ReplyAsync("User not found or already befriended");
            return;
        }

        _ = ReplyAsync("Friend Request sent");
    }
}