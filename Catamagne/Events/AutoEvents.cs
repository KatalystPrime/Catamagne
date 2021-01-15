using System;
using Catamagne.API;
using System.Threading;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    if (DateTime.UtcNow >= (referenceTime + (interval * index)))
                    {
                        await action.Invoke(clans[index]);
                        index = (index + 1) % clans.Count;
                    }
                    Thread.Sleep(interval / 2);
               }
            }).Start();  
        }
        public static async Task AutoReadAsync(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
        }
        public static async Task AutoBulkUpdateAsync(Clan clan)
        {
            Console.WriteLine("Bulk updating for " + clan.clanName);
            var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Bulk updating " + clan.clanName, "Automatically updating every spreadsheet element.");
            DiscordMessage message = await Core.Discord.SendFancyMessage(Core.Discord.updatesChannel, discordEmbed);
            await SpreadsheetTools.BulkUpdate(clan);
            discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Bulk updated " + clan.clanName, "Updated every cell in spreadsheet.");
            await message.ModifyAsync(discordEmbed);
        }
        public static async Task AutoScanForChangesAsync(Clan clan)
        {
            Console.WriteLine("Scanning for changes for " + clan.clanName);
            var changed = await SpreadsheetTools.CheckForChangesAsync(clan);
            if (changed.TotalChanges > 0)
            {
                await SpreadsheetTools.SelectiveUpdate(clan,changed);
                if (changed.TotalChanges == 1)
                {
                    var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.clanName, "Automatically processed 1 entry.");
                    DiscordMessage message = await Core.Discord.SendFancyMessage(Core.Discord.updatesChannel, discordEmbed);
                }
                else
                {
                    var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.clanName, string.Format("Automatically processed {0} entries", changed.TotalChanges));
                    DiscordMessage message = await Core.Discord.SendFancyMessage(Core.Discord.updatesChannel, discordEmbed);
                }
            }
        }
        public static async Task AutoCheckForLeavers(Clan clan)
        {
            Console.WriteLine("checking");
            var Leavers = await BungieTools.CheckForLeaves(clan);

            Core.Discord.SendFancyListMessage(Core.Discord.alertsChannel ,clan, Leavers, "Users found leaving " + clan.clanName + ":");
        }
    }
}
