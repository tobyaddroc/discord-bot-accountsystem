using DiscordBotAccountSystem.Auth;
using System.IO;

namespace DiscordBotAccountSystem
{
    public static class CGlobal
    {
        public static CDataBase DataBaseHandle;
        public static FileStream GroupsHandle;
        public static class CConfig
        {
            public static readonly string Token = "suda_token";
            public static readonly ulong GuildId = 0; // suda id servera
        }
    }
}
