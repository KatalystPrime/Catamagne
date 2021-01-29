using Catamagne.API.Models;
using Catamagne.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
namespace Catamagne.API
{
    namespace Models
    {
        enum UserStatus : int
        {
            [StringValue("Okay")]
            ok,
            [StringValue("Left clan")]
            leftClan,
            [StringValue("Left discord")]
            leftDiscord,
            [StringValue("Left clan and discord")]
            leftDiscordClan,
            [StringValue("Lobby")]
            lobby

        }
        class SpreadsheetUser
        {
            public string BungieNetLink;
            public string BungieNetName;
            public string SteamLink;
            public string SteamName;
            public ulong? DiscordID;
            public UserStatus UserStatus;
            public string[] ExtraColumns;
            public SpreadsheetUser(string BungieNetLink, string BungieNetName, string SteamLink, string SteamName, ulong? DiscordID, UserStatus UserStatus = UserStatus.ok, string[] ExtraColumns = null)
            {
                this.BungieNetLink = BungieNetLink; this.BungieNetName = BungieNetName; this.SteamLink = SteamLink; this.SteamName = SteamName; this.UserStatus = UserStatus; this.ExtraColumns = ExtraColumns;
            }
            public SpreadsheetUser()
            {
                this.BungieNetLink = null; this.BungieNetName = null; this.SteamLink = null; this.SteamName = null; this.DiscordID = null; this.UserStatus = UserStatus.ok; this.ExtraColumns = null;
            }
            public static explicit operator BungieUser(SpreadsheetUser s) => new BungieUser(s.BungieNetLink, null, s.BungieNetName, s.SteamLink, null, s.SteamName, s.DiscordID, s.UserStatus, s.ExtraColumns);
        }
        public class StringValueAttribute : Attribute
        {
            public string StringValue { get; protected set; }
            public StringValueAttribute(string value)
            {
                this.StringValue = value;
            }
            public string GetStringValue()
            {
                Type type = GetType();
                FieldInfo fieldInfo = type.GetField(ToString());

                // Get the stringvalue attributes
                StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
                    typeof(StringValueAttribute), false) as StringValueAttribute[];

                // Return the first if there was a match.
                return attribs.Length > 0 ? attribs[0].StringValue : null;
            }

        }


    }
    class SpreadsheetTools
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Glads Automation Program";
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
        public static async Task ReadSheet(Clan clan)
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
            bool forceBulkUpdate = false;

            if (spreadsheetData == null)
            {
                var _ = new SpreadsheetUser();
            }
        }
    }
}