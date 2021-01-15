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
        //[Command("help")]
        //public async Task GiveHelp(CommandContext ctx)
        //{
        //    var _ = string.Join('\n', CatamagneCore.commandsList);
        //    var text = string.Format("Commands List: \n{0}", _);
        //    await ctx.RespondAsync(text);
        //}
        //[Command("readsheet")]
        //[Description("Read the sheet. admin exclusive command.")]
        //[Aliases("read")]
        //public async Task ReadSheet(CommandContext ctx)
        //{
        //    var roles = ctx.Member.Roles.ToList();
        //    var verification = await IsVerifiedAsync(ctx, true);
        //    if (verification == ErrorCode.Qualify)
        //    {
        //        new Thread(async () =>
        //        {
        //            Thread.CurrentThread.IsBackground = true;
        //            var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Orange, "Reading sheet data...", "If this takes over a minute, it's generating data due to missing files.");
        //            DiscordMessage message = await ctx.RespondAsync(discordEmbed);

        //            //call bulkupdate method
        //            await SpreadsheetTools.ReadSheet();
        //            discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Done", "Sucessfully read sheet data.");
        //            await message.ModifyAsync(discordEmbed);
        //        }).Start();
        //    }
        //}
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
                    var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Orange, "Updating Configuration Files");
                    DiscordMessage message = await ctx.RespondAsync(discordEmbed);


                    //call bulkupdate method
                    ConfigValues.configValues.TryLoadConfigFromFile();
                    await Core.Core.UpdateChannels();
                    discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Done", "Sucessfully updated configuration files");
                    await message.ModifyAsync(discordEmbed);
                }).Start();
            }
        }
        //[Command("updatesheet")]
        //[Description("Scan for changed data.")]
        //[Aliases("update")]
        //public async Task UpdateSpreadSheet(CommandContext ctx)
        //{
        //    var roles = ctx.Member.Roles.ToList();
        //    var verification = await IsVerifiedAsync(ctx, true);

        //    if (verification == ErrorCode.Qualify)
        //    {
        //        var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Turquoise, "Scanning for Changes...");
        //        DiscordMessage msg = await ctx.RespondAsync(discordEmbed);
        //        new Thread(async () =>
        //        {
        //            Thread.CurrentThread.IsBackground = true;
        //            //call bulkupdate method
        //            await SpreadsheetTools.ReadSheet();
        //            var _ = await SpreadsheetTools.CheckForChangesAsync();
        //            if (_.TotalChanges > 0)
        //            {
        //                TimeSpan t = TimeSpan.FromSeconds(_.TotalChanges * 5);
        //                discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Yellow, "Found changes", string.Format("{0} change(s) found...", _.TotalChanges), new List<Field>() { new Field("ETA", t.ToString(@"mm\:ss")) });
        //                await msg.ModifyAsync(discordEmbed);
        //                await SpreadsheetTools.SelectiveUpdateSheet(_);
        //                discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Done", string.Format("Successfully processed {0} changes.", _.TotalChanges));
        //                await msg.ModifyAsync(discordEmbed);
        //            }
        //            else
        //            {
        //                var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "No changes found", "To update steam names, please run a bulk update.");
        //                await msg.ModifyAsync(discordEmbed);
        //            }

        //        }).Start();
        //    }
        //}
        //[Command("bulkupdate")]
        //[Description("Update all the data in the sheets. This takes upwards of 10 minutes")]
        //[Aliases("bulk")]
        //public async Task BulkUpdateSheet(CommandContext ctx)
        //{
        //    var roles = ctx.Member.Roles.ToList();
        //    var verification = await IsVerifiedAsync(ctx, true);
        //    if (verification == ErrorCode.Qualify)
        //    {
        //        new Thread(async () =>
        //        {
        //            Thread.CurrentThread.IsBackground = true;
        //            //call bulkupdate method
        //            await SpreadsheetTools.ReadSheet();
        //            TimeSpan t = TimeSpan.FromSeconds(100 * 5);
        //            var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Yellow, "Bulk updating sheets", "This will update every single element in the spreadsheet", new List<Field>() { new Field("ETA", t.ToString(@"mm\:ss")) });
        //            DiscordMessage msg = await ctx.RespondAsync(discordEmbed);
        //            await SpreadsheetTools.BulkUpdateSheet();

        //            discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Done", "Successfully bulk updated data!");
        //            await msg.ModifyAsync(discordEmbed);
        //        }).Start();
        //    }
        //}
        //[Command("displayUsers")]
        //[Description("Output all stored users")]
        //[Aliases("users")]
        //public async Task DisplayUsers(CommandContext ctx, [RemainingText] string args)
        //{
        //    var roles = ctx.Member.Roles.ToList();
        //    var verification = await IsVerifiedAsync(ctx, true);
        //    if (verification == ErrorCode.Qualify)
        //    {
        //        new Thread(async () =>
        //        {
        //            if (args == "spreadsheet" || args == "sheet")
        //            {
        //                await Catamagne.API.SpreadsheetTools.ReadSheet();
        //                List<Field> fields = new List<Field>();
        //                List<SpreadsheetTools.User> users = SpreadsheetTools.users.ToList();
        //                users.OrderBy(t => t.steamName);
        //                foreach (SpreadsheetTools.User user in users)
        //                {
        //                    if (!string.IsNullOrEmpty(user.bungieProfile))
        //                    {
        //                        var _ = new Field(user.steamName, user.discordID);
        //                        fields.Add(_);
        //                    }

        //                }

        //                List<DiscordEmbed> embeds = new List<DiscordEmbed>();
        //                if (fields.Count < 25)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, fields.Count)));
        //                }
        //                else if (fields.Count < 50)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, 25)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(25, fields.Count)));
        //                }
        //                else if (fields.Count < 75)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, 25)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(25, 50)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(50, fields.Count)));
        //                }
        //                else
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, 25)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(25, 50)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(50, 75)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(75, fields.Count)));
        //                }
        //                List<DiscordMessage> messages = new List<DiscordMessage>();
        //                foreach (DiscordEmbed embed in embeds)
        //                {
        //                    messages.Add(await Core.Core.SendFancyMessage(ctx.Channel, embed));
        //                }
        //            }
        //            else if (args == "saved data" || args == "saved" || args == "file")
        //            {
        //                List<Field> fields = new List<Field>();
        //                List<SpreadsheetTools.User> users = SpreadsheetTools.users;
        //                users.OrderBy(t => t.steamName);
        //                foreach (SpreadsheetTools.User user in users)
        //                {
        //                    if (user.discordID != null)
        //                    {
        //                        var _ = new Field(user.steamName, user.discordID);
        //                        fields.Add(_);
        //                    }

        //                }

        //                List<DiscordEmbed> embeds = new List<DiscordEmbed>();
        //                if (fields.Count < 25)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, fields.Count)));
        //                }
        //                else if (fields.Count < 50)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, 25)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(25, fields.Count)));
        //                }
        //                else if (fields.Count < 75)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, 25)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(25, 50)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(50, fields.Count)));
        //                }
        //                else
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(0, 25)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(25, 50)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(50, 75)));
        //                    embeds.Add(GetUsersToDisplayInRange(fields, new Range(75, fields.Count)));

        //                }
        //                List<DiscordMessage> messages = new List<DiscordMessage>();
        //                embeds.ForEach(async embed => messages.Add(await Core.Core.SendFancyMessage(ctx.Channel, embed)));
        //            }
        //            else
        //            {
        //                var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Additional arguments required. eithe use:\ndisplayusers sheet or displayusers saved");
        //                await ctx.RespondAsync(discordEmbed);
        //            }
        //        }).Start();
        //    }
        //}
        //[Command("checkleavers")]
        //[Description("Check if any users have left the clan.")]
        //[Aliases("leavers", "checkleaves", "leaves")]
        //public async Task CheckForLeavers(CommandContext ctx)
        //{
        //    var roles = ctx.Member.Roles.ToList();
        //    var verification = await IsVerifiedAsync(ctx, true);

        //    if (verification == ErrorCode.Qualify)
        //    {
        //        var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.Turquoise, "Checking for leavers...");
        //        DiscordMessage msg = await ctx.RespondAsync(discordEmbed);
        //        new Thread(async () =>
        //        {
        //            Thread.CurrentThread.IsBackground = true;
        //            //call bulkupdate method
        //            var Leavers = await BungieTools.CheckForLeaves(ConfigValues.configValues.BungieGroupID, true);
        //            if (Leavers.Count > 0)
        //            {
        //                await msg.DeleteAsync();
        //                List<Field> fields = new List<Field>();
        //                foreach (SpreadsheetTools.User user in Leavers)
        //                {
        //                    if (user.discordID != null)
        //                    {
        //                        var _ = new Field(user.steamName, user.discordID);
        //                        fields.Add(_);
        //                    }

        //                }

        //                List<DiscordEmbed> embeds = new List<DiscordEmbed>();
        //                if (fields.Count < 25)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, fields.Count), "Users found leaving:"));
        //                }
        //                else if (fields.Count < 50)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, 25), "Users found leaving:"));
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, fields.Count)));
        //                }
        //                else if (fields.Count < 75)
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, 25), "Users found leaving:"));
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, fields.Count)));
        //                }
        //                else
        //                {
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(0, 25), "Users found leaving:"));
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(25, 50)));
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(50, 75)));
        //                    embeds.Add(GetUsersToDisplayInRange(DiscordColor.IndianRed, fields, new Range(75, fields.Count)));

        //                }
        //                List<DiscordMessage> messages = new List<DiscordMessage>();
        //                embeds.ForEach(async embed => messages.Add(await Core.Core.SendFancyMessage(ctx.Channel, embed)));
        //            }
        //            else
        //            {
        //                var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "No leavers found", "No one to kick <:unipeepo:601277029459034112>");
        //                await msg.ModifyAsync(discordEmbed);
        //            }

        //        }).Start();
        // }

        public static async Task<ErrorCode> IsVerifiedAsync(CommandContext ctx, bool isAdminCommand = false, bool isAvailableEverywhere = false)
        {
            DiscordEmbed discordEmbed;
            if (ctx.Member.IsOwner)
            {
                return ErrorCode.Qualify;
            }
            if (!Core.Core.commandChannels.Contains(ctx.Channel) && !isAvailableEverywhere)
            {
                discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You're not allowed to run that command here.");
                await ctx.RespondAsync(discordEmbed);
                return ErrorCode.UnqualifyChannel;
            }
            if (ctx.Member.Id == ConfigValues.configValues.CataID)
            {
                return ErrorCode.Qualify;
            }
            var roleVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.configValues.RoleIDs);
            var adminVerf = ctx.Member.Roles.Select(t => t.Id).Intersect(ConfigValues.configValues.AdminRoleIDs);
            if (adminVerf.Count() > 0)
            {
                return ErrorCode.Qualify;
            }
            if (ctx.Member.IsOwner)
            {
                return ErrorCode.Qualify;
            }
            if (roleVerf.Count() > 0 && !isAdminCommand)
            {
                return ErrorCode.Qualify;
            }
            discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You're not allowed to run that command.");
            await ctx.RespondAsync(discordEmbed);
            return ErrorCode.UnqualifyRole;
        }
        public static Clan GetClan(string clanTag)
        {
            if (ConfigValues.clansList.Any(t => t.clanTag == clanTag))
            {
                return (ConfigValues.clansList.Where(t => t.clanTag == clanTag).FirstOrDefault());
            }
            return null;
        }

    }


    public class UserInteractionsModule : BaseCommandModule
    {

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
                    Response[] responses = ConfigValues.configValues.Responses;
                    bool fail = false;
                    if (args == "list")
                    {
                        List<Field> fields = new List<Field>();
                        foreach (Response response in responses)
                        {
                            fields.Add(new Field(response.trigger, response.description));
                        }
                        var Embed = Core.Core.CreateFancyMessage(DiscordColor.CornflowerBlue, "Responses", "Control Catamagne's behaviours when responding to users:", fields);
                        var Message = await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                    }
                    else if (args == "add")
                    {
                        if (text != null)
                        {
                            var trigger = text;
                            var Embed = Core.Core.CreateFancyMessage(DiscordColor.Orange, "Adding Response", "Please send the text to use as the body of the response:");
                            await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                            var body = await ctx.Message.GetNextMessageAsync();
                            if (!body.TimedOut)
                            {
                                Embed = Core.Core.CreateFancyMessage(DiscordColor.Yellow, "Adding Response", "Please give a description of the response:");
                                await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                                var description = await ctx.Message.GetNextMessageAsync();
                                if (!description.TimedOut)
                                {
                                    Embed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Please enter the channel ids where the responseis allowed, seperated by commas.\nsend 'all' for all channels");
                                    await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                                    var channelString = await ctx.Message.GetNextMessageAsync();
                                    if (!channelString.TimedOut)
                                    {
                                        var _ = ConfigValues.configValues.Responses.ToList();
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
                                                channels.Add(await Core.Core.discord.GetChannelAsync(Convert.ToUInt64(channel)));
                                            });
                                            var response = new Response(trigger, body.Result.Content, description.Result.Content, channels);
                                            _.Add(response);
                                        }
                                        ConfigValues.configValues.Responses = _.ToArray();
                                        ConfigValues.configValues.SaveConfigToFile();
                                        Embed = Core.Core.CreateFancyMessage(DiscordColor.CornflowerBlue, "Added", "Successfully added response to pool.");
                                        var message = await Core.Core.SendFancyMessage(ctx.Channel, Embed);

                                    }
                                    else fail = true;
                                }
                                else fail = true;

                            }
                            else fail = true;
                            if (fail)
                            {
                                var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You took too long to respond.");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please mention a trigger to add");
                            await ctx.RespondAsync(discordEmbed);
                        }

                    }
                    else if (args == "edit")
                    {
                        if (text != null)
                        {
                            var Embed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Finding response from pool");
                            var message = await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                            var match = ConfigValues.configValues.Responses.Select(t => t.trigger);
                            if (match.Contains(text))
                            {
                                var trigger = text;
                                Embed = Core.Core.CreateFancyMessage(DiscordColor.Orange, "Editing Response", "Please send the text to use as the body of the response:");
                                await message.ModifyAsync(Embed);
                                var body = await ctx.Message.GetNextMessageAsync();
                                if (!body.TimedOut)
                                {
                                    Embed = Core.Core.CreateFancyMessage(DiscordColor.Yellow, "Editing Response", "Please give a description of the response:");
                                    await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                                    var description = await ctx.Message.GetNextMessageAsync();
                                    if (!description.TimedOut)
                                    {
                                        Embed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Editing Response", "Please enter the channel ids where the responseis allowed, seperated by commas.\nsend 'all' for all channels");
                                        await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                                        var channelString = await ctx.Message.GetNextMessageAsync();
                                        if (!channelString.TimedOut)
                                        {
                                            Embed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Updating response.");
                                            message = await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                                            var _ = ConfigValues.configValues.Responses.ToList();
                                            _.Remove(ConfigValues.configValues.Responses.ToList().Find(t => t.trigger == text));
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
                                                    channels.Add(await Core.Core.discord.GetChannelAsync(Convert.ToUInt64(channel)));
                                                });
                                                var response = new Response(trigger, body.Result.Content, description.Result.Content, channels);
                                                _.Add(response);
                                            }

                                            ConfigValues.configValues.Responses = _.ToArray();
                                            ConfigValues.configValues.SaveConfigToFile();
                                            Embed = Core.Core.CreateFancyMessage(DiscordColor.CornflowerBlue, "Updated", "Successfully updated response.");
                                            await message.ModifyAsync(Embed);

                                        }
                                        else fail = true;
                                    }
                                    else fail = true;

                                }
                                else fail = true;
                                if (fail)
                                {
                                    var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "You took too long to respond.");
                                    await ctx.RespondAsync(discordEmbed);
                                }

                            }
                            else
                            {
                                var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Trigger not found");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please mention a trigger to update");
                            await ctx.RespondAsync(discordEmbed);
                        }
                    }
                    else if (args == "remove")
                    {
                        if (text != null)
                        {
                            var Embed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Working", "Removing response from pool.");
                            var message = await Core.Core.SendFancyMessage(ctx.Channel, Embed);
                            var match = ConfigValues.configValues.Responses.Select(t => t.trigger);
                            if (match.Contains(text))
                            {
                                var _ = ConfigValues.configValues.Responses.ToList();
                                _.Remove(ConfigValues.configValues.Responses.ToList().Find(t => t.trigger == text));
                                ConfigValues.configValues.Responses = _.ToArray();
                                ConfigValues.configValues.SaveConfigToFile();
                                Embed = Core.Core.CreateFancyMessage(DiscordColor.SpringGreen, "Removed", "Successfully removed response from pool.");
                                message = await message.ModifyAsync(Embed);
                            }
                            else
                            {
                                var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Trigger not found");
                                await ctx.RespondAsync(discordEmbed);
                            }
                        }
                        else
                        {
                            var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please mention a trigger to remove");
                            await ctx.RespondAsync(discordEmbed);
                        }
                    }
                    else if (args == null)
                    {
                        var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Please provide arguments");
                        await ctx.RespondAsync(discordEmbed);
                    }
                    else
                    {
                        var discordEmbed = Core.Core.CreateFancyMessage(DiscordColor.IndianRed, "Sorry!", "Invalid argument. either use:\nresponse add, response remove, response list or response edit");
                        await ctx.RespondAsync(discordEmbed);
                    }
                }).Start();
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
