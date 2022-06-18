namespace WholesomeBot.Models;

public class LinkedAccountDetails
{
    public LinkedAccountDetails(string vrcId, ulong discordId)
    {
        VrcId = vrcId;
        DiscordId = discordId;
    }

    public string VrcId { get; set; }
    public ulong DiscordId { get; set; }
}