using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PrototonBot.MongoUtility;

namespace PrototonBot
{
    public class Utilities
    {
        private static Random random = new Random();

        // The user is a marked as a developer if any of these IDs match.
        public static bool IsUserDeveloper(string userId) => Program.DeveloperIDs.Contains(userId);

        // Take the input of a channel (formatted ID), raw ID, or "here" to validate the channel's existance in the server
        public static string? FilterChannelIdInput(SocketCommandContext context, string input)
        {
            var result = input;
            SocketGuildChannel channel;

            if (input != null && input != "here") result = input.Trim('<', '#', '@', '>', ' ');
            else result = context.Message.Channel.Id.ToString();
            try
            {
                channel = context.Guild.GetChannel(Convert.ToUInt64(result));
                return result;
            }
            catch (NullReferenceException)
            {
                context.Message.ReplyAsync("Hmm... That channel doesn't exist in this server.");
                return null;
            }
        }

        // For developer peek/poke commands
        public static string formatPeekData(string json)
        {
            return json.Replace("{", "\n  ").Replace(",", ",\n  ").Replace("}", "\n}");
        }

        // Handle giving the user money and xp for talking. Cooldown: 1 minute
        public static Task chatReward(IUser author)
        {
            var user = MongoHandler.GetUser(author.Id.ToString()).Result;
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // If the user received chat rewards within the last 60 seconds, cancel.
            if (user.LastMessage > (currentTime - 60)) return Task.CompletedTask;

            // Update the user's Money. Multiply depending on stats.
            long moneyToSet = random.Next(1, 8);
            if (user.Mutuals && user.Boosted) moneyToSet = (long)Math.Round(moneyToSet * 1.1);
            else if (user.Mutuals || user.Boosted) moneyToSet = (long)Math.Round(moneyToSet * 1.05);
            moneyToSet += user.Money;

            // Update the user's EXP. Multiply depending on stats.
            double expToSet = random.Next(10, 21);
            if (user.Mutuals && user.Boosted) expToSet = Math.Round(expToSet * 1.1);
            else if (user.Mutuals || user.Boosted) expToSet = Math.Round(expToSet * 1.05);
            expToSet += user.EXP;

            // Update the user's data in the database.
            MongoHandler.UpdateUser(author.Id.ToString(), "Money", moneyToSet);
            MongoHandler.UpdateUser(author.Id.ToString(), "EXP", expToSet);
            MongoHandler.UpdateUser(author.Id.ToString(), "LastMessage", currentTime);
            return Task.CompletedTask;
        }

        // Handle checking if the user is ready to level up and inform them (depending on if it's enabled)
        public static Task LevelUpdater(SocketUserMessage message)
        {
            var user = MongoHandler.GetUser(message.Author.Id.ToString()).Result;
            var server = MongoHandler.GetServer((message.Author as SocketGuildUser).Guild.Id.ToString()).Result;
            long currentLevel = (long)Math.Floor((170 + Math.Sqrt(28900 - (6 * 310 * -user.EXP))) / 620);

            if (currentLevel != user.Level)
            {
                // Reply with a level up message is the level has changed, messages are enabled, and the channel is bot-enabled.
                if (server.LevelUpMessages && server.EnabledChannels.Contains(message.Channel.Id.ToString()))
                    message.ReplyAsync($":tada: **Congratulations, {message.Author.Username}, you've reached Level {currentLevel}!** :tada:");

                // Regardless of if a reply was sent, now update the user's level.
                MongoHandler.UpdateUser(message.Author.Id.ToString(), "Level", currentLevel);
            }
            return Task.CompletedTask;
        }

        // Take the input of a ping (formatted ID), or a raw ID to validate their presence in the server.
        public static string? FilterUserIdInput(SocketCommandContext context, string input)
        {
            var result = (input != null ? input.Trim('<', '!', '@', '>', ' ') : context.Message.Author.Id.ToString()); ;
            IUser user;
            try
            {
                user = context.Client.GetUserAsync(Convert.ToUInt64(result)).Result;
                return (!user.IsBot ? result : throw new ArgumentException());
            }
            catch (NullReferenceException)
            {
                context.Message.ReplyAsync("Hmm.. Shucks, I'm sorry but the user you've specified doesn't appear to be in this server or valid.");
            }
            catch (FormatException)
            {
                context.Message.ReplyAsync("Sorry, but you need to tag someone for this command to work!");
            }
            catch (ArgumentException)
            {
                context.Message.ReplyAsync("Apologies, but bots cannot be used for commands.");
            }
            return null;
        }

        public static string taggedSomeone(SocketCommandContext context, string input)
        {
            if (input == null)
            {
                context.Message.ReplyAsync("Sorry, but you need to tag someone for this command to work!");
                return null;
            }
            else
            {
                return FilterUserIdInput(context, input);
            }
        }
    }
}
