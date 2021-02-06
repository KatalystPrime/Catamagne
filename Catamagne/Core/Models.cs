using Catamagne.API.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Catamagne.API.Models
{
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

            if (lowered == "okay")
                return StatusEnum.ok;
            if (lowered == "left clan")
                return StatusEnum.leftClan;
            if (lowered == "left discord")
                return StatusEnum.leftDiscord;
            if (lowered == "left discord & clan")
                return StatusEnum.leftDiscordClan;
            if (lowered == "lobby")
                return StatusEnum.lobby;

            return StatusEnum.ok; //return your default value
        }
    }
    public class SpreadsheetUser
    {
        public string BungieNetLink;
        public string BungieNetName;
        public string SteamLink;
        public string SteamName;
        public ulong? DiscordID;
        public UserStatus.StatusEnum UserStatus;
        public string[] ExtraColumns;
        public SpreadsheetUser(string BungieNetLink, string BungieNetName, string SteamLink, string SteamName, ulong? DiscordID, UserStatus.StatusEnum UserStatus = Models.UserStatus.StatusEnum.ok, string[] ExtraColumns = null)
        {
            this.BungieNetLink = BungieNetLink; this.BungieNetName = BungieNetName; this.SteamLink = SteamLink; this.SteamName = SteamName; this.UserStatus = UserStatus; this.ExtraColumns = ExtraColumns;
        }
        public SpreadsheetUser()
        {
            this.BungieNetLink = null; this.BungieNetName = null; this.SteamLink = null; this.SteamName = null; this.DiscordID = null; this.UserStatus = Models.UserStatus.StatusEnum.ok; this.ExtraColumns = null;
        }
        public static explicit operator BungieUser(SpreadsheetUser s) => new BungieUser(s.BungieNetLink, null, s.BungieNetName, s.SteamLink, null, s.SteamName, s.DiscordID, s.UserStatus, s.ExtraColumns);
    }
    public class BungieUser
    {
        public string BungieNetLink;
        public ulong? BungieNetID;
        public string BungieNetName;
        public string SteamLink;
        public ulong? SteamID;
        public string SteamName;
        public ulong? DiscordID;
        public UserStatus.StatusEnum UserStatus;
        public string[] ExtraColumns;
        public bool? Private;
        public BungieUser(string BungieNetLink, ulong? BungieNetID, string BungieNetName, string SteamLink, ulong? SteamID, string SteamName, ulong? DiscordID, UserStatus.StatusEnum UserStatus = Models.UserStatus.StatusEnum.ok, string[] ExtraColumns = null, bool? Private = false)
        {
            this.BungieNetLink = BungieNetLink; this.BungieNetID = BungieNetID; this.BungieNetName = BungieNetName; this.SteamLink = SteamLink; this.SteamID = SteamID; this.SteamName = SteamName; this.DiscordID = DiscordID; this.UserStatus = UserStatus; this.ExtraColumns = ExtraColumns; this.Private = true;
        }
        public static explicit operator SpreadsheetUser(BungieUser b) => new SpreadsheetUser(b.BungieNetLink, b.BungieNetName, b.SteamLink, b.SteamName, b.DiscordID, b.UserStatus, b.ExtraColumns);
    }
    public class ClanLeaver
    {
        public string BungieNetLink;
        public string SteamLink;
        public ulong? DiscordID;
        public string[] ExtraColumns;
        public DateTime TimeLeft;
        public ClanLeaver(string BungieNetLink, string SteamLink, ulong? DiscordID, string[] ExtraColumns)
        {
            this.BungieNetLink = BungieNetLink; this.SteamLink = SteamLink; this.DiscordID = DiscordID; this.ExtraColumns = ExtraColumns;
        }
        public static explicit operator ClanLeaver(BungieUser b) => new ClanLeaver(b.BungieNetLink, b.SteamLink, b.DiscordID, b.ExtraColumns);
    }
}
namespace Catamagne.Configuration.Models
{
    enum UserType : int
    {
        BungieUser,
        SpreadsheetUser,
        ClanLeaver
    }
    public struct Changes
    {
        public Changes(List<SpreadsheetUser> addedUsers, List<SpreadsheetUser> removedUsers, List<SpreadsheetUser> updatedUsers)
        {
            this.addedUsers = addedUsers; this.removedUsers = removedUsers; this.updatedUsers = updatedUsers; this.TotalChanges = addedUsers.Count + removedUsers.Count + updatedUsers.Count;
        }
        public List<SpreadsheetUser> addedUsers; public List<SpreadsheetUser> removedUsers; public List<SpreadsheetUser> updatedUsers; public int TotalChanges;
    }
}
