using System;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catamagne.API;
using Serilog;

namespace Catamagne.Events
{
    class AutoEvents
    {
        public static void EventScheduler<T>(DateTime referenceTime, TimeSpan timeSpan, List<T> items, Func<T, Task> action, bool repeat = true)
        {
            new Thread(async () =>
            {
                TimeSpan interval = timeSpan / (items.Count + 1);
                var done = false;
                var index = 0;
                var nextTime = DateTime.UtcNow;
                while (!done)
                {
                    if (DateTime.UtcNow >= (referenceTime + timeSpan))
                    {
                        if (repeat)
                        {
                            referenceTime = DateTime.UtcNow;
                        }
                        else
                        {
                            done = true;
                        }
                    }
                    nextTime = DateTime.UtcNow + interval;
                    if (DateTime.UtcNow >= (referenceTime + (interval * index)))
                    {

                        await action.Invoke(items[index]);
                        index = (index + 1) % items.Count;
                    }

                    Thread.Sleep(nextTime - DateTime.UtcNow);
                }
            }).Start();
        }
        public static async Task AutoReadAsync(Clan clan)
        {
            Log.Information("Loading members of " + clan.details.BungieNetName);
            await SpreadsheetTools.Read(clan);
        }
    }
}
