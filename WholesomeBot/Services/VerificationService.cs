using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using WholesomeBot.Models;

namespace WholesomeBot.Services;

public class VerificationService
{
    private readonly IServiceProvider _services;
    private readonly LoggingService _logger;

    //TODO: Add Database to safe this shit in instead of Memory to persistently store it
    public List<LinkedAccountDetails> VerifiedUsers = new List<LinkedAccountDetails>();

    public VerificationService(IServiceProvider services)
    {
        _services = services;
        _logger = _services.GetRequiredService<LoggingService>();
    }

    /// <summary>
    /// Add a Verified User to the List
    /// TODO: Move to private after Testing
    /// </summary>
    /// <param name="vrchatId"></param>
    /// <param name="discordId"></param>
    /// <returns></returns>
    public bool AddVerifiedUser(string vrchatId, ulong discordId)
    {
        var user = new LinkedAccountDetails(vrchatId, discordId);
        if (VerifiedUsers.Contains(user)) return false;
        VerifiedUsers.Add(user);
        return true;
    }

    /// <summary>
    /// TODO: Implement a way to check if a user is validly the proposed Discord-User
    /// </summary>
    /// <param name="vrchatId"></param>
    /// <param name="discordId"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public bool VerifyUser(string vrchatId, ulong discordId)
    {
        throw new NotImplementedException();
    }

}