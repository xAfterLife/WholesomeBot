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

    public VrcSharedInstance(LinkedAccountDetails hostUser, NotificationInviteDetails worldInstance, SharedInstancePrivacyMode privacyMode)
    {
        HostUser = hostUser;
        WorldInstance = worldInstance;
        PrivacyMode = privacyMode;
        CreationDateTime = DateTime.UtcNow;
    }
}