using System;
using Catamagne.API;
using System.Threading;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;

namespace Catamagne.Events
{
    class AutoEvents
    {
        public static void EventScheduler(DateTime referenceTime, TimeSpan timeSpan, List<Clan> clans, Func<Clan, Task> action, bool repeat = true)
        {
            new Thread(async () =>
            {
               TimeSpan interval = timeSpan / (clans.Count + 1);
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
                       
                        await action.Invoke(clans[index]);
                        index = (index + 1) % clans.Count;
                    }
                    var time = TimeSpan.FromMilliseconds(Math.Max(0d, (nextTime - DateTime.UtcNow).TotalMilliseconds));
                    Thread.Sleep(time);
               }
            }).Start();  
        }
        public static async Task AutoReadAsync(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
        }
        public static async Task AutoBulkUpdateAsync(Clan clan)
        {
            Log.Information("Bulk updating for " + clan.details.BungieNetName);
            //Console.WriteLine("Bulk updating for " + clan.details.BungieNetName);
            var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Bulk updating " + clan.details.BungieNetName, "Automatically updating every spreadsheet element.");
            DiscordMessage message = await Core.Discord.SendFancyMessage(Core.Discord.updatesChannel, discordEmbed);
            await SpreadsheetTools.BulkUpdate(clan);
            discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Bulk updated " + clan.details.BungieNetName, "Updated every cell in spreadsheet.");
            await message.ModifyAsync(discordEmbed);
        }
        public static async Task AutoScanForChangesAsync(Clan clan)
        {
            Log.Information("Scanning for changes for " + clan.details.BungieNetName);
            var changed = await SpreadsheetTools.CheckForChangesAsync(clan);
            if (changed.TotalChanges > 0)
            {
                await SpreadsheetTools.SelectiveUpdate(clan, changed);
                if (changed.TotalChanges == 1)
                {
                    var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.details.BungieNetName, "Automatically processed 1 entry.");
                    DiscordMessage message = await Core.Discord.SendFancyMessage(Core.Discord.updatesChannel, discordEmbed);
                }
                else
                {
                    var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.details.BungieNetName, string.Format("Automatically processed {0} entries", changed.TotalChanges));
                    DiscordMessage message = await Core.Discord.SendFancyMessage(Core.Discord.updatesChannel, discordEmbed);
                }
            }
        }
        public static async Task AutoCheckForLeavers(Clan clan)
        {
            Log.Information("Checking for leavers for " + clan.details.BungieNetName);
            var Leavers = await BungieTools.CheckForLeaves(clan);

            Core.Discord.SendFancyListMessage(Core.Discord.alertsChannel ,clan, Leavers, "Users found leaving " + clan.details.BungieNetName + ":");
        }
    }
}
