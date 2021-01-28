using HtmlAgilityPack;
using System.Linq;
using System.Text.RegularExpressions;

namespace Catamagne.API
{
    public class SteamTools
    {
        public static string GetSteamID(string url)
        {
            var pattern = new Regex(@"(\(ID: (.*[0-9])\))");
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var removedspace = doc.DocumentNode.InnerText.Split('\n').Select(s => s.Trim());
            string filteredString = string.Concat(removedspace.Where(t => !string.IsNullOrEmpty(t)).ToArray());
            string result = pattern.Match(filteredString).Value;
            result = result[5..^1];
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            return null;
        }
        public class SteamUser
        {
            public string displayname;
            public ulong steamID64;
            public SteamUser(string username, ulong steamID)
            {
                displayname = username; steamID64 = steamID;
            }
        }
    }
}
