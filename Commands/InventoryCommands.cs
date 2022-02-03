using System;
using System.Threading.Tasks;
using Discord.Commands;
using PrototonBot.MongoUtil;

namespace PrototonBot.Commands
{
  public class InventoryCommands : ModuleBase<SocketCommandContext>
  {
    Random rand = new Random();

    [Command("chopdown")] [Alias("chop")]
    public async Task ChopWood() {
      var user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      var failedChop = rand.Next(0, 101) <= 20 ? true : false;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

      if (inv.Axes == 0) { await Context.Channel.SendMessageAsync($"Sorry <@{user.Id}>, but you don't have any axes! Get one from the store!"); return; }
      if (inv.LastChop > (currentTime - 180))  {
        long secsRemaining = ((inv.LastChop + 180) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Minutes + " minute(s) and " + spanOfTime.Seconds + " second(s)";
        await Context.Channel.SendMessageAsync($"Hey hey, take a quick break! Chopping trees takes a lot of stamina!\nTry again in {str}, <@{user.Id}>.");
        return;
      }

      if (failedChop) {
        //If you have 2 axes uses, take 1 use
        if (inv.AxeUses >= 2)  {
          await Context.Channel.SendMessageAsync($"You chopped and you chopped all day, but that bark was just too tough, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", (inv.AxeUses - 1));
        }
        //If you have 1 axe with 1 use left, take 1 use and the axe
        else if (inv.AxeUses == 1 && inv.Axes == 1) {
          await Context.Channel.SendMessageAsync($"You chopped at that tree all day until your only axe ended up breaking! Oh no, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", (inv.AxeUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Axes", (inv.Axes - 1));
        }
        //If you have 2+ axes, but 1 use left, reset uses and take 1 axe
        else if (inv.AxeUses == 1 && inv.Axes >= 2) {
          await Context.Channel.SendMessageAsync($"You chopped at that tree all day until one of your axes ended up breaking! Oh no, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", 10);
          await MongoHelper.UpdateInventory(user.Id, "Axes", (inv.Axes - 1));
        }
        
        await MongoHelper.UpdateInventory(user.Id, "LastChop", currentTime);
        return;
      } else {
        var LogsGet = rand.Next(1, 4);
        var LeavesGet = rand.Next(1, 20);
        if (inv.AxeUses == 10) {
          await Context.Channel.SendMessageAsync($"<@{user.Id}> hacked away with their brand new axe and chopped down a tree!\nThey got {LogsGet} :evergreen_tree: and {LeavesGet} :fallen_leaf:!");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", (inv.AxeUses - 1));
        }
        else if (inv.AxeUses >= 7 && inv.AxeUses <= 9) {
          if (LeavesGet >= 18) LeavesGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> successfully chopped down a tree!\nThey got {LogsGet} :evergreen_tree: and {LeavesGet} :fallen_leaf:!");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", (inv.AxeUses - 1));
        }
        else if (inv.AxeUses >= 4 && inv.AxeUses <= 6) {
          if (LeavesGet >= 14) LeavesGet -= 2;
          if (LogsGet == 4) LogsGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> managed to chop down a tree!\nThey got {LogsGet} :evergreen_tree: and {LeavesGet} :fallen_leaf:!");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", (inv.AxeUses - 1));
        }
        else if (inv.AxeUses >= 2 && inv.AxeUses <= 3) {
          if (LeavesGet >= 10) LeavesGet -= 3;
          if (LogsGet >= 3) LogsGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> struggled with a weak axe, but managed to chop some wimpy tree.\nThey got {LogsGet} :evergreen_tree: and {LeavesGet} :fallen_leaf:!");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", (inv.AxeUses - 1));
        }
        else if (inv.AxeUses == 1 && inv.Axes >= 2) {
          if (LeavesGet >= 6) LeavesGet -= 4;
          if (LogsGet >= 3) LogsGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> broke one of their super weak axes cutting down their last tree!\nThey got {LogsGet} :evergreen_tree: and {LeavesGet} :fallen_leaf:!");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", 10);
          await MongoHelper.UpdateInventory(user.Id, "Axes", (inv.Axes - 1));
        }
        else if (inv.AxeUses == 1 && inv.Axes == 1) {
          if (LeavesGet >= 6) LeavesGet -= 4;
          if (LogsGet >= 2) LogsGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> broke their only super weak axe cutting down their last tree!\nThey got {LogsGet} :evergreen_tree: and {LeavesGet} :fallen_leaf:!");
          await MongoHelper.UpdateInventory(user.Id, "AxeUses", (inv.AxeUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Axes", (inv.Axes - 1));
        }

        await MongoHelper.UpdateInventory(user.Id, "Logs", (inv.Logs + LogsGet));
        await MongoHelper.UpdateInventory(user.Id, "Leaves", (inv.Leaves + LeavesGet));
        await MongoHelper.UpdateInventory(user.Id, "LastChop", currentTime);
        return;
      }
    }

    [Command("mine")]
    public async Task PickMine() {
      var user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      var failedMine = rand.Next(0, 101) <= 30 ? true : false;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

      if (inv.Picks == 0) { await Context.Channel.SendMessageAsync($"Sorry <@{user.Id}>, but you don't have any Picks! Get one from the store!"); return; }
      if (inv.LastMine > (currentTime - 180)) {
        long secsRemaining = ((inv.LastMine + 180) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Minutes + " minute(s) and " + spanOfTime.Seconds + " second(s)";
        await Context.Channel.SendMessageAsync($"Hey hey, take a quick break! Mining ores really breaks out a sweat!\nTry again in {str}, <@{user.Id}>.");
        return;
      }

      if (failedMine) {
        //If you have 2 Picks uses, take 1 use
        if (inv.PickUses >= 2) {
          await Context.Channel.SendMessageAsync($"You swung your pick all day, but only ended up with severe back pain, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", (inv.PickUses - 1));
        }
        //If you have 1 axe with 1 use left, take 1 use and the axe
        if (inv.PickUses == 1 && inv.Picks == 1) {
          await Context.Channel.SendMessageAsync($"You mined as hard as you could but ended up breaking your only pick! Not good, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", (inv.PickUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Picks", (inv.Picks - 1));
        }
        //If you have 2+ Picks, but 1 use left, reset uses and take 1 axe
        if (inv.PickUses == 1 && inv.Picks >= 2) {
          await Context.Channel.SendMessageAsync($"You mined as hard as you could but ended up breaking one of your picks! Not good, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", 10);
          await MongoHelper.UpdateInventory(user.Id, "Picks", (inv.Picks - 1));
        }

        await MongoHelper.UpdateInventory(user.Id, "LastMine", currentTime);
        return;
      }
      else {
        var GemsGet = rand.Next(1, 3);
        var BricksGet = rand.Next(1, 12);
        if (inv.PickUses == 10) {
          await Context.Channel.SendMessageAsync($"<@{user.Id}> showed off with their new pick in the mines!\nThey got {GemsGet} :gem: and {BricksGet} :bricks:!");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", (inv.PickUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Diamonds", (inv.Diamonds + GemsGet));
        }
        if (inv.PickUses >= 7 && inv.PickUses <= 9) {
          if (BricksGet >= 10) BricksGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> successfully mined some of the new areas in the mines!\nThey got {GemsGet} :gem: and {BricksGet} :bricks:!");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", (inv.PickUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Diamonds", (inv.Diamonds + GemsGet));
        }
        if (inv.PickUses >= 4 && inv.PickUses <= 6) {
          if (BricksGet >= 8) BricksGet -= 2;
          if (GemsGet >= 2) GemsGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> wore down their pick getting some loot from the mines!\nThey got {GemsGet} :gem: and {BricksGet} :bricks:!");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", (inv.PickUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Diamonds", (inv.Diamonds + GemsGet));
        }
        if (inv.PickUses >= 2 && inv.PickUses <= 3) {
          if (BricksGet >= 6) BricksGet -= 3;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> struggled with a cracked pick, but got their hands on some bricks.\nThey got {BricksGet} :bricks:!");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", (inv.PickUses - 1));
        }
        if (inv.PickUses == 1 && inv.Picks >= 2) {
          if (BricksGet >= 6) BricksGet -= 3;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> broke one of their weak picks mining some in the mines!\nThey got {BricksGet} :bricks:!");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", 10);
          await MongoHelper.UpdateInventory(user.Id, "Picks", (inv.Picks - 1));
        }
        if (inv.PickUses == 1 && inv.Picks == 1) {
          if (BricksGet >= 5) BricksGet -= 4;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> broke their only weak pick mining some in the mines!\nThey got {BricksGet} :bricks:!");
          await MongoHelper.UpdateInventory(user.Id, "PickUses", (inv.PickUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Picks", (inv.Picks - 1));
        }

        await MongoHelper.UpdateInventory(user.Id, "Bricks", (inv.Bricks + BricksGet));
        await MongoHelper.UpdateInventory(user.Id, "LastMine", currentTime);
        return;
      }
    }

    [Command("salvage")]
    public async Task SalvageJunk() {
      var user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      var failedSalvage = rand.Next(0, 101) <= 25 ? true : false;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

      if (inv.Wrenches == 0) { await Context.Channel.SendMessageAsync($"Sorry <@{user.Id}>, but you don't have any Wrenches! Get one from the store!"); return; }
      if (inv.LastSalvage > (currentTime - 180)) {
        long secsRemaining = ((inv.LastSalvage + 180) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Minutes + " minute(s) and " + spanOfTime.Seconds + " second(s)";
        await Context.Channel.SendMessageAsync($"Hey hey, take a quick break! Salvaging through junk can get really tiring!\nTry again in {str}, <@{user.Id}>.");
        return;
      }

      if (failedSalvage) {
        //If you have 2 Wrenches uses, take 1 use
        if (inv.WrenchUses >= 2) {
          await Context.Channel.SendMessageAsync($"You turned that wrench for hours but just stripped everything, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", (inv.WrenchUses - 1));
        }
        //If you have 1 axe with 1 use left, take 1 use and the axe
        if (inv.WrenchUses == 1 && inv.Wrenches == 1) {
          await Context.Channel.SendMessageAsync($"You broke your only wrench in half turning it too hard, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", (inv.WrenchUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Wrenches", (inv.Wrenches - 1));
        }
        //If you have 2+ Wrenches, but 1 use left, reset uses and take 1 axe
        if (inv.WrenchUses == 1 && inv.Wrenches >= 2) {
          await Context.Channel.SendMessageAsync($"You broke one of your wrenches in half turning it too hard, <@{user.Id}>.");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", 10);
          await MongoHelper.UpdateInventory(user.Id, "Wrenches", (inv.Wrenches - 1));
        }
        
        await MongoHelper.UpdateInventory(user.Id, "LastSalvage", currentTime);
        return;
      }
      else {
        var BoltsGet = rand.Next(1, 7);
        var GearsGet = rand.Next(1, 16);
        if (inv.WrenchUses == 10) {
          await Context.Channel.SendMessageAsync($"<@{user.Id}> showed that machine who's boss with their new wrench!\nThey got {BoltsGet} :nut_and_bolt: and {GearsGet} :gear:!");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", (inv.WrenchUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Bolts", (inv.Bolts + BoltsGet));
        }
        if (inv.WrenchUses >= 7 && inv.WrenchUses <= 9) {
          if (GearsGet >= 12) GearsGet -= 3;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> successfully disassembled that machine!\nThey got {BoltsGet} :nut_and_bolt: and {GearsGet} :gear:!");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", (inv.WrenchUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Bolts", (inv.Bolts + BoltsGet));
        }
        if (inv.WrenchUses >= 4 && inv.WrenchUses <= 6) {
          if (GearsGet >= 9) GearsGet -= 4;
          if (BoltsGet >= 4) BoltsGet -= 1;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> stripped their wrench a bit on that machine...\nThey got {BoltsGet} :nut_and_bolt: and {GearsGet} :gear:!");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", (inv.WrenchUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Bolts", (inv.Bolts + BoltsGet));
        }
        if (inv.WrenchUses >= 2 && inv.WrenchUses <= 3) {
          if (GearsGet >= 6) GearsGet -= 5;
          if (BoltsGet >= 2) BoltsGet -= 2;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> struggled with that machine, but got something out of it.\nThey got {BoltsGet} :nut_and_bolt: and {GearsGet} :gear:!");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", (inv.WrenchUses - 1));
        }
        if (inv.WrenchUses == 1 && inv.Wrenches >= 2) {
          if (GearsGet >= 4) GearsGet -= 6;
          if (BoltsGet >= 1) BoltsGet -= 2;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> broke one of their wrenches turning it too aggressively!\nThey got {BoltsGet} :nut_and_bolt: and {GearsGet} :gear:!");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", 10);
          await MongoHelper.UpdateInventory(user.Id, "Wrenches", (inv.Wrenches - 1));
        }
        if (inv.WrenchUses == 1 && inv.Wrenches == 1) {
          if (GearsGet >= 4) GearsGet -= 6;
          if (BoltsGet >= 1) BoltsGet -= 2;
          await Context.Channel.SendMessageAsync($"<@{user.Id}> broke their only wrench turning it too aggressively!\nThey got {BoltsGet} :nut_and_bolt: and {GearsGet} :gear:!");
          await MongoHelper.UpdateInventory(user.Id, "WrenchUses", (inv.WrenchUses - 1));
          await MongoHelper.UpdateInventory(user.Id, "Wrenches", (inv.Wrenches - 1));
        }
      
        await MongoHelper.UpdateInventory(user.Id, "Gears", (inv.Gears + GearsGet));
        await MongoHelper.UpdateInventory(user.Id, "LastSalvage", currentTime);
        return;
      }
    }

    [Command("gamble")] [Alias("bet")]
    public async Task GambleCommand(long bet = 0) {
      var user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      var GambleVariant = ((rand.NextDouble() * 0.5) + 0.75);
      var userXPMultiplier = (1 * (user.Boosted ? 1.05 : 1) * (user.Mutuals ? 1.05 : 1)); //1, 1.05, or 1.1025

      if (user.Level < 3) {
        await Context.Channel.SendMessageAsync($"Sorry, but gambling is only available to users Level **3** or higher, <@{user.Id}>!\nTip: Chat in servers for a while that I'm also in to get experience to get levels.");
        return;
      }

      if (bet <= 0) {
        await Context.Channel.SendMessageAsync($"Please enter a valid amount to gamble up to 500 Protobucks, <@{user.Id}>");
        return;
      }

      if (user.Money < bet) {
        await Context.Channel.SendMessageAsync($"You don't even have that many Protobucks, <@{user.Id}>!");
        return;
      }

      if (bet > 500) {
        await Context.Channel.SendMessageAsync($"Sorry, but the casino has some spies that do.. bad things to people pushing their luck..\nPlease bet a valid amount up to 500 Protobucks, <@{user.Id}>.");
        return;
      }

      if (user.LastGamble > (currentTime - 180)) {
        long secsRemaining = ((user.LastGamble + 180) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Minutes + " minute(s) and " + spanOfTime.Seconds + " second(s)";
        await Context.Channel.SendMessageAsync($"You're gambling a bit too fast, I don't think you want the casino spies coming after you, <@{user.Id}>!\nTry again in {str}.");
        return;
      }

      else {
        long bet3 = (long) Math.Round((bet * 3) * GambleVariant);
        long bet2 = (long) Math.Round((bet * 2) * GambleVariant);
        
        await MongoHelper.UpdateUser(user.Id, "LastGamble", currentTime);
        await MongoHelper.UpdateUser(user.Id, "Gambles", (user.Gambles + 1));
        long gambleChance = (long) Math.Floor((double) (rand.Next(0, (-user.Luck + 100))));
        if (gambleChance <= 12) {
          await MongoHelper.UpdateInventory(user.Id, "GambleCoins", (inv.GambleCoins + 2));
          await MongoHelper.UpdateInventory(user.Id, "GambleCoinsTotal", (inv.GambleCoinsTotal + 2));
          await MongoHelper.UpdateUser(user.Id, "GamblesWon", (user.GamblesWon + 1));
          await MongoHelper.UpdateUser(user.Id, "GamblesNetGain", ((user.GamblesNetGain - bet) + bet3));
          await MongoHelper.UpdateUser(user.Id, "Money", ((user.Money - bet) + bet3));
          await MongoHelper.UpdateUser(user.Id, "EXP", (Math.Floor(user.EXP + (30 * userXPMultiplier))));
          await Context.Channel.SendMessageAsync($"Wow, you did great at the casino, <@{user.Id}>.\nYou bet {bet} Protobacks and made a profit of {bet3 - bet} Protobucks!\nYou also received 2 Gamble Coins!");
        }
        else if (gambleChance >= 13 && gambleChance <= 37) {
          await MongoHelper.UpdateInventory(user.Id, "GambleCoins", (inv.GambleCoins + 1));
          await MongoHelper.UpdateInventory(user.Id, "GambleCoinsTotal", (inv.GambleCoinsTotal + 1));
          await MongoHelper.UpdateUser(user.Id, "GamblesWon", (user.GamblesWon + 1));
          await MongoHelper.UpdateUser(user.Id, "GamblesNetGain", ((user.GamblesNetGain - bet) + bet2));
          await MongoHelper.UpdateUser(user.Id, "Money", ((user.Money - bet) + bet2));
          await MongoHelper.UpdateUser(user.Id, "EXP", (Math.Floor(user.EXP + (20 * userXPMultiplier))));
          await Context.Channel.SendMessageAsync($"You seem to have done well at the casino, <@{user.Id}>.\nYou bet {bet} Protobucks and made a profit of {bet2 - bet} Protobucks!\nYou also received a Gamble Coin!");
        }
        else if (gambleChance >= 38 && gambleChance <= 66) {
          await MongoHelper.UpdateUser(user.Id, "EXP", (Math.Floor(user.EXP + (10 * userXPMultiplier))));
          await Context.Channel.SendMessageAsync($"<@{user.Id}>, although you had a bit of fun at the casino, you left keeping your {bet} Protobucks.");
        }
        else if (gambleChance >= 67) {
          await MongoHelper.UpdateUser(user.Id, "GamblesLost", (user.GamblesLost + 1));
          await MongoHelper.UpdateUser(user.Id, "GamblesNetLoss", (user.GamblesNetLoss - bet));
          await MongoHelper.UpdateUser(user.Id, "Money", (user.Money - bet));
          await Context.Channel.SendMessageAsync($"Awe gee, no.. I'm so sorry <@{user.Id}>, but the casino has beaten you!\nYou've lost your {bet} Protobucks!");
        }

        return;
      }
    }
  }
}
