using BungieSharper.Schema.User;
using Catamagne.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq.Extensions;


namespace Catamagne.API
{

    public class SpreadsheetTools
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Umbral Management Automation Experiment";
        //public static User[] spreadsheetUsers;
        //public static List<User> users;
        static GoogleCredential credential;
        static SheetsService service;
        public static async Task SetUpSheet()
        {
            using (var stream =
                new FileStream("credentials.dat", FileMode.Open, FileAccess.ReadWrite))
            {
                //fuck OATH 2.0 for console apps, that shit does not belong in a console app.
                credential = await GoogleCredential.FromStreamAsync(stream, CancellationToken.None);
            }
            // Create Google Sheets API service.
            service = new SheetsService(new()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            }); ;
        }
        public static async Task Read(Clan clan)
        {
            // Define requestRead parameters.
            String spreadsheetId = ConfigValues.SpreadsheetID;
            String range = clan.details.SpreadsheetRange;
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
                    _ = new User(BungieTools.GetBungieProfileLink(member), null, null, null, null, null, null, UserStatus.StatusEnum.ok, clan.details.Tag);
                    workingList.Add(_);
                });
                invalidMembers.ForEach(async member =>
                {
                    _ = new User(BungieTools.GetBungieProfileLink(member), null, null, null, null, null, null, UserStatus.StatusEnum.ok, clan.details.Tag);
                    workingList.Add(_);
                });
                Log.Information("Spreadsheet for " + clan.details.Name + " is empty, generating (will take 10 minutes)");
                forceBulkUpdate = true;
            }
            else
            {
                for (int i = 0; i < spreadsheetData.Count; i++)
                {
                    var user = new User();
                    //if (spreadsheetData[i] != null && spreadsheetData[i].Count > 0 && !string.IsNullOrEmpty(spreadsheetData[i][0].ToString()))
                    //{
                    //    switch (spreadsheetData[i].Count)
                    //    {
                    //        case 0:
                    //            break;
                    //        case 1:
                    //            user = new User(spreadsheetData[i][0].ToString(), null, null, null, null, null, null, UserStatus.ok, clan.details.Tag);
                    //            workingList.Add(user);
                    //            break;
                    //        case 2:
                    //            user = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, null, null, null, null, UserStatus.ok, clan.details.Tag);
                    //            workingList.Add(user);
                    //            break;
                    //        case 3:
                    //            user = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, null, null, UserStatus.ok, clan.details.Tag);
                    //            workingList.Add(user);
                    //            break;
                    //        case 4:
                    //            user = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), null, UserStatus.ok, clan.details.Tag);
                    //            workingList.Add(user);
                    //            break;
                    //        case 5:
                    //            user = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), spreadsheetData[i][4].ToString(), UserStatus.ok, clan.details.Tag);
                    //            workingList.Add(user);
                    //            break;
                    //        case 6:
                    //            user = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), spreadsheetData[i][4].ToString(), Enum.Parse<UserStatus>(spreadsheetData[i][5].ToString().ToLower()), clan.details.Tag);
                    //            workingList.Add(user);
                    //            break;
                    //        default:
                    //            List<string> extraColumns = new List<string>();
                    //            for (int index = 6; index < spreadsheetData[i].Count; index++)
                    //            {
                    //                extraColumns.Add(spreadsheetData[i][index].ToString());
                    //            }
                    //            var a = spreadsheetData[i];
                    //            var b = a[5].ToString().ToLower();
                    //            UserStatus d = UserStatus.ok;
                    //            var c = Enum.TryParse(b, false, out d);
                    //            user = new User(spreadsheetData[i][0].ToString(), spreadsheetData[i][1].ToString(), null, spreadsheetData[i][2].ToString(), null, spreadsheetData[i][3].ToString(), spreadsheetData[i][4].ToString(), d, clan.details.Tag, extraColumns);
                    //            workingList.Add(user);
                    //            break;
                    //    }
                    //}
                    if (spreadsheetData[i] != null && spreadsheetData[i].Count > 0 && !string.IsNullOrEmpty(spreadsheetData[i][0].ToString()))
                    {
                        var count = spreadsheetData[i].Count;
                        if (count > 0)
                        {
                            user.BungieProfile = spreadsheetData[i][0].ToString();
                        }
                        if (count > 1)
                        {
                            user.BungieName = spreadsheetData[i][1].ToString();
                        }
                        if (count > 2)
                        {
                            user.SteamProfile = spreadsheetData[i][2].ToString();
                        }
                        if (count > 3)
                        {
                            user.SteamName = spreadsheetData[i][3].ToString();
                        }
                        if (count > 4)
                        {
                            user.DiscordID = spreadsheetData[i][4].ToString();
                        }
                        if (count > 5)
                        {
                            user.UserStatus = UserStatus.ToEnum(spreadsheetData[i][5].ToString());
                        }
                        if (count > 6)
                        {
                            List<string> extraColumns = new List<string>();
                            for (int index = 6; index < spreadsheetData[i].Count; index++)
                            {
                                extraColumns.Add(spreadsheetData[i][index].ToString());
                            }
                            user.ExtraColumns = extraColumns;
                        }
                        workingList.Add(user);
                    }
                }
            }

            clan.members.SpreadsheetUsers = workingList;
            if (clan.members.BungieUsers == null || clan.members.BungieUsers.Count == 0)
            {
                forceBulkUpdate = true;
                Log.Information("User details for " + clan.details.Name + " is empty, generating (will take 10 minutes)");
                //Console.WriteLine("User details for " + clan.clanName + " is empty, generating (will take 10 minutes)");
            }

            if (forceBulkUpdate)
            {
                await BulkUpdate(clan, true);

            }
        }
        public static void Write(Clan clan)
        {
            String spreadsheetId = ConfigValues.SpreadsheetID;
            String range = clan.details.SpreadsheetRange;
            ValueRange valueRange = new ValueRange();
            var table = new List<IList<object>>();
            for (int c = 0; c < clan.members.BungieUsers.Count; c++)
            {
                List<object> _;
                if (string.IsNullOrEmpty(clan.members.BungieUsers[c].BungieProfile))
                {
                    _ = new List<object>(6) { "", "", "", "", "", "", "" };
                }
                else
                {
                    _ = new List<object>(6) { clan.members.BungieUsers[c].BungieProfile, clan.members.BungieUsers[c].BungieName, clan.members.BungieUsers[c].SteamProfile, clan.members.BungieUsers[c].SteamName, clan.members.BungieUsers[c].DiscordID, UserStatus.ToString(clan.members.BungieUsers[c].UserStatus) };
                }

                if (clan.members.BungieUsers[c].ExtraColumns != null)
                {
                    foreach (string column in clan.members.BungieUsers[c].ExtraColumns)
                    {
                        _.Add(column);
                    }
                    var letterStrings = clan.details.SpreadsheetRange.Split('!')[1].Split(':');
                    var a = char.ToUpper(letterStrings[0][0]) - 64;
                    var b = char.ToUpper(letterStrings[1][0]) - 64;
                    var forRange = b - a - clan.members.BungieUsers[c].ExtraColumns.Count - 5;
                    for (int i = 0; i < forRange; i++)
                    {
                        _.Add("");
                    }
                }
                else
                {
                    var letterStrings = clan.details.SpreadsheetRange.Split('!')[1].Split(':');
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
            for (int c = clan.members.BungieUsers.Count; c < 100; c++)
            {
                List<object> _ = new List<object>(5) { "", "", "", "", "", "" };
                table.Add(_);
            }
            valueRange.Values = table;
            SpreadsheetsResource.ValuesResource.UpdateRequest requestUpdate = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            requestUpdate.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

            // Prints the names and majors of students in a sample spreadsheet:
            var response = requestUpdate.Execute();
        }
        public static async Task BulkUpdate(Clan clan, bool skipRead = false)
        {
            if (!skipRead) await Read(clan);
            //ShowLoading("processing...");
            var _ = clan.members.SpreadsheetUsers;
            List<User> workingList = new List<User>();
            foreach (User user in _)
            {
                var workingUser = new User();
                if (!string.IsNullOrEmpty(user.BungieProfile))
                {
                    long? a = await BungieTools.GetBungieUserID(user.BungieProfile);
                    workingUser.BungieID = a.ToString();
                    if (!string.IsNullOrEmpty(workingUser.BungieID))
                    {
                        List<string> extraColumns = new List<string>();
                        string bungieProfile = user.BungieProfile;
                        string bungieID = workingUser.BungieID;
                        GeneralUser bungieUser = await BungieTools.GetBungieUser(Convert.ToInt64(bungieID));
                        string steamID = SteamTools.GetSteamID(bungieProfile);
                        string steamName = SteamTools.GetSteamUserName(steamID);
                        string steamProfile = "https://steamcommunity.com/profiles/" + steamID;
                        string bungieName = bungieUser.displayName;
                        string discordID = user.DiscordID;
                        UserStatus.StatusEnum userStatus = user.UserStatus;

                        if (user.ExtraColumns != null)
                        {
                            extraColumns = user.ExtraColumns;
                        }
                        if (steamName == null || steamID == null)
                        {
                            steamName = "N/A"; steamProfile = "N/A";
                        }
                        workingList.Add(new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, clan.details.Tag, extraColumns));
                    }
                    else
                    {
                        List<string> extraColumns = new List<string>();
                        string bungieProfile = user.BungieProfile;
                        string bungieID = "N/A";
                        string steamID = "N/A";
                        string steamProfile = "N/A";
                        string bungieName = "N/A";
                        string steamName = "N/A";
                        string discordID = user.DiscordID;
                        UserStatus.StatusEnum userStatus = user.UserStatus;
                        if (user.BungieID != "N/A" || user.BungieID != null)
                        {
                            bungieID = user.BungieID;
                        }
                        if (user.SteamID != "N/A" || user.SteamID != null)
                        {
                            steamID = user.SteamID;
                        }
                        if (user.SteamProfile != "N/A" || user.SteamProfile != null)
                        {
                            steamProfile = user.SteamProfile;
                        }
                        if (user.BungieName != "N/A" || user.BungieName != null)
                        {
                            bungieName = user.BungieName;
                        }
                        if (user.SteamName != "N/A" || user.SteamName != null)
                        {
                            steamName = user.SteamName;
                        }

                        if (user.ExtraColumns != null)
                        {
                            extraColumns = user.ExtraColumns;
                        }
                        workingList.Add(new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, clan.details.Tag, extraColumns));
                    }
                }
            }
            workingList.RemoveAll(t => string.IsNullOrEmpty(t.BungieProfile));
            workingList = workingList.DistinctBy(t => t.BungieProfile).ToList();
            workingList = workingList.OrderBy(t => t.SteamProfile).ToList();
            clan.members.BungieUsers = workingList;
            Write(clan);
            Clans.SaveClanMembers(clan);
        }
        public static async Task SelectiveUpdate(Clan clan, Changes changes)
        {
            await Read(clan);
            var workingList = clan.members.BungieUsers;

            foreach (User addedUser in changes.addedUsers)
            {
                var workingUser = new User();
                if (!string.IsNullOrEmpty(addedUser.BungieProfile))
                {
                    var a = await BungieTools.GetBungieUserID(addedUser.BungieProfile);
                    workingUser.BungieID = a.ToString();
                    if (!string.IsNullOrEmpty(workingUser.BungieID))
                    {
                        string bungieProfile = addedUser.BungieProfile;
                        string bungieID = workingUser.BungieID;
                        UserStatus.StatusEnum userStatus = addedUser.UserStatus;
                        GeneralUser bungieUser = await BungieTools.GetBungieUser(Convert.ToInt64(bungieID));
                        string steamID = SteamTools.GetSteamID(bungieProfile);
                        string steamProfile = "https://steamcommunity.com/profiles/" + steamID;
                        string bungieName = bungieUser.displayName;
                        string steamName = SteamTools.GetSteamUserName(steamID);
                        string discordID = addedUser.DiscordID;
                        string userClanTag = addedUser.ClanTag;
                        workingUser = new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, userClanTag);
                        workingList.Add(workingUser);

                    }
                    else
                    {
                        List<string> extraColumns = new List<string>();
                        string bungieProfile = addedUser.BungieProfile;
                        string bungieID = "N/A";
                        string steamID = "N/A";
                        string steamProfile = "N/A";
                        string bungieName = "N/A";
                        string steamName = "N/A";
                        string discordID = addedUser.DiscordID;
                        if (addedUser.BungieID != "N/A" || addedUser.BungieID != "")
                        {
                            bungieID = addedUser.BungieID;
                        }
                        if (addedUser.SteamID != "N/A" || addedUser.SteamID != "")
                        {
                            steamID = addedUser.SteamID;
                        }
                        if (addedUser.SteamProfile != "N/A" || addedUser.SteamProfile != "")
                        {
                            steamProfile = addedUser.SteamProfile;
                        }
                        if (addedUser.BungieName != "N/A" || addedUser.BungieName != "")
                        {
                            bungieName = addedUser.BungieName;
                        }
                        if (addedUser.SteamName != "N/A" || addedUser.SteamName != "")
                        {
                            steamName = addedUser.SteamName;
                        }

                        if (addedUser.ExtraColumns != null)
                        {
                            extraColumns = addedUser.ExtraColumns;
                        }
                        UserStatus.StatusEnum userStatus = addedUser.UserStatus;

                        if (addedUser.ExtraColumns != null)
                        {
                            extraColumns = addedUser.ExtraColumns;
                        }
                        workingList.Add(new User(bungieProfile, bungieName, bungieID, steamProfile, steamID, steamName, discordID, userStatus, clan.details.Tag, extraColumns));
                    }
                }
            }
            for (int i = 0; i < changes.updatedUsers.Count; i++)
            {
                var index = workingList.FindIndex(t => t.BungieProfile == changes.updatedUsers[i].BungieProfile);
                workingList[index] = changes.updatedUsers[i];
            }
            changes.removedUsers.ForEach(removedUser =>
            {
                var index = workingList.FindIndex(t => t.BungieProfile == removedUser.BungieProfile);
                workingList.RemoveAt(index);
            });
            workingList.RemoveAll(t => string.IsNullOrEmpty(t.BungieProfile));
            workingList = workingList.DistinctBy(t => t.BungieProfile).ToList();
            workingList = workingList.OrderBy(t => t.SteamProfile).ToList();
            clan.members.BungieUsers = workingList;
            Write(clan);
            Clans.SaveClanMembers(clan);
        }
        public static async Task SelectiveUpdate(Clan clan)
        {
            await Read(clan);
            var workingList = clan.members.BungieUsers;
            workingList.RemoveAll(t => string.IsNullOrEmpty(t.BungieProfile));
            workingList = workingList.DistinctBy(t => t.BungieProfile).ToList();
            workingList = workingList.OrderBy(t => t.SteamProfile).ToList();
            clan.members.BungieUsers = workingList;
            Write(clan);
            Clans.SaveClanMembers(clan);
        }
        public static async Task<Changes> CheckForChangesAsync(Clan clan)
        {
            await Read(clan);
            List<User> addedUsers = new List<User>(); List<User> removedUsers = new List<User>(); List<User> updatedUsers = new List<User>();
            for (int i = 0; i < clan.members.BungieUsers.Count; i++)
            {
                if (clan.members.BungieUsers[i].BungieProfile != null)
                {
                    if (!clan.members.SpreadsheetUsers.Select(t => t.BungieProfile).Contains(clan.members.BungieUsers[i].BungieProfile))
                    {
                        removedUsers.Add(clan.members.BungieUsers[i]);
                    }
                }
            }
            for (int i = 0; i < clan.members.SpreadsheetUsers.Count; i++)
            {
                if (clan.members.SpreadsheetUsers[i].BungieProfile != null)
                {
                    if (!clan.members.BungieUsers.Select(t => t.BungieProfile).Contains(clan.members.SpreadsheetUsers[i].BungieProfile))
                    {
                        addedUsers.Add(clan.members.SpreadsheetUsers[i]);
                    }
                }
            }
            for (int i = 0; i < clan.members.SpreadsheetUsers.Count; i++)
            {
                if (clan.members.SpreadsheetUsers[i].BungieProfile != null)
                {
                    if (clan.members.BungieUsers.Select(t => t.BungieProfile).Contains(clan.members.SpreadsheetUsers[i].BungieProfile))
                    {
                        bool userUpdated = false;
                        var workingList = clan.members.BungieUsers.Where(t => t.BungieProfile == clan.members.SpreadsheetUsers[i].BungieProfile);
                        User workingUser = workingList.FirstOrDefault();
                        if (workingList.FirstOrDefault().UserStatus != clan.members.SpreadsheetUsers[i].UserStatus)
                        {
                            workingUser.UserStatus = clan.members.SpreadsheetUsers[i].UserStatus;
                            userUpdated = true;
                        }
                        if (workingList.FirstOrDefault().DiscordID != clan.members.SpreadsheetUsers[i].DiscordID)
                        {
                            workingUser.DiscordID = clan.members.SpreadsheetUsers[i].DiscordID;
                            userUpdated = true;
                        }
                        if (workingList.FirstOrDefault().SteamName != clan.members.SpreadsheetUsers[i].SteamName)
                        {
                            workingUser.SteamName = clan.members.SpreadsheetUsers[i].SteamName;
                            userUpdated = true;
                        }
                        if (workingList.FirstOrDefault().SteamProfile != clan.members.SpreadsheetUsers[i].SteamProfile)
                        {
                            workingUser.SteamProfile = clan.members.SpreadsheetUsers[i].SteamProfile;
                            userUpdated = true;
                        }
                        if (workingList.FirstOrDefault().BungieName != clan.members.SpreadsheetUsers[i].BungieName)
                        {
                            workingUser.BungieName = clan.members.SpreadsheetUsers[i].BungieName;
                            userUpdated = true;
                        }
                        if (clan.members.SpreadsheetUsers[i].ExtraColumns != null)
                        {
                            if (workingList.FirstOrDefault().ExtraColumns != null)
                            {
                                var a = workingList.FirstOrDefault().ExtraColumns;
                                var b = clan.members.SpreadsheetUsers[i].ExtraColumns;
                                var c = (!a.SequenceEqual(b));
                                if (c)
                                {
                                    workingUser.ExtraColumns = clan.members.SpreadsheetUsers[i].ExtraColumns;
                                    userUpdated = true;
                                }
                            }
                            else
                            {
                                workingUser.ExtraColumns = clan.members.SpreadsheetUsers[i].ExtraColumns;
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
            if (Clans.clans.Any(clan => clan.members.BungieUsers.Select(t => t.DiscordID).Contains(userID)))
            {
                var clan = Clans.clans.Where(t => t.members.BungieUsers.Select(t => t.DiscordID).Contains(userID)).ToList();
                //var _ = clan.clanUsers.FindIndex(t => t.discordID == userID);
                var _ = clan.FirstOrDefault().members.BungieUsers.FindIndex(t => t.DiscordID == userID);
                if (_ != -1)
                {
                    User workingUser = clan.FirstOrDefault().members.BungieUsers[_];
                    workingUser.UserStatus = UserStatus.StatusEnum.leftDiscord;
                    clan.FirstOrDefault().members.BungieUsers[_] = workingUser;
                    Write(clan.FirstOrDefault());

                    return workingUser;
                }
            }
            return null;

        }
        public class User
        {
            public string BungieProfile;
            public string BungieName;
            public string BungieID;
            public string SteamProfile;
            public string SteamID;
            public string SteamName;
            public string DiscordID;
            public UserStatus.StatusEnum UserStatus;
            public List<string> ExtraColumns;
            public string ClanTag;

            public User()
            {
            }

            public User(string bungieLink, string bungieName, string bungieID, string steamProfile, string steamID, string steamName, string discordID, UserStatus.StatusEnum userStatus, string userClanTag, List<string> ExtraColumns = null)
            {
                this.BungieProfile = bungieLink;
                this.BungieName = bungieName;
                this.BungieID = bungieID;
                this.SteamProfile = steamProfile;
                this.SteamID = steamID;
                this.SteamName = steamName;
                this.DiscordID = discordID;
                this.UserStatus = userStatus;
                this.ClanTag = userClanTag;
                this.ExtraColumns = ExtraColumns;
            }
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
    public static class UserStatus
    {
        public enum StatusEnum
        {
            ok,
            leftClan,
            leftDiscord,
            leftDiscordClan,
            lobby
        }
        public static string ToString(this StatusEnum status)
        => status switch
        {
            StatusEnum.ok => "Okay",
            StatusEnum.leftClan => "Left Clan",
            StatusEnum.leftDiscord => "Left Discord",
            StatusEnum.leftDiscordClan => "Left Discord & Clan",
            StatusEnum.lobby => "Lobby"
        };
        public static StatusEnum ToEnum(this string status)
        {
            var lowered = status.ToLowerInvariant();

            if (lowered == "okay" || lowered == "ok")
                return StatusEnum.ok;
            if (lowered == "left clan" || lowered == "leftclan")
                return StatusEnum.leftClan;
            if (lowered == "left discord" || lowered == "leftdiscord")
                return StatusEnum.leftDiscord;
            if (lowered == "left discord & clan" || lowered == "left")
                return StatusEnum.leftDiscordClan;
            if (lowered == "lobby")
                return StatusEnum.lobby;

            return StatusEnum.ok; //return your default value
        }
    }
}
