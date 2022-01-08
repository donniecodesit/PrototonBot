using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PrototonBot.MongoUtil;

namespace PrototonBot.Commands
{
  public class StoreCommands : ModuleBase<SocketCommandContext>
  {
    [Command("store")]
    [Alias("shop")]
    [Summary("haha this is test for store")]
    public async Task ItemStore(string page = null)
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;

      if (page == null)
      {
        var embed = new EmbedBuilder();
        embed.WithColor(0xFF00FF);
        embed.WithTitle("PrototonBot Store Directory");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithDescription("PrototonBot has different store pages you can view! Which one do you want?");
        embed.AddField($"{serverObj.Prefix}store 1", $"View personal and profile effects you can purchase.");
        embed.AddField($"{serverObj.Prefix}store 2", $"View inventory items you can purchase.");
        await Context.Channel.SendMessageAsync("", false, embed.Build());
        return;
      }
      if (page == "1")
      {
        if (serverObj.Id == Program.MasterSvr)
        {
          var embed = new EmbedBuilder();
          embed.WithColor(0xFF00FF);
          embed.WithTitle("PrototonBot Store - Page 1");
          embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
          embed.WithFooter($"Replying to: {Context.User.Username}");
          embed.AddField($"``{serverObj.Prefix}buy Boosted`` - 10,000 Protobucks", "Get a 5% multiplier on Chat EXP and Protobucks!");
          embed.AddField($"``{serverObj.Prefix}buy DailyPat`` - 1,000 Protobucks", "Restore your daily pat so that you can give another one!");
          embed.AddField("Your Protobucks", user.Money);
          await Context.Channel.SendMessageAsync("", false, embed.Build());
          return;
        }
        else
        {
          var embed = new EmbedBuilder();
          embed.WithColor(0xFF00FF);
          embed.WithTitle("PrototonBot Store - Page 1");
          embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
          embed.WithFooter($"Replying to: {Context.User.Username}");
          embed.AddField($"``{serverObj.Prefix}buy Boosted`` - 10,000 Protobucks", "Get a 5% multiplier on Chat EXP and Protobucks!");
          embed.AddField($"``{serverObj.Prefix}buy DailyPat`` - 1,000 Protobucks", "Restore your daily pat so that you can give another one!");
          embed.AddField("Your Protobucks", user.Money);
          await Context.Channel.SendMessageAsync("", false, embed.Build());
          return;
        }
      }
      if (page == "2")
      {
        var embed = new EmbedBuilder();
        embed.WithColor(0xFF00FF);
        embed.WithTitle("PrototonBot Store - Page 2");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter($"Replying to: {Context.User.Username}");
        embed.AddField($":axe: ``{serverObj.Prefix}buy axe`` - 500 Protobucks", $"Allows you to use ``{serverObj.Prefix}chopdown``!");
        embed.AddField($":pick: ``{serverObj.Prefix}buy pick`` - 700 Protobucks", $"Allows you to use ``{serverObj.Prefix}mine``!");
        embed.AddField($":wrench: ``{serverObj.Prefix}buy wrench`` - 900 Protobucks", $"Allows you to use ``{serverObj.Prefix}salvage``!");
        embed.AddField("Your Protobucks", user.Money);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
        return;
      }
    }

    [Command("buy")]
    [Alias("purchase", "redeem", "get")]
    public async Task BuyCommand(string item = null)
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;

      if (item == null)
      {
        await Context.Channel.SendMessageAsync($"Please enter a valid item to buy.\nUse {serverObj.Prefix}store to learn more.");
        return;
      }

      if (item.ToLower() == "boosted")
      {
        if (user.Money >= 10000)
        {
          if (user.Boosted == true)
          {
            await Context.Channel.SendMessageAsync($"You already are boosted, <@{user.Id}>!");
          }
          else
          {
            await Context.Channel.SendMessageAsync($"Congratulations! You're now boosted and had 10000 Protobucks taken from your bank. Enjoy! <@{user.Id}>");
            await MongoHelper.UpdateUser(user.Id, "Money", (user.Money - 10000));
            await MongoHelper.UpdateUser(user.Id, "Purchases", (user.Purchases + 1));
            await MongoHelper.UpdateUser(user.Id, "Boosted", true);
          }
          return;
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {user.Money}/10000 Protobucks.");
          return;
        }
      }

