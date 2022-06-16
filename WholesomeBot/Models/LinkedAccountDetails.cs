namespace WholesomeBot.Models;

public class LinkedAccountDetails
{
    public string VrcId { get; set; }
    public ulong DiscordId { get; set; }

    public LinkedAccountDetails(string vrcId, ulong discordId)
    {
        VrcId = vrcId;
        DiscordId = discordId;
    }
}
