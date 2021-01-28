using System.Threading;
using System.Threading.Tasks;
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
