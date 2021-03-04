using Catamagne.Configuration;
using HtmlAgilityPack;
using Serilog;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace Catamagne.API
{
        public class SteamTools
        {
            //public static 
            static ConfigValues ConfigValues => ConfigValues.configValues;
            public static string GetSteamUserName(string steamID)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load($"https://steamcommunity.com/profiles/{steamID}?xml=1");
                var steamIDs = doc.GetElementsByTagName("steamID");
                if (steamIDs != null && steamIDs.Count > 0)
                {
                    return steamIDs[0].InnerText;
                }
                return null;
            }
        public static string GetSteamID(string url)
        {
            var pattern = new Regex(@"(\(ID: (.*[0-9])\))");
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var removedspace = doc.DocumentNode.InnerText.Split('\n').Select(s => s.Trim());
            string filteredString = string.Concat(removedspace.Where(t => !string.IsNullOrEmpty(t)).ToArray());
            string result = pattern.Match(filteredString).Value;
            try
            {
                result = result[5..^1];
            }
            catch
            {
                Log.Debug("error grabbing steam ID from " + result + " at " + url);
                return null;
            }
            return result;
        }
    }
}
