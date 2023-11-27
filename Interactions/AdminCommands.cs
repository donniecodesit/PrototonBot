using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using PrototonBot.MongoUtility;

namespace PrototonBot.Interactions
{
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.ManageGuild)]
    [Group("admin", "Admin Commands")]
    public class AdminCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("channel", "Configure settings for channels.")]
        public async Task AdminChannelConfig(
            [Summary(description: "The action you want to do.")]
            [Choice("Enable Commands", "enablecmds")]
            [Choice("Disable Commands", "disablecmds")]
            [Choice("Set For Welcomes", "setforwelcomes")]
            [Choice("Set For Leaves", "setforleaves")]
            [Choice("Set For Logs", "setforlogs")] string action,
            [Summary(description: "The channel you want to modify. For 'set's, to remove it, set the same channel again.")] SocketTextChannel channel)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;

            switch (action)
            {
                case "enablecmds":
                    if (mongoSvr.EnabledChannels.Contains(channel.Id.ToString()))
                    {
                        await RespondAsync($":information_source: <#{channel.Id}> is already listening to commands.");
                    }
                    else
                    {
                        var updatedChannels = mongoSvr.EnabledChannels;
                        updatedChannels.Add(channel.Id.ToString());
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "EnabledChannels", updatedChannels);
                        await RespondAsync($":white_check_mark: <#{channel.Id}> is now listening to commands!");
                    }
                    break;
                case "disablecmds":
                    if (!mongoSvr.EnabledChannels.Contains(channel.Id.ToString()))
                    {
                        await RespondAsync($":information_source: <#{channel.Id}> already wasn't listening to commands.");
                    }
                    else
                    {
                        var updatedChannels = mongoSvr.EnabledChannels;
                        updatedChannels.Remove(channel.Id.ToString());
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "EnabledChannels", updatedChannels);
                        await RespondAsync($":white_check_mark: <#{channel.Id}> is no longer listening to commands!");
                    }
                    break;
                case "setforwelcomes":
                    if (mongoSvr.WelcomeChannel == channel.Id.ToString())
                    {
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeChannel", "");
                        await RespondAsync($":white_check_mark: <#{channel.Id}> is no longer the welcome messages channel!");
                    }
                    else
                    {
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeChannel", channel.Id.ToString());
                        await RespondAsync($":white_check_mark: Welcome messages have been set to send in <#{channel.Id}>!");
                    }
                    break;
                case "setforleaves":
                    if (mongoSvr.LeaveChannel == channel.Id.ToString())
                    {
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LeaveChannel", "");
                        await RespondAsync($":white_check_mark: <#{channel.Id}> is no longer the leave messages channel!");
                    }
                    else
                    {
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LeaveChannel", channel.Id.ToString());
                        await RespondAsync($":white_check_mark: Leave messages have been set to send in <#{channel.Id}>!");
                    }
                    break;
                case "setforlogs":
                    if (mongoSvr.LogChannel == channel.Id.ToString())
                    {
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LogChannel", "");
                        await RespondAsync($":white_check_mark: <#{channel.Id}> is no longer logging events!");
                    }
                    else
                    {
                        await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LogChannel", channel.Id.ToString());
                        await RespondAsync($":white_check_mark: Logs have been set to <#{channel.Id}>!");
                    }
                    break;
                default:
                    break;
            }
        }

        [SlashCommand("toggle", "Configure server toggles for automatic behaviors.")]
        public async Task AdminToggleConfig(
            [Summary(description: "The setting you want to toggle.")]
            [Choice("Welcome Messages", "welcomemsgs")]
            [Choice("Leave Messages", "leavemsgs")]
            [Choice("Level Messages", "levelmsgs")]
            [Choice("Global Visibility", "public")] string action,
            [Summary(description: "True to enable, False to disable.")] bool enabled)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;

            switch (action)
            {
                case "welcomemsgs":
                    if (enabled)
                    {
                        if (mongoSvr.WelcomeMessages)
                        {
                            await RespondAsync($":information_source: Welcome messages are already enabled for {Context.Guild.Name}.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeMessages", true);
                            await RespondAsync($":white_check_mark: Welcome messages are now enabled for {Context.Guild.Name}!");
                        }
                    }
                    else
                    {
                        if (!mongoSvr.WelcomeMessages)
                        {
                            await RespondAsync($":information_source: Welcome messages are already disabled for {Context.Guild.Name}.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeMessages", false);
                            await RespondAsync($":white_check_mark: Welcome messages are now disabled for {Context.Guild.Name}!");
                        }
                    }
                    break;
                case "leavemsgs":
                    if (enabled)
                    {
                        if (mongoSvr.LeaveMessages)
                        {
                            await RespondAsync($":information_source: Leave messages are already enabled for {Context.Guild.Name}.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LeaveMessages", true);
                            await RespondAsync($":white_check_mark: Leave messages are now enabled for {Context.Guild.Name}!");
                        }
                    }
                    else
                    {
                        if (!mongoSvr.LeaveMessages)
                        {
                            await RespondAsync($":information_source: Leave messages are already disabled for {Context.Guild.Name}.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LeaveMessages", false);
                            await RespondAsync($":white_check_mark: Leave messages are now disabled for {Context.Guild.Name}!");
                        }
                    }
                    break;
                case "levelmsgs":
                    if (enabled)
                    {
                        if (mongoSvr.LevelUpMessages)
                        {
                            await RespondAsync($":information_source: Level Up messages are already enabled for {Context.Guild.Name}.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LevelUpMessages", true);
                            await RespondAsync($":white_check_mark: Level Up messages are now enabled for {Context.Guild.Name}!");
                        }
                    }
                    else
                    {
                        if (!mongoSvr.LevelUpMessages)
                        {
                            await RespondAsync($":information_source: Level Up messages are already disabled for {Context.Guild.Name}.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LevelUpMessages", false);
                            await RespondAsync($":white_check_mark: Level Up messages are now disabled for {Context.Guild.Name}!");
                        }
                    }
                    break;
                case "public":
                    if (enabled)
                    {
                        if (mongoSvr.Public)
                        {
                            await RespondAsync($":information_source: {Context.Guild.Name} is already a global server.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "Public", true);
                            await RespondAsync($":white_check_mark: {Context.Guild.Name} ia now a global server!");
                        }
                    }
                    else
                    {
                        if (!mongoSvr.Public)
                        {
                            await RespondAsync($":information_source: {Context.Guild.Name} is already not a global server.");
                        }
                        else
                        {
                            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "Public", false);
                            await RespondAsync($":white_check_mark: {Context.Guild.Name} is no longer a global server!");
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        [SlashCommand("purge", "[admin] Delete a specific number of messages")]
        public async Task Purge([Summary(description: "The number of messages you wish to delete. Count if unsure. Cannot deleted older than 14 days.")] int amount)
        {
            if (amount <= 0 || amount > 100)
            {
                await RespondAsync("Please set a number of messages to delete between 1 and 100.");
                return;
            }

            // var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();
            var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
            var messagesFiltered = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);
            var count = messagesFiltered.Count();
            if (count == 0)
            {
                await RespondAsync("Nothing was deleted, I can only delete messages within the last 14 days.");
                return;
            }
            else
            {
                await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messagesFiltered);
                await RespondAsync($"Succesfully purged {count} {(count >= 1 ? "messages" : "message")}");
            }

            await Task.Delay(2500);
            await DeleteOriginalResponseAsync();
        }
    }
}