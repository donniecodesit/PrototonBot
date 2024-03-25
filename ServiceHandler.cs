using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PrototonBot.MongoUtility;

namespace PrototonBot
{
    public class ServiceHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        public ServiceHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            _client = client;
            _commands = commands;
            _config = config;
        }

        public Task InitializeAsync()
        {
            _client.MessageReceived += HandleMessageAsync;
            _client.UserJoined += OnUserJoin;
            _client.UserLeft += OnUserLeave;
            _client.JoinedGuild += OnNewGuild;
            _client.LeftGuild += OnLeaveGuild;
            _client.ChannelDestroyed += OnChannelDeleted;
            _client.MessageDeleted += OnMessageDeleted;
            _client.MessageUpdated += OnMessageUpdated;
            return Task.CompletedTask;
        }

        public void AddModule<T>()
        {
            _commands.AddModuleAsync<T>(null);
        }

        private async Task HandleMessageAsync(SocketMessage socketMessage)
        {
            // Ignore if the message is not from a user, and stop if in DMs.
            var message = socketMessage as SocketUserMessage;
            if (message == null || message.Source != MessageSource.User || message.Author.IsBot) return;
            if (message.Channel.GetType() == typeof(SocketDMChannel)) { await message.ReplyAsync("Only slash commands are supported in DMs."); return; }

            // create the user if they don't exist, then update their money, xp, and level.
            if (MongoHandler.GetUser(message.Author.Id.ToString()).Result == null) await MongoHandler.CreateNewUser(message.Author);
            await Utilities.chatReward(message.Author);
            await Utilities.LevelUpdater(message);

            // Fetch the user and server information and update the name value if it has changed.
            var user = MongoHandler.GetUser(message.Author.Id.ToString()).Result;
            var server = MongoHandler.GetServer(((SocketGuildChannel)message.Channel).Guild.Id.ToString()).Result;

            if (server == null)
            {
                await MongoHandler.CreateNewServer(((SocketGuildChannel)message.Channel).Guild);
                server = MongoHandler.GetServer(((SocketGuildChannel)message.Channel).Guild.Id.ToString()).Result;
            }

            if (user.Name != message.Author.Username) await MongoHandler.UpdateUser(user.Id, "Name", message.Author.Username);
            if (server.Name != ((SocketGuildChannel)message.Channel).Guild.Name) await MongoHandler.UpdateServer(server.Id, "Name", ((SocketGuildChannel)message.Channel).Guild.Name);

            return;
        }

        private async Task OnUserJoin(SocketGuildUser user)
        {
            var mongoSvr = MongoHandler.GetServer(user.Guild.Id.ToString()).Result;

            if (mongoSvr.WelcomeMessages && mongoSvr.WelcomeChannel != "")
            {
                var welcomeChannel = user.Guild.GetTextChannel(Convert.ToUInt64(mongoSvr.WelcomeChannel));
                await welcomeChannel.SendMessageAsync($":sparkling_heart: Welcome <@{user.Id}>, to **{mongoSvr.Name}**! Have a wonderful time here! :sparkling_heart:");
            }

            return;
        }

        private async Task OnUserLeave(SocketGuild guild, SocketUser user)
        {
            var mongoSvr = MongoHandler.GetServer(guild.Id.ToString()).Result;

            if (mongoSvr.LeaveMessages && mongoSvr.LeaveChannel != "")
            {
                var leaveChannel = guild.GetTextChannel(Convert.ToUInt64(mongoSvr.LeaveChannel));
                await leaveChannel.SendMessageAsync($"{user.Username} has departed from this server.. We wish them a friendly farewell! :broken_heart:");
            }

            return;
        }

        private async Task OnNewGuild(SocketGuild guild)
        {
            await MongoHandler.CreateNewServer(guild);
            await guild.SystemChannel.SendMessageAsync($"Thank you for inviting me to this server! :sparkling_heart:\nStart by having an Admin with the ManageChannel permission use `/admin channelenable` to allow commands in a channel!");
            Console.WriteLine($"Joined {guild.Name}");
        }

        private async Task OnLeaveGuild(SocketGuild guild)
        {
            await MongoHandler.DeleteServer(guild);
            Console.Write($"Left {guild.Name}");
        }

        private async Task OnChannelDeleted(SocketChannel channel)
        {
            var server = MongoHandler.GetServer(((SocketGuildChannel)channel).Guild.Id.ToString()).Result;

            if (server.WelcomeChannel == channel.Id.ToString())
            {
                await MongoHandler.UpdateServer(server.Id.ToString(), "WelcomeChannel", "");
            }
            if (server.LogChannel == channel.Id.ToString())
            {
                await MongoHandler.UpdateServer(server.Id.ToString(), "LogChannel", "");
            }
            if (server.EnabledChannels.Contains(channel.Id.ToString()))
            {
                var currentlyEnabled = server.EnabledChannels;
                currentlyEnabled.Remove(channel.Id.ToString());
                await MongoHandler.UpdateServer(server.Id.ToString(), "EnabledChannels", currentlyEnabled);
            }

            return;
        }

        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> cachedMsg, Cacheable<IMessageChannel, ulong> cachedChannel)
        {
            IMessage message = await cachedMsg.GetOrDownloadAsync();
            if (message == null) return;
            if (message.Content == null || message.Content == "" || message.Author.IsBot) return;

            SocketGuildChannel channel = (SocketGuildChannel)_client.GetChannel(cachedChannel.Id);
            SocketGuild guild = channel.Guild;
            SocketUser user = (SocketUser) message.Author;
            var mongoSvr = MongoHandler.GetServer(guild.Id.ToString()).Result;

            if (mongoSvr.LogDeletedMessages && mongoSvr.LogChannel != "")
            {
                var messageTrimmed = message.Content.Length <= 700 ? message.Content : (message.Content.Substring(0, 700) + "...");

                var deletedLogsChannel = guild.GetTextChannel(Convert.ToUInt64(mongoSvr.LogChannel));
                var _embed = new EmbedBuilder();
                _embed.WithColor(0xFFAB59);
                _embed.WithDescription($"**Message from <@{user.Id}> was deleted in <#{channel.Id}>:**\n{messageTrimmed}");
                _embed.WithAuthor("Deleted Message", user.GetAvatarUrl());
                await deletedLogsChannel.SendMessageAsync("", embed: _embed.Build());
            }

            return;
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> cachedMsg, SocketMessage newMessage, ISocketMessageChannel channelSource)
        {
            IMessage originalMsg = await cachedMsg.GetOrDownloadAsync();
            if (originalMsg.Author.IsBot || originalMsg.Content == newMessage.Content) return;


            SocketGuildChannel channel = (SocketGuildChannel)_client.GetChannel(channelSource.Id);
            SocketGuild guild = channel.Guild;
            var mongoSvr = MongoHandler.GetServer(guild.Id.ToString()).Result;

            if (mongoSvr.LogUpdatedMessages && mongoSvr.LogChannel != "")
            {
                var oldMsgTrimmed = "*(no message)*";
                var newMsgTrimmed = "*(no message)*";

                if (originalMsg.Content != null && originalMsg.Content.Length != 0)
                    oldMsgTrimmed = originalMsg.Content.Length <= 700 ? originalMsg.Content : (originalMsg.Content.Substring(0, 700) + "...");

                if (newMessage.Content != null && newMessage.Content.Length != 0)
                    newMsgTrimmed = newMessage.Content.Length <= 700 ? newMessage.Content : (newMessage.Content.Substring(0, 700) + "...");

                var deletedLogsChannel = guild.GetTextChannel(Convert.ToUInt64(mongoSvr.LogChannel));
                var _embed = new EmbedBuilder();
                _embed.WithColor(0xFFAB59);
                _embed.WithDescription($"**<@{newMessage.Author.Id}> edited a message in <#{channel.Id}>:**\n{oldMsgTrimmed}\n🔻\n{newMsgTrimmed}");
                _embed.WithAuthor("Edited Message", newMessage.Author.GetAvatarUrl());
                await deletedLogsChannel.SendMessageAsync("", embed: _embed.Build());
            }

            return;
        }
    }
}