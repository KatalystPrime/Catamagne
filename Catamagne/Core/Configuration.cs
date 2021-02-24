using Catamagne.API;
using Catamagne.Core;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Catamagne.Configuration
{
    [Serializable]
    class ConfigValues
    {
        [NonSerialized] public static ConfigValues configValues = new ConfigValues();

        public List<ulong> RoleIDs;
        public List<ulong> AdminRoleIDs;
        public List<ulong> CommandChannels;
        public List<ulong?> AlertChannels;
        public List<ulong?> UpdatesChannels;
        public DiscordActivity DiscordActivity;
        public string[] Prefixes;
        public string FolderPath;
        [NonSerialized] public string ConfigFolder;
        [NonSerialized] public string ClansFolder;
        public string SpreadsheetID;
        public string BungieAPIKey;
        public string DiscordToken;
        public ulong? DevID;
        [NonSerialized] public List<Response> Responses;
        [NonSerialized] public Dictionary<string, string> Events;
        public ConfigValues()
        {
            DevID = 194439970797256706;
            RoleIDs = new List<ulong>();
            CommandChannels = new List<ulong>();
            AdminRoleIDs = new List<ulong>();
            AlertChannels = new List<ulong?>();
            UpdatesChannels = new List<ulong?>();
            DiscordActivity = new DiscordActivity()
            {
                Name = "over Destiny...",
                ActivityType = ActivityType.Watching,
            };
            Prefixes = new string[] { "ct!" };
            FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Catamagne");
            ConfigFolder = Path.Combine(FolderPath, "Config");
            ClansFolder = Path.Combine(FolderPath, "Clans");
            SpreadsheetID = null;
            BungieAPIKey = null;
            DiscordToken = null;
            Responses = new List<Response>();
            Events = JsonConvert.DeserializeObject<Dictionary<string, string>>("{ \"AutoBulk\": \"06:00:00\", \"Read\": \"00:15:00\" }");
        }
        public void SaveConfig()
        {
            Directory.CreateDirectory(configValues.ConfigFolder);
            var ConfigFile = Path.Combine(ConfigFolder, "config.json");
            var ResponsesFile = Path.Combine(ConfigFolder, "responses.json");
            File.WriteAllText(ConfigFile, JsonConvert.SerializeObject(this, Formatting.Indented));
            File.WriteAllText(ResponsesFile, JsonConvert.SerializeObject(this.Responses, Formatting.Indented));
        }
        public void LoadConfig()
        {
            var ConfigFile = Path.Combine(ConfigFolder, "config.json");
            var ResponsesFile = Path.Combine(ConfigFolder, "responses.json");
            var EventsFile = Path.Combine(ConfigFolder, "events.json");
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                configValues = JsonConvert.DeserializeObject<ConfigValues>(json);
                configValues.ConfigFolder = Path.Combine(configValues.FolderPath, "Config");
                configValues.ClansFolder = Path.Combine(configValues.FolderPath, "Clans");
                Console.WriteLine(string.Format("{0,-25} {1}", "Read configuration values from", ConfigFile));
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
                configValues.ConfigFolder = Path.Combine(configValues.FolderPath, "Config");
                configValues.ClansFolder = Path.Combine(configValues.FolderPath, "Clans");
                Console.WriteLine(string.Format("{0,-25} {1}", "Read configuration values from", ConfigFile));
            }
            if (File.Exists(ResponsesFile))
            {
                var json = File.ReadAllText(ResponsesFile);
                configValues.Responses = JsonConvert.DeserializeObject<List<Response>>(json);
                Console.WriteLine(string.Format("{0,-25} {1}", "Read responses from", ResponsesFile));
            }
            else
            {
                Console.WriteLine("No responses file found at {0}, creating one.", configValues.ConfigFolder);
                SaveConfig();
                var json = File.ReadAllText(ResponsesFile);
                configValues.Responses = JsonConvert.DeserializeObject<List<Response>>(json);
                Console.WriteLine(string.Format("{0,-25} {1}", "Read responses from", ResponsesFile));
            }
            if (File.Exists(EventsFile))
            {
                var json = File.ReadAllText(EventsFile);
                configValues.Events = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                Console.WriteLine(string.Format("{0,-25} {1}", "Read events from", EventsFile));
            }
            else
            {
                Console.WriteLine("No events file found at {0}, creating one.", configValues.ConfigFolder);
                configValues.Events = configValues.Events = JsonConvert.DeserializeObject<Dictionary<string, string>>("{ \"AutoBulk\": \"06:00:00\", \"Read\": \"00:15:00\" }");
                File.WriteAllText(EventsFile, JsonConvert.SerializeObject(this.Events, Formatting.Indented));
            }
        }
    }
    [Serializable]
    class Clans
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        public static List<Clan> clans = new List<Clan>() {
            new Clan(
                new Clan.Details(4170189, "Umbral", "ug", "Umbral!A2:F101", "#6E63BD"),
                new Clan.Members(new List<SpreadsheetTools.User>(), new List<SpreadsheetTools.User>(), new List<SpreadsheetTools.User>())
            )};
        public static void SaveClanMembers(Clan clan)
        {
            Directory.CreateDirectory(ConfigValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.ClansFolder, clan.details.ID.ToString());
            string clanMembersFile = Path.Combine(clanFolder, "Users.dat");
            string clanSpreadsheetFile = Path.Combine(clanFolder, "SpreadsheetUsers.dat");
            string clanLeaversFile = Path.Combine(clanFolder, "Leavers.dat");

            Directory.CreateDirectory(clanFolder);
            File.WriteAllText(clanMembersFile, JsonConvert.SerializeObject(clan.members.BungieUsers, Formatting.Indented));
            File.WriteAllText(clanSpreadsheetFile, JsonConvert.SerializeObject(clan.members.SpreadsheetUsers, Formatting.Indented));
            File.WriteAllText(clanLeaversFile, JsonConvert.SerializeObject(clan.members.ClanLeavers, Formatting.Indented));
            Console.WriteLine(string.Format("Wrote {0,-9} members to {1}\\", clan.details.Name, clanFolder));
            //Console.WriteLine("Wrote {0} members to {1}\\", clan.details.BungieNetName, clanFolder);
        }
        public static void SaveClanMembers(Clan clan, UserType userType)
        {
            Directory.CreateDirectory(ConfigValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.ClansFolder, clan.details.ID.ToString());
            string clanFile;
            Directory.CreateDirectory(clanFolder);
            switch (userType)
            {
                case UserType.BungieUser:
                    clanFile = Path.Combine(clanFolder, "Users.dat");
                    File.WriteAllText(clanFile, JsonConvert.SerializeObject(clan.members.BungieUsers, Formatting.Indented));
                    break;
                case UserType.SpreadsheetUser:
                    clanFile = Path.Combine(clanFolder, "SpreadsheetUsers.dat");
                    File.WriteAllText(clanFile, JsonConvert.SerializeObject(clan.members.SpreadsheetUsers, Formatting.Indented));
                    break;
                case UserType.Leaver:
                    clanFile = Path.Combine(clanFolder, "Leavers.dat");
                    File.WriteAllText(clanFile, JsonConvert.SerializeObject(clan.members.ClanLeavers, Formatting.Indented));
                    break;
                default:
                    throw new ArgumentException("Type provided is invalid.");

            }
            //var _ = string.Format("Wrote {0} members to", clan.details.BungieNetName);
            //Console.WriteLine(string.Format("{0,-35} {1}\\", _, clanFolder));
            Console.WriteLine(string.Format("Wrote {0,-9} members to {1}\\", clan.details.Name, clanFolder));
            //Console.WriteLine("Wrote {0} members to {1}\\", clan.details.BungieNetName, clanFile);
        }
        public static void LoadClanMembers(Clan clan)
        {
            string clanFolder = Path.Combine(ConfigValues.ClansFolder, clan.details.ID.ToString());
            string clanMembersFile = Path.Combine(clanFolder, "Users.dat");
            string clanSpreadsheetFile = Path.Combine(clanFolder, "SpreadsheetUsers.dat");
            string clanLeaversFile = Path.Combine(clanFolder, "Leavers.dat");
            if (!File.Exists(clanMembersFile) || !File.Exists(clanSpreadsheetFile) || !File.Exists(clanLeaversFile))
            {
                SaveClanMembers(clan);
            }
            var a = File.ReadAllText(clanMembersFile);
            clans[clans.FindIndex(t => t.details.ID == clan.details.ID)].members.BungieUsers = JsonConvert.DeserializeObject<List<SpreadsheetTools.User>>(a);
            var b = File.ReadAllText(clanSpreadsheetFile);
            clans[clans.FindIndex(t => t.details.ID == clan.details.ID)].members.SpreadsheetUsers = JsonConvert.DeserializeObject<List<SpreadsheetTools.User>>(b);
            var c = File.ReadAllText(clanLeaversFile);
            clans[clans.FindIndex(t => t.details.ID == clan.details.ID)].members.ClanLeavers = JsonConvert.DeserializeObject<List<SpreadsheetTools.User>>(c);
            //Console.WriteLine("Read {0} members from {1}\\", clan.details.BungieNetName, clanFolder);
            Console.WriteLine(string.Format("Read {0,-9} members from {1}\\", clan.details.Name, clanFolder));
        }
        public static void LoadClanMembers(Clan clan, UserType userType)
        {
            string clanFolder = Path.Combine(ConfigValues.ClansFolder, clan.details.ID.ToString());
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
                    clans[clans.FindIndex(t => t.details.ID == clan.details.ID)].members.BungieUsers = JsonConvert.DeserializeObject<List<SpreadsheetTools.User>>(a);
                    break;
                case UserType.SpreadsheetUser:
                    if (!File.Exists(clanSpreadsheetFile))
                    {
                        SaveClanMembers(clan, UserType.SpreadsheetUser);
                    }

                    var b = File.ReadAllText(clanSpreadsheetFile);
                    clans[clans.FindIndex(t => t.details.ID == clan.details.ID)].members.SpreadsheetUsers = JsonConvert.DeserializeObject<List<SpreadsheetTools.User>>(b);
                    break;
                case UserType.Leaver:
                    if (!File.Exists(clanLeaversFile))
                    {
                        SaveClanMembers(clan, UserType.Leaver);
                    }
                    var c = File.ReadAllText(clanLeaversFile);
                    clans[clans.FindIndex(t => t.details.ID == clan.details.ID)].members.ClanLeavers = JsonConvert.DeserializeObject<List<SpreadsheetTools.User>>(c);
                    break;
                default:
                    throw new ArgumentException("Type provided is invalid.");
            }
            Console.WriteLine(string.Format("Read {0,-9} members from {1}\\", clan.details.Name, clanFolder));
            //Console.WriteLine("Read {0} members from {1}\\", clan.details.BungieNetName, clanFolder);
        }
        public static void SaveClans()
        {
            Directory.CreateDirectory(ConfigValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.ClansFolder);
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
            Directory.CreateDirectory(ConfigValues.ClansFolder);
            string clanFolder = Path.Combine(ConfigValues.ClansFolder);
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
