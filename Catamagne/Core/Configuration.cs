using Catamagne.API;
using Catamagne.API.Models;
using Catamagne.Core;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Catamagne.Configuration
{

    [Serializable]
    class ConfigValues
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
        public ulong DevID;
        public ConfigValues()
        {
            DevID = 194439970797256706;
            //CataID = 194439970797256706;
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
            Prefixes = new string[] { "ctr!" };
            ShortInterval = TimeSpan.FromMinutes(5);
            LongInterval = TimeSpan.FromHours(6);
            Folderpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CatamagneRE");
            SpreadsheetID = "ENTER SHEET ID HERE";
            BungieAPIKey = "ENTER BUNGIE API KEY HERE";
            DiscordToken = "ENTER DISCORD BOT TOKEN HERE";
            Responses = new Response[] { new Response("hi catamagne", "hello!", "greets users") };
            clansList = new List<Clan>() { new Clan("3928553", "Xenolith!A2:F101", "Xenolith", "xg", new List<User>(), new List<User>()), new Clan("3872177", "Zodiac!A2:F101", "Zodiac", "zog", new List<User>(), new List<User>()) };
        }
        public void SaveConfig(bool clanMode)
        {
            string folderpath = Path.Combine(Folderpath, "Config");
            string configpath = Path.Combine(folderpath, "config.json");
            string clanspath = Path.Combine(folderpath, "clans.json");
            if (clanMode)
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
        public void LoadConfig(bool clanMode)
        {
            string folderpath = Path.Combine(Folderpath, "Config");
            string configpath = Path.Combine(folderpath, "config.json");
            string clanspath = Path.Combine(folderpath, "clans.json");
            if (!clanMode)
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
                    SaveConfig(clanMode);
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
    }
}
