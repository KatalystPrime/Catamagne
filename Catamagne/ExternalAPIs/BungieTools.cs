using System;
using System.Collections.Generic;
using BungieSharper.Client;
using System.Linq;
using System.Threading.Tasks;
using BungieSharper.Schema.User;
using BungieSharper.Schema;
using BungieSharper.Schema.GroupsV2;
using Catamagne.Configuration;

namespace Catamagne.API
{
    public class Clan
    {
        public string clanID;
        public string clanName;
        public string clanTag;
        public string clanSheetRange;
        public List<SpreadsheetTools.User> spreadsheetUsers;
        public List<SpreadsheetTools.User> Users;
        public List<SpreadsheetTools.User> Leavers;
        public Clan(string clanID, string clanSheetRange, string clanName, string clanTag, List<SpreadsheetTools.User> spreadsheetUsers, List<SpreadsheetTools.User> clanUsers)
        {
            this.clanID = clanID; this.clanName = clanName; this.clanTag = clanTag ; this.clanSheetRange = clanSheetRange; this.spreadsheetUsers = spreadsheetUsers; this.Users = clanUsers;
        }
    }
    class BungieTools
    {
        static BungieApiClient bungieApi = new BungieApiClient(ConfigValues.configValues.BungieAPIKey);
        public static async Task<long?> GetBungieUserID(string profileLink)
        {
            UserMembershipData user = null;
            long? _ = GetMemberIDFromLink(profileLink);
            if (_.HasValue)
            {
                long profileID = (long)_;
                if (profileLink.Length > 0)
                {
                    switch (Convert.ToInt32(profileLink.Split('/')[5]))
                    {
                        case (int)BungieMembershipType.BungieNext:
                            user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.BungieNext);
                            break;
                        case (int)BungieMembershipType.TigerBlizzard:
                            user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerBlizzard);
                            break;
                        case (int)BungieMembershipType.TigerDemon:
                            user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerDemon);
                            break;
                        case (int)BungieMembershipType.TigerPsn:
                            user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerPsn);
                            break;
                        case (int)BungieMembershipType.TigerStadia:
                            user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerStadia);
                            break;
                        case (int)BungieMembershipType.TigerSteam:
                            user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerSteam);
                            break;
                        case (int)BungieMembershipType.TigerXbox:
                            user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerXbox);
                            break;
                    }
                }
                if (user.bungieNetUser != null)
                {
                    return (user).bungieNetUser.membershipId;
                }

            }

            return null;
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
                var profile = await bungieApi.ApiEndpoints.Destiny2_GetLinkedProfiles(profileID, BungieSharper.Schema.BungieMembershipType.TigerSteam, true);
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
        public static async Task<(List<UserInfoCard> validMembers, List<GroupUserInfoCard> invalidMembers)> GetClanMembers(Clan clan)
        {
            SearchResultOfGroupMember group = await bungieApi.ApiEndpoints.GroupV2_GetMembersOfGroup(1, Convert.ToInt64(clan.clanID));
            var groupList = group.results.ToList();
            var validMembers = groupList.Where(t => t.bungieNetUserInfo != null).Select(t => t.bungieNetUserInfo).ToList();
            var invalidMembers = groupList.Where(t => t.bungieNetUserInfo == null).Select(t => t.destinyUserInfo).ToList();
            return (validMembers, invalidMembers);
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
        public static async Task<List<SpreadsheetTools.User>> CheckForLeaves(Clan clan, bool DontWrite = false)
        {
            if (!DontWrite)
            {
                if (clan.Leavers != null)
                {
                    await CheckForRejoiners(clan);
                    List<SpreadsheetTools.User> oldLeavers = ConfigValues.clansList.Find(t => t == clan).Leavers;
                    List<SpreadsheetTools.User> leavers = new List<SpreadsheetTools.User>();
                    var ClanMembers = await GetClanMembers(clan);
                    clan.Users.ForEach(member =>
                    {

                        if (!ClanMembers.validMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                        {
                            if (!oldLeavers.Select(t => t.bungieID).Contains(member.bungieID))
                            {
                                if (member.bungieID != null)
                                {

                                    leavers.Add(member);
                                }
                            }
                        }
                    });
                    foreach (var member in leavers)
                    {
                        var workingMember = member;
                        var _ = clan.Users.FindIndex(t => t.bungieProfile == workingMember.bungieProfile);
                        workingMember.UserStatus = SpreadsheetTools.UserStatus.LeftClan;
                        clan.Users[_] = workingMember;
                    }
                    oldLeavers.AddRange(leavers);
                    ConfigValues.configValues.SaveConfig(true);
                    SpreadsheetTools.Write(clan);

                    return leavers;
                }
                else
                {
                    List<SpreadsheetTools.User> leavers = new List<SpreadsheetTools.User>();
                    var ClanMembers = await GetClanMembers(clan);
                    clan.Users.ForEach(member =>
                    {
                        if (!ClanMembers.validMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                        {
                            if (member.bungieID != null)
                            {
                                leavers.Add(member);
                            }
                        }
                    });
                    foreach (var member in leavers)
                    {
                        var workingMember = member;
                        var _ = clan.Users.FindIndex(t => t.bungieProfile == workingMember.bungieProfile);
                        workingMember.UserStatus = SpreadsheetTools.UserStatus.LeftClan;
                        clan.Users[_] = workingMember;
                    }
                    ConfigValues.configValues.SaveConfig(true);
                    SpreadsheetTools.Write(clan);

                    return leavers;
                }
            }
            else
            {
                await SpreadsheetTools.Read(clan);
                List<SpreadsheetTools.User> leavers = new List<SpreadsheetTools.User>();
                var ClanMembers = await GetClanMembers(clan);
                clan.Users.ForEach(member => {
                    if (!ClanMembers.validMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                    {
                        if (member.bungieID != null)
                        {
                            leavers.Add(member);
                        }
                    }
                });
                foreach (var member in leavers)
                {
                    var workingMember = member;
                    var _ = clan.Users.FindIndex(t => t.bungieProfile == workingMember.bungieProfile);
                    workingMember.UserStatus = SpreadsheetTools.UserStatus.LeftClan;
                    clan.Users[_] = workingMember;
                }
                SpreadsheetTools.Write(clan);
                return leavers;
            }
           

        }
        public static async Task CheckForRejoiners(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
            List<SpreadsheetTools.User> rejoiners = clan.Leavers;
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
            rejoiners.ForEach(member =>
            {
                if (ClanMembers.validMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                {
                    rejoiners.Remove(member);
                }
            });;
        }
    }
}
