using System.Text.Json.Serialization;
using DiscordBotAccountSystem.Auth.Prototypes;

namespace DiscordBotAccountSystem.Auth
{
    
    public class CPDB
    {
        [JsonPropertyName("accounts")]
        public CPAccount[] Accounts { get; set; }
    }
}
