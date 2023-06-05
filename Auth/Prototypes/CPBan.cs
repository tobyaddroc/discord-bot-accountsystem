using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
