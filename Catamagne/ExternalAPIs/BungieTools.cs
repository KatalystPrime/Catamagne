using BungieSharper.Client;
using BungieSharper.Schema;
using BungieSharper.Schema.GroupsV2;
using Catamagne.Configuration;
using Catamagne.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BungieSharper.Schema.User;

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
            public UserStatus.StatusEnum UserStatus;
            public string[] ExtraColumns;
            public bool? Private;
            public BungieUser(string BungieNetLink, ulong? BungieNetID, string BungieNetName, string SteamLink, ulong? SteamID, string SteamName, ulong? DiscordID, UserStatus.StatusEnum UserStatus = Models.UserStatus.StatusEnum.ok, string[] ExtraColumns = null, bool? Private = false)
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

        public static async Task<GroupResponse> GetClan(Clan clan)
        {
            var bungieClan = await bungieApi.ApiEndpoints.GroupV2_GetGroup(clan.details.BungieNetID);
            return bungieClan;
        }

        public static async Task<(List<GroupMember> publicMembers, List<GroupMember> privateMembers)> GetClanMembers(Clan clan)
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
        public static async Task<Clan> GetClanContainingUser(BungieUser user)
        {
            var _ = Clans.clans.Where(t => t.members.BungieUsers.Contains(user)).FirstOrDefault();
            return _;
        }
        public static async Task<long?> GetDestinyUserID(string profileLink)
        {
            UserMembershipData user = null;
            long? _ = GetMemberIDFromLink(profileLink);
            if (_.HasValue)
            {
                long profileID = (long)_;
                string profileType = "";
                if (profileLink.Length > 0)
                {
                    int numcount = 0;
                    var profileSplit = profileLink.Split('/');
                    foreach (var part in profileSplit)
                    {
                        int pType;
                        if (int.TryParse(part, out pType) && numcount == 0)
                        {
                            numcount++;
                            profileType = pType.ToString();
                        }
                    }
                    user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, Enum.Parse<BungieMembershipType>(profileType));
                }
                if (user.bungieNetUser != null)
                {
                    return user.bungieNetUser.membershipId;
                    var a = user.destinyMemberships.FirstOrDefault().membershipId;
                }
                else
                {
                    return user.destinyMemberships.FirstOrDefault().membershipId;
                }
            }
            return null;
        }
        public static long? GetMemberIDFromLink(string profileLink)
        {
            if (profileLink.Length > 0)
            {
                var _ = profileLink.Split('/');
                int numCount = 0;
                foreach (var part in _)
                {
                    long numPart;
                    if (long.TryParse(part, out numPart))
                    {
                        numCount++;
                    }
                    if (numCount == 2)
                    {
                        return Convert.ToInt64(numPart);
                    }
                }
                return null;
            }
            return null;
        }
    }
}
