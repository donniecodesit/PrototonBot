using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PrototonBot.MongoUtil;
using System.Linq;
using System.Reflection;
using MongoDB.Driver.Core.Authentication;

namespace PrototonBot.Commands
{
  [RequireContext(ContextType.Guild)]
  [RequireUserPermission(GuildPermission.Administrator)]
  public class AdminCommands : ModuleBase<SocketCommandContext>
  {
    [Command("enablechannel")]
    [Alias("enable", "addchannel")]
    public async Task EnableChannelAsync(string channelCalled = null)
    {
      if (channelCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to specify a channel for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterChannelIdInput(Context, channelCalled);
      if (filteredId == null) return;

      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var actualChannel = Context.Guild.GetChannel(Convert.ToUInt64(filteredId));
      if (actualChannel == null)
      {
        await ReplyAsync($"{filteredId} does not exist in this server.");
        return;
      }

      if (serverObj.EnabledChannels.Contains(filteredId))
      {
        await ReplyAsync($"<#{filteredId}> is already enabled!");
        return;
      }

      var currentlyEnabled = serverObj.EnabledChannels;
      currentlyEnabled.Add(filteredId);

      await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "EnabledChannels", currentlyEnabled);
      await ReplyAsync($"<#{filteredId}> is now listening to commands!");
    }

    [Command("disablechannel")]
    [Alias("disable", "removechannel")]
    public async Task DisableChannelAsync(string channelCalled = null)
    {
      if (channelCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to specify a channel for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterChannelIdInput(Context, channelCalled);
      if (filteredId == null) return;

      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var actualChannel = Context.Guild.GetChannel(Convert.ToUInt64(filteredId));
      if (actualChannel == null)
      {
        await ReplyAsync($"{filteredId} does not exist in this server.");
        return;
      }

      if (!serverObj.EnabledChannels.Contains(filteredId))
      {
        await ReplyAsync($"<#{filteredId}> is already disabled!");
        return;
      }

      var currentlyEnabled = serverObj.EnabledChannels;
      currentlyEnabled.Remove(filteredId);

      await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "EnabledChannels", currentlyEnabled);
      await ReplyAsync($"<#{filteredId}> is no longer listening to commands!");
    }

    [Command("logchannel")]
    [Alias("log", "logging")]
    public async Task SetLogChannel(string channelCalled = null)
    {
      if (channelCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to specify a channel for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterChannelIdInput(Context, channelCalled);
      if (filteredId == null) return;

      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var actualChannel = Context.Guild.GetChannel(Convert.ToUInt64(filteredId));
      if (actualChannel == null)
      {
        await ReplyAsync($"{filteredId} does not exist in this server.");
        return;
      }

      if (serverObj.LogChannel == filteredId)
      {
        await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "LogChannel", "");
        await ReplyAsync($"<#{filteredId}> is no longer the Logging channel!");
        return;
      }

      await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "LogChannel", filteredId);
      await ReplyAsync($"Logging has been set to: <#{filteredId}>!");
    }

