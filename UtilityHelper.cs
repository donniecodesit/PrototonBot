using System;
using Discord.Commands;
using Discord.WebSocket;
using Nett;
using System.IO;
using System.Collections.Generic;

namespace PrototonBot {
  class UtilityHelper {
    //The user is a marked as a developer if any of these IDs match.
    public static bool IsUserDeveloper(string userId) {
      TomlTable config = Toml.ReadFile(Path.Combine("Storage", "config.toml"));
      var developers = config.Get<List<string>>("DeveloperIDs");
      return developers.Contains(userId);
    }

    //Take the input of a ping (formatted ID), or a raw ID to validate their presence in the server.
    public static string FilterUserIdInput(SocketCommandContext context, string input) {
      var result = input;
      SocketGuildUser user;
      result = (input != null ? input.Trim('<', '!', '@', '>', ' ') : context.Message.Author.Id.ToString());

      try {
        user = context.Guild.GetUser(Convert.ToUInt64(result));
        return (!user.IsBot ? result : throw new ArgumentException());
      }
      catch (NullReferenceException) {
        context.Channel.SendMessageAsync("Hmm.. Shucks, I'm sorry but the user you've specified doesn't appear to be in this server or valid.");
      }
      catch (FormatException) {
        context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
      }
      catch (ArgumentException) {
        context.Channel.SendMessageAsync("Apologies, but bots cannot be used for commands.");
      }
      return null;
    }

    //Take the input of a channel (formatted ID), raw ID, or "here" to validate the channel's presence in the server.
    public static string FilterChannelIdInput(SocketCommandContext context, string input) {
      var result = input;
      SocketGuildChannel channel;
      
      if (input != null && input != "here") result = input.Trim('<', '#', '@', '>', ' ');
      else result = context.Message.Channel.Id.ToString();
      try {
        channel = context.Guild.GetChannel(Convert.ToUInt64(result));
        return result;
      }
      catch (NullReferenceException) {
        context.Channel.SendMessageAsync("Hmm.. Shucks, I'm sorry but the channel you've specified doesn't appear to be in this server or valid.");
        return null;
      }
    }

    //For the developer peek/poke commands, format the result to be more readable.
    public static string FormatPeekData(string json) {
      return json
          .Replace("{", "{\n  ")
          .Replace(",", ",\n  ")
          .Replace("}", "\n}");
    }
  }
}