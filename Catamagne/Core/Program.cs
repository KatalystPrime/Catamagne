using System;
using System.Reflection;
using System.Threading.Tasks;
using Catamagne.API;
using Catamagne.Configuration;

namespace Catamagne.Core
{
    class Core
    {
        public static DateTime startTime;
        public static bool PauseEvents;

        static void Main(string[] args)
        {

            Console.Title = "Catamagne | Watcher of Destiny";
            ConfigValues.configValues.LoadConfig();
            Clans.LoadClans();
            MainAsync().GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            PauseEvents = false;

            startTime = DateTime.UtcNow;
            await SpreadsheetTools.Configure();
            await Discord.SetupClient();
            await Task.Delay(-1);
        }
    }  
}