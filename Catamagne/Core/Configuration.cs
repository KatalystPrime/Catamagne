using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.IO;
using Catamagne.API.Models;
using Catamagne.API;
using System.Collections.Generic;
using Catamagne.Configuration.Models;

namespace Catamagne.Configuration
{
    [Serializable]
    class ConfigValues
    {
        [NonSerialized] public static ConfigValues configValues = new ConfigValues();
        
        public List<ulong?> RoleIDs;
        public List<ulong?> AdminRoleIDs;
        public List<ulong?> CommandChannels;
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
            RoleIDs = new List<ulong?>();
            CommandChannels = new List<ulong?>();
            AdminRoleIDs = new List<ulong?>();
            AlertChannel = null;
            UpdatesChannel = null;
            DiscordActivity = new DiscordActivity()
            {
                Name = "over Destiny...",
                ActivityType = ActivityType.Watching,
            };
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
            var ConfigFile = Path.Combine(ConfigFolder, "config.json");
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public void LoadConfig()
        {
            var ConfigFile = Path.Combine(ConfigFolder, "config.json");
            if (File.Exists(ConfigFile))
            {
                var _ = File.ReadAllText(ConfigFile);
                configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                Console.WriteLine("Read configuration values from {0}", ConfigFile);
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
                var _ = File.ReadAllText(ConfigFile);
                configValues = JsonConvert.DeserializeObject<ConfigValues>(_);
                Console.WriteLine("Read configuration values from {0}", ConfigFile);
            }
        }
    }
    [Serializable]
    class Clans
    {
        public static List<Clan> clans = new List<Clan>() {
            new Clan(
                new Clan.Details(4170189, "Umbral", "ug", "Umbral!A2:F101"),
                new Clan.Members(new List<BungieUser>(), new List<SpreadsheetUser>(), new List<ClanLeaver>())
            )};
        public static void SaveClanMembers(Clan clan)
        {
            Directory.CreateDirectory(ConfigValues.configValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder, clan.details.BungieNetID.ToString());
            string clanMembersFile = Path.Combine(clanFolder, "Users.dat");
            string clanSpreadsheetFile = Path.Combine(clanFolder, "SpreadsheetUsers.dat");
            string clanLeaversFile = Path.Combine(clanFolder, "Leavers.dat");

            Directory.CreateDirectory(clanFolder);
            File.WriteAllText(clanMembersFile, JsonConvert.SerializeObject(clan.members.BungieUsers, Formatting.Indented));
            File.WriteAllText(clanSpreadsheetFile, JsonConvert.SerializeObject(clan.members.SpreadsheetUsers, Formatting.Indented));
            File.WriteAllText(clanLeaversFile, JsonConvert.SerializeObject(clan.members.ClanLeavers, Formatting.Indented));
            Console.WriteLine("Wrote {0} members to {1}", clan.details.BungieNetName, clanFolder);
        }
        public static void SaveClanMembers(Clan clan, UserType userType)
        {
            Directory.CreateDirectory(ConfigValues.configValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder, clan.details.BungieNetID.ToString());
            Directory.CreateDirectory(clanFolder);
            switch (userType)
            {
                case UserType.BungieUser:
                    string clanMembersFile = Path.Combine(clanFolder, "Users.dat");
                    File.WriteAllText(clanMembersFile, JsonConvert.SerializeObject(clan.members.BungieUsers, Formatting.Indented));
                    break;
                case UserType.SpreadsheetUser:
                    string clanSpreadsheetFile = Path.Combine(clanFolder, "SpreadsheetUsers.dat");
                    File.WriteAllText(clanSpreadsheetFile, JsonConvert.SerializeObject(clan.members.SpreadsheetUsers, Formatting.Indented));
                    break;
                case UserType.ClanLeaver:
                    string clanLeaversFile = Path.Combine(clanFolder, "Leavers.dat");
                    File.WriteAllText(clanLeaversFile, JsonConvert.SerializeObject(clan.members.ClanLeavers, Formatting.Indented));
                    break;
                default:
                    throw new ArgumentException("Type provided is invalid.");

            }
            Console.WriteLine("Wrote {0} members to {1}", clan.details.BungieNetName, clanFolder);
        }
        public static void LoadClanMembers(Clan clan)
        {
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder, clan.details.BungieNetID.ToString());
            string clanMembersFile = Path.Combine(clanFolder, "Users.dat");
            string clanSpreadsheetFile = Path.Combine(clanFolder, "SpreadsheetUsers.dat");
            string clanLeaversFile = Path.Combine(clanFolder, "Leavers.dat");
            if (!File.Exists(clanMembersFile) || !File.Exists(clanSpreadsheetFile) || !File.Exists(clanLeaversFile))
            {
                SaveClanMembers(clan);
            }
            var a = File.ReadAllText(clanMembersFile);
            clans[clans.FindIndex(t => t.details.BungieNetID == clan.details.BungieNetID)].members.BungieUsers = JsonConvert.DeserializeObject<List<BungieUser>>(a);
            var b = File.ReadAllText(clanSpreadsheetFile);
            clans[clans.FindIndex(t => t.details.BungieNetID == clan.details.BungieNetID)].members.SpreadsheetUsers = JsonConvert.DeserializeObject<List<SpreadsheetUser>>(b);
            var c = File.ReadAllText(clanLeaversFile);
            clans[clans.FindIndex(t => t.details.BungieNetID == clan.details.BungieNetID)].members.ClanLeavers = JsonConvert.DeserializeObject<List<ClanLeaver>>(c);
            Console.WriteLine("Read {0} members from {1}", clan.details.BungieNetName, clanFolder);
        }
        public static void LoadClanMembers(Clan clan, UserType userType)
        {
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder, clan.details.BungieNetID.ToString());
            string clanMembersFile = Path.Combine(clanFolder, "Users.dat");
            string clanSpreadsheetFile = Path.Combine(clanFolder, "SpreadsheetUsers.dat");
            string clanLeaversFile = Path.Combine(clanFolder, "Leavers.dat");
            switch (userType)
            {
                case UserType.BungieUser:
                    if (!File.Exists(clanMembersFile))
                    {
                        SaveClanMembers(clan, UserType.BungieUser);
                    }
                    var a = File.ReadAllText(clanMembersFile);
                    clans[clans.FindIndex(t => t.details.BungieNetID == clan.details.BungieNetID)].members.BungieUsers = JsonConvert.DeserializeObject<List<BungieUser>>(a);
                    break;
                case UserType.SpreadsheetUser:
                    if (!File.Exists(clanSpreadsheetFile))
                    {
                        SaveClanMembers(clan, UserType.SpreadsheetUser);
                    }

                    var b = File.ReadAllText(clanSpreadsheetFile);
                    clans[clans.FindIndex(t => t.details.BungieNetID == clan.details.BungieNetID)].members.SpreadsheetUsers = JsonConvert.DeserializeObject<List<SpreadsheetUser>>(b);
                    break;
                case UserType.ClanLeaver:
                    if (!File.Exists(clanLeaversFile))
                    {
                        SaveClanMembers(clan, UserType.ClanLeaver);
                    }
                    var c = File.ReadAllText(clanLeaversFile);
                    clans[clans.FindIndex(t => t.details.BungieNetID == clan.details.BungieNetID)].members.ClanLeavers = JsonConvert.DeserializeObject<List<ClanLeaver>>(c);
                    break;
                default:
                    throw new ArgumentException("Type provided is invalid.");
            }
            Console.WriteLine("Read {0} members from {1}", clan.details.BungieNetName, clanFolder);
        }
        public static void SaveClans()
        {
            Directory.CreateDirectory(ConfigValues.configValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder);
            string clanFile = Path.Combine(clanFolder, "clans.dat");

            Directory.CreateDirectory(clanFolder);
            List<Clan.Details> clanDetails = clans.Select(t => t.details).ToList();
            var _ = JsonConvert.SerializeObject(clanDetails, Formatting.Indented);
            File.WriteAllText(clanFile, _);
            Console.WriteLine("Wrote clan details to {0}", clanFile);
            foreach (Clan clan in clans)
            {
                SaveClanMembers(clan);
            }
        }
        public static void LoadClans()
        {
            Directory.CreateDirectory(ConfigValues.configValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.configValues.ClansFolder);
            string clanFile = Path.Combine(clanFolder, "clans.dat");
            if (!File.Exists(clanFile))
            {
                SaveClans();
            }
            var _ = File.ReadAllText(clanFile);
            List<Clan.Details> clanDetails = JsonConvert.DeserializeObject<List<Clan.Details>>(_);
            clans = new List<Clan>();

            foreach (var details in clanDetails)
            {
                clans.Add(new Clan(details, new Clan.Members()));
            }
            foreach (Clan clan in clans)
            {
                LoadClanMembers(clan);
            }
        }
    }
}
