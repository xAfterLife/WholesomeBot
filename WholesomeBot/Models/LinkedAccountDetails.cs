namespace WholesomeBot.Models
{
    public class LinkedAccountDetails
    {
        public string VrcId{ get; set; }
        public string DiscordId { get; set; }

        public LinkedAccountDetails(string vrcId, string discordId)
        {
            VrcId = vrcId;
            DiscordId = discordId;
        }
    }
}