using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBotAccountSystem.Auth.Prototypes
{
    public class CPProfile
    {
        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("phrase")]
        public string Phrase { get; set; }
    }

    public class CPAccount
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("id")]
        public ulong Id { get; set; }

        [JsonPropertyName("registered")]
        public ulong RegisteredAt { get; set; }

        [JsonPropertyName("group")]
        public string Group { get; set; }

        [JsonPropertyName("marshal")]
        public bool Immunity { get; set; }

        [JsonPropertyName("profile")]
        public CPProfile Profile { get; set; }

        [JsonPropertyName("ban")]
        public CPBan BanInfo { get; set; }
    }
}
