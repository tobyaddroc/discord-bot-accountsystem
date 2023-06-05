using System;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Lumi.ClientCommands.Handlers;
using System.Security.Principal;
using System.Reflection;
using Lumi.Properties;
using System.Diagnostics;
using System.Windows.Forms;
using Lumi.Kernel;

namespace Lumi
{
    class CEntry
    {
        private static SignalHandler signalHandler;
        [STAThread] public static Task Main(string[] args) => new CEntry().Preload();

        CConfig config = CCSConfig.Config;

        private DiscordSocketClient socket = new DiscordSocketClient(new DiscordSocketConfig()
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
            var _guild = _client.GetGuild(config.guildId);
            await provider.GetRequiredService<CSlashCMDs>().Initialize();

            _client.Log += async (LogMessage log) => Console.WriteLine("[CLIENT_LOG] => " + log.Message);
            _scommands.Log += async (LogMessage log) => Console.WriteLine("[SLASH_COMMANDS] => " + log.Message);

            _client.Ready += async () =>
            {
                Console.WriteLine("[BOT_LOG] => Started");
                await _scommands.RegisterCommandsToGuildAsync(config.guildId);
                await new Events.ENewUser().Trigger(_client);
                await new CAutoRunHook().Hook(_client);
                if (CGlobal.isElevated)
                {
                    await socket.SetStatusAsync(UserStatus.Idle);
                    await socket.SetGameAsync("Elevated");
                }
            };

            signalHandler += async (c) => await new Events.EDisconnectedUser().Trigger(_client);
            ConsoleHelper.SetSignalHandler(signalHandler, true);

            await _client.LoginAsync(TokenType.Bot, config.token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}