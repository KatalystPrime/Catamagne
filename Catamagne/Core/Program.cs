using System;
using System.Threading.Tasks;
using Catamagne.API;

namespace Catamagne.Core
{
    class Core
    {
        public static DateTime startTime;
        public static bool PauseEvents;

        static void Main(string[] args)
        {

            Console.Title = "Catamagne | Watcher of Destiny";
            MainAsync().GetAwaiter().GetResult();
        }
        static async Task MainAsync()
        {
            PauseEvents = false;

            startTime = DateTime.UtcNow;
            await Discord.SetupClient();
            await Task.Delay(-1);
        }
    }  
}