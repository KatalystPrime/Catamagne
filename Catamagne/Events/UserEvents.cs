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

namespace Catamagne.Events
{
    class UserEvents
    {
        public static async Task Discord_GuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            new Thread(async () =>
            {
                var _ = SpreadsheetTools.CheckUserAgainstSpreadsheet(e.Member.Id.ToString());
                if (_ != null)
                {
                    var a = e.Member.Username;
                    var b = e.Member.Discriminator;
                    var c = e.Member.Id;
                    var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Member left Discord server!", "User was found on spreadsheet.", new List<Field>(2) { new Field("Username", e.Member.Username + '#' + e.Member.Discriminator), new Field("ID", e.Member.Id.ToString()) });
                    DiscordMessage message = await Core.Core.SendFancyMessage(Core.Core.alertsChannel, discordEmbed);
                    //DiscordMessage message = await CatamagneCore.SendAlert(string.Format("User detected leaving discord server; was on spreadsheet, id = {0}", e.Member.Id.ToString()));
                }
            }).Start();
        }
        public static async Task Discord_Ready(DiscordClient client, ReadyEventArgs r)
        {
            await Task.Run(() =>
            {
                DateTime startTime = DateTime.UtcNow + ConfigValues.configValues.LongInterval;
                AutoEvents.EventScheduler(startTime, ConfigValues.configValues.LongInterval, ConfigValues.clansList, AutoEvents.AutoBulkUpdateAsync);
                AutoEvents.EventScheduler(startTime, ConfigValues.configValues.ShortInterval , ConfigValues.clansList, AutoEvents.AutoScanForChangesAsync);
                AutoEvents.EventScheduler(startTime, ConfigValues.configValues.LongInterval, ConfigValues.clansList, AutoEvents.AutoCheckForLeavers);
                AutoEvents.EventScheduler(DateTime.UtcNow, ConfigValues.configValues.ShortInterval, ConfigValues.clansList, AutoEvents.AutoReadAsync);
                //AutoEvents.AutoScanForChanges();
                //AutoEvents.AutoBulkUpdate();
                //AutoEvents.AutoCheckForLeavers();
                return Task.CompletedTask;
            });
        }
        public static async Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            new Thread(async () =>
            {
                var _ = ConfigValues.configValues.Responses.ToList();
                if (_.Select(t => t.trigger).Contains(e.Message.Content))
                {
                    Response response = _.Find(t => t.trigger == e.Message.Content);
                    if (response.allowedChannels.Contains(e.Channel) || response.allowedChannels == null)
                    {
                        await e.Message.RespondAsync(response.response);
                    }
                }
            }).Start();
        }
    }
}
