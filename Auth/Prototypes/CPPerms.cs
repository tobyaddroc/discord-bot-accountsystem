using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBotAccountSystem.Auth.Prototypes
{
    public class CPPerms
    {
        [JsonPropertyName("adminmenu")]
        public bool AdminMenu { get; set; }

        [JsonPropertyName("nocooldown")]
        public bool NoCooldown { get; set; }

        [JsonPropertyName("viewprofiles")]
        public bool ViewAnotherProfiles { get; set; }

        [JsonPropertyName("color")]
        public uint Color { get; set; }

    }
}
