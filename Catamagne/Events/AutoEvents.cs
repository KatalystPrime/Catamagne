using System;
using Catamagne.API;
using Catamagne.Core;
using System.Threading;
using DSharpPlus.Entities;
using System.Collections.Generic;
using Catamagne.Commands;
using System.Threading.Tasks;
using Catamagne.Configuration;

namespace Catamagne.Events
{
    class AutoEvents
    {
        public static void EventScheduler(DateTime referenceTime,TimeSpan timeSpan, List<Clan> clans, Func<Clan,Task> action)
        {
            new Thread(async () =>
            {
               TimeSpan interval = timeSpan / (clans.Count + 1);
               while (true)
               {
                    if (DateTime.UtcNow >= (referenceTime + timeSpan))
                    {
                        referenceTime = DateTime.UtcNow;
                    }
                    for (int i = 0; i < clans.Count; i++)
                    {
                        if (DateTime.UtcNow >= (referenceTime + (interval * i)))
                        {
                            await action.Invoke(clans[i]);
                        }
                    }
                    Thread.Sleep(interval);
               }
            }).Start();  
        }
        //public static void AutoBulkUpdate()
        //{

        //    DateTime startTime = Core.Core.startTime + (ConfigValues.configValues.ShortInterval / 4) - (ConfigValues.configValues.LongInterval / 4 * 3);

        //    new Thread(async () =>
        //    {
        //        Thread.CurrentThread.IsBackground = true;
        //        while (true)
        //        {
        //            if (((DateTime.UtcNow - startTime).TotalHours >= ConfigValues.configValues.LongInterval.TotalHours) && !Core.Core.PauseEvents)
        //            {
        //                Console.WriteLine("Bulk updating");
        //                var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Orange, "Bulk updating", "Automatically updating every spreadsheet element.");
        //                DiscordMessage message = await Core.Core.SendFancyMessage(Core.Core.updatesChannel, discordEmbed);
        //                startTime = DateTime.UtcNow;
        //                await SpreadsheetTools.BulkUpdateSheet();
        //                discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Bulk updated", "Updated every cell in spreadsheet.");
        //                await message.ModifyAsync(discordEmbed);

        //            }
        //            Thread.Sleep(TimeSpan.FromMinutes(15));
        //        }
        //    }).Start();
        //}
        public static async Task AutoReadAsync(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
        }
        public static async Task AutoBulkUpdateAsync(Clan clan)
        {
            Console.WriteLine("Bulk updating for " + clan.clanName);
            var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Orange, "Bulk updating " + clan.clanName, "Automatically updating every spreadsheet element.");
            DiscordMessage message = await Core.Core.SendFancyMessage(Core.Core.updatesChannel, discordEmbed);
            await SpreadsheetTools.BulkUpdate(clan);
            discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Bulk updated " + clan.clanName, "Updated every cell in spreadsheet.");
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
                    var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.clanName, "Automatically processed 1 entry.");
                    DiscordMessage message = await Core.Core.SendFancyMessage(Core.Core.updatesChannel, discordEmbed);
                }
                else
                {
                    var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes for " + clan.clanName, string.Format("Automatically processed {0} entries", changed.TotalChanges));
                    DiscordMessage message = await Core.Core.SendFancyMessage(Core.Core.updatesChannel, discordEmbed);
                }
            }
        }
        public static async Task AutoCheckForLeavers(Clan clan)
        {
            Console.WriteLine("checking");
            var Leavers = await BungieTools.CheckForLeaves(clan);

            Core.Core.SendFancyListMessage(Core.Core.alertsChannel ,clan, Leavers, "Users found leaving " + clan.clanName + ":");
        }
        //public static void AutoScanForChanges()
        //{

        //    DateTime startTime = Core.Core.startTime + ConfigValues.configValues.ShortInterval * 5;
        //    new Thread(async () =>
        //    {
        //        Thread.CurrentThread.IsBackground = true;
        //        while (true)
        //        {
        //            if (((DateTime.UtcNow - startTime) > ConfigValues.configValues.ShortInterval) && !Core.Core.PauseEvents)
        //            {
        //                Console.WriteLine("Scanning for changes");
        //                startTime = DateTime.UtcNow;
        //                var changed = await SpreadsheetTools.CheckForChangesAsync();
        //                if (changed.TotalChanges > 0)
        //                {
        //                    await SpreadsheetTools.SelectiveUpdateSheet(changed);
        //                    if (changed.TotalChanges == 1)
        //                    {
        //                        var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes", "Automatically processed 1 entry.");
        //                        DiscordMessage message = await Core.Core.SendFancyMessage(Core.Core.updatesChannel, discordEmbed);
        //                    }
        //                    else
        //                    {
        //                        var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Processed changes", string.Format("Automatically processed {0} entries", changed.TotalChanges));
        //                        DiscordMessage message = await Core.Core.SendFancyMessage(Core.Core.updatesChannel, discordEmbed);
        //                    }
        //                }
        //            }
        //            Thread.Sleep(TimeSpan.FromMinutes(1));
        //        }
        //    }).Start();
        //}

        //public static void AutoCheckForLeavers()
        //{
        //    DateTime startTime = Core.Core.startTime + (ConfigValues.configValues.LongInterval);
        //    Console.WriteLine("Checking for leavers");
        //    new Thread(async () =>
        //    {
        //        Thread.CurrentThread.IsBackground = true;
        //        while (true)
        //        {
        //            if ((DateTime.UtcNow - startTime) >= ConfigValues.configValues.LongInterval && !Core.Core.PauseEvents)
        //            {
        //                Console.WriteLine("checking");
        //                var Leavers = await BungieTools.CheckForLeaves(ConfigValues.configValues.BungieGroupID);

        //                if (Leavers.Count > 0)
        //                {
        //                    List<Field> fields = new List<Field>();
        //                    foreach (SpreadsheetTools.User user in Leavers)
        //                    {
        //                        if (user.discordID != null)
        //                        {
        //                            var _ = new Field(user.steamName, user.discordID);
        //                            fields.Add(_);
        //                        }

        //                    }

        //                    List<DiscordEmbed> embeds = new List<DiscordEmbed>();
        //                    if (fields.Count < 25)
        //                    {
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, fields.Count), "Users found leaving clan:"));
        //                    }
        //                    else if (fields.Count < 50)
        //                    {
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, 25), "Users found leaving clan:"));
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, fields.Count)));
        //                    }
        //                    else if (fields.Count < 75)
        //                    {
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, 25), "Users found leaving clan:"));
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, fields.Count)));
        //                    }
        //                    else
        //                    {
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, 25), "Users found leaving clan:"));
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, 75)));
        //                        embeds.Add(CoreModule.GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(75, fields.Count)));

        //                    }
        //                    List<DiscordMessage> messages = new List<DiscordMessage>();
        //                    embeds.ForEach(async embed => messages.Add(await Core.Core.SendFancyMessage(Core.Core.alertsChannel, embed)));
        //                }
        //            }
        //            Thread.Sleep(TimeSpan.FromMinutes(1));
        //        }
        //    }).Start();
        //}
    }
}
