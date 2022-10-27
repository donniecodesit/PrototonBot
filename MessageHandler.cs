using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using PrototonBot.MongoUtility;

namespace PrototonBot
{
    public class MessageHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;

        public MessageHandler(DiscordSocketClient client, CommandService commands, IConfigurationRoot config)
        {
            _client = client;
            _commands = commands;
            _config = config;
        }

        public Task InitializeAsync()
        {
            _client.MessageReceived += HandleMessageAsync;
            return Task.CompletedTask;
        }

        public void AddModule<T>()
        {
            _commands.AddModuleAsync<T>(null);
        }

        private async Task HandleMessageAsync(SocketMessage socketMessage)
        {
            // Stop if system message, a bot, null/empty, or web integration user. Reply if DM.
            var message = socketMessage as SocketUserMessage;
            if (message == null || message.Source != MessageSource.User || message.Author.IsBot) return;
            if (message.Channel.GetType() == typeof(SocketDMChannel)) { await message.ReplyAsync("Only slash commands are supported in DMs."); return; }

            // The message was from a user, now run any and all checks.
            if (MongoHandler.GetUser(message.Author.Id.ToString()).Result == null) await MongoHandler.CreateNewUser(message.Author);
            await Utilities.chatReward(message.Author);
            await Utilities.LevelUpdater(message);

            // Get the user and server, check if either have had their primary name changed to update in the DB.
            var user = MongoHandler.GetUser(message.Author.Id.ToString()).Result;
            var server = MongoHandler.GetServer(((SocketGuildChannel)message.Channel).Guild.Id.ToString()).Result;
            if (user.Name != message.Author.Username) await MongoHandler.UpdateUser(user.Id, "Name", message.Author.Username);
            if (server.Name != ((SocketGuildChannel)message.Channel).Guild.Name) await MongoHandler.UpdateServer(server.Id, "Name", ((SocketGuildChannel)message.Channel).Guild.Name);

            return;
        }
    }
}