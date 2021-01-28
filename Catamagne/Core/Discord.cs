using Catamagne.Configuration;
using Catamagne.Events;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Serilog;
using Serilog.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catamagne.Core
{
    class Discord
    {
        public static DiscordChannel alertsChannel;
        public static DiscordChannel updatesChannel;
        static SerilogLoggerFactory logFactory;
        public static DiscordClient discord;
        public static List<DiscordChannel> commandChannels;
        public static async Task SetupClient()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .CreateLogger();
            logFactory = new SerilogLoggerFactory();

            discord = new DiscordClient(new DiscordConfiguration()
            {
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.GuildIntegrations | DiscordIntents.GuildMessages | DiscordIntents.Guilds | DiscordIntents.GuildPresences,
                Token = ConfigValues.configValues.DiscordToken,
                TokenType = TokenType.Bot,
                AlwaysCacheMembers = true,
                LoggerFactory = logFactory
            }); ;
            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = ConfigValues.configValues.Prefixes,
                CaseSensitive = false,
            });
            discord.GuildMemberRemoved += UserEvents.Discord_GuildMemberRemoved;
            discord.MessageCreated += UserEvents.Discord_MessageCreated;
            discord.Ready += UserEvents.Discord_Ready;

            await discord.ConnectAsync(ConfigValues.configValues.DiscordActivity);
        }
    }
}
