using BungieSharper.Schema.User;
using Catamagne.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Catamagne.API
{

    public class SpreadsheetTools
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Umbral Management Automation Experiment";
        //public static User[] spreadsheetUsers;
        //public static List<User> users;
        static UserCredential credential;
        static SheetsService service;
        public static async Task SetUpSheet()
        {
            using (var stream =
                new FileStream("credentials.dat", FileMode.Open, FileAccess.ReadWrite))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "drive",
                    CancellationToken.None,
                    new FileDataStore(Path.Combine(ConfigValues.configValues.Folderpath, "config", credPath), true));
                Console.WriteLine("Credential file saved to: " + ConfigValues.configValues.Folderpath + credPath);
            }

            // Create Google Sheets API service.
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }
        public static async Task Read(Clan clan)
        {
            // Define requestRead parameters.
            String spreadsheetId = ConfigValues.configValues.SpreadsheetID;
            String range = clan.clanSheetRange;
            SpreadsheetsResource.ValuesResource.GetRequest requestRead =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = requestRead.Execute();
            IList<IList<Object>> spreadsheetData = response.Values;
            var workingList = new List<User>();
            bool forceBulkUpdate = false;
            if (spreadsheetData == null)
            {
                var _ = new User();
                var members = await BungieTools.GetClanMembers(clan);

                var validMembers = members.validMembers;
                var invalidMembers = members.invalidMembers;
                validMembers.ForEach(async member =>
                {
                    _ = new User(BungieTools.GetBungieProfileLink(member), null, null, null, null, null, null, UserStatus.ok, clan.clanTag);
                    workingList.Add(_);
                });
                invalidMembers.ForEach(async member =>
                {
                    _ = new User(BungieTools.GetBungieProfileLink(member), null, null, null, null, null, null, UserStatus.ok, clan.clanTag);
                    workingList.Add(_);
                });
                Log.Information("Spreadsheet for " + clan.clanName + " is empty, generating (will take 10 minutes)");
                forceBulkUpdate = true;
            }
            else
            {
                for (int i = 0; i < spreadsheetData.Count; i++)
                {
                    var _ = new User();
                    if (spreadsheetData[i] != null)
                    {
                        switch (spreadsheetData[i].Count)
                        {
                            case 0:
                                break;
                            case 1:
                                _ = new User(spreadsheetData[i][0].ToString(), null, null, null, null, null, null, UserStatus.ok, clan.clanTag);
                                workingList.Add(_);
                                break;
                            case 2:
                                _ = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, null, null, null, null, UserStatus.ok, clan.clanTag);
                                workingList.Add(_);
                                break;
                            case 3:
                                _ = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, null, null, UserStatus.ok, clan.clanTag);
                                workingList.Add(_);
                                break;
                            case 4:
                                _ = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), null, UserStatus.ok, clan.clanTag);
                                workingList.Add(_);
                                break;
                            case 5:
                                _ = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), spreadsheetData[i][4].ToString(), UserStatus.ok, clan.clanTag);
                                workingList.Add(_);
                                break;
                            case 6:
                                _ = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), spreadsheetData[i][4].ToString(), Enum.Parse<UserStatus>(spreadsheetData[i][5].ToString().ToLower()), clan.clanTag);
                                workingList.Add(_);
                                break;
                            default:
                                List<string> extraColumns = new List<string>();
                                for (int index = 6; index < spreadsheetData[i].Count; index++)
                                {
                                    extraColumns.Add(spreadsheetData[i][index].ToString());
                                }
                                _ = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), spreadsheetData[i][4].ToString(), Enum.Parse<UserStatus>(spreadsheetData[i][5].ToString().ToLower()), clan.clanTag, extraColumns);
                                workingList.Add(_);
                                break;
                        }
                    }
                }
            }

            clan.spreadsheetUsers = workingList;
            if (clan.Users == null || clan.Users.Count == 0)
            {
                forceBulkUpdate = true;
                Log.Information("User details for " + clan.clanName + " is empty, generating (will take 10 minutes)");
                //Console.WriteLine("User details for " + clan.clanName + " is empty, generating (will take 10 minutes)");
            }

            if (forceBulkUpdate)
            {
                await BulkUpdate(clan, true);

            }
        }
        public static void Write(Clan clan)
        {
            String spreadsheetId = ConfigValues.configValues.SpreadsheetID;
            String range = clan.clanSheetRange;
            ValueRange valueRange = new ValueRange();
            var table = new List<IList<object>>();
            for (int c = 0; c < clan.Users.Count; c++)
            {
                List<object> _;
                if (string.IsNullOrEmpty(clan.Users[c].bungieProfile))
                {
                    _ = new List<object>(6) { "", "", "", "", "", "", "" };
                }
                else
                {
                    _ = new List<object>(6) { clan.Users[c].bungieProfile, clan.Users[c].bungieName, clan.Users[c].steamProfile, clan.Users[c].steamName, clan.Users[c].discordID, clan.Users[c].UserStatus.ToString() };
                }

                if (clan.Users[c].ExtraColumns != null)
                {
                    foreach (string column in clan.Users[c].ExtraColumns)
                    {
                        _.Add(column);
                    }
                    var letterStrings = clan.clanSheetRange.Split('!')[1].Split(':');
                    var a = char.ToUpper(letterStrings[0][0]) - 64;
                    var b = char.ToUpper(letterStrings[1][0]) - 64;
                    var forRange = b - a - clan.Users[c].ExtraColumns.Count() - 5;
                    for (int i = 0; i < forRange; i++)
                    {
                        _.Add("");
                    }
                }
                else
                {
                    var letterStrings = clan.clanSheetRange.Split('!')[1].Split(':');
                    var a = char.ToUpper(letterStrings[0][0]) - 64;
                    var b = char.ToUpper(letterStrings[1][0]) - 64;
                    var forRange = b - a - 5;
                    for (int i = 0; i < forRange; i++)
                    {
                        _.Add("");
                    }
                }
                table.Add(_);
            }
            for (int c = clan.Users.Count; c < 100; c++)
            {
                List<object> _ = new List<object>(5) { "", "", "", "", "", "" };
                table.Add(_);
            }
            valueRange.Values = table;
            SpreadsheetsResource.ValuesResource.UpdateRequest requestUpdate = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            requestUpdate.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            // Prints the names and majors of students in a sample spreadsheet:
            var response = requestUpdate.Execute();
            ConfigValues.configValues.SaveConfig(true);
        }
        public static async Task BulkUpdate(Clan clan, bool skipRead = false)
        {
            if (!skipRead) await Read(clan);
            //ShowLoading("processing...");
            Core.Core.PauseEvents = true;
            var _ = clan.spreadsheetUsers;
            List<User> workingList = new List<User>();
            foreach (User user in _)
            {
                var workingUser = new User();
                if (!string.IsNullOrEmpty(user.bungieProfile))
                {
                    long? a = await BungieTools.GetBungieUserID(user.bungieProfile);
                    workingUser.bungieID = a.ToString();
                    if (!string.IsNullOrEmpty(workingUser.bungieID))
                    {
                        List<string> extraColumns = new List<string>();
                        string bungieProfile = user.bungieProfile;
                        string bungieID = workingUser.bungieID;
                        GeneralUser bungieUser = await BungieTools.GetBungieUser(Convert.ToInt64(bungieID));
                        SteamTools.SteamUser steamUser = await BungieTools.GetSteamUser(bungieProfile);
                        string steamID = SteamTools.GetSteamID(bungieProfile);
                        string steamProfile = "https://steamcommunity.com/profiles/" + steamID;
                        string bungieName = bungieUser.displayName;
                        string steamName = steamUser.displayname;
                        string discordID = user.discordID;
                        UserStatus userStatus = user.UserStatus;

                        if (user.ExtraColumns != null)
                        {
                            extraColumns = user.ExtraColumns;
                        }
                        workingList.Add(new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, clan.clanTag, extraColumns));
                    }
                    else
                    {
                        List<string> extraColumns = new List<string>();
                        string bungieProfile = user.bungieProfile;
                        string bungieID = "N/A";
                        string steamID = "N/A";
                        string steamProfile = "N/A";
                        string bungieName = "N/A";
                        string steamName = "N/A";
                        string discordID = user.discordID;
                        UserStatus userStatus = user.UserStatus;
                        if (user.bungieID != "N/A" || user.bungieID != null)
                        {
                            bungieID = user.bungieID;
                        }
                        if (user.steamID != "N/A" || user.steamID != null)
                        {
                            steamID = user.steamID;
                        }
                        if (user.steamProfile != "N/A" || user.steamProfile != null)
                        {
                            steamProfile = user.steamProfile;
                        }
                        if (user.bungieName != "N/A" || user.bungieName != null)
                        {
                            steamID = user.bungieName;
                        }
                        if (user.steamName != "N/A" || user.steamName != null)
                        {
                            steamProfile = user.steamName;
                        }

                        if (user.ExtraColumns != null)
                        {
                            extraColumns = user.ExtraColumns;
                        }
                        workingList.Add(new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, clan.clanTag, extraColumns));
                    }
                }
            }
            workingList.RemoveAll(t => string.IsNullOrEmpty(t.bungieProfile));
            workingList.OrderBy(t => t.steamName);
            clan.Users = workingList;
            Write(clan);
            Core.Core.PauseEvents = false;
        }
        public static async Task SelectiveUpdate(Clan clan, Changes changes)
        {
            await Read(clan);
            var _ = clan.Users;

            foreach (User addedUser in changes.addedUsers)
            {
                var workingUser = new User();
                if (!string.IsNullOrEmpty(addedUser.bungieProfile))
                {
                    var a = await BungieTools.GetBungieUserID(addedUser.bungieProfile);
                    workingUser.bungieID = a.ToString();
                    if (!string.IsNullOrEmpty(workingUser.bungieID))
                    {
                        string bungieProfile = addedUser.bungieProfile;
                        string bungieID = workingUser.bungieID;
                        UserStatus userStatus = addedUser.UserStatus;
                        GeneralUser bungieUser = await BungieTools.GetBungieUser(Convert.ToInt64(bungieID));
                        SteamTools.SteamUser steamUser = await BungieTools.GetSteamUser(bungieProfile);
                        string steamID = SteamTools.GetSteamID(bungieProfile);
                        string steamProfile = "https://steamcommunity.com/profiles/" + steamID;
                        string bungieName = bungieUser.displayName;
                        string steamName = steamUser.displayname;
                        string discordID = addedUser.discordID;
                        string userClanTag = addedUser.clanTag;
                        workingUser = new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, userClanTag);
                        _.Add(workingUser);

                    }
                    else
                    {
                        List<string> extraColumns = new List<string>();
                        string bungieProfile = addedUser.bungieProfile;
                        string bungieID = "N/A";
                        string steamID = "N/A";
                        string steamProfile = "N/A";
                        string bungieName = "N/A";
                        string steamName = "N/A";
                        string discordID = addedUser.discordID;
                        if (addedUser.bungieID != "N/A" || addedUser.bungieID != "")
                        {
                            bungieID = addedUser.bungieID;
                        }
                        if (addedUser.steamID != "N/A" || addedUser.steamID != "")
                        {
                            steamID = addedUser.steamID;
                        }
                        if (addedUser.steamProfile != "N/A" || addedUser.steamProfile != "")
                        {
                            steamProfile = addedUser.steamProfile;
                        }
                        if (addedUser.bungieName != "N/A" || addedUser.bungieName != "")
                        {
                            steamID = addedUser.bungieName;
                        }
                        if (addedUser.steamName != "N/A" || addedUser.steamName != "")
                        {
                            steamProfile = addedUser.steamName;
                        }

                        if (addedUser.ExtraColumns != null)
                        {
                            extraColumns = addedUser.ExtraColumns;
                        }
                        UserStatus userStatus = addedUser.UserStatus;

                        if (addedUser.ExtraColumns != null)
                        {
                            extraColumns = addedUser.ExtraColumns;
                        }
                        _.Add(new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, clan.clanTag, extraColumns));
                    }
                }
            }
            for (int i = 0; i < changes.updatedUsers.Count; i++)
            {
                var index = _.FindIndex(t => t.bungieProfile == changes.updatedUsers[i].bungieProfile);
                _[index] = changes.updatedUsers[i];
            }
            changes.removedUsers.ForEach(removedUser =>
            {
                var index = _.FindIndex(t => t.bungieProfile == removedUser.bungieProfile);
                _.RemoveAt(index);
            });
            _.RemoveAll(t => string.IsNullOrEmpty(t.bungieProfile));
            _.OrderBy(t => t.steamName);
            clan.Users = _;
            Write(clan);
            Core.Core.PauseEvents = false;
        }
        public static async Task<Changes> CheckForChangesAsync(Clan clan)
        {
            await Read(clan);
            List<User> addedUsers = new List<User>(); List<User> removedUsers = new List<User>(); List<User> updatedUsers = new List<User>();
            for (int i = 0; i < clan.Users.Count; i++)
            {
                if (clan.Users[i].bungieProfile != null)
                {
                    if (!clan.spreadsheetUsers.Select(t => t.bungieProfile).Contains(clan.Users[i].bungieProfile))
                    {
                        removedUsers.Add(clan.Users[i]);
                    }
                }
            }
            for (int i = 0; i < clan.spreadsheetUsers.Count; i++)
            {
                if (clan.spreadsheetUsers[i].bungieProfile != null)
                {
                    if (!clan.Users.Select(t => t.bungieProfile).Contains(clan.spreadsheetUsers[i].bungieProfile))
                    {
                        addedUsers.Add(clan.spreadsheetUsers[i]);
                    }
                }
            }
            for (int i = 0; i < clan.spreadsheetUsers.Count; i++)
            {
                if (clan.spreadsheetUsers[i].bungieProfile != null)
                {
                    if (clan.Users.Select(t => t.bungieProfile).Contains(clan.spreadsheetUsers[i].bungieProfile))
                    {
                        bool userUpdated = false;
                        var _ = clan.Users.Where(t => t.bungieProfile == clan.spreadsheetUsers[i].bungieProfile);
                        User workingUser = _.FirstOrDefault();
                        if (_.FirstOrDefault().UserStatus != clan.spreadsheetUsers[i].UserStatus)
                        {
                            workingUser.UserStatus = clan.spreadsheetUsers[i].UserStatus;
                            userUpdated = true;
                        }
                        if (_.FirstOrDefault().discordID != clan.spreadsheetUsers[i].discordID)
                        {
                            workingUser.discordID = clan.spreadsheetUsers[i].discordID;
                            userUpdated = true;
                        }
                        if (_.FirstOrDefault().steamName != clan.spreadsheetUsers[i].steamName)
                        {
                            workingUser.steamName = clan.spreadsheetUsers[i].steamName;
                            userUpdated = true;
                        }
                        if (_.FirstOrDefault().steamProfile != clan.spreadsheetUsers[i].steamProfile)
                        {
                            workingUser.steamProfile = clan.spreadsheetUsers[i].steamProfile;
                            userUpdated = true;
                        }
                        if (_.FirstOrDefault().bungieName != clan.spreadsheetUsers[i].bungieName)
                        {
                            workingUser.bungieName = clan.spreadsheetUsers[i].bungieName;
                            userUpdated = true;
                        }
                        if (clan.spreadsheetUsers[i].ExtraColumns != null)
                        {
                            if (_.FirstOrDefault().ExtraColumns != null)
                            {
                                var a = _.FirstOrDefault().ExtraColumns;
                                var b = clan.spreadsheetUsers[i].ExtraColumns;
                                var c = (!a.SequenceEqual(b));
                                if (c)
                                {
                                    workingUser.ExtraColumns = clan.spreadsheetUsers[i].ExtraColumns;
                                    userUpdated = true;
                                }
                            }
                            else
                            {
                                workingUser.ExtraColumns = clan.spreadsheetUsers[i].ExtraColumns;
                                userUpdated = true;
                            }
                        }
                        if (userUpdated) updatedUsers.Add(workingUser);
                    }
                }
            }
            return new Changes(addedUsers, removedUsers, updatedUsers);
        }
        public static User CheckUserAgainstSpreadsheet(string userID)
        {
            //ConfigValues.clansList.ForEach(User) clan =>
            //{
            //    //var _ = clan.clanUsers.FindIndex(t => t.discordID == userID);
            //    //if (_ != -1)
            //    //{
            //    //    User workingUser = clan.clanUsers[_];
            //    //    workingUser.UserStatus = UserStatus.LeftDiscord;
            //    //    clan.clanUsers[_] = workingUser;
            //    //    WriteData(ConfigValues.clansList.FirstOrDefault());

            //    //    return workingUser;
            //    //}
            //    //return null;
            //});
            if (ConfigValues.clansList.Any(clan => clan.Users.Select(t => t.discordID).Contains(userID)))
            {
                var clan = ConfigValues.clansList.Where(t => t.Users.Select(t => t.discordID).Contains(userID)).ToList();
                //var _ = clan.clanUsers.FindIndex(t => t.discordID == userID);
                var _ = clan.FirstOrDefault().Users.FindIndex(t => t.discordID == userID);
                if (_ != -1)
                {
                    User workingUser = clan.FirstOrDefault().Users[_];
                    workingUser.UserStatus = UserStatus.leftdiscord;
                    clan.FirstOrDefault().Users[_] = workingUser;
                    Write(clan.FirstOrDefault());

                    return workingUser;
                }
            }
            return null;

        }
        public class User
        {
            public string bungieProfile;
            public string bungieName;
            public string bungieID; public string steamProfile;
            public string steamID; public string steamName;
            public string discordID; public UserStatus UserStatus;
            public List<string> ExtraColumns;
            public string clanTag;

            public User()
            {
            }

            public User(string bungieLink, string bungieName, string bungieID, string steamProfile, string steamID, string steamName, string discordID, UserStatus userStatus, string userClanTag, List<string> ExtraColumns = null)
            {
                this.bungieProfile = bungieLink;
                this.bungieName = bungieName;
                this.bungieID = bungieID;
                this.steamProfile = steamProfile;
                this.steamID = steamID;
                this.steamName = steamName;
                this.discordID = discordID;
                this.UserStatus = userStatus;
                this.clanTag = userClanTag;
                this.ExtraColumns = ExtraColumns;
            }

        }
        public enum UserStatus : ushort
        {
            ok,
            leftclan,
            leftdiscord,
            lobby
        }
        public struct Changes
        {
            public Changes(List<User> addedUsers, List<User> removedUsers, List<User> updatedUsers)
            {
                this.addedUsers = addedUsers; this.removedUsers = removedUsers; this.updatedUsers = updatedUsers; this.TotalChanges = addedUsers.Count + removedUsers.Count + updatedUsers.Count;
            }
            public List<User> addedUsers; public List<User> removedUsers; public List<User> updatedUsers; public int TotalChanges;
        }
    }
}
