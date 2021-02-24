using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Catamagne.Core;
using Catamagne.API;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System.Threading;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Catamagne.Configuration;

namespace Catamagne.Commands
{
    public class CoreModule : BaseCommandModule
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        [Command("updateconfig")]
        [Description("Update configuration for the bot. Only admins can execute this.")]
        [Aliases("updateconf", "conf", "confupdate")]
        public async Task UpdateConf(CommandContext ctx)
        {
            var verification = await IsVerifiedAsync(ctx, true);
            if (verification == ErrorCode.Qualify)
            {
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Updating Configuration Files");
                    DiscordMessage message = await ctx.RespondAsync(discordEmbed);


                    //call bulkupdate method
                    ConfigValues.LoadConfig();
                    Clans.LoadClans();
                    await Core.Discord.UpdateChannels();
                    discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Done", "Sucessfully updated configuration files");
                    await message.ModifyAsync(discordEmbed);
                }).Start();
            }
        }
        [Command("updatesheet")]
        [Description("Scan for changed data.")]
        [Aliases("update")]
        public async Task UpdateSpreadSheet(CommandContext ctx, string clanTag)
        {
            var roles = ctx.Member.Roles.ToList();
            var verification = await IsVerifiedAsync(ctx, true);
            var clan = await GetClanFromTagOrNameAsync(ctx, clanTag);
            clanTag = clanTag.ToLower();

            if (clan != null && verification == ErrorCode.Qualify && !string.IsNullOrEmpty(clan.details.Tag))
            {
                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Scanning for Changes...");
                DiscordMessage msg = await ctx.RespondAsync(discordEmbed);
                new Thread(async () =>
                {
                    await SpreadsheetTools.Read(clan);
                    var _ = await SpreadsheetTools.CheckForChangesAsync(clan);
                    if (_.TotalChanges > 0)
                    {
                        TimeSpan t = TimeSpan.FromSeconds((_.addedUsers.Count * 5) + (_.updatedUsers.Count * 0.1) + (_.removedUsers.Count * 0.1));
                        discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Yellow, "Found changes", string.Format("{0} change(s) found...", _.TotalChanges), new List<Field>() { new Field("ETA", t.ToString(@"mm\:ss")) });
                        await msg.ModifyAsync(discordEmbed);
                        await SpreadsheetTools.SelectiveUpdate(clan, _);
                        discordEmbed = Core.Discord.CreateFancyMessage(clan.details.DiscordColour, "Done", string.Format("Successfully processed {0} changes.", _.TotalChanges));
                        await msg.ModifyAsync(discordEmbed);
                    }
                    else
                    {
                        await SpreadsheetTools.SelectiveUpdate(clan);
                        var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "No changes found", "To update steam names, please run a bulk update.");
                        await msg.ModifyAsync(discordEmbed);
                    }

                }).Start();
            }
        }
        [Command("bulkupdate")]
        [Description("Update all the data in the sheets. This takes upwards of 10 minutes")]
        [Aliases("bulk")]
        public async Task BulkUpdateSheet(CommandContext ctx, string clanTag)
        {
            var roles = ctx.Member.Roles.ToList();
            var verification = await IsVerifiedAsync(ctx, true);
            var clan = await GetClanFromTagOrNameAsync(ctx, clanTag);
            clanTag = clanTag.ToLower();

            if (clan != null && verification == ErrorCode.Qualify && !string.IsNullOrEmpty(clan.details.Tag))
            {
                TimeSpan t = TimeSpan.FromSeconds(5 * clan.members.SpreadsheetUsers.Count);
                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Bulk Updating", "Updating every element in spreadsheet...", new List<Field>() { new Field("ETA", t.ToString(@"mm\:ss")) });
                DiscordMessage msg = await ctx.RespondAsync(discordEmbed);
                new Thread(async () =>
                {
                    await SpreadsheetTools.BulkUpdate(clan);
                    discordEmbed = Core.Discord.CreateFancyMessage(clan.details.DiscordColour, "Done", string.Format("Successfully bulk updated {0} members", clan.members.BungieUsers.Count));
                    await msg.ModifyAsync(discordEmbed);
                }).Start();
            }
        }
        [Command("displayUsers")]
        [Description("Output all stored users")]
        [Aliases("users")]
        public async Task DisplayUsers(CommandContext ctx, string clanTag, [RemainingText] string mode)
        {
            var roles = ctx.Member.Roles.ToList();
            var verification = await IsVerifiedAsync(ctx, true);
            var clan = await GetClanFromTagOrNameAsync(ctx, clanTag);
            clanTag = clanTag.ToLower();

            if (clan != null && verification == ErrorCode.Qualify && !string.IsNullOrEmpty(clan.details.Tag))
            {
                new Thread(async () =>
                {
                    if (string.IsNullOrEmpty(mode) || mode == "spreadsheet" || mode == "sheet")
                    {
                        await SpreadsheetTools.Read(clan);
                        var users = clan.members.SpreadsheetUsers.ToList();
                        users = users.OrderBy(t => t.SteamName).ToList();

                        Core.Discord.SendFancyListMessage(ctx.Channel, clan, users, "Users on spreadsheet for " + clan.details.Name + ":");
                    }
                    else if (mode == "saved data" || mode == "saved" || mode == "file")
                    {
                        List<SpreadsheetTools.User> users = clan.members.BungieUsers;
                        users = users.OrderBy(t => t.SteamName).ToList();

                        Core.Discord.SendFancyListMessage(ctx.Channel, clan, users, "Users for " + clan.details.Name + ":");

                    }
                    else
                    {
                        var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Additional arguments required. eithe use:\ndisplayusers sheet or displayusers saved");
                        await ctx.RespondAsync(discordEmbed);
                    }
                }).Start();
            }
        }
        [Command("checkleavers")]
        [Description("Check if any users have left the clan.")]
        [Aliases("leavers", "checkleaves", "leaves")]
        public async Task CheckForLeavers(CommandContext ctx, string clanTag)
        {
            var roles = ctx.Member.Roles.ToList();
            var verification = await IsVerifiedAsync(ctx, true);
            var clan = await GetClanFromTagOrNameAsync(ctx, clanTag);
            clanTag = clanTag.ToLower();

            if (clan != null && verification == ErrorCode.Qualify && !string.IsNullOrEmpty(clan.details.Tag))
            {
                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Turquoise, "Checking for leavers...");
                DiscordMessage msg = await ctx.RespondAsync(discordEmbed);
                new Thread(async () =>
                {
                    var leavers = await BungieTools.CheckForLeaves(clan, true);
                    if (leavers.Count > 0)
                    {
                        await msg.DeleteAsync();
                        var fields = new List<Field>();
                        Core.Discord.SendFancyListMessage(ctx.Channel, clan, leavers, "Users found leaving " + clan.details.Name + ":");

                    }
                    else
                    {
                        var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "No leavers found", "No one to remove from sheet <:unipeepo:601277029459034112>");
                        await msg.ModifyAsync(discordEmbed);
                    }

                }).Start();
            }
        }

        public static async Task<ErrorCode> IsVerifiedAsync(CommandContext ctx, bool isAdminCommand = false, bool isAvailableEverywhere = false)
        {
            DiscordEmbed discordEmbed;
            if (ctx.Member.IsOwner)
            {
                return ErrorCode.Qualify;
            }
            if (!Core.Discord.commandChannels.Contains(ctx.Channel) && !isAvailableEverywhere)
            {
                discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You're not allowed to run that command here.");
                await ctx.RespondAsync(discordEmbed);
                return ErrorCode.UnqualifyChannel;
            }
            if (ctx.Member.Id == ConfigValues.DevID)
            {
                return ErrorCode.Qualify;
            }
            var roleVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.RoleIDs);
            var adminVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.AdminRoleIDs);
            if (adminVerf.Any())
            {
                return ErrorCode.Qualify;
            }
            if (ctx.Member.IsOwner)
            {
                return ErrorCode.Qualify;
            }
            if (roleVerf.Any() && !isAdminCommand)
            {
                return ErrorCode.Qualify;
            }
            discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You're not allowed to run that command.");
            await ctx.RespondAsync(discordEmbed);
            return ErrorCode.UnqualifyRole;
        }
        static async Task<Clan> GetClanFromTagOrNameAsync(CommandContext ctx, string clanNameOrTag)
        {
            var clanByTag = BungieTools.GetClanFromTag(clanNameOrTag);
            var clanByName = BungieTools.GetClanFromName(clanNameOrTag);
            if (clanByTag != null)
            {
                return clanByTag;
            }
            else if (clanByName != null)
            {
                return clanByName;
            }
            else
            {
                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "The clan you provided is invalid!");
                await ctx.RespondAsync(discordEmbed);
                return null;
            }
        }
    }

    public class UserInteractionsModule : BaseCommandModule
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        [Command("responses")]
        [Description("View current responses stored and watched for")]
        [Aliases("response")]
        public async Task Responses(CommandContext ctx, string args = null, [RemainingText] string text = null)
        {
            var roles = ctx.Member.Roles.ToList();
            var verification = await CoreModule.IsVerifiedAsync(ctx);
            if (verification == ErrorCode.Qualify)
            {
                new Thread(async () =>
                {
                    List<Response> responses = ConfigValues.Responses;
                    bool fail = false;
                    if (args == "list")
                    {
                        List<Field> fields = new List<Field>();
                        foreach (Response response in responses)
                        {
                            fields.Add(new Field(response.trigger, response.description));
                        }
                        var Embed = Core.Discord.CreateFancyMessage(DiscordColor.CornflowerBlue, "Responses", "Control Catamagne's behaviours when responding to users:", fields);
                        var Message = await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                    }
                    else if (args == "add")
                    {
                        if (text != null)
                        {
                            var trigger = text;
                            var Embed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Adding Response", "Please send the text to use as the body of the response:");
                            await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                            var body = await ctx.Message.GetNextMessageAsync();
                            if (!body.TimedOut)
                            {
                                Embed = Core.Discord.CreateFancyMessage(DiscordColor.Yellow, "Adding Response", "Please give a description of the response:");
                                await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                                var description = await ctx.Message.GetNextMessageAsync();
                                if (!description.TimedOut)
                                {
                                    Embed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Please enter the channel ids where the responseis allowed, seperated by commas.\nsend 'all' for all channels");
                                    await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                                    var channelString = await ctx.Message.GetNextMessageAsync();
                                    if (!channelString.TimedOut)
                                    {
                                        var workingList = ConfigValues.Responses.ToList();
                                        if (channelString.Result.Content == "all" || string.IsNullOrWhiteSpace(channelString.Result.Content))
                                        {
                                            var response = new Response(trigger, body.Result.Content, description.Result.Content);
                                            workingList.Add(response);
                                        }
                                        else
                                        {
                                            string[] channelsStrings = string.Join("", channelString.Result.Content.Where(t => !char.IsWhiteSpace(t))).Split(',');
                                            List<DiscordChannel> channels = new List<DiscordChannel>();
                                            channelsStrings.ToList().ForEach(async channel =>
                                            {
                                                channels.Add(await Core.Discord.discord.GetChannelAsync(Convert.ToUInt64(channel)));
                                            });
                                            var response = new Response(trigger, body.Result.Content, description.Result.Content, channels);
                                            workingList.Add(response);
                                        }
                                        ConfigValues.Responses = workingList;
                                        ConfigValues.SaveConfig();
                                        Embed = Core.Discord.CreateFancyMessage(DiscordColor.CornflowerBlue, "Added", "Successfully added response to pool.");
                                        var message = await Core.Discord.SendFancyMessage(ctx.Channel, Embed);

                                    }
                                    else fail = true;
                                }
                                else fail = true;

                            }
                            else fail = true;
                            if (fail)
                            {
                                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You took too long to respond.");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please mention a trigger to add");
                            await ctx.RespondAsync(discordEmbed);
                        }

                    }
                    else if (args == "edit")
                    {
                        if (text != null)
                        {
                            var Embed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Finding response from pool");
                            var message = await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                            var match = ConfigValues.Responses.Select(t => t.trigger);
                            if (match.Contains(text))
                            {
                                var trigger = text;
                                Embed = Core.Discord.CreateFancyMessage(DiscordColor.Orange, "Editing Response", "Please send the text to use as the body of the response:");
                                await message.ModifyAsync(Embed);
                                var body = await ctx.Message.GetNextMessageAsync();
                                if (!body.TimedOut)
                                {
                                    Embed = Core.Discord.CreateFancyMessage(DiscordColor.Yellow, "Editing Response", "Please give a description of the response:");
                                    await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                                    var description = await ctx.Message.GetNextMessageAsync();
                                    if (!description.TimedOut)
                                    {
                                        Embed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Editing Response", "Please enter the channel ids where the responseis allowed, seperated by commas.\nsend 'all' for all channels");
                                        await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                                        var channelString = await ctx.Message.GetNextMessageAsync();
                                        if (!channelString.TimedOut)
                                        {
                                            Embed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Updating response.");
                                            message = await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                                            var _ = ConfigValues.Responses.ToList();
                                            _.Remove(ConfigValues.Responses.ToList().Find(t => t.trigger == text));
                                            if (channelString.Result.Content == "all" || string.IsNullOrWhiteSpace(channelString.Result.Content))
                                            {
                                                var response = new Response(trigger, body.Result.Content, description.Result.Content);
                                                _.Add(response);
                                            }
                                            else
                                            {
                                                string[] channelsStrings = string.Join("", channelString.Result.Content.Where(t => !char.IsWhiteSpace(t))).Split(',');
                                                List<DiscordChannel> channels = new List<DiscordChannel>();
                                                channelsStrings.ToList().ForEach(async channel =>
                                                {
                                                    channels.Add(await Core.Discord.discord.GetChannelAsync(Convert.ToUInt64(channel)));
                                                });
                                                var response = new Response(trigger, body.Result.Content, description.Result.Content, channels);
                                                _.Add(response);
                                            }

                                            ConfigValues.Responses = _;
                                            ConfigValues.SaveConfig();
                                            Embed = Core.Discord.CreateFancyMessage(DiscordColor.CornflowerBlue, "Updated", "Successfully updated response.");
                                            await message.ModifyAsync(Embed);

                                        }
                                        else fail = true;
                                    }
                                    else fail = true;

                                }
                                else fail = true;
                                if (fail)
                                {
                                    var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You took too long to respond.");
                                    await ctx.RespondAsync(discordEmbed);
                                }

                            }
                            else
                            {
                                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Trigger not found");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please mention a trigger to update");
                            await ctx.RespondAsync(discordEmbed);
                        }
                    }
                    else if (args == "remove")
                    {
                        if (text != null)
                        {
                            var Embed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Removing response from pool.");
                            var message = await Core.Discord.SendFancyMessage(ctx.Channel, Embed);
                            var match = ConfigValues.Responses.Select(t => t.trigger);
                            if (match.Contains(text))
                            {
                                var _ = ConfigValues.Responses.ToList();
                                _.Remove(ConfigValues.Responses.ToList().Find(t => t.trigger == text));
                                ConfigValues.Responses = _;
                                ConfigValues.SaveConfig();
                                Embed = Core.Discord.CreateFancyMessage(DiscordColor.SpringGreen, "Removed", "Successfully removed response from pool.");
                                message = await message.ModifyAsync(Embed);
                            }
                            else
                            {
                                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Trigger not found");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please mention a trigger to remove");
                            await ctx.RespondAsync(discordEmbed);
                        }
                    }
                    else if (args == null)
                    {
                        var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please provide arguments");
                        await ctx.RespondAsync(discordEmbed);
                    }
                    else
                    {
                        var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Invalid argument. either use:\nresponse add, response remove, response list or response edit");
                        await ctx.RespondAsync(discordEmbed);
                    }
                }).Start();
            }
        }

        [Command("exit")]
        [Description("Shut Rosalina down. this cannot be reverted without manually restarting her. only possible by Squid and Cata.")]
        [Aliases("stop")]
        public async Task Stop(CommandContext ctx)
        {
            var member = ctx.Member;
            bool isVerified = false;
            if (member.Id == ConfigValues.DevID || member.IsOwner) isVerified = true;

            if (isVerified)
            {
                var discordEmbed = Core.Discord.CreateFancyMessage(DiscordColor.Red, "Shutting down.", "Goodbye World! <:winsad:712225483445633024>");
                DiscordMessage msg = await ctx.RespondAsync(discordEmbed);
                Environment.Exit(-1);
            }
        }
    }
    public enum ErrorCode : ushort
    {
        Qualify,
        UnqualifyRole,
        UnqualifyChannel,
    }
}
