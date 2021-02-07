using System;
using System.Collections.Generic;
using System.Text;

namespace Catamagne.API
{
    [Serializable]
    public class Clan
    {
        public class Details
        {
            public long BungieNetID;
            public string BungieNetName;
            public string Tag;
            public string SpreadsheetRange;

            public Details(long BungieNetClanID, string BungieNetClanName, string ClanTag, string SpreadsheetClanRange)
            {
                this.BungieNetID = BungieNetClanID; this.BungieNetName = BungieNetClanName; this.Tag = ClanTag; this.SpreadsheetRange = SpreadsheetClanRange;
            }
        }
        public class Members
        {
            public List<SpreadsheetTools.User> BungieUsers;
            public List<SpreadsheetTools.User> SpreadsheetUsers;
            public List<SpreadsheetTools.User> ClanLeavers;

            public Members(List<SpreadsheetTools.User> BungieUsers, List<SpreadsheetTools.User> SpreadsheetUsers, List<SpreadsheetTools.User> ClanLeavers)
            {
                this.BungieUsers = BungieUsers; this.SpreadsheetUsers = SpreadsheetUsers; this.ClanLeavers = ClanLeavers;
            }
            public Members()
            {
                this.BungieUsers = new List<SpreadsheetTools.User>(); this.SpreadsheetUsers = new List<SpreadsheetTools.User>(); this.ClanLeavers = new List<SpreadsheetTools.User>();
            }
        }

        public Details details; public Members members;

        public Clan(Details ClanDetails, Members ClanMembers)
        {
            this.details = ClanDetails; this.members = ClanMembers;
        }

        //public Clan(Details ClanDetails, List<Models.BungieUser> BungieUsers, List<Models.SpreadsheetUser> SpreadsheetUsers, List<Models.ClanLeaver> ClanLeavers)
        //{
        //    this.ClanDetails = ClanDetails; this.BungieUsers = BungieUsers; this.SpreadsheetUsers = SpreadsheetUsers; this.ClanLeavers = ClanLeavers;
        //}
    }
    public enum UserType
    {
        BungieUser,
        SpreadsheetUser,
        Leaver,
    }
}
