using System;
using Discord.Commands;
using Discord.WebSocket;

namespace PrototonBot
{
  class UtilityHelper
  {
    public static bool IsUserDeveloper(string userId)
    {
      return userId == "285937628363227138" || userId == "103610948526284800";
    }
    public static string FilterUserIdInput(SocketCommandContext context, string input)
    {
      var tempVar = input;
      SocketGuildUser user;
      if (input != null)
      {
        tempVar = input.Trim('<', '!', '@', '>', ' ');
      }
      else
      {
        tempVar = context.Message.Author.Id.ToString();
      }

      try
      {
        user = context.Guild.GetUser(Convert.ToUInt64(tempVar));
        if (user.IsBot) throw new ArgumentException();
      }
      catch (NullReferenceException)
      {
        context.Channel.SendMessageAsync("Hmm.. Shucks, I'm sorry but the user you've specified doesn't appear to be in this server or valid.");
        return null;
      }
      catch (FormatException)
      {
        context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return null;
      }
      catch (ArgumentException)
      {
        context.Channel.SendMessageAsync("Apologies, but bots cannot be used for commands.");
        return null;
      }


      return tempVar;
    }

    public static string FilterChannelIdInput(SocketCommandContext context, string input)
    {
      var tempVar = input;
      SocketGuildChannel channel;
      if (input.ToLower() == "here")
      {
        tempVar = context.Message.Channel.Id.ToString();
      }
      else if (input != null)
      {
        tempVar = input.Trim('<', '#', '@', '>', ' ');
      }
      else
      {
        tempVar = context.Message.Channel.Id.ToString();
      }

      try
      {
        channel = context.Guild.GetChannel(Convert.ToUInt64(tempVar));
      }
      catch (NullReferenceException)
      {
        context.Channel.SendMessageAsync("Hmm.. Shucks, I'm sorry but the channel you've specified doesn't appear to be in this server or valid.");
        return null;
      }

      return tempVar;
    }

    public static string FormatPeekData(string json)
    {
      return json
          .Replace("{", "{\n  ")
          .Replace(",", ",\n  ")
          .Replace("}", "\n}");
    }
  }
}
