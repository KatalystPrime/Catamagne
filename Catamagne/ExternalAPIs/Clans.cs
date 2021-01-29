using System;
using System.Collections.Generic;
using System.Text;

namespace Catamagne.API
{
    [Serializable] class Clan
    {
        public class Details
        {
            public string BungieNetClanID;
            public string BungieNetClanName;
            public string ClanTag;
            public string SpreadsheetClanRange;

            public Details(string BungieNetClanID, string BungieNetClanName, string ClanTag, string SpreadsheetClanRange)
            {
                this.BungieNetClanID = BungieNetClanID; this.BungieNetClanName = BungieNetClanName; this.ClanTag = ClanTag; this.SpreadsheetClanRange = SpreadsheetClanRange;
            }
        }
        public class Members
        {
            public List<Models.BungieUser> BungieUsers;
            public List<Models.SpreadsheetUser> SpreadsheetUsers;
            public List<Models.ClanLeaver> ClanLeavers;

            public Members(List<Models.BungieUser> BungieUsers, List<Models.SpreadsheetUser> SpreadsheetUsers, List<Models.ClanLeaver> ClanLeavers)
            {
                this.BungieUsers = BungieUsers; this.SpreadsheetUsers = SpreadsheetUsers; this.ClanLeavers = ClanLeavers;
            }
            public Members()
            {
                this.BungieUsers = new List<Models.BungieUser>(); this.SpreadsheetUsers = new List<Models.SpreadsheetUser>(); this.ClanLeavers = new List<Models.ClanLeaver>();
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
}