      if (item.ToLower() == "dailypat")
      {
        if (user.Money >= 1000)
        {
          var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

          if (user.LastPat > (currentTime - 86400))
          {
            await MongoHelper.UpdateUser(user.Id, "LastPat", 0);
            await MongoHelper.UpdateUser(user.Id, "Money", (user.Money - 1000));
            await MongoHelper.UpdateUser(user.Id, "Purchases", (user.Purchases + 1));
            await Context.Channel.SendMessageAsync($"Awesome! Your daily pat was reset and 1000 Protobucks taken from your bank. Enjoy, <@{user.Id}>!");
            return;
          }
          else
          {
            await Context.Channel.SendMessageAsync($"You haven't even given away your daily pat today, wouldn't that be a waste of Protobucks, <@{user.Id}>?");
            return;
          }
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {user.Money}/1000 Protobucks.");
          return;
        }
      }

      if (item.ToLower() == "axe")
      {
        if (user.Money >= 500)
        {
          await MongoHelper.UpdateInventory(user.Id, "Axes", (inv.Axes + 1));
          if (inv.AxeUses == 0) await MongoHelper.UpdateInventory(user.Id, "AxeUses", 10);
          await MongoHelper.UpdateUser(user.Id, "Money", (user.Money - 500));
          await MongoHelper.UpdateUser(user.Id, "Purchases", (user.Purchases + 1));
          await Context.Channel.SendMessageAsync($"Awesome, you've purchased an :axe: for 500 Protobucks! You now have {inv.Axes + 1} Axe(s)! <@{user.Id}>");
          return;
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {user.Money}/500 Protobucks.");
          return;
        }
      }

      if (item.ToLower() == "pick")
      {
        if (user.Money >= 700)
        {
          await MongoHelper.UpdateInventory(user.Id, "Picks", (inv.Picks + 1));
          if (inv.PickUses == 0) await MongoHelper.UpdateInventory(user.Id, "PickUses", 10);
          await MongoHelper.UpdateUser(user.Id, "Money", (user.Money - 700));
          await MongoHelper.UpdateUser(user.Id, "Purchases", (user.Purchases + 1));
          await Context.Channel.SendMessageAsync($"Awesome, you've purchased a :pick: for 700 Protobucks! You now have {inv.Picks + 1} Pick(s)! <@{user.Id}>");
          return;
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {user.Money}/700 Protobucks.");
          return;
        }
      }

      if (item.ToLower() == "wrench")
      {
        if (user.Money >= 900)
        {
          await MongoHelper.UpdateInventory(user.Id, "Wrenches", (inv.Wrenches + 1));
          if (inv.WrenchUses == 0) await MongoHelper.UpdateInventory(user.Id, "WrenchUses", 10);
          await MongoHelper.UpdateUser(user.Id, "Money", (user.Money - 700));
          await MongoHelper.UpdateUser(user.Id, "Purchases", (user.Purchases + 1));
          await Context.Channel.SendMessageAsync($"Awesome, you've purchased a :wrench: for 700 Protobucks! You now have {inv.Wrenches + 1} Wrench(es)! <@{user.Id}>");
          return;
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Sorry, but you don't have enough Protobucks to afford this!\nYou have {user.Money}/900 Protobucks.");
          return;
        }
      }

