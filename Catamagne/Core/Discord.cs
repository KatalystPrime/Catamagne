using Catamagne.API;
using Catamagne.Commands;
using Catamagne.Configuration;
using Catamagne.Events;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Serilog;
using Serilog.Extensions.Logging;
using System;
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

            commands.RegisterCommands<CoreModule>();
            commands.RegisterCommands<UserInteractionsModule>();

            discord.UseInteractivity(new InteractivityConfiguration()
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                Timeout = TimeSpan.FromSeconds(30)
            });

            discord.GuildMemberRemoved += UserEvents.Discord_GuildMemberRemoved;
            discord.MessageCreated += UserEvents.Discord_MessageCreated;
            discord.Ready += UserEvents.Discord_Ready;

            await discord.ConnectAsync(ConfigValues.configValues.DiscordActivity);
            await UpdateChannels();
        }
        public static async Task UpdateChannels()
        {
            try
            {
                alertsChannel = await discord.GetChannelAsync(ConfigValues.configValues.AlertChannel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType() + " error when getting channel id " + ConfigValues.configValues.AlertChannel);
            }
            try
            {
                updatesChannel = await discord.GetChannelAsync(ConfigValues.configValues.UpdatesChannel);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType() + " error when getting channel id " + ConfigValues.configValues.UpdatesChannel);
            }

            commandChannels = new List<DiscordChannel>();
            foreach (ulong channel in ConfigValues.configValues.CommandChannels)
            {
                try
                {
                    commandChannels.Add(await discord.GetChannelAsync(channel));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetType() + " error when getting channel id " + channel);
                }
            }
        }

        public static async Task<DiscordMessage> SendMessage(string text, DiscordChannel channel)
        {
            return await discord.SendMessageAsync(channel, text);
        }

        public static async Task UpdateMessage(string text, DiscordMessage message)
        {
            await message.ModifyAsync(text);
        }
        public static async Task<DiscordMessage> SendFancyMessage(DiscordChannel channel, DiscordEmbed embed)
        {
            return await discord.SendMessageAsync(channel, embed);
        }
        public static DiscordEmbed CreateFancyMessage(DiscordColor color, string title, string description, List<Field> fields)
        {
            var embedBuilder = new DiscordEmbedBuilder()
            {
                Color = color,
                Description = description,
                Title = title
            };
            foreach (var field in fields)
            {
                embedBuilder.AddField(field.name, field.value, field.inline);
            }
            return embedBuilder.Build();

        }
        public static DiscordEmbed CreateFancyMessage(DiscordColor color, string title, string description = null)
        {
            var embedBuilder = new DiscordEmbedBuilder()
            {
                Color = color,
                Description = description,
                Title = title
            };
            return embedBuilder.Build();

        }
        public static DiscordEmbed CreateFancyMessage(DiscordColor color, string description)
        {
            var embedBuilder = new DiscordEmbedBuilder()
            {
                Color = color,
                Description = description
            };
            return embedBuilder.Build();

        }
        public static DiscordEmbed CreateFancyMessage(DiscordColor color, List<Field> fields, string title = null, string description = null)
        {
            var embedBuilder = new DiscordEmbedBuilder()
            {
                Color = color,
                Title = title,
                Description = description
            };
            fields.ForEach(field => embedBuilder.AddField(field.name, field.value, field.inline));
            //fields.ForEach(field => embedBuilder.AddField(field.name, field.value, field.inline));
            return embedBuilder.Build();

        }
        public static DiscordEmbed GetUsersToDisplayInRange(List<Field> fields, Range range, string title = null)
        {
            List<Field> _ = new List<Field>();
            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                _.Add(fields[i]);
            }
            if (title != null)
            {
                return CreateFancyMessage(DiscordColor.CornflowerBlue, _, title);
            }
            return CreateFancyMessage(DiscordColor.CornflowerBlue, _);
        }
        public static DiscordEmbed GetUsersToDisplayInRange(DiscordColor color, List<Field> fields, Range range, string title = null)
        {
            List<Field> _ = new List<Field>();
            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                _.Add(fields[i]);
            }
            if (title != null)
            {
                return CreateFancyMessage(color, _, title);
            }
            return CreateFancyMessage(color, _);
        }
        public static void SendFancyListMessage(DiscordChannel channel, Clan clan, List<SpreadsheetTools.User> Users, string title)
        {
            if (Users.Count > 0)
            {
                List<Field> fields = new List<Field>();
                foreach (SpreadsheetTools.User user in Users)
                {
                    if (!string.IsNullOrEmpty(user.discordID))
                    {
                        var _ = new Field("Steam name: " + user.steamName, "Discord ID: " + user.discordID);
                        fields.Add(_);
                    }
                    else
                    {
                        var _ = new Field("Steam name: " + user.steamName, "Discord ID: N/A");
                        fields.Add(_);
                    }
                }
                List<DiscordEmbed> embeds = new List<DiscordEmbed>();
                
                if (fields.Count < 25)
                {
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, Math.Min(25, fields.Count)), title + " " + clan.clanName + ":"));
                }
                else if (fields.Count < 50)
                {
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, Math.Min(25, fields.Count)), title + " " + clan.clanName + ":"));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, fields.Count)));
                }
                else if (fields.Count < 75)
                {
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, Math.Min(25, fields.Count)), title + " " + clan.clanName + ":"));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, fields.Count)));
                }
                else
                {
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, Math.Min(25, fields.Count)), title + " " + clan.clanName + ":"));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, 75)));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(75, fields.Count)));

                }
                List<DiscordMessage> messages = new List<DiscordMessage>();
                embeds.ForEach(async embed => messages.Add(await SendFancyMessage(channel, embed)));
            }
        }
        public static Task RotateActivity(Clan clan)
        {
            var activity = new DiscordActivity()
            {
                Name = string.Format("over {0}...", clan.clanName),
                ActivityType = ActivityType.Watching,
            };
            Log.Information("Rotating status to " + clan.clanName);
            discord.UpdateStatusAsync(activity);
            return Task.CompletedTask;
        }
    }
    public struct Field
    {

        public Field(string name, string value, bool inline = false)
        {
            this.name = name; this.value = value; this.inline = inline;
        }
        public string name; public string value; public bool inline;
    }
    public struct Response
    {
        public Response(string trigger, string response, string description = null, List<DiscordChannel> allowedChannels = null)
        {
            this.trigger = trigger; this.response = response; this.description = description; this.allowedChannels = allowedChannels;
        }
        public string trigger; public string response; public string description; public List<DiscordChannel> allowedChannels;
    }
}
