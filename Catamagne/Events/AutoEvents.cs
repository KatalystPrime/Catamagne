using System;
using Catamagne.API;
using System.Threading;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Catamagne.Configuration;

namespace Catamagne.Events
{
    class AutoEvents
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        public static void SetUp()
        {
            var startTimeLong = DateTime.UtcNow + ConfigValues.LongInterval / 2;
            var startTimeShort = DateTime.UtcNow + ConfigValues.MediumInterval / 5 * 7;
            var activityTimeSpan = ConfigValues.MediumInterval * (Clans.clans.Count + 1);
            //var dailyTimeSpan = TimeSpan.FromDays(1);
            AutoEvents.EventScheduler(startTimeShort, ConfigValues.ShortInterval, Clans.clans, AutoEvents.AutoScanForChangesAsync);
            AutoEvents.EventScheduler(startTimeLong, ConfigValues.LongInterval, Clans.clans, AutoEvents.AutoCheckForLeavers);
            AutoEvents.EventScheduler(DateTime.UtcNow, activityTimeSpan, Clans.clans, AutoEvents.AutoRotateActivity);
            AutoEvents.EventScheduler(DateTime.UtcNow, ConfigValues.LongInterval, Clans.clans, AutoEvents.AutoBulkUpdateAsync);
            //AutoEvents.AutoScanForChanges();
            //AutoEvents.AutoBulkUpdate();
            //AutoEvents.AutoCheckForLeavers();
        }
        public static void EventScheduler(DateTime referenceTime, TimeSpan timeSpan, List<Clan> clans, Func<Clan, Task> action, bool repeat = true)
        {
            new Thread(async () =>
            {
               Thread.Sleep(DateTime.UtcNow - referenceTime);
               TimeSpan interval = timeSpan / clans.Count;
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
                    action.Invoke(clans[index]);
                    index = (index + 1) % clans.Count;
                    Thread.Sleep(interval);
               }
            }).Start();  
        }
        public static async Task AutoReadAsync(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
        }
        public static async Task AutoBulkUpdateAsync(Clan clan)
        {
            Log.Information("Bulk updating " + clan.details.Name);
            //Console.WriteLine("Bulk updating for " + clan.details.BungieNetName);
            List<DiscordMessage> messages = new List<DiscordMessage>();
            await SpreadsheetTools.BulkUpdate(clan);
            var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Bulk updated " + clan.details.Name, "Updated every cell in spreadsheet.");
            foreach (var channel in Core.Discord.updatesChannels)
            {
                messages.Add(await Core.Discord.SendFancyMessage(channel, discordEmbed));
            }
        }
        public static async Task AutoScanForChangesAsync(Clan clan)
        {
            Log.Information("Scanning for changes for " + clan.details.Name);
            var changed = await SpreadsheetTools.CheckForChangesAsync(clan);
            if (changed.TotalChanges > 0)
            {
                await SpreadsheetTools.SelectiveUpdate(clan, changed);
                DiscordEmbed discordEmbed;
                if (changed.TotalChanges == 1)
                {
                    discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.details.Name, "Automatically processed 1 entry.");
                }
                else
                {
                    discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.details.Name, string.Format("Automatically processed {0} entries", changed.TotalChanges));
                }
                List<DiscordMessage> messages = new List<DiscordMessage>();
                foreach (var channel in Core.Discord.updatesChannels)
                {
                    messages.Add(await Core.Discord.SendFancyMessage(channel, discordEmbed));
                }
            }
        }
        public static async Task AutoCheckForLeavers(Clan clan)
        {
            Log.Information("Checking for leavers for " + clan.details.Name);
            var Leavers = await BungieTools.CheckForLeaves(clan);

            foreach (var channel in Core.Discord.alertsChannels)
            {
                Core.Discord.SendFancyListMessage(channel, clan, Leavers, "Users found leaving " + clan.details.Name + ":");
            }
            //Core.Discord.alertsChannels.ForEach(async channel =>
            //{
            //    Core.Discord.SendFancyListMessage(channel, clan, Leavers, "Users found leaving " + clan.details.BungieNetName + ":");
            //});
            //Core.Discord.SendFancyListMessage(Core.Discord.alertsChannel ,clan, Leavers, "Users found leaving " + clan.details.BungieNetName + ":");
        }
        public static Task AutoRotateActivity(Clan clan)
        {
            var activity = new DiscordActivity()
            {
                Name = string.Format("over {0}...", clan.details.Name),
                ActivityType = ActivityType.Watching,
            };
            Log.Information("Rotating status to " + clan.details.Name);
            Core.Discord.discord.UpdateStatusAsync(activity);
            return Task.CompletedTask;
        }
    }
}
