using Microsoft.Extensions.DependencyInjection;
using WholesomeBot.Models;

namespace WholesomeBot.Services;

public class VerificationService
{
    private readonly LoggingService _logger;
    private readonly IServiceProvider _services;

    //TODO: Add Database to safe this shit in instead of Memory to persistently store it
    private readonly List<LinkedAccountDetails> _verifiedUsers = new();

    public VerificationService(IServiceProvider services)
    {
        _services = services;
        _logger = _services.GetRequiredService<LoggingService>();
    }

    /// <summary>
    ///     Add a Verified User to the List
    ///     TODO: Move to private after Testing
    /// </summary>
    /// <param name="vrChatId"></param>
    /// <param name="discordId"></param>
    /// <returns>If the operation was successful</returns>
    public bool AddVerifiedUser(string vrChatId, ulong discordId)
    {
        var user = new LinkedAccountDetails(vrChatId, discordId);
        if (_verifiedUsers.Contains(user))
        {
            _logger.LogAsync("User already verified");
            return false;
        }

        if (_verifiedUsers.Any(x => x.DiscordId == discordId))
        {
            _logger.LogAsync("Discord User already linked another Account");
            return false;
        }

        if (_verifiedUsers.Any(x => x.VrcId == vrChatId))
        {
            _logger.LogAsync("VRC User already linked another Account");
            return false;
        }

        _logger.LogAsync($"New user verified [Discord: {user.DiscordId} - VRC: {user.VrcId}]");
        _verifiedUsers.Add(user);
        return true;
    }

    /// <summary>
    ///     TODO: Implement a way to check if a user is validly the proposed Discord-User
    /// </summary>
    /// <param name="vrChatId"></param>
    /// <param name="discordId"></param>
    /// <returns>If a user can be verified</returns>
    public bool VerifyUser(string vrChatId, ulong discordId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="vrChatId"></param>
    /// <returns>If a user is verified</returns>
    public bool IsVerified(string vrChatId)
    {
        return _verifiedUsers.Any(x => x.VrcId == vrChatId);
    }

    /// <summary>
    /// </summary>
    /// <param name="discordId"></param>
    /// <returns>If a user is verified</returns>
    public bool IsVerified(ulong discordId)
    {
        return _verifiedUsers.Any(x => x.DiscordId == discordId);
    }

    /// <summary>
    /// </summary>
    /// <param name="vrChatId"></param>
    /// <returns></returns>
    public LinkedAccountDetails? GetVerifiedUser(string vrChatId)
    {
        return _verifiedUsers.FirstOrDefault(x => x.VrcId == vrChatId);
    }

    /// <summary>
    /// </summary>
    /// <param name="discordId"></param>
    /// <returns></returns>
    public LinkedAccountDetails? GetVerifiedUser(ulong discordId)
    {
        return _verifiedUsers.FirstOrDefault(x => x.DiscordId == discordId);
    }
}