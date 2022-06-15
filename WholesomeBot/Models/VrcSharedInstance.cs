using Discord;
using Discord.WebSocket;

namespace WholesomeBot.Models;

public enum SharedInstancePrivacyMode
{
    Public = 0,
    RoleRequired = 1,
    Private = 2
}

public class VrcSharedInstance
{
    public LinkedAccountDetails HostUser { get; }
    public NotificationInviteDetails WorldInstance { get; }
    public SharedInstancePrivacyMode PrivacyMode { get; }
    public DateTime CreationDateTime { get; }
    public SocketRole? SocketRole { get; }
    public IEnumerable<LinkedAccountDetails>? AllowedUsers { get; }

    public VrcSharedInstance(LinkedAccountDetails hostUser, NotificationInviteDetails worldInstance)
    {
        HostUser = hostUser;
        WorldInstance = worldInstance;
        PrivacyMode = SharedInstancePrivacyMode.Public;
        CreationDateTime = DateTime.UtcNow;
    }

    public VrcSharedInstance(LinkedAccountDetails hostUser, NotificationInviteDetails worldInstance, SocketRole socketRole)
    {
        HostUser = hostUser;
        WorldInstance = worldInstance;
        PrivacyMode = SharedInstancePrivacyMode.RoleRequired;
        SocketRole = socketRole;
        CreationDateTime = DateTime.UtcNow;
    }

    public VrcSharedInstance(LinkedAccountDetails hostUser, NotificationInviteDetails worldInstance, IEnumerable<LinkedAccountDetails> allowedUsers)
    {
        HostUser = hostUser;
        WorldInstance = worldInstance;
        PrivacyMode = SharedInstancePrivacyMode.Private;
        AllowedUsers = allowedUsers;
        CreationDateTime = DateTime.UtcNow;
    }
}