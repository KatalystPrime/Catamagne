using Catamagne.API;
using Catamagne.Core;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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
