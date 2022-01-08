using System;
using System.Threading.Tasks;
using PrototonBot.MongoUtil;
using Discord;
using Discord.WebSocket;

namespace PrototonBot {
  public class UserUpdates {
    private static Random random = new Random();
    //Handles giving the user Money and XP for talking, with a 1 minute cooldown.
    public static Task MinuteRewards(IUser author) {
      var user = MongoHelper.GetUser(author.Id.ToString()).Result;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      //If the user received MinuteRewards within the last 60 seconds, skip this task.
      if (user.LastMessage > (currentTime - 60)) return Task.CompletedTask;
      
      //Update the user's money. Multiply if mutual partners and/or boosted user.
      double moneyToSet = random.Next(1, 8);
      if (user.Mutuals && user.Boosted) moneyToSet = Math.Round(moneyToSet * 1.1);
      else if (user.Mutuals || user.Boosted) moneyToSet = Math.Round(moneyToSet * 1.05);
      moneyToSet += user.Money;

      //Update the user's EXP. Multiply if mutual partners and/or boosted user.
      double expToSet = random.Next(10, 21);
      if (user.Mutuals && user.Boosted) expToSet = Math.Round(expToSet * 1.1);
      else if (user.Mutuals || user.Boosted) expToSet = Math.Round(expToSet * 1.05);
      expToSet += user.EXP;

      //Update the user in the database with the new information.
      MongoHelper.UpdateUser(author.Id.ToString(), "Money", moneyToSet);
      MongoHelper.UpdateUser(author.Id.ToString(), "EXP", expToSet);
      MongoHelper.UpdateUser(author.Id.ToString(), "LastMessage", currentTime);
      return Task.CompletedTask;
    }

    //Handles update the user's level based on a mathematical formula.
    public static Task LevelUpdater(SocketUserMessage message) {
      var user = MongoHelper.GetUser(message.Author.Id.ToString()).Result;
      var server = MongoHelper.GetServer((message.Author as SocketGuildUser).Guild.Id.ToString()).Result;
      long currentLevel = (long) Math.Floor((170 + Math.Sqrt(28900 - (6 * 310 * -user.EXP))) / 620);
      if (currentLevel != user.Level) {
        //Only reply if the server is not the TOP.GG server, the current channel is enabled, and level up messages are enabled.
        if (server.Id != "264445053596991498" && server.EnabledChannels.Contains(message.Channel.Id.ToString()) && server.LevelUpMessages) {
          message.Channel.SendMessageAsync($":tada: **Congratulations {message.Author.Username}, you've reached Level {currentLevel}!** :tada:");
        }
        //Regardless of if a reply was sent, update the user's level.
        MongoHelper.UpdateUser(message.Author.Id.ToString(), "Level", currentLevel);
      }
      return Task.CompletedTask;
    }
  }
}
