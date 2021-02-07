using System;
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

            ConfigValues.configValues.LoadConfig();
            Console.Title = "Catamagne | Watcher of Destiny";
            Clans.LoadClans();
            MainAsync().GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            await SpreadsheetTools.SetUpSheet();
            PauseEvents = false;

            startTime = DateTime.UtcNow;
            await Discord.SetupClient();
            await Task.Delay(-1);
        }
    }  
}