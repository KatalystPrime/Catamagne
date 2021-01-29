using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.IO;
using Catamagne.API.Models;
using Catamagne.API;
using System.Collections.Generic;

namespace Catamagne.Configuration
{
    [Serializable]
    class ConfigValues
    {
        [NonSerialized] public static ConfigValues configValues = new ConfigValues();
        
        public ulong?[] RoleIDs;
        public ulong?[] AdminRoleIDs;
        public ulong?[] CommandChannels;
        public ulong? AlertChannel;
        public ulong? UpdatesChannel;
        public DiscordActivity DiscordActivity;
        public string[] Prefixes;
        public TimeSpan ShortInterval;
        public TimeSpan MediumInterval;
        public TimeSpan LongInterval;
        public string FolderPath;
        public string ConfigFolder;
        public string ClansFolder;
        public string SpreadsheetID;
        public string BungieAPIKey;
        public string DiscordToken;
        public ulong? DevID;
        public ConfigValues()
        {
            DevID = 194439970797256706;
            RoleIDs = null;
            CommandChannels = null;
            AlertChannel = null;
            UpdatesChannel = null;
            DiscordActivity = new DiscordActivity()
            {
                Name = "over Destiny...",
                ActivityType = ActivityType.Watching,
            };
            AdminRoleIDs = null;
            Prefixes = new string[] { "ct!" };
            ShortInterval = TimeSpan.FromMinutes(15);
            LongInterval = TimeSpan.FromHours(6);
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CatamagneRE");
            ConfigFolder = Path.Combine(FolderPath, "Config");
            ClansFolder = Path.Combine(FolderPath, "Clans");
            SpreadsheetID = null;
            BungieAPIKey = null;
            DiscordToken = null;
        }
        public void SaveConfig()
        {
            Directory.CreateDirectory(configValues.ConfigFolder);
            File.WriteAllText(configValues.ConfigFolder, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public void LoadConfig()
        {
            if (File.Exists(configValues.ConfigFolder))
            {
                var _ = File.ReadAllText(configValues.ConfigFolder);
                configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                Console.WriteLine("Read configuration values from {0}", configValues.ConfigFolder);
            }
            else
            {
                Console.WriteLine("No configuration file found at {0}\nCreating one. please edit the file with your api keys and google secrets and press 'K'", configValues.ConfigFolder);
                SaveConfig();
                bool notEnter = true;
                while (notEnter)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.K)
                    {
                        notEnter = false;
                    }
                }
                var _ = File.ReadAllText(configValues.ConfigFolder);
                configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                Console.WriteLine("Read configuration values from {0}", configValues.ConfigFolder);
            }
        }
    }
    [Serializable]
    public class Clans
    {
        List<Clan> clans = new List<Clan>() {
            new Clan(
                new Clan.Details("4170189", "Umbral", "ug", "Umbral!A2:F101"),
                new Clan.Members(new List<BungieUser>(), new List<SpreadsheetUser>(), new List<ClanLeaver>())
            )};
        void SaveClanMembers(Clan clan)
        {
            Directory.CreateDirectory(ConfigValues.configValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder, clan.details.BungieNetClanName);
            string clanFile = Path.Combine(clanFolder, "clan.dat");

            Directory.CreateDirectory(clanFolder);
            File.WriteAllText(clanFile, JsonConvert.SerializeObject(clan.members, Formatting.Indented));
            Console.WriteLine("Wrote {0} members to {1}", clan.details.BungieNetClanName, clanFile);
        }
        void LoadClanMembers(Clan clan)
        {
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder, clan.details.BungieNetClanName);
            string clanFile = Path.Combine(clanFolder, "clan.dat");
            if (File.Exists(clanFile))
            {
                var _ = File.ReadAllText(clanFile);
                clans[clans.FindIndex(t => t.details.BungieNetClanID == clan.details.BungieNetClanID)].members = JsonConvert.DeserializeObject<Clan.Members>(_);
                Console.WriteLine("Read {0} members from {1}",clan.details.BungieNetClanName ,clanFile);
            }
            else
            {
                SaveClanMembers(clan);
                var _ = File.ReadAllText(clanFile);
                clans[clans.FindIndex(t => t.details.BungieNetClanID == clan.details.BungieNetClanID)].members = JsonConvert.DeserializeObject<Clan.Members>(_);
                Console.WriteLine("Read {0} members from {1}", clan.details.BungieNetClanName, clanFile);
            }
        }
        void SaveClans()
        {
            Directory.CreateDirectory(ConfigValues.configValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder);
            string clanFile = Path.Combine(clanFolder, "clans.dat");

            Directory.CreateDirectory(clanFolder);
            var clanDetails = clans.Select(t => t.details).ToList();
            File.WriteAllText(clanFile, JsonConvert.SerializeObject(clanDetails, Formatting.Indented));
            Console.WriteLine("Wrote clan details to {1}", clanFile);
            foreach (Clan clan in clans)
            {
                SaveClanMembers(clan);
            }
        }
        void LoadClans()
        {
            Directory.CreateDirectory(ConfigValues.configValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder);
            string clanFile = Path.Combine(clanFolder, "clans.dat");
            if (File.Exists(clanFile))
            {
                var _ = File.ReadAllText(clanFile);
                var clanDetails = JsonConvert.DeserializeObject<List<Clan.Details>>(clanFile);
                clans = new List<Clan>();

                foreach (var details in clanDetails)
                {
                    clans.Append(new Clan(details, new Clan.Members()));
                }
            }
            else
            {
                SaveClans();
                var _ = File.ReadAllText(clanFile);
                var clanDetails = JsonConvert.DeserializeObject<List<Clan.Details>>(clanFile);
                clans = new List<Clan>();

                foreach (var details in clanDetails)
                {
                    clans.Append(new Clan(details, new Clan.Members()));
                }
            }
        }
    }
}
