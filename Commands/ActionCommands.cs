using System;
using System.Threading.Tasks;
using Discord.Commands;
using PrototonBot.MongoUtil;
using System.Text;

namespace PrototonBot.Commands {
  public class ActionCommands : ModuleBase<SocketCommandContext> {
    Random rand = new Random();

    //Ship the user with the user that they tagged, random rand from 0-100.
    [Command("love%")] [Alias("ship")]
    public async Task LoveCalculation(string userCalled = null) {
      if (userCalled == null)  {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString()) {
        await Context.Channel.SendMessageAsync("Hehehe. That's quite the self love you've got there!");
        return;
      }

      double lovePercent = rand.Next(0, 101);
      await Context.Channel.SendMessageAsync($"Consulting the love goddess.. {Context.User.Username} and {Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username} are {lovePercent}% in love!");
      return;
    }

    //Replies with a hug message to the person they hugged, chance of getting a hug coin.
    [Command("hug")]
    public async Task HugCommand(string userCalled = null) {
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString()) {
        await Context.Channel.SendMessageAsync("You can't give yourself a hug, but I can! Come here, you big cutie! :heart:");
        return;
      }

      var userInv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      double chanceOfHugCoin = rand.Next(0, 4);
      if (chanceOfHugCoin == 1) {
        await MongoHelper.UpdateInventory(Context.User.Id.ToString(), "HugCoins", (userInv.HugCoins + 1));
        await MongoHelper.UpdateInventory(Context.User.Id.ToString(), "HugCoinsTotal", (userInv.HugCoinsTotal + 1));
        await Context.Channel.SendMessageAsync($"{Context.User.Username} gave {Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username} a big ol' stinkin' hug! So sweet! :heart: :heart:\nThey also got a Hug Coin for being kind!");
        return;
      }

      await Context.Channel.SendMessageAsync($"{Context.User.Username} gave {Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username} a big ol' stinkin' hug! So sweet! :heart: :heart:");
      return;
    }

    //Gives a friend a boop!
    [Command("boop")] [Alias("nuzzle")]
    public async Task NuzzleCommand(string userCalled = null) {
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString()) {
        await Context.Channel.SendMessageAsync("You can't boop yourself, but I can! Hehe, boop! :nose: :heart:");
        return;
      }

      await Context.Channel.SendMessageAsync($"{Context.User.Username} gave {Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username} a sweet little nuzzle! Awee. :heart:");
      return;
    }

    //Teasingly punish the user, for whatever specified.
    [Command("punish")]
    public async Task PunishCommand(string userCalled = null, [Remainder] string input = null) {
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString()) {
        await Context.Channel.SendMessageAsync($"{Context.User.Username}, why would you want to punish yourself? Here, I'll do it for you.. bad bad!");
        return;
      }

      if (input != null) {
        await Context.Channel.SendMessageAsync($"{Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username}, you have been punished by {Context.User.Username} for {input}!");
        return;
      }

      await Context.Channel.SendMessageAsync($"{Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username}, you have been punished by {Context.User.Username} for being bad!");
      return;
    }

    //Specific joke to teasingly punish a user for being lewd/innapropriate.
    [Command("lpunish")] [Alias("lewdpunish")]
    public async Task LewdPunishCommand(string userCalled = null) {
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString()) {
        await Context.Channel.SendMessageAsync($"{Context.User.Username}, well that depends, have you been lewd? Do I need to.. **tell on you**!?");
        return;
      }

      await Context.Channel.SendMessageAsync($"{Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username}, you have been punished by {Context.User.Username} for being too **lewd*!");
      return;
    }

    //Lick a user {people are odd}
    [Command("lick")]
    public async Task LickCommand(string userCalled = null) {
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }

      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString()) {
        await Context.Channel.SendMessageAsync($"Well, you can't lick yourself so, :tongue: Here ya go, bleeehp! {Context.User.Username}");
        return;
      }

      await Context.Channel.SendMessageAsync($"{Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username} was given a cute slobber to the face by {Context.User.Username}! :tongue:");
      return;
    }

    //"Yeet" a user {slang for throwing someone}
    [Command("yeet")]
    public async Task YeetCommand(string userCalled = null) {
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.User.Id.ToString()) {
        await Context.Channel.SendMessageAsync($"Are.. you feeling okay? You can't exactly yeet yourself.. {Context.User.Username}");
        return;
      }

      await Context.Channel.SendMessageAsync($"{Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result.Username} was yeeted into the oblivion by {Context.User.Username}! :wastebasket:");
      return;
    }

    //Convert the user's input to alternating case shift.
    [Command("sass")]
    public async Task SassCommand([Remainder] string message) {
      if (message == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to give me something to sass!");
      }
      
      var rand = new Random();
      int capitalFirst = rand.Next(1, 3);
      var Builder = new StringBuilder();
      
      //For each character in the message, dictate what to convert it to.
      var index = 0;
      foreach(char c in message) {
        //If capital first, upper-lower, otherwise, lower-upper.
        if (capitalFirst == 1) {
          if (index % 2 == 0) Builder.Append(char.ToUpper(c));
          else Builder.Append(char.ToLower(c));
        } else {
          if (index % 2 == 0) Builder.Append(char.ToLower(c));
          else Builder.Append(char.ToUpper(c));
        }
        index++;
      }
      
      await Context.Channel.SendMessageAsync(Builder.ToString());
    }
  }
}
