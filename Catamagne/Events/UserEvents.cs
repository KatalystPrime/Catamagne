using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catamagne.API;
using Catamagne.Configuration;
using Catamagne.Core;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;

namespace Catamagne.Events
{
    class UserEvents
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        public static async Task Discord_GuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            new Thread(async () =>
            {
                var member = SpreadsheetTools.CheckUserAgainstSpreadsheet(e.Member.Id.ToString());
                if (member != null)
                {
                    var clan = BungieTools.GetClanFromTag(member.clanTag);
                    var discordEmbed = Core.Discord.CreateFancyMessage(clan.details.DiscordColour, clan.details.Name + " Member left Discord server!", "User was found on spreadsheet.", new List<Field>(2) { new Field("Username", e.Member.Username + '#' + e.Member.Discriminator), new Field("ID", e.Member.Id.ToString()) });
                    Log.Information("Detected " + clan.details.Name + " member leaving discord");
                    List<DiscordMessage> messages = new List<DiscordMessage>();
                    foreach (var channel in Core.Discord.alertsChannels)
                    {
                        messages.Add(await Core.Discord.SendFancyMessage(channel, discordEmbed));
                    }
                    //Core.Discord.alertsChannels.ForEach(async channel =>
                    //{
                    //    messages.Add(await Core.Discord.SendFancyMessage(channel, discordEmbed));
                    //});
                    //Core.Discord.alertsChannels.ForEach(async channel =>
                    //{
                    //    messages.Add(await Core.Discord.SendFancyMessage(channel, discordEmbed));
                    //});
                    //DiscordMessage message = await CatamagneCore.SendAlert(string.Format("User detected leaving discord server; was on spreadsheet, id = {0}", e.Member.Id.ToString()));
                }
            }).Start();
        }
        public static async Task Discord_Ready(DiscordClient client, ReadyEventArgs r)
        {
            await Task.Run(() =>
            {
                var startTimeLong = DateTime.UtcNow + ConfigValues.LongInterval / 2;
                var startTimeShort = DateTime.UtcNow + ConfigValues.MediumInterval / 5 * 7;
                var activityTimeSpan = ConfigValues.MediumInterval * (Clans.clans.Count+1);
                //var dailyTimeSpan = TimeSpan.FromDays(1);
                AutoEvents.EventScheduler(startTimeShort, ConfigValues.ShortInterval , Clans.clans, AutoEvents.AutoScanForChangesAsync);
                AutoEvents.EventScheduler(startTimeLong, ConfigValues.LongInterval, Clans.clans, AutoEvents.AutoCheckForLeavers);
                AutoEvents.EventScheduler(DateTime.UtcNow, activityTimeSpan, Clans.clans, Core.Discord.RotateActivity);
                //AutoEvents.AutoScanForChanges();
                //AutoEvents.AutoBulkUpdate();
                //AutoEvents.AutoCheckForLeavers();
                return Task.CompletedTask;
            });
        }
        public static async Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs messageArgs)
        {
            new Thread(async () =>
            {
                var responses = ConfigValues.Responses.ToList();
                if (responses.Select(response => response.trigger).Contains(messageArgs.Message.Content))
                {
                    Response response = responses.Find(r => r.trigger == messageArgs.Message.Content);
                    if (response.allowedChannels == null || response.allowedChannels.Contains(messageArgs.Channel))
                    {
                        await messageArgs.Message.RespondAsync(response.response);
                    }
                }
            }).Start();
        }
    }
}
