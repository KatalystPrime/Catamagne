using System;
using System.Threading;
using System.Threading.Tasks;
using Catamagne.Configuration;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Catamagne.Events
{
    class UserEvents
    {
        public static async Task Discord_GuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            new Thread(async () =>
            {
            }).Start();
        }
        public static async Task Discord_Ready(DiscordClient client, ReadyEventArgs r)
        {
            await Task.Run(() =>
            {
                var startTimeLong = DateTime.UtcNow + ConfigValues.configValues.LongInterval / 2;
                var startTimeShort = DateTime.UtcNow + ConfigValues.configValues.LongInterval / 4;
                var activityTimeSpan = TimeSpan.FromMinutes(5) * (Clans.clans.Count + 1);
                AutoEvents.EventScheduler(DateTime.UtcNow, ConfigValues.configValues.ShortInterval, Clans.clans, AutoEvents.AutoReadAsync);
                //AutoEvents.EventScheduler(DateTime.UtcNow, ConfigValues.configValues.LongInterval, Clans.clans, AutoEvents.AutoCheckForLeavers);
                //AutoEvents.EventScheduler(DateTime.UtcNow, activityTimeSpan, ConfigValues.clansList, Core.Discord.RotateActivity);
                return Task.CompletedTask;
            });
        }
        public static async Task Discord_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs messageArgs)
        {
            new Thread(async () =>
            {
            }).Start();
        }
    }
}
