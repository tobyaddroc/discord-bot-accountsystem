using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DiscordBotAccountSystem.Auth.Prototypes;

namespace DiscordBotAccountSystem.Auth
{
    
    public class CPDB
    {
        [JsonPropertyName("accounts")]
        public CPAccount[] Accounts { get; set; }
    }
}
