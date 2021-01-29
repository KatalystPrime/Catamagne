using System;
using System.Collections.Generic;
using System.Text;

namespace Catamagne.API
{
    class Clan
    {
        [Serializable] public class Details
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
        public Details ClanDetails;

        [NonSerialized] public List<Models.BungieUser> BungieUsers;
        [NonSerialized] public List<Models.SpreadsheetUser> SpreadsheetUsers;
        [NonSerialized] public List<Models.ClanLeaver> ClanLeavers;

        public Clan(Details ClanDetails, List<Models.BungieUser> BungieUsers, List<Models.SpreadsheetUser> SpreadsheetUsers, List<Models.ClanLeaver> ClanLeavers)
        {
            this.ClanDetails = ClanDetails; this.BungieUsers = BungieUsers; this.SpreadsheetUsers = SpreadsheetUsers; this.ClanLeavers = ClanLeavers;
        }
    }
}
