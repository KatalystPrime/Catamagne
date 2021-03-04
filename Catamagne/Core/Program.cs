using System;
using System.Threading;
using System.Threading.Tasks;
using Catamagne.API;
using Catamagne.Configuration;

namespace Catamagne.Core
{
    class Core
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        public static DateTime startTime;
        public static Random Random;

        //static void Main(string[] args)
        //{

        //    ConfigValues.LoadConfig();
        //    TrackTimeLive();
        //    Clans.LoadClans();
        //    MainAsync().GetAwaiter().GetResult();
        //}
        static async Task Main(string[] args)
        {
            Random = new Random();
            ConfigValues.LoadConfig();
            //TrackTimeLive();
            Clans.LoadClans();
            await SpreadsheetTools.SetUpSheet();

            startTime = DateTime.UtcNow;
            await Discord.SetupClient();
            await Task.Delay(-1);
        }
        //static void TrackTimeLive()
        //{
        //    new Thread(async () =>
        //    {
        //        while (true) 
        //        {
        //            var timePassed = DateTime.UtcNow - startTime;
        //            var timeString = "0 ms";
        //            if (timePassed > TimeSpan.FromSeconds(1))
        //            {
        //                timeString = string.Format("{0}s", timePassed.Seconds);
        //            }
        //            if (timePassed > TimeSpan.FromMinutes(1))
        //            {
        //                timeString = string.Format("{0}m {1}s", timePassed.Minutes, timePassed.Seconds);
        //            }
        //            if (timePassed > TimeSpan.FromHours(1))
        //            {
        //                timeString = string.Format("{0}h {1}m {2}s", timePassed.Hours, timePassed.Minutes, timePassed.Seconds);
        //            }
        //            if (timePassed > TimeSpan.FromDays(1))
        //            {
        //                timeString = string.Format("{0}d {1}h {2}m {3}s", timePassed.Days, timePassed.Hours, timePassed.Minutes, timePassed.Seconds);
        //            }
        //            Console.Title = string.Format("Catamagne | Watcher of Destiny | Live for {0}", timeString);
        //        }
        //    }).Start();
        //}
    }  
}