using System.Text.Json.Serialization;

namespace DiscordBotAccountSystem.Auth.Prototypes
{
    public class CPBan
    {
        [JsonPropertyName("banned")]
        public ulong BannedAt { get; set; }

        [JsonPropertyName("unban")]
        public ulong UnbannedAt { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }
    }
}
