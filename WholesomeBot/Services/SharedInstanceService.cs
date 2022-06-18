using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using WholesomeBot.Models;

namespace WholesomeBot.Services;

public class SharedInstanceService
{
    private readonly LoggingService _logger;
    private readonly IServiceProvider _services;

    private readonly Dictionary<Guid, VrcSharedInstance> _sharedInstances = new();
    private readonly UtilityService _utilityService;
    private readonly VerificationService _verificationService;
    private readonly VRChatApiService _vrChatApiService;

    public SharedInstanceService(IServiceProvider services)
    {
        _services = services;
        _logger = _services.GetRequiredService<LoggingService>();
        _verificationService = _services.GetRequiredService<VerificationService>();
        _vrChatApiService = _services.GetRequiredService<VRChatApiService>();
        _utilityService = _services.GetRequiredService<UtilityService>();
    }

    /// <summary>
    ///     Creates an Instance and adds it to the Dictionary - returns a Guid
    /// </summary>
    /// <param name="discordId"></param>
    /// <returns></returns>
    public async ValueTask<Guid?> CreateInstance(ulong discordId)
    {
        return await CreateInstance(discordId, null, null);
    }

    /// <summary>
    ///     Creates an Instance and adds it to the Dictionary - returns a Guid
    /// </summary>
    /// <param name="discordId"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public async ValueTask<Guid?> CreateInstance(ulong discordId, SocketRole role)
    {
        return await CreateInstance(discordId, role, null);
    }

    /// <summary>
    ///     Creates an Instance and adds it to the Dictionary - returns a Guid
    /// </summary>
    /// <param name="discordId"></param>
    /// <param name="users"></param>
    /// <returns></returns>
    public async ValueTask<Guid?> CreateInstance(ulong discordId, IEnumerable<LinkedAccountDetails> users)
    {
        return await CreateInstance(discordId, null, users);
    }

    /// <summary>
    ///     Creates an Instance and adds it to the Dictionary - returns a Guid
    /// </summary>
    /// <param name="discordId"></param>
    /// <param name="role"></param>
    /// <param name="users"></param>
    /// <returns></returns>
    private async ValueTask<Guid?> CreateInstance(ulong discordId, SocketRole? role = null,
        IEnumerable<LinkedAccountDetails>? users = null)
    {
        var user = _verificationService.GetVerifiedUser(discordId);
        if (user == null)
        {
            _ = _logger.LogAsync("Invalid user", new Exception("User not verified"));
            return null;
        }

        var inviteDetails = await _vrChatApiService.GetInviteDetails(user.VrcId);

        VrcSharedInstance instance;
        if (role != null) instance = new VrcSharedInstance(user, inviteDetails, role);
        else if (users != null) instance = new VrcSharedInstance(user, inviteDetails, users);
        else instance = new VrcSharedInstance(user, inviteDetails);


        var guid = Guid.NewGuid();
        _sharedInstances.Add(guid, instance);
        _ = _logger.LogAsync($"Instance added with Privacy-Mode {instance.PrivacyMode}");

        return guid;
    }

    /// <summary>
    ///     Checks if the requested user has Permission to join the Instance
    /// </summary>
    /// <param name="id"></param>
    /// <param name="discordUser"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public bool HasPermission(string id, SocketGuildUser? discordUser)
    {
        if (discordUser == null)
        {
            _ = _logger.LogAsync("", new NullReferenceException("SocketGuildUser is null"));
            return false;
        }

        if (!Guid.TryParse(id, out var guid))
        {
            _ = _logger.LogAsync("", new Exception("Couldn't Parse Guid"));
            return false;
        }

        var instance = _sharedInstances[guid];
        return instance.PrivacyMode switch
        {
            SharedInstancePrivacyMode.Public => true,
            SharedInstancePrivacyMode.RoleRequired => discordUser.Roles.Contains(instance.SocketRole),
            SharedInstancePrivacyMode.Private => instance.AllowedUsers?.Contains(
                _verificationService.GetVerifiedUser(discordUser.Id)) ?? false,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    ///     Checks if an Instance is still available
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool InstanceActive(string id)
    {
        if (Guid.TryParse(id, out var guid)) return _sharedInstances.ContainsKey(guid);
        _ = _logger.LogAsync("", new Exception("Couldn't Parse Guid"));
        return false;
    }

    /// <summary>
    ///     Checks if an Instance is still available
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public bool InstanceActive(Guid guid)
    {
        return _sharedInstances.ContainsKey(guid);
    }

    /// <summary>
    ///     returns the Instance
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ValueTask<VrcSharedInstance?> GetInstance(string id)
    {
        return ValueTask.FromResult(InstanceActive(id) ? _sharedInstances[Guid.Parse(id)] : null);
    }

    /// <summary>
    ///     returns the Instance
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public ValueTask<VrcSharedInstance?> GetInstance(Guid guid)
    {
        return ValueTask.FromResult(InstanceActive(guid) ? _sharedInstances[guid] : null);
    }

    public async ValueTask<bool> InvitePlayer(string id, SocketGuildUser? discordUser)
    {
        if (discordUser == null)
        {
            _ = _logger.LogAsync("", new NullReferenceException("SocketGuildUser is null"));
            return false;
        }

        if (!Guid.TryParse(id, out var guid))
        {
            _ = _logger.LogAsync("", new Exception("Couldn't Parse Guid"));
            return false;
        }

        var user = _verificationService.GetVerifiedUser(discordUser.Id);
        if (user == null)
        {
            _ = _logger.LogAsync("", new NullReferenceException("User not verified"));
            return false;
        }

        await _vrChatApiService.Invite(user.VrcId, _sharedInstances[guid].WorldInstance.worldId);
        return true;
    }
}