    [Command("serverprivacy")]
    [Alias("privacy")]
    public async Task SetGuildPrivacy([Remainder] string setPrivacy)
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      if (setPrivacy == "public")
      {
        if (serverObj.Public) await ReplyAsync($"{Context.Guild.Name} is already public!");
        else
        {
          await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "Public", true);
          await ReplyAsync($"{Context.Guild.Name} has been set to public!");
          return;
        }
      }
      else if (setPrivacy == "private")
      {
        if (!serverObj.Public) await ReplyAsync($"{Context.Guild.Name} is already private!");
        else
        {
          await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "Public", false);
          await ReplyAsync($"{Context.Guild.Name} has been set to private!");
          return;
        }
      }
      else
      {
        await ReplyAsync("Please specify either `public` or `private`.");
        return;
      }
    }

    [Command("levelups")]
    [Alias("levelmessages", "levelupmessages")]
    public async Task SetGuildLevelUps([Remainder] string enableLevels)
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      if (enableLevels == "enable" || enableLevels == "on")
      {
        if (serverObj.LevelUpMessages) await ReplyAsync($"Level Up Messages are already enabled for {Context.Guild.Name}!");
        else
        {
          await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "LevelUpMessages", true);
          await ReplyAsync($"Level Up Messages are now enabled for {Context.Guild.Name}!");
          return;
        }
      }
      else if (enableLevels == "disable" || enableLevels == "off")
      {
        if (!serverObj.LevelUpMessages) await ReplyAsync($"Level Up Messages are already disabled for {Context.Guild.Name}!");
        else
        {
          await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "LevelUpMessages", false);
          await ReplyAsync($"Level Up Messages are now disabled for {Context.Guild.Name}!");
          return;
        }
      }
      else
      {
        await ReplyAsync("Please specify either `enable`/`disable` or `on`/`off`.");
        return;
      }
    }

    [Command("changeprefix")]
    [Alias("setprefix", "prefix")]
    public async Task ChangeGuildPrefix([Remainder] string newPrefix)
    {
      var tempPre = newPrefix;
      if (!tempPre.Contains('!') && !tempPre.Contains('?') && !tempPre.Contains('+') || tempPre.Count() > 7 || tempPre.Contains(' '))
      {
        await ReplyAsync("Your new prefix must be 7 characters or less, contain `!`, `?`, or `+`, and may not contain spaces.");
        return;
      }
      await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "Prefix", tempPre);
      await ReplyAsync($"All set! The new prefix for {Context.Guild.Name} is now ``{tempPre}``!");
      return;
    }

    [Command("togglewelcomes")]
    public async Task SetGuildWelcomes([Remainder] string enableWelcomes)
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      if (enableWelcomes == "enable" || enableWelcomes == "on")
      {
        if (serverObj.WelcomeMessages) await ReplyAsync($"Welcome Messages are already enabled for {Context.Guild.Name}!");
        else
        {
          await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "WelcomeMessages", true);
          await ReplyAsync($"Welcome Messages are now enabled for {Context.Guild.Name}!");
          return;
        }
      }
      else if (enableWelcomes == "disable" || enableWelcomes == "off")
      {
        if (!serverObj.WelcomeMessages) await ReplyAsync($"Welcome Messages are already disabled for {Context.Guild.Name}!");
        else
        {
          await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "WelcomeMessages", false);
          await ReplyAsync($"Welcome Messages are now disabled for {Context.Guild.Name}!");
          return;
        }
      }
      else
      {
        await ReplyAsync("Please specify either `enable`/`disable` or `on`/`off`.");
        return;
      }
    }

    [Command("welcomechannel")]
    public async Task SetWelcomeChannel(string channelCalled = null)
    {
      if (channelCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to specify a channel for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterChannelIdInput(Context, channelCalled);
      if (filteredId == null) return;

      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var actualChannel = Context.Guild.GetChannel(Convert.ToUInt64(filteredId));
      if (actualChannel == null)
      {
        await ReplyAsync($"{filteredId} does not exist in this server.");
        return;
      }

      if (serverObj.LogChannel == filteredId)
      {
        await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "WelcomeChannel", "");
        await ReplyAsync($"<#{filteredId}> is no longer the Welcome Channel!");
        return;
      }

      await MongoHelper.UpdateServer(Context.Guild.Id.ToString(), "WelcomeChannel", filteredId);
      await ReplyAsync($"The Welcome Channel has been set to: <#{filteredId}>!");
    }

    [Command("massrolecheck")]
    public async Task MassRoleCheck([Remainder] string style = null)
    {
      if (style == null)
      {
        await Context.Channel.SendMessageAsync("Please specify which list you want by typing `name` or `id` after the command.");
        return;
      }
      var userList = "";
      if (style.ToLower() == "name")
      {
        if (Context.Guild.Id.ToString() == Program.MasterSvr)
        {
          var staffRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == "Trusted Staff");
          var query =
              from c in Context.Guild.Users
              orderby c.Roles.Count descending
              select c;
          var objects = query.Take(24);
          foreach (var obj in objects)
          {
            if (obj.Roles.Contains(staffRole)) { continue; }
            userList += $"{obj.Username}#{obj.Discriminator} has {obj.Roles.Count - 1} roles.\n";
          }
          var embed = new EmbedBuilder();
          embed.AddField("Top Users", $"{userList}", true);
          embed.WithFooter("This command does not tag/ping the users, as it's inside of an embed.");
          await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
        else
        {
          var query =
              from c in Context.Guild.Users
              orderby c.Roles.Count descending
              select c;
          var objects = query.Take(15);
          foreach (var obj in objects)
          {
            userList += $"{obj.Username}#{obj.Discriminator} has {obj.Roles.Count - 1} roles.\n";
          }
          var embed = new EmbedBuilder();
          embed.AddField("Top Users", $"{userList}", true);
          embed.WithFooter("This command does not tag/ping the users, as it's inside of an embed.");
          await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
      }
      else if (style.ToLower() == "id")
      {
        if (Context.Guild.Id.ToString() == Program.MasterSvr)
        {
          var staffRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == "Trusted Staff");
          var query =
              from c in Context.Guild.Users
              orderby c.Roles.Count descending
              select c;
          var objects = query.Take(24);
          foreach (var obj in objects)
          {
            if (obj.Roles.Contains(staffRole)) { continue; }
            userList += $"<@{obj.Id}> with {obj.Roles.Count - 1} roles.\n";
          }
          var embed = new EmbedBuilder();
          embed.AddField("Top Users", $"{userList}", true);
          embed.WithFooter("This command does not tag/ping the users, as it's inside of an embed.");
          await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
        else
        {
          var query =
              from c in Context.Guild.Users
              orderby c.Roles.Count descending
              select c;
          var objects = query.Take(15);
          foreach (var obj in objects)
          {
            userList += $"<@{obj.Id}> with {obj.Roles.Count - 1} roles.\n";
          }
          var embed = new EmbedBuilder();
          embed.AddField("Top Users", $"{userList}", true);
          embed.WithFooter("This command does not tag/ping the users, as it's inside of an embed.");
          await Context.Channel.SendMessageAsync("", false, embed.Build());
        }
      }
      else
      {
        await Context.Channel.SendMessageAsync("Please specify which list you want by typing `name` or `id` after the command.");
        return;
      }
    }

    [Command("purge")]
    [Alias("prune")]
    public async Task Purge([Remainder] int amount = 0)
    {
      if (amount <= 0) { await Context.Channel.SendMessageAsync("Please specify how many messages to delete (1 - 100)"); return; }
      if (amount > 100) { await Context.Channel.SendMessageAsync("Sorry, I can only purge up to 100 messages!"); return; }
      var messages = await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, amount).FlattenAsync();
      var messagesFiltered = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);
      var count = messagesFiltered.Count();
      if (count == 0) { await Context.Channel.SendMessageAsync("Nothing was deleted, I may only delete messages up to 14 days old."); }
      else
      {
        await Context.Channel.DeleteMessageAsync(Context.Message.Id);
        await (Context.Channel as ITextChannel).DeleteMessagesAsync(messagesFiltered);
        const int delay = 2500;
        var m = await ReplyAsync($"I've swept up {count} {(count > 1 ? "messages" : "message")}.");
        await Task.Delay(delay);
        await m.DeleteAsync();
      }
      return;
    }
  }
}
