using HtmlAgilityPack;
using System;
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
