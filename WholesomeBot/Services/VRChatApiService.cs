﻿using Discord;
using Microsoft.Extensions.DependencyInjection;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;
using WholesomeBot.Models;
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
    private readonly UtilityService _utility;
    private readonly WorldsApi _worldApi;

    public VRChatApiService(IServiceProvider services)
    {
        _logger = services.GetRequiredService<LoggingService>();
        _utility = services.GetRequiredService<UtilityService>();

        // Authentication credentials
        var config = new Configuration
        {
            Username = Environment.GetEnvironmentVariable("vrc_username", EnvironmentVariableTarget.User),
            Password = Environment.GetEnvironmentVariable("vrc_password", EnvironmentVariableTarget.User)
        };

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

    /// <summary>
    ///     Invites a Player to another Players Instance
    /// </summary>
    /// <param name="target"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public async Task GetInviteForPlayer(string target, string sender)
    {
        var targetUser =
            (await _userApi.SearchUsersAsync(target, null, 1)).FirstOrDefault(x => x.DisplayName == target);
        var senderUser =
            (await _userApi.SearchUsersAsync(sender, null, 1)).FirstOrDefault(x => x.DisplayName == sender);

        if (targetUser == null)
        {
            _ = _logger.LogAsync("Target not found");
            return;
        }

        if (!targetUser.IsFriend)
        {
            _ = _logger.LogAsync("Target not befriended");
            return;
        }

        if (senderUser == null)
        {
            _ = _logger.LogAsync("Sender not found");
            return;
        }

        if (!senderUser.IsFriend)
        {
            _ = _logger.LogAsync("Sender not befriended");
            return;
        }

        var notificationInviteDetails = await GetInviteDetails(targetUser.Id);

        _ = Invite(senderUser.Id, notificationInviteDetails.worldId!);
    }


    /// <summary>
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<NotificationInviteDetails> GetInviteDetails(string userId)
    {
        var notification = await RequestInvite(userId);
        return await _utility.DeserializeJsonAsync<NotificationInviteDetails>(notification.Details);
        ;
    }


    /// <summary>
    ///     Invites a userId to an Instance
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    public Task Invite(string userId, string? instanceId)
    {
        var request = new InviteRequest(instanceId);
        _ = _inviteApi.InviteUserAsync(userId, request);
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Requests an Invite to a user and returns the awaited Notification
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async ValueTask<Notification> RequestInvite(string userId)
    {
        await _notificationsApi.ClearNotificationsAsync();
        await _inviteApi.RequestInviteAsync(userId);

        return await GetInviteNotification(userId);
    }

    /// <summary>
    ///     Gets an Invite Notification matching to the UserID requested
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async ValueTask<Notification> GetInviteNotification(string userId)
    {
        Notification? notification = null;

        while (notification == null)
            notification =
                (await _notificationsApi.GetNotificationsAsync("invite")).FirstOrDefault(x =>
                    x.SenderUserId == userId)!;

        return notification;
    }

    public async ValueTask<User> GetUserById(string id)
    {
        return await _userApi.GetUserAsync(id);
    }

    public async ValueTask<LimitedUser?> GetUserByDisplayname(string displayName)
    {
        var user = await _userApi.SearchUsersAsync(displayName);
        return user.FirstOrDefault(x => x.DisplayName == displayName);
    }

    /// <summary>
    ///     Sends a Friend-Request to a user if it can be found and is not already befriended
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public async Task<bool> SendFriendRequest(string userName)
    {
        var user = (await _userApi.SearchUsersAsync(userName, null, 1)).FirstOrDefault(x => x.DisplayName == userName);
        if (user == null || user.IsFriend)
            return false;
        _ = _friendsApi.FriendAsync(user.Id);
        return true;
    }

    /// <summary>
    ///     Search Users and give them out as separate Embeds
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public async Task<List<Embed>> SearchUsersEmbed(string username)
    {
        var embeds = new List<Embed>();

        try
        {
            var searchResult = await _userApi.SearchUsersAsync(username);

            foreach (var user in searchResult)
            {
                var embedBuilder = new EmbedBuilder();

                embedBuilder.WithColor(GetTrustColor(user));

                embedBuilder.WithAuthor($"{user.DisplayName} ({user.Username}) - {GetOnlineStatus(user)}",
                    user.CurrentAvatarImageUrl, @"https://vrchat.com/home/user/" + user.Id);
                embedBuilder.WithDescription(string.IsNullOrEmpty(user.Bio) ? "(No Bio available)" : user.Bio);

                embedBuilder.AddField("Display-Name", user.DisplayName, true);
                embedBuilder.AddField("User-Name", user.Username, true);
                _ = UtilityService.EmbedAddEmptyField(embedBuilder);

                embedBuilder.AddField("Status", Enum.GetName(typeof(UserStatus), user.Status), true);
                embedBuilder.AddField("Status-Description",
                    string.IsNullOrEmpty(user.StatusDescription) ? "(Not set)" : user.StatusDescription, true);
                _ = UtilityService.EmbedAddEmptyField(embedBuilder);

                embedBuilder.AddField("VRC+", user.Tags.Contains("system_supporter") ? "yes" : "no", true);
                embedBuilder.AddField("Trust Rank", GetTrustRank(user.Tags), true);
                embedBuilder.AddField("Last Platform",
                    string.IsNullOrEmpty(user.LastPlatform) ? "(Not set)" : user.LastPlatform, true);

                embedBuilder.AddField("User-ID", user.Id);

                embedBuilder.WithThumbnailUrl(user
                    .ProfilePicOverride); //Not safed as file TODO: Find out how to display this by only linking the file
                embedBuilder.WithImageUrl(user.CurrentAvatarImageUrl);
                embedBuilder.WithFooter("To find out more about a person use \'UserInfo <UserID>\'");
                embedBuilder.WithCurrentTimestamp();

                embeds.Add(embedBuilder.Build());
            }

            return embeds;
        }
        catch (ApiException e)
        {
            _ = _logger.LogAsync("Exception when calling API", e);
            return embeds;
        }
    }

    /// <summary>
    ///     Check if user is online by looking at the Location
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private bool IsOnline(LimitedUser user)
    {
        var realUser = _userApi.GetUserAsync(user.Id).Result;
        return !string.IsNullOrEmpty(realUser.Location);
    }

    /// <summary>
    ///     Wrapper for the Online-Status function
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public string GetOnlineStatus(LimitedUser user)
    {
        return IsOnline(user) ? "online" : "offline";
    }

    /// <summary>
    ///     Get's the ThumbnailImageUrl of a WorldId (Used for Embeds)
    /// </summary>
    /// <param name="worldId"></param>
    /// <returns></returns>
    public async ValueTask<string?> GetWorldImageUrl(string? worldName)
    {
        var world = (await _worldApi.SearchWorldsAsync(search: worldName, n: 10)).FirstOrDefault(x => x.Name == worldName);
        return world?.ThumbnailImageUrl;
    }

    /// <summary>
    ///     Gets the Trust Rank of a User's Tags
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public static string GetTrustRank(ICollection<string> tags)
    {
        var trust = "Visitor";
        if (tags.Contains("system_trust_basic"))
            trust = "New User";
        if (tags.Contains("system_trust_known"))
            trust = "User";
        if (tags.Contains("system_trust_trusted"))
            trust = "Known User";
        if (tags.Contains("system_trust_veteran"))
            trust = "Trusted User";
        if (tags.Contains("system_trust_legend"))
            trust = "Veteran User";
        return trust;
    }

    /// <summary>
    ///     Converts a User to it's Rank Color to be Displayed as a Background Color in Discord
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static Color GetTrustColor(LimitedUser user)
    {
        return GetTrustRank(user.Tags) switch
        {
            "New User" => Color.Blue,
            "User" => Color.Green,
            "Known User" => Color.Orange,
            "Trusted User" => Color.Purple,
            "Veteran User" => Color.Gold,
            _ => Color.LightGrey
        };
    }

    /// <summary>
    ///     Converts a User to it's Rank Color to be Displayed as a Background Color in Discord
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static Color GetTrustColor(User user)
    {
        return GetTrustRank(user.Tags) switch
        {
            "New User" => Color.Blue,
            "User" => Color.Green,
            "Known User" => Color.Orange,
            "Trusted User" => Color.Purple,
            "Veteran User" => Color.Gold,
            _ => Color.LightGrey
        };
    }
}