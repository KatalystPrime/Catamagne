using System;
using System.Threading.Tasks;
using Catamagne.Commands;
using Catamagne.Events;
using Catamagne.API;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity;
using Serilog;
using Serilog.Extensions.Logging;
using System.Linq;
using Catamagne.Configuration;
using Catamagne.Core;

namespace Catamagne.Core
{
    class Core
    {
        public static DiscordClient discord;
        public static DateTime startTime;
        public static List<DiscordChannel> commandChannels;
        public static DiscordChannel alertsChannel;
        public static DiscordChannel updatesChannel;
        public static bool PauseEvents;
        static SerilogLoggerFactory logFactory;
        static void Main(string[] args)
        {

            ConfigValues.configValues.LoadConfig(false);
            ConfigValues.configValues.LoadConfig(true);
            Console.Title = "Catamagne | Watcher of Umbral";
            Log.Logger = new LoggerConfiguration().WriteTo.Console()
                .MinimumLevel.Is(Serilog.Events.LogEventLevel.Debug)
                .CreateLogger();
            logFactory = new SerilogLoggerFactory();
            MainAsync().GetAwaiter().GetResult();
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
        static async Task MainAsync()
        {
            await SpreadsheetTools.SetUpSheet();
            PauseEvents = false;

            startTime = DateTime.UtcNow;
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
            await Task.Delay(-1);
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
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, fields.Count)));
                }
                else if (fields.Count < 75)
                {
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, fields.Count)));
                }
                else
                {
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, 75)));
                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(75, fields.Count)));

                }
                List<DiscordMessage> messages = new List<DiscordMessage>();
                embeds.ForEach(async embed => messages.Add(await SendFancyMessage(channel, embed)));
            }
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

namespace Catamagne.Configuration
{

