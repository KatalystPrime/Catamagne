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
    class BungieTools
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;

        static BungieApiClient bungieApi = new BungieApiClient(ConfigValues.BungieAPIKey);
        public static async Task<long?> GetBungieUserID(string profileLink)
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
                        if (int.TryParse(part, out int pType) && numcount == 0)
                        {
                            numcount++;
                            profileType = pType.ToString();
                        }
                    }
                    user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, Enum.Parse<BungieMembershipType>(profileType));
                    //switch (Convert.ToInt32(profileLink.Split('/')[5]))
                    //{
                    //    case (int)BungieMembershipType.BungieNext:
                    //        user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.BungieNext);
                    //        break;
                    //    case (int)BungieMembershipType.TigerBlizzard:
                    //        user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerBlizzard);
                    //        break;
                    //    case (int)BungieMembershipType.TigerDemon:
                    //        user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerDemon);
                    //        break;
                    //    case (int)BungieMembershipType.TigerPsn:
                    //        user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerPsn);
                    //        break;
                    //    case (int)BungieMembershipType.TigerStadia:
                    //        user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerStadia);
                    //        break;
                    //    case (int)BungieMembershipType.TigerSteam:
                    //        user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerSteam);
                    //        break;
                    //    case (int)BungieMembershipType.TigerXbox:
                    //        user = await bungieApi.ApiEndpoints.User_GetMembershipDataById(profileID, BungieMembershipType.TigerXbox);
                    //        break;
                    //}
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
                var _ = profileLink.Split('/');
                int numCount = 0;
                foreach (var part in _)
                {
                    if (long.TryParse(part, out long numPart))
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
        public static async Task<(List<UserInfoCard> validMembers, List<GroupUserInfoCard> invalidMembers)> GetClanMembers(Clan clan)
        {
            SearchResultOfGroupMember group = await bungieApi.ApiEndpoints.GroupV2_GetMembersOfGroup(1, Convert.ToInt64(clan.details.ID));
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
            if (clan.members.ClanLeavers != null)
            {
                await CheckForRejoiners(clan);
            }
            if (!DontWrite)
            {
                if (clan.members.ClanLeavers != null)
                {
                    List<SpreadsheetTools.User> oldLeavers = clan.members.ClanLeavers;
                    List<SpreadsheetTools.User> leavers = new List<SpreadsheetTools.User>();
                    var ClanMembers = await GetClanMembers(clan);
                    foreach (var member in clan.members.BungieUsers)
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
                    }
                    //clan.Users.ForEach(member =>
                    //{

                        
                    //});
                    foreach (var member in leavers)
                    {
                        var workingMember = member;
                        var _ = clan.members.BungieUsers.FindIndex(t => t.bungieProfile == workingMember.bungieProfile);
                        workingMember.UserStatus = SpreadsheetTools.UserStatus.leftclan;
                        clan.members.BungieUsers[_] = workingMember;
                    }
                    oldLeavers.AddRange(leavers);
                    clan.members.ClanLeavers = oldLeavers;
                    Clans.SaveClanMembers(clan);
                    SpreadsheetTools.Write(clan);

                    return leavers;
                }
                else
                {
                    List<SpreadsheetTools.User> leavers = new List<SpreadsheetTools.User>();
                    var ClanMembers = await GetClanMembers(clan);
                    clan.members.BungieUsers.ForEach(member =>
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
                        var _ = clan.members.BungieUsers.FindIndex(t => t.bungieProfile == workingMember.bungieProfile);
                        workingMember.UserStatus = SpreadsheetTools.UserStatus.leftclan;
                        clan.members.BungieUsers[_] = workingMember;
                    }
                    clan.members.ClanLeavers = leavers;
                    Clans.SaveClanMembers(clan);
                    SpreadsheetTools.Write(clan);

                    return leavers;
                }
            }
            else
            {
                await SpreadsheetTools.Read(clan);
                List<SpreadsheetTools.User> leavers = new List<SpreadsheetTools.User>();
                var ClanMembers = await GetClanMembers(clan);
                clan.members.BungieUsers.ForEach(member => {
                    if (!ClanMembers.validMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                    {
                        if (member.bungieID != null)
                        {
                            leavers.Add(member);
                        }
                    }
                });
                //foreach (var member in leavers)
                //{
                //    var workingMember = member;
                //    var _ = clan.members.BungieUsers.FindIndex(t => t.bungieProfile == workingMember.bungieProfile);
                //    workingMember.UserStatus = SpreadsheetTools.UserStatus.leftclan;
                //    clan.members.BungieUsers[_] = workingMember;
                //}
                //SpreadsheetTools.Write(clan);
                return leavers;
            }
           

        }
        public static async Task CheckForRejoiners(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
            List<SpreadsheetTools.User> rejoiners = new List<SpreadsheetTools.User>();
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
            foreach (var member in clan.members.ClanLeavers)
            {
                if (!ClanMembers.validMembers.Select(t => t.membershipId).Contains(Convert.ToInt64(member.bungieID)))
                {
                    rejoiners.Add(member);
                }
            }
            clan.members.ClanLeavers = rejoiners;
            Clans.SaveClanMembers(clan, UserType.Leaver);

        }
        public static Clan GetClanFromTag(string clanTag)
        {
            if (Clans.clans.Any(t => t.details.Tag.ToLower() == clanTag.ToLower()))
            {
                return (Clans.clans.Where(t => t.details.Tag.ToLower() == clanTag.ToLower()).FirstOrDefault());
            }
            else return null;
        }
        public static Clan GetClanFromName(string clanName)
        {
            if (Clans.clans.Any(t => t.details.Name.ToLower() == clanName.ToLower()))
            {
                return (Clans.clans.Where(t => t.details.Name.ToLower() == clanName.ToLower()).FirstOrDefault());
            }
            else return null;
        }
        public static async Task<List<SpreadsheetTools.User>> GetInactiveUsersAsync(Clan clan, int threshold = 14)
        {
            var clanMembers = await GetClanMembers(clan);
            foreach (var member in clanMembers.validMembers)
            {
                //Console.WriteLine(string.Format("Destiny2/{0}/Profile/{1}/", member.membershipType, member.membershipId));
                var destinyMember = await bungieApi.ApiEndpoints.Destiny2_GetProfile(member.membershipId, member.membershipType);
                //var a = destinyMember.characterActivities.data.Values;
            }
            throw new NotImplementedException("not finished.");
        }
    }
}
