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
        [SlashCommand("channelenable", "[admin] Enable a channel to listen to bot commands")]
        public async Task EnableChannel([Summary(description: "The channel to be added. Channels start with #.")] SocketTextChannel channel)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            if (mongoSvr.EnabledChannels.Contains(channel.Id.ToString()))
            {
                await RespondAsync($":information_source: <#{channel.Id}> is already listening to commands.");
                return;
            }

            if (mongoSvr.LogChannel == channel.Id.ToString())
            {
                await RespondAsync($"⚠ <#{channel.Id}> is currently a logging channel, commands probably shouldn't be there.");
                return;
            }

            if (mongoSvr.WelcomeChannel == channel.Id.ToString())
            {
                await RespondAsync($"⚠ <#{channel.Id}> is currently the welcome channel, commands probably shouldn't be there.");
                return;
            }

            var updatedChannels = mongoSvr.EnabledChannels;
            updatedChannels.Add(channel.Id.ToString());
            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "EnabledChannels", updatedChannels);
            await RespondAsync($":white_check_mark: <#{channel.Id}> is now listening to commands!");
        }

        [SlashCommand("channeldisable", "[admin] Stop a channel from listening to bot commands")]
        public async Task DisableChannel([Summary(description: "The channel to be removed. Channels start with #.")] SocketTextChannel channel)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            if (!mongoSvr.EnabledChannels.Contains(channel.Id.ToString()))
            {
                await RespondAsync($":information_source: <#{channel.Id}> was not enabled, so all good.");
                return;
            }

            var updatedChannels = mongoSvr.EnabledChannels;
            updatedChannels.Remove(channel.Id.ToString());
            await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "EnabledChannels", updatedChannels);
            await RespondAsync($":white_check_mark: <#{channel.Id}> is no longer listening to commands!");
        }

        [SlashCommand("loggingchannel", "[admin] Set or remove a channel to log bot actions (Feature TBA)")]
        public async Task SetLogChannel([Summary(description: "The channel to set for logging. To remove the current one, set the same channel again.")] SocketTextChannel channel)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;

            if (mongoSvr.WelcomeChannel == channel.Id.ToString())
            {
                await RespondAsync($"⚠ <#{channel.Id}> is currently this server's welcome channel, logs probably shouldn't be there.");
                return;
            }

            if (mongoSvr.EnabledChannels.Contains(channel.Id.ToString()))
            {
                await RespondAsync($"⚠ <#{channel.Id}> is currently a command enabled channel, logs probably shouldn't be there.");
                return;
            }

            if (mongoSvr.LogChannel == channel.Id.ToString())
            {
                await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LogChannel", "");
                await RespondAsync($":white_check_mark: <#{channel.Id}> is no longer logging events!");
                return;
            }
            else
            {
                await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LogChannel", channel.Id.ToString());
                await RespondAsync($":white_check_mark: Logs have been set to <#{channel.Id}>!");
                return;
            }
        }

        [SlashCommand("welcomechannel", "[admin] Set or remove a channel to log welcome messages.")]
        public async Task SetWelcomeChannel([Summary(description: "The channel to set for welcomes. To remove the current one, set the same channel again.")] SocketTextChannel channel)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;

            if (mongoSvr.LogChannel == channel.Id.ToString())
            {
                await RespondAsync($"⚠ <#{channel.Id}> is currently a logging channel, welcomes probably shouldn't be there.");
                return;
            }

            if (mongoSvr.EnabledChannels.Contains(channel.Id.ToString()))
            {
                await RespondAsync($"⚠ <#{channel.Id}> is currently a command enabled channel, welcomes probably shouldn't be there.");
                return;
            }

            if (mongoSvr.WelcomeChannel == channel.Id.ToString())
            {
                await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeChannel", "");
                await RespondAsync($":white_check_mark: <#{channel.Id}> is no longer the welcome messages channel!");
                return;
            }
            else
            {
                await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeChannel", channel.Id.ToString());
                await RespondAsync($":white_check_mark: Welcome messages have been set to send in <#{channel.Id}>!");
                return;
            }
        }

        [SlashCommand("welcomemessages", "[admin] Enable or disable welcome messages.")]
        public async Task SetGuildWelcomes([Summary(description: "True for on, false for off.")] bool enabled)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            if (enabled)
            {
                if (mongoSvr.WelcomeMessages)
                {
                    await RespondAsync($":information_source: Welcome messages are already enabled for {Context.Guild.Name}.");
                    return;
                }
                else
                {
                    await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeMessages", true);
                    await RespondAsync($":white_check_mark: Welcome messages are now enabled for {Context.Guild.Name}!");
                    return;
                }
            }
            else
            {
                if (!mongoSvr.WelcomeMessages)
                {
                    await RespondAsync($":information_source: Welcome messages are already disabled for {Context.Guild.Name}.");
                    return;
                }
                else
                {
                    await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "WelcomeMessages", false);
                    await RespondAsync($":white_check_mark: Welcome messages are now disabled for {Context.Guild.Name}!");
                    return;
                }
            }
        }

        [SlashCommand("levelmessages", "[admin] Enable or disable level up messages.")]
        public async Task SetGuildLevelups([Summary(description: "True for on, false for off.")] bool enabled)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            if (enabled)
            {
                if (mongoSvr.LevelUpMessages)
                {
                    await RespondAsync($":information_source: Level Up messages are already enabled for {Context.Guild.Name}.");
                    return;
                }
                else
                {
                    await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LevelUpMessages", true);
                    await RespondAsync($":white_check_mark: Level Up messages are now enabled for {Context.Guild.Name}!");
                    return;
                }
            }
            else
            {
                if (!mongoSvr.LevelUpMessages)
                {
                    await RespondAsync($":information_source: Level Up messages are already disabled for {Context.Guild.Name}.");
                    return;
                }
                else
                {
                    await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "LevelUpMessages", false);
                    await RespondAsync($":white_check_mark: Level Up messages are now disabled for {Context.Guild.Name}!");
                    return;
                }
            }


        }

        [SlashCommand("serverispublic", "[admin] Set the server to public or private. Defaults to private. (Features TBA)")]
        public async Task SetGuildPrivacy([Summary(description: "True for public, false for private.")] bool enabled)
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            if (enabled)
            {
                if (mongoSvr.Public)
                {
                    await RespondAsync($":information_source: {Context.Guild.Name} is already a public server.");
                    return;
                }
                else
                {
                    await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "Public", true);
                    await RespondAsync($":white_check_mark: {Context.Guild.Name} ia now a public server!");
                    return;
                }
            }
            else
            {
                if (!mongoSvr.Public)
                {
                    await RespondAsync($":information_source: {Context.Guild.Name} is already a private server.");
                    return;
                }
                else
                {
                    await MongoHandler.UpdateServer(Context.Guild.Id.ToString(), "Public", false);
                    await RespondAsync($":white_check_mark: {Context.Guild.Name} is now a private server!");
                    return;
                }
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