    [Serializable]
    public class ConfigValues
    {
        [NonSerialized] public static ConfigValues configValues = new ConfigValues();
        [NonSerialized] public static List<Clan> clansList = new List<Clan>();
        public ulong[] RoleIDs;
        public ulong[] AdminRoleIDs;
        public ulong[] CommandChannels;
        public ulong AlertChannel;
        public ulong UpdatesChannel;
        public DiscordActivity DiscordActivity;
        public string[] Prefixes;
        public TimeSpan ShortInterval;
        public TimeSpan LongInterval;
        public string Folderpath;
        public string Filepath;
        public string SpreadsheetID;
        public string BungieAPIKey;
        public string DiscordToken;
        public Core.Response[] Responses;
        public ulong CataID;
        public ConfigValues()
        {
            CataID = 194439970797256706;
            RoleIDs = new ulong[] { 720914599536492545, 796437863667466274 };
            CommandChannels = new ulong[] { 796409176803901441 };
            AlertChannel = 796644582447906857;
            UpdatesChannel = 796658619257061400;
            DiscordActivity = new DiscordActivity()
            {
                Name = "over Umbral...",
                ActivityType = ActivityType.Watching,
            };
            AdminRoleIDs = new ulong[] { 743831304771993640, 743060988768550914 };
            Prefixes = new string[] { "ct!" };
            ShortInterval = TimeSpan.FromMinutes(5);
            LongInterval = TimeSpan.FromHours(6);
            Folderpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CatamagneMULT");
            SpreadsheetID = "ENTER SHEET ID HERE";
            BungieAPIKey = "ENTER BUNGIE API KEY HERE";
            DiscordToken = "ENTER DISCORD BOT TOKEN HERE";
            Responses = new Response[] { new Response("cataisnuts", "Our <#627494111821430794> channel is used to chat and talk about the game as a whole.\nYou can pick up activity-specific roles from over at <#686486603711119360>. With these roles you can ping and be pinged for their respective activities. For example, you can ping `@Strikes D2` to find players for strikes, and be pinged for other players doing strikes.\n<#628753173959671829> is used to matchmake and talk about PVE activities, with the same applying for <#628753046712614912>.\nFor hosted activities, you can sign up to raids and other activities hosted by your fellow gladiators in <#779175016611971123>. Furthermore, you can host your own activities by typing `!event` in <#342214163323551744>. \nLastly, the standalone <#628753070846640174> channel is used for spontaneous raids, discussions about raids and communication between raiders.", "Message to welcome and introduce new members to the destiny channels!") };
            clansList = new List<Clan>() { new Clan("3928553", "Xenolith!A2:F101", "Xenolith", "xg", new List<SpreadsheetTools.User>(), new List<SpreadsheetTools.User>()), new Clan("3872177", "Zodiac!A2:F101", "Zodiac", "zog", new List<SpreadsheetTools.User>(), new List<SpreadsheetTools.User>()) };
        }
        public void SaveConfig(bool? clanMode = false)
        {
            string folderpath = Path.Combine(Folderpath, "Config");
            string configpath = Path.Combine(folderpath, "config.json");
            string clanspath = Path.Combine(folderpath, "clans.json");
            if (clanMode.HasValue)
            {
                if (clanMode.Value)
                {
                    Directory.CreateDirectory(folderpath);
                    File.WriteAllText(clanspath, JsonConvert.SerializeObject(clansList, Formatting.Indented));
                }
                else
                {
                    Directory.CreateDirectory(folderpath);
                    File.WriteAllText(configpath, JsonConvert.SerializeObject(this, Formatting.Indented));
                }
            }
            else
            {
                Directory.CreateDirectory(folderpath);
                File.WriteAllText(clanspath, JsonConvert.SerializeObject(clansList, Formatting.Indented));
                Directory.CreateDirectory(folderpath);
                File.WriteAllText(configpath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            
        }
        public void LoadConfig(bool? clanMode = false)
        {
            string folderpath = Path.Combine(Folderpath, "Config");
            string configpath = Path.Combine(folderpath, "config.json");
            string clanspath = Path.Combine(folderpath, "clans.json");
            if (clanMode.HasValue)
            {
                if (!clanMode.Value)
                {
                    if (File.Exists(configpath))
                    {
                        var _ = File.ReadAllText(configpath);
                        configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                        Console.WriteLine("Read configuration values from {0}", configpath);
                    }
                    else
                    {
                        Console.WriteLine("No configuration file found at {0}\nCreating one. please edit the file with your api keys and google secrets and press 'K'", configpath);
                        SaveConfig();
                        bool notEnter = true;
                        while (notEnter)
                        {
                            if (Console.ReadKey(true).Key == ConsoleKey.K)
                            {
                                notEnter = false;
                            }
                        }
                        var _ = File.ReadAllText(configpath);
                        configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                        Console.WriteLine("Read configuration values from {0}", configpath);
                    }
                }
                else
                {
                    if (File.Exists(clanspath))
                    {
                        var _ = File.ReadAllText(clanspath);
                        clansList = JsonConvert.DeserializeObject<List<Clan>>(_);
                        Console.WriteLine("Read clans list from {0}", clanspath);
                    }
                    else
                    {
                        Console.WriteLine("No clans list found at {0}\nCreating one.", clanspath);
                        SaveConfig(true);
                        var _ = File.ReadAllText(clanspath);
                        clansList = JsonConvert.DeserializeObject<List<Clan>>(_);
                        Console.WriteLine("Read clans list from {0}", clanspath);
                    }
                }
            }
            else
            {
                if (File.Exists(configpath))
                {
                    var _ = File.ReadAllText(configpath);
                    configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                    Console.WriteLine("Read configuration values from {0}", configpath);
                }
                else
                {
                    Console.WriteLine("No configuration file found at {0}\nCreating one. please edit the file with your api keys and google secrets and press 'K'", configpath);
                    SaveConfig();
                    bool notEnter = true;
                    while (notEnter)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.K)
                        {
                            notEnter = false;
                        }
                    }
                    var _ = File.ReadAllText(configpath);
                    configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                    Console.WriteLine("Read configuration values from {0}", configpath);
                }
                if (File.Exists(clanspath))
                {
                    var _ = File.ReadAllText(clanspath);
                    clansList = JsonConvert.DeserializeObject<List<Clan>>(_);
                    Console.WriteLine("Read clans list from {0}", clanspath);
                }
                else
                {
                    Console.WriteLine("No clans list found at {0}\nCreating one.", clanspath);
                    SaveConfig(true);
                    var _ = File.ReadAllText(clanspath);
                    clansList = JsonConvert.DeserializeObject<List<Clan>>(_);
                    Console.WriteLine("Read clans list from {0}", clanspath);
                }
            }
        }
    }
    
}