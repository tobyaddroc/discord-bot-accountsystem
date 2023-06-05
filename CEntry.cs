using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace DiscordBotAccountSystem
{
    class CEntry
    {
        [STAThread] public static Task Main() => new CEntry().Preload();

        private readonly DiscordSocketClient socket = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = true
        });

        public async Task Preload()
        {
            using (IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
            services
            .AddSingleton(socket)
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CSlashCMDs>()
            )
            .Build())

                await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var _client = provider.GetRequiredService<DiscordSocketClient>();
            var _scommands = provider.GetRequiredService<InteractionService>();
            var _guild = _client.GetGuild(CGlobal.CConfig.GuildId);
            await provider.GetRequiredService<CSlashCMDs>().Initialize();

            _client.Log += async (LogMessage log) => Console.WriteLine("[CLIENT_LOG] => " + log.Message);
            _scommands.Log += async (LogMessage log) => Console.WriteLine("[SLASH_COMMANDS] => " + log.Message);

            _client.Ready += async () =>
            {
                Console.WriteLine("[BOT_LOG] => Started");
                try
                {
                    if (!File.Exists("db.json") || !File.Exists("groups.json"))
                        throw new FileNotFoundException("Database not found");
                    CGlobal.DataBaseHandle = new Auth.CDataBase("db.json");
                    CGlobal.GroupsHandle = File.OpenRead("groups.json");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[DATABASE] => {0}", ex.ToString());
                    return;
                }
                Console.WriteLine("[DATABASE] => Connected to database");
                await _scommands.RegisterCommandsToGuildAsync(CGlobal.CConfig.GuildId);
            };

            await _client.LoginAsync(TokenType.Bot, CGlobal.CConfig.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}