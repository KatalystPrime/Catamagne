﻿using System;
using Catamagne.API;
using Catamagne.Attributes;
using System.Threading;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Catamagne.Configuration;
using System.Reflection;
using System.Linq;

namespace Catamagne.Events
{
    class AutoEvents
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        
        [ExcludeFromFind]
        public static void SetUp()
        {
            Dictionary<string, MethodInfo> methods = new();
            var random = new Random();

            var type = typeof(AutoEvents);
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var a = method.GetCustomAttribute<ExcludeFromFind>();
                if (a == null && !methods.ContainsKey(method.Name))
                {
                    methods.Add(method.Name, method);
                }
            }

            foreach (var method in methods)
            {
                var timeSpan = TimeSpan.FromHours(6);
                bool enabled = true;
                if (ConfigValues.Events.ContainsKey(method.Key))
                {
                    var matches = ConfigValues.Events.Where(t => t.Key == method.Key).ToList();
                    foreach (var match in matches) {
                        bool.TryParse(match.Value, out enabled);
                        TimeSpan.TryParse(match.Value, out timeSpan);
                    }
                }
                var startTime = DateTime.UtcNow + TimeSpan.FromMinutes(random.Next(0, 60));
                EventScheduler(startTime, timeSpan, Clans.clans, method.Value, enabled);
            }
        }
        [ExcludeFromFind]
        public static void EventScheduler(DateTime referenceTime, TimeSpan timeSpan, List<Clan> clans, MethodInfo action, bool enabled)
        {
            if (enabled)
            {
                new Thread(async () =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(Math.Max((referenceTime - DateTime.UtcNow).TotalMilliseconds, 0d)));
                    TimeSpan interval = timeSpan / clans.Count;
                    var done = false;
                    var index = 0;
                    var nextTime = DateTime.UtcNow;
                    while (!done)
                    {
                        if (DateTime.UtcNow >= (referenceTime + timeSpan))
                        {
                            referenceTime = DateTime.UtcNow;
                        }
                        action.Invoke(action, new[] { clans[index] });
                        index = (index + 1) % clans.Count;
                        Thread.Sleep(interval);
                    }
                }).Start();
            }
        }
        public static async Task Read(Clan clan)
        {
            await SpreadsheetTools.Read(clan);
        }
        public static async Task BulkUpdate(Clan clan)
        {
            List<DiscordMessage> messages = new List<DiscordMessage>();
            var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Bulk updating " + clan.details.Name + ".");
            Core.Discord.updatesChannels.ForEach(async channel => { messages.Add(Core.Discord.SendFancyMessage(channel, discordEmbed).Result); });
            //Console.WriteLine("Bulk updating for " + clan.details.BungieNetName);
            
            await SpreadsheetTools.BulkUpdate(clan);
            discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Bulk updated " + clan.details.Name, "Updated every cell in spreadsheet.");
            messages.ForEach(async message => { message.ModifyAsync(discordEmbed); });
        }
        public static async Task SelectiveUpdate(Clan clan)
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
        public static async Task CheckForLeavers(Clan clan)
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
        public static Task RotateActivity(Clan clan)
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
