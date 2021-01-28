using System;
using System.Collections.Generic;
using BungieSharper.Client;
using System.Linq;
using System.Threading.Tasks;
using BungieSharper.Schema.User;
using BungieSharper.Schema;
using BungieSharper.Schema.GroupsV2;
using Catamagne.Configuration;
using Catamagne.API.Models;

namespace Catamagne.API
{
    namespace Models
    {
        public class Clan
        {
            public string clanID;
            public string clanName;
            public string clanTag;
            public string clanSheetRange;
            public List<User> SpreadsheetUsers;
            public List<User> Users;
            public List<User> Leavers;
            public Clan(string clanID, string clanSheetRange, string clanName, string clanTag, List<User> spreadsheetUsers, List<User> clanUsers)
            {
                this.clanID = clanID; this.clanName = clanName; this.clanTag = clanTag; this.clanSheetRange = clanSheetRange; this.SpreadsheetUsers = spreadsheetUsers; this.Users = clanUsers;
            }
        }
    }

    class BungieTools
    {
        static BungieApiClient bungieApi = new BungieApiClient(ConfigValues.configValues.BungieAPIKey);
        public static async Task<long?> GetBungieUserID(string profileLink)
        {
            UserMembershipData user = null;
            var _ = GetMemberIDFromLink(profileLink);
            if (_.HasValue && profileLink.Length > 0)
            {
                long profileID = (long)_;
                user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, Enum.Parse<BungieMembershipType>(profileLink.Split('/')[5]));
            }
            return user.primaryMembershipId;
        }
        public static async Task<GeneralUser> GetBungieUser(long id)
        {
            var user = await bungieApi.ApiEndpoints.User_GetBungieNetUserById(id);
            return user;
        }
        public static async Task<GeneralUser> GetBungieUser(string profileLink)
        {
            return await GetBungieUser((long)await GetBungieUserID(profileLink));
        }
        public static async Task<SteamTools.SteamUser> GetSteamUser(string profileLink)
        {
            long? _ = GetMemberIDFromLink(profileLink);
            if (_.HasValue)
            {
                long profileID = (long)_;
                //var _ = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieSharper.Schema.BungieMembershipType.BungieNext);
                var profile = await bungieApi.ApiEndpoints.Destiny2_GetLinkedProfiles(profileID, BungieMembershipType.TigerSteam);
                ulong steamID = (ulong)profile.profiles.FirstOrDefault().membershipId;
                string steamName = profile.profiles.FirstOrDefault().displayName;
                return new SteamTools.SteamUser(steamName, steamID);
            }
            else return null;
        }
        public static long? GetMemberIDFromLink(string profileLink)
        {
            //Regex TypeandID = new Regex(@"\d.+(/)");
            //string _ = TypeandID.Match(profileLink).Value;
            //Regex ID = new Regex(@"/\b(.+)");
            //_ = ID.Match(_).Value;
            //_ = _.Substring(1, _.Length - 2);
            //return Convert.ToInt64(_);
            if (profileLink.Length > 0)
            {
                var _ = Convert.ToInt64(profileLink.Split('/')[6]);
                return _;
            }
            return null;
        }
        public static async Task<(List<UserInfoCard> publicMembers, List<GroupUserInfoCard> privateMembers)> GetClanMembers(Clan clan)
        {
            SearchResultOfGroupMember group = await bungieApi.ApiEndpoints.GroupV2_GetMembersOfGroup(1, Convert.ToInt64(clan.clanID));
            var groupList = group.results.ToList();
            var publicMembers = groupList.Where(t => t.bungieNetUserInfo != null).Select(t => t.bungieNetUserInfo).ToList();
            var privateMembers = groupList.Where(t => t.bungieNetUserInfo == null).Select(t => t.destinyUserInfo).ToList();
            return (publicMembers, privateMembers);
        }
        public static string GetBungieProfileLink(UserInfoCard bungieUser)
        {
            var membershipType = bungieUser.membershipType;
            var membershipId = bungieUser.membershipId;

            return "https://www.bungie.net/en/Profile/" + Convert.ToInt32(membershipType) + "/" + membershipId.ToString();
        }
        public static string GetBungieProfileLink(GroupUserInfoCard bungieUser)
        {
            var membershipType = bungieUser.membershipType;
            var membershipId = bungieUser.membershipId;

            return "https://www.bungie.net/en/Profile/" + Convert.ToInt32(membershipType) + "/" + membershipId.ToString();
        }
        public static async Task<List<User>> CheckForLeaves(Clan clan)
        {
            if (clan.Leavers != null)
            {
                await CheckForRejoiners(clan);
            }

            List<User> leavers = new List<User>();
            var ClanMembers = await GetClanMembers(clan);
            foreach (var member in clan.Users)
            {
                if (!ClanMembers.publicMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                {
                    if (member.bungieID != null)
                    {

                        leavers.Add(member);
                    }
                }
                if (!ClanMembers.privateMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                {
                    if (member.bungieID != null)
                    {
                        leavers.Add(member);
                    }
                }
            }
            foreach (var member in leavers)
            {
                var workingMember = member;
                var _ = clan.Users.FindIndex(t => t.bungieProfile == workingMember.bungieProfile);
                workingMember.UserStatus = Catamagne.API.Models.UserStatus.leftclan;
                clan.Users[_] = workingMember;
            }

            if (clan.Leavers != null)
            {
                List<User> oldLeavers = ConfigValues.clansList.Find(t => t == clan).Leavers;
                oldLeavers.AddRange(leavers);
                oldLeavers.Distinct().ToList();
                clan.Leavers = oldLeavers;
                ConfigValues.configValues.SaveConfig(true);
                SpreadsheetTools.Write(clan);
                return leavers;
            }
            else
            {
                clan.Leavers = leavers;
                ConfigValues.configValues.SaveConfig(true);
                SpreadsheetTools.Write(clan);

                return leavers;
            }
        }
        public static async Task CheckForRejoiners(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
            List<User> leavers = new List<User>();
            var ClanMembers = await GetClanMembers(clan);
            //SpreadsheetTools.savedUsers.ToList().ForEach(member => {
            //    if (!ClanMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
            //    {
            //        if (!oldLeavers.Select(t => t.bungieID).Contains(member.bungieID))
            //        {
            //            if (member.bungieID != null) leavers.Add(member);
            //        }
            //    }
            //});
            //clan.Leavers.ForEach(member =>
            //{

            //});;
            foreach (var member in clan.Leavers)
            {
                if (!ClanMembers.publicMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                {
                    leavers.Add(member);
                }
                if (!ClanMembers.privateMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                {
                    leavers.Add(member);
                }
            }
            clan.Leavers = leavers;
            ConfigValues.configValues.SaveConfig(true);

        }
        public static Clan GetClanFromTag(string clanTag)
        {
            if (ConfigValues.clansList.Any(t => t.clanTag == clanTag))
            {
                return (ConfigValues.clansList.Where(t => t.clanTag == clanTag).FirstOrDefault());
            }
            else return null;
        }
        public static async Task<List<User>> GetInactiveUsersAsync(Clan clan, int threshold = 14)
        {
            SearchResultOfGroupMember group = await bungieApi.ApiEndpoints.GroupV2_GetMembersOfGroup(1, Convert.ToInt64(clan.clanID));
            foreach (var member in group.results)
            {
                //Console.WriteLine(string.Format("Destiny2/{0}/Profile/{1}/", member.membershipType, member.membershipId));
                DateTime lastOnline = DateTime.UtcNow - new TimeSpan(member.lastOnlineStatusChange);
                //var a = destinyMember.characterActivities.data.Values;
            }
            throw new NotImplementedException("not finished.");
        }
    }
}
