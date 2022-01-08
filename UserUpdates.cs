using System;
using System.Threading.Tasks;
using PrototonBot.MongoUtil;
using Discord;
using Discord.WebSocket;

namespace PrototonBot
{
  public class UserUpdates
  {
    private static Random random = new Random();

    public static Task MinuteRewards(IUser author)
    {
      var user = MongoHelper.GetUser(author.Id.ToString()).Result;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

      if (user.LastMessage > (currentTime - 60))
      {
        //User has received XP and Money within the last minute, do not repeat.
        return Task.CompletedTask;
      }
      //Update the User's Money
      var currentMoney = user.Money;
      double moneyToSet = random.Next(1, 8);
      if (user.Mutuals && user.Boosted) moneyToSet = Math.Round(moneyToSet * 1.1);
      else if (user.Mutuals || user.Boosted) moneyToSet = Math.Round(moneyToSet * 1.05);
      moneyToSet += currentMoney;

      var currentExp = user.EXP;
      double expToSet = random.Next(10, 21);
      if (user.Mutuals && user.Boosted) expToSet = Math.Round(expToSet * 1.1);
      else if (user.Mutuals || user.Boosted) expToSet = Math.Round(expToSet * 1.05);
      expToSet += currentExp;

      MongoHelper.UpdateUser(author.Id.ToString(), "Money", moneyToSet);
      MongoHelper.UpdateUser(author.Id.ToString(), "EXP", expToSet);
      MongoHelper.UpdateUser(author.Id.ToString(), "LastMessage", currentTime);
      return Task.CompletedTask;
    }

    public static Task LevelUpdater(SocketUserMessage message)
    {
      var user = MongoHelper.GetUser(message.Author.Id.ToString()).Result;
      var guild = MongoHelper.GetServer((message.Author as SocketGuildUser).Guild.Id.ToString()).Result;
      long currentLevel = (long) Math.Floor((170 + Math.Sqrt(28900 - (6 * 310 * -user.EXP))) / 620);
      if (currentLevel != user.Level)
      {
        if (guild.Id != "264445053596991498" && guild.EnabledChannels.Contains(message.Channel.Id.ToString()) && guild.LevelUpMessages)
        {
          message.Channel.SendMessageAsync($":tada: **Congratulations {message.Author.Username}, you've reached Level {currentLevel}!** :tada:");
        }
        MongoHelper.UpdateUser(message.Author.Id.ToString(), "Level", currentLevel);
      }
      return Task.CompletedTask;
    }
  }
}
