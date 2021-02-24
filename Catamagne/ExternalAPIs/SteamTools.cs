using Catamagne.Configuration;
using HtmlAgilityPack;
using Serilog;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Catamagne.API
{
    public class SteamTools
    {
        //public static 
        static ConfigValues ConfigValues => ConfigValues.configValues;
        public static string GetSteamUserName(string steamID)
        {
            var xmlData = WebRequest.Create($"https://steamcommunity.com/profiles/{steamID}?xml=1").GetResponse().GetResponseStream();

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlData);
            var steamIDs = doc.GetElementsByTagName("steamid");
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
            //foreach (var line in filteredArray)
            //{
            //    lineNumber++;
            //    if (line.Length > 4)
            //    {
            //        //Console.WriteLine(line.Substring(0, 5));
            //        if (line.Substring(0, 5) == "(ID: ")
            //        {
            //            Console.WriteLine(line.Substring(5, line.Length-6));
            //            return line;
            //        }
            //    }
            //}

            //Console.WriteLine(filteredArray[160]);
        }
        //public class SteamUser
        //{
        //    public string displayname;
        //    public ulong steamID64;
        //    public SteamUser(string username, ulong steamID)
        //    {
        //        displayname = username; steamID64 = steamID;
        //    }
        //}
    }
}
