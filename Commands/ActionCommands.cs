using System;
using System.Threading.Tasks;
using Discord.Commands;
using PrototonBot.MongoUtil;
using System.Text;

namespace PrototonBot.Commands
{
  public class ActionCommands : ModuleBase<SocketCommandContext>
  {
    Random rand = new Random();

    [Command("love%")]
    [Alias("ship")]
    public async Task LoveCalculation(string userCalled = null)
    {
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString())
      {
        await Context.Channel.SendMessageAsync("Hehehe. That's quite the ego you've got there!");
        return;
      }

      double lovePercent = rand.Next(0, 101);
      await Context.Channel.SendMessageAsync($"I say that {Context.User.Username} and {Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username} are {lovePercent}% compatible!");
    }

    [Command("hug")]
    public async Task HugCommand(string userCalled = null)
    {
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.Message.Author.Id.ToString())
      {
        await Context.Channel.SendMessageAsync("You can't give yourself a hug, but I can! Come here, you big cutie! :heart:");
        return;
      }
      var authorInventory = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      double chanceOfHugCoin = rand.Next(0, 2);
      if (chanceOfHugCoin == 1)
      {
        await MongoHelper.UpdateInventory(Context.User.Id.ToString(), "HugCoins", (authorInventory.HugCoins + 1));
        await MongoHelper.UpdateInventory(Context.User.Id.ToString(), "HugCoinsTotal", (authorInventory.HugCoinsTotal + 1));

        await Context.Channel.SendMessageAsync($"{Context.User.Username} gave {Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username} a big ol' stinkin' hug! So sweet! :heart: :heart:\nThey also got a Hug Coin for being so sweet!");
        return;
      }
      await Context.Channel.SendMessageAsync($"{Context.User.Username} gave {Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username} a big ol' stinkin' hug! So sweet! :heart: :heart:");
      return;
    }

    [Command("nuzzle")]
    [Alias("boop")]
    public async Task NuzzleCommand(string userCalled = null)
    {
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString())
      {
        await Context.Channel.SendMessageAsync("You can't nuzzle yourself, but I can! Hehe, boop! :nose: :heart:");
        return;
      }
      await Context.Channel.SendMessageAsync($"{Context.User.Username} gave {Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username} a sweet little nuzzle! Awee. :heart:");
      return;
    }

    [Command("punish")]
    public async Task PunishCommand(string userCalled = null, [Remainder] string input = null)
    {
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      if (input == null)
      {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        if (filteredId == Context.User.Id.ToString())
        {
          await Context.Channel.SendMessageAsync($"{Context.User.Username}, as bad as it is to you this on yourself, you can't punish yourself, so...\n**I PUNISH YOU! BAD BAD BAD!**");
          return;
        }
        await Context.Channel.SendMessageAsync($"{Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username}, you have been punished by {Context.User.Username} for being bad!");
        return;
      }
      else
      {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        if (filteredId == Context.User.Id.ToString())
        {
          await Context.Channel.SendMessageAsync($"{Context.User.Username}, as bad as it is to you this on yourself, you can't punish yourself, so...\n**I PUNISH YOU! BAD BAD BAD!**");
          return;
        }
        await Context.Channel.SendMessageAsync($"{Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username}, you have been punished by {Context.User.Username} for {input}!");
        return;
      }
    }

    [Command("lpunish")]
    [Alias("lewdpunish")]
    public async Task LewdPunishCommand(string userCalled = null)
    {
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString())
      {
        await Context.Channel.SendMessageAsync($"{Context.User.Username}, well that depends, have you been lewd? Do I need to... *tell on you?*");
        return;
      }
      await Context.Channel.SendMessageAsync($"{Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username}, you have been punished by {Context.User.Username} for being too *lewd!*");
      return;
    }

    [Command("lick")]
    public async Task LickCommand(string userCalled = null)
    {
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString())
      {
        await Context.Channel.SendMessageAsync($"Well, you can't lick yourself, so... :tongue: Here ya go, bleeehp! {Context.User.Username}");
        return;
      }
      await Context.Channel.SendMessageAsync($"{Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username} was given a cute slobber to the face by {Context.User.Username}!:tongue:");
      return;
    }

    [Command("yeet")]
    public async Task YeetCommand(string userCalled = null)
    {
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString())
      {
        await Context.Channel.SendMessageAsync($"Are.. you feeling okay? You can't exactly yeet yourself.. {Context.User.Username}");
        return;
      }
      await Context.Channel.SendMessageAsync($"{Context.Guild.GetUser(Convert.ToUInt64(filteredId)).Username} was yeeted into the oblivion by {Context.User.Username}!:tongue:");
      return;
    }

    [Command("sass")]
    public async Task SassCommand([Remainder] string message)
    {
      var rand = new Random();
      int num = rand.Next(1, 6);

      if (message == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to give me something to sass!");
      }

      var Builder = new StringBuilder();
      var index = 0;

      // If even, start with upper case.
      if (num % 2 == 0)
      {
        foreach (char c in message)
        {
          if (index % 2 == 0)
          {
            Builder.Append(char.ToUpper(c));
          }
          else
          {
            Builder.Append(char.ToLower(c));
          }
          index++;
        }
      }
      // If odd, start with lower case.
      else
      {
        foreach (char c in message)
        {
          if (index % 2 == 0)
          {
            Builder.Append(char.ToLower(c));
          }
          else
          {
            Builder.Append(char.ToUpper(c));
          }
          index++;
        }
      }

      await Context.Channel.SendMessageAsync(Builder.ToString());
    }
  }
}
