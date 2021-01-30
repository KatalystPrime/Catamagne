using Catamagne.API.Models;
using Catamagne.Configuration;
using Catamagne.Configuration.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
namespace Catamagne.API
{
    class SpreadsheetTools
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Glads Automation Program";
        //public static List<User> users;
        static UserCredential credential;
        static SheetsService service;
        public static async Task Configure()
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
                    new FileDataStore(Path.Combine(ConfigValues.configValues.FolderPath, "config", credPath), true));
                Console.WriteLine("Credential file saved to: " + ConfigValues.configValues.FolderPath + credPath);
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
            var spreadsheetID = ConfigValues.configValues.SpreadsheetID;
            string spreadsheetRange = clan.details.SpreadsheetRange;
            SpreadsheetsResource.ValuesResource.GetRequest requestRead =
                    service.Spreadsheets.Values.Get(spreadsheetID, spreadsheetRange);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = requestRead.Execute();
            var spreadsheetData = response.Values;
            var workingList = new List<SpreadsheetUser>();
            bool grabAllMembers = false;

            if (spreadsheetData == null)
            {
                var _ = new SpreadsheetUser();
                var members = await BungieTools.GetClanMembers(clan);
                var publicMembers = members.publicMembers;
                var privateMembers = members.privateMembers;

                publicMembers.ForEach(async member =>
                {
                    _ = new SpreadsheetUser(BungieTools.GetBungieProfileLink(member), null, null, null, null, default, default);
                    workingList.Add(_);
                });
                privateMembers.ForEach(async member =>
                {
                    _ = new SpreadsheetUser(BungieTools.GetBungieProfileLink(member), null, null, null, null, default, default);
                });
                grabAllMembers = true;
            }
            else
            {
                for (int i = 0; i < spreadsheetData.Count; i++)
                {
                    var _ = new SpreadsheetUser();
                    if (spreadsheetData[i] != null && spreadsheetData[i].Count > 0)
                    {
                        var count = spreadsheetData[i].Count;
                        if (count > 0)
                        {
                            _.BungieNetLink = spreadsheetData[i][0].ToString();
                        }
                        if (count > 1)
                        {
                            _.BungieNetName = spreadsheetData[i][1].ToString();
                        }
                        if (count > 2)
                        {
                            _.SteamLink = spreadsheetData[i][2].ToString();
                        }
                        if (count > 3)
                        {
                            _.SteamName = spreadsheetData[i][3].ToString();
                        }
                        if (count > 4)
                        {
                            _.DiscordID = Convert.ToUInt64(spreadsheetData[i][4]);
                        }
                        if (count > 5)
                        {
                            _.UserStatus = UserStatus.ToEnum(spreadsheetData[i][5].ToString());
                        }
                        if (count > 6)
                        {
                            List<string> extraColumns = new List<string>();
                            for (int index = 6; index < spreadsheetData[i].Count; index++)
                            {
                                extraColumns.Add(spreadsheetData[i][index].ToString());
                            }
                            _.ExtraColumns = extraColumns.ToArray();
                        }
                        workingList.Add(_);
                    }
                }
            }
            clan.members.SpreadsheetUsers = workingList;
            if (clan.members.BungieUsers == null || clan.members.BungieUsers.Count == 0)
            {
                grabAllMembers = true;
            }

            if (grabAllMembers)
            {
                //BulkUpdate(clan);
            }
            Clans.SaveClanMembers(clan, UserType.SpreadsheetUser);
        }
        public static async Task Write(Clan clan)
        {
            String spreadsheetId = ConfigValues.configValues.SpreadsheetID;
            String range = clan.details.SpreadsheetRange;
            ValueRange valueRange = new ValueRange();
            var table = new List<IList<object>>();

            for (int c = 0; c < clan.members.BungieUsers.Count; c++)
            {
                List<object> _;
                _ = new List<object>(6)
                {
                    clan.members.BungieUsers[c].BungieNetLink,
                    clan.members.BungieUsers[c].BungieNetName,
                    clan.members.BungieUsers[c].SteamLink,
                    clan.members.BungieUsers[c].SteamName,
                    clan.members.BungieUsers[c].DiscordID,
                    UserStatus.ToString(clan.members.BungieUsers[c].UserStatus)
                };
                if (clan.members.BungieUsers[c].ExtraColumns != null)
                {
                    foreach (string column in clan.members.BungieUsers[c].ExtraColumns)
                    {
                        _.Add(column);
                    }
                    var letterStrings = clan.details.SpreadsheetRange.Split('!')[1].Split(':');
                    var a = char.ToUpper(letterStrings[0][0]) - 64;
                    var b = char.ToUpper(letterStrings[1][0]) - 64;
                    var forRange = b - a - clan.members.BungieUsers[c].ExtraColumns.Length - 5;
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
            Clans.SaveClanMembers(clan);
        }
    }
}