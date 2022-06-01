using Discord;
using Microsoft.Extensions.DependencyInjection;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;
using UserStatus = VRChat.API.Model.UserStatus;

namespace WholesomeBot.Services;

public class VRChatApiService
{
    private readonly AuthenticationApi _authApi;
    private readonly EconomyApi _economyApi;
    private readonly FavoritesApi _favoritesApi;
    private readonly FilesApi _filesApi;
    private readonly FriendsApi _friendsApi;
    private readonly InstancesApi _instanceApi;
    private readonly InviteApi _inviteApi;
    private readonly LoggingService _logger;
    private readonly NotificationsApi _notificationsApi;
    private readonly PermissionsApi _permissionsApi;
    private readonly PlayermoderationApi _playermoderationApi;
    private readonly SystemApi _systemApi;
    private readonly UsersApi _userApi;
    private readonly WorldsApi _worldApi;

    public VRChatApiService(IServiceProvider services)
    {
        _logger = services.GetRequiredService<LoggingService>();

        // Authentication credentials
        var config = new Configuration { Username = Environment.GetEnvironmentVariable("vrc_username", EnvironmentVariableTarget.User), Password = Environment.GetEnvironmentVariable("vrc_password", EnvironmentVariableTarget.User) };

        // Create instances of API's we'll need
        _authApi = new AuthenticationApi(config);
        _economyApi = new EconomyApi(config);
        _favoritesApi = new FavoritesApi(config);
        _filesApi = new FilesApi(config);
        _friendsApi = new FriendsApi(config);
        _instanceApi = new InstancesApi(config);
        _inviteApi = new InviteApi(config);
        _notificationsApi = new NotificationsApi(config);
        _permissionsApi = new PermissionsApi(config);
        _playermoderationApi = new PlayermoderationApi(config);
        _systemApi = new SystemApi(config);
        _userApi = new UsersApi(config);
        _worldApi = new WorldsApi(config);

        //Login and Post current User to Debug-Out
        var currentUser = _authApi.GetCurrentUser();
        _logger.LogAsync($"Connected as User: {currentUser.Username} ({currentUser.Id})");
    }

    //Test purposes
    public async Task Invite(string userId)
    {
        //var hidStr = $"~hidden({usr})";
        //$"{wrldId}:{instance}{hidStr}~region(eu)~nonce({Guid.NewGuid()})";

        var request = new InviteRequest(@"wrld_ee4dd872-ca55-44e1-900f-a2b7b875aeb0:45785~region(eu)", 1);
        var notif = await _inviteApi.InviteUserAsync(userId);

        _ = _logger.LogAsync(notif.Details);
    }

    public async Task RequestInvite(string userId)
    {
        await _notificationsApi.ClearNotificationsAsync();
        await _inviteApi.RequestInviteAsync(userId);

        Notification not = null!;

        while ( not == null )
        {
            foreach ( var notification in await _notificationsApi.GetNotificationsAsync() )
            {
                if ( notification.Type != NotificationType.Invite )
                {
                    await _notificationsApi.DeleteNotificationAsync(notification.Id);
                    continue;
                }

                not = notification;
            }

            await Task.Delay(1);
        }

        _ = _logger.LogAsync(not.Details);
    }

    public async Task<bool> SendFriendRequest(string userid)
    {
        var user = await _userApi.GetUserAsync(userid);
        if ( user == null || user.IsFriend )
            return false;
        _ = _friendsApi.FriendAsync(userid);
        return true;
    }

    public async Task<List<Embed>> SearchUsersEmbed(string username)
    {
        var embeds = new List<Embed>();

        try
        {
            var searchResult = await _userApi.SearchUsersAsync(username);

            foreach ( var user in searchResult )
            {
                var embedBuilder = new EmbedBuilder();

                embedBuilder.WithColor(GetTrustColor(user));

                embedBuilder.WithAuthor($"{user.DisplayName} ({user.Username}) - {GetOnlineStatus(user)}", user.CurrentAvatarImageUrl, @"https://vrchat.com/home/user/" + user.Id);
                embedBuilder.WithDescription(string.IsNullOrEmpty(user.Bio) ? "(No Bio available)" : user.Bio);

                embedBuilder.AddField("Display-Name", user.DisplayName, true);
                embedBuilder.AddField("User-Name", user.Username, true);
                _ = EmbedAddEmptyField(embedBuilder);

                embedBuilder.AddField("Status", Enum.GetName(typeof(UserStatus), user.Status), true);
                embedBuilder.AddField("Status-Description", string.IsNullOrEmpty(user.StatusDescription) ? "(Not set)" : user.StatusDescription, true);
                _ = EmbedAddEmptyField(embedBuilder);

                embedBuilder.AddField("VRC+", user.Tags.Contains("system_supporter") ? "yes" : "no", true);
                embedBuilder.AddField("Trust Rank", GetTrustRank(user.Tags), true);
                embedBuilder.AddField("Last Platform", string.IsNullOrEmpty(user.LastPlatform) ? "(Not set)" : user.LastPlatform, true);

                embedBuilder.AddField("User-ID", user.Id);

                embedBuilder.WithThumbnailUrl(user.ProfilePicOverride); //Not safed as file TODO: Find out how to display this by only linking the file
                embedBuilder.WithImageUrl(user.CurrentAvatarImageUrl);
                embedBuilder.WithFooter("To find out more about a person use \'UserInfo <UserID>\'");
                embedBuilder.WithCurrentTimestamp();

                embeds.Add(embedBuilder.Build());
            }

            return embeds;
        }
        catch ( ApiException e )
        {
            _ = _logger.LogAsync("Exception when calling API", e);
            return embeds;
        }
    }

    private static Task EmbedAddEmptyField(EmbedBuilder embedBuilder)
    {
        embedBuilder.AddField("" + '\u200B', '\u200B', true);
        return Task.CompletedTask;
    }

    private bool IsOnline(LimitedUser user)
    {
        var realUser = _userApi.GetUserAsync(user.Id).Result;
        return !string.IsNullOrEmpty(realUser.Location);
    }

    private string GetOnlineStatus(LimitedUser user)
    {
        return IsOnline(user) ? "online" : "offline";
    }

    private static string GetTrustRank(ICollection<string> tags)
    {
        var trust = "Visitor";
        if ( tags.Contains("system_trust_basic") )
            trust = "New User";
        if ( tags.Contains("system_trust_known") )
            trust = "User";
        if ( tags.Contains("system_trust_trusted") )
            trust = "Known User";
        if ( tags.Contains("system_trust_veteran") )
            trust = "Trusted User";
        if ( tags.Contains("system_trust_legend") )
            trust = "Veteran User";
        return trust;
    }

    private static Color GetTrustColor(LimitedUser user)
    {
        return GetTrustRank(user.Tags) switch
        {
            "New User"     => Color.Blue,
            "User"         => Color.Green,
            "Known User"   => Color.Orange,
            "Trusted User" => Color.Purple,
            "Veteran User" => Color.Gold,
            _              => Color.LightGrey
        };
    }
}