      await Context.Channel.SendMessageAsync($"Sorry, but the item you typed doesn't exist in our store. Please check the spelling or store and try again ^^ <@{user.Id}>");
      return;


    }

    [Command("upgrade")]
    public async Task UpgradeStore(string item = null)
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;

      if (item == null || item == "shop")
      {
        var embed = new EmbedBuilder();
        embed.WithColor(0xFF00FF);
        embed.WithTitle("PrototonBot Upgrade Shop");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithDescription($"Hey! Welcome to the super magic Upgrade Shop! We take those amazing coins you've earned from using our services and infuse them with your profile to give you better statistics all around! Currently, we provide 2 stats. <@{user.Id}>");
        embed.AddField($"``{serverObj.Prefix}upgrade Luck`` (Your stat: {user.Luck})", $"Costs: 14 Daily Coins, 14 Pat Coins, 50 Gamble Coins.\nRaises your Luck by 1, increasing your probability at gambling. Maximum of 25.");
        embed.AddField($"``{serverObj.Prefix}upgrade DailyBonus`` (Your stat: {user.DailyBonus})", $"Costs: 7 Daily Coins.\nRaises your Daily earnings by 100.");
        embed.AddField("Your Coins", $"DC: {inv.DailyCoins} | PC: {inv.PatCoins} | GC: {inv.GambleCoins} | HC: {inv.HugCoins}");
        await Context.Channel.SendMessageAsync("", false, embed.Build());
        return;
      }

      if (item.ToLower() == "luck")
      {
        if (inv.DailyCoins >= 14 && inv.PatCoins >= 14 && inv.GambleCoins >= 50)
        {
          if (user.Luck >= 0 && user.Luck <= 23)
          {
            await Context.Channel.SendMessageAsync($"Wowwee! Look at all these coins, oh my gosh! That's amazing! <@{user.Id}>, thank you for this massive heap! You must have worked really hard for these, so I'll fuse them into your soul and give you an increase of 1 on your Luck, but keep in mind it maxes out at 25.");
            await MongoHelper.UpdateInventory(user.Id, "DailyCoins", (inv.DailyCoins - 14));
            await MongoHelper.UpdateInventory(user.Id, "PatCoins", (inv.PatCoins - 14));
            await MongoHelper.UpdateInventory(user.Id, "GambleCoins", (inv.GambleCoins - 50));
            await MongoHelper.UpdateUser(user.Id, "Luck", (user.Luck + 1));
            return;

          }
          if (user.Luck == 24)
          {
            await Context.Channel.SendMessageAsync($"Wowwee! Look at all these coins, oh my gosh! That's amazing! <@{user.Id}>, thank you for this massive heap! You must have worked really hard for these, so I'll fuse them into your soul and give you an increase of 1 on your Luck-\n***OH MY GOSH WOW YOU REACHED THE MAXIMUM LUCK STAT OF 25! CONGRATULATIONS!***");
            await MongoHelper.UpdateInventory(user.Id, "DailyCoins", (inv.DailyCoins - 14));
            await MongoHelper.UpdateInventory(user.Id, "PatCoins", (inv.PatCoins - 14));
            await MongoHelper.UpdateInventory(user.Id, "GambleCoins", (inv.GambleCoins - 50));
            await MongoHelper.UpdateUser(user.Id, "Luck", (user.Luck + 1));
            return;
          }
          if (user.Luck == 25)
          {
            await Context.Channel.SendMessageAsync($"<@{user.Id}>, I'm super sorry, but the Luck stat caps out at 25, and.. you're already there. Try upgrading other stats now!");
            return;
          }
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Seems like you don't have enough coins. Here's what you have compared to what you need! <@{user.Id}>\nDC: ``{inv.DailyCoins}/14``, PC: ``{inv.PatCoins}/14``, ``{inv.GambleCoins}/50``.");
          return;
        }
      }

      if (item.ToLower() == "dailybonus" || item.ToLower() == "daily")
      {
        if (inv.DailyCoins >= 7)
        {
          await Context.Channel.SendMessageAsync($"Wowza, you really are devoted! I'll take these 7 Daily Coins from you and fuse them into your soul...\nNow you earn 100 more Protobucks on your dailies! <@{user.Id}>");
          await MongoHelper.UpdateInventory(user.Id, "DailyCoins", (inv.DailyCoins - 7));
          await MongoHelper.UpdateUser(user.Id, "DailyBonus", (user.DailyBonus + 100));
          return;
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Seems like you don't have enough coins. Here's what you have compared to what you need! <@{user.Id}>\nDC: ``{inv.DailyCoins}/7``.");
          return;
        }
      }
    }
  }
}
