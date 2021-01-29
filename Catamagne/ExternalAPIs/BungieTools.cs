using System;

namespace Catamagne.API
{
    namespace Models
    {
        class BungieUser
        {
            public string BungieNetLink;
            public ulong? BungieNetID;
            public string BungieNetName;
            public string SteamLink;
            public ulong? SteamID;
            public string SteamName;
            public ulong? DiscordID;
            public UserStatus UserStatus;
            public string[] ExtraColumns;
            public BungieUser(string BungieNetLink, ulong? BungieNetID, string BungieNetName, string SteamLink, ulong? SteamID, string SteamName, ulong? DiscordID, UserStatus UserStatus = UserStatus.ok, string[] ExtraColumns = null)
            {
                this.BungieNetLink = BungieNetLink; this.BungieNetID = BungieNetID; this.BungieNetName = BungieNetName; this.SteamLink = SteamLink; this.SteamID = SteamID; this.SteamName = SteamName; this.DiscordID = DiscordID; this.UserStatus = UserStatus; this.ExtraColumns = ExtraColumns;
            }
            public static explicit operator SpreadsheetUser(BungieUser b) => new SpreadsheetUser(b.BungieNetLink, b.BungieNetName, b.SteamLink, b.SteamName, b.DiscordID, b.UserStatus, b.ExtraColumns);
        }
        class ClanLeaver
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
}
