using BungieSharper.Client;
using BungieSharper.Schema;
using BungieSharper.Schema.GroupsV2;
using Catamagne.Configuration;
using Catamagne.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            public bool? Private;
            public BungieUser(string BungieNetLink, ulong? BungieNetID, string BungieNetName, string SteamLink, ulong? SteamID, string SteamName, ulong? DiscordID, UserStatus UserStatus = UserStatus.ok, string[] ExtraColumns = null, bool? Private = false)
            {
                this.BungieNetLink = BungieNetLink; this.BungieNetID = BungieNetID; this.BungieNetName = BungieNetName; this.SteamLink = SteamLink; this.SteamID = SteamID; this.SteamName = SteamName; this.DiscordID = DiscordID; this.UserStatus = UserStatus; this.ExtraColumns = ExtraColumns; this.Private = true;
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
    class BungieTools
    {
        static BungieApiClient bungieApi = new BungieApiClient(ConfigValues.configValues.BungieAPIKey);

        public async Task<GroupResponse> GetClan(Clan clan)
        {
            var bungieClan = await bungieApi.ApiEndpoints.GroupV2_GetGroup(clan.details.BungieNetID);
            return bungieClan;
        }

        public async Task<(List<GroupMember> publicMembers, List<GroupMember> privateMembers)> GetClanMembers(Clan clan)
        {
            SearchResultOfGroupMember group = await bungieApi.ApiEndpoints.GroupV2_GetMembersOfGroup(1, Convert.ToInt64(clan.details.BungieNetID));
            var groupList = group.results.ToList();
            var publicMembers = groupList.Where(t => t.bungieNetUserInfo != null).ToList();
            var privateMembers = groupList.Where(t => t.bungieNetUserInfo == null).ToList();

            return (publicMembers, privateMembers);
        }
        public static string GetBungieProfileLink(GroupMember groupMember)
        {
            var membershipType = groupMember.destinyUserInfo.membershipType;
            var membershipId = groupMember.destinyUserInfo.membershipId;

            return "https://www.bungie.net/en/Profile/" + Convert.ToInt32(membershipType) + "/" + membershipId.ToString();
        }
        public async Task<Clan> GetClanContainingUser(BungieUser user)
        {
            var _ = Clans.clans.Where(t => t.members.BungieUsers.Contains(user)).FirstOrDefault();
            return _;
        }
    }
}
