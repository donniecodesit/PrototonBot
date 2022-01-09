using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DnsClient.Protocol;
using PrototonBot.MongoUtil;

namespace PrototonBot.Commands
{
  public class HelpCommandGroup : ModuleBase<SocketCommandContext>
  {
    [Group("help")]
    [Alias("commands")]
    [RequireContext(ContextType.Guild)]
    public class HelpCommand : ModuleBase<SocketCommandContext> {
      [Command]
      public async Task Default() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Command Menu");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the categories of commands you can use with PrototonBot!\nThis server's prefix is set to: `{svr.Prefix}`\nTo view a category, type `{svr.Prefix}help CATEGORYNAME`.\n If you want a simple version of this menu, do `{svr.Prefix}commands simple`.");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("Categories", "1. **general** - General Commands\n2. **fun** - Fun/Action Commands\n3. **profile** - Profiling Commands\n4. **economy** - Economy Commands\n5. **media** - Image & Video commands\n6. **scenario** - RNG Scenario Commands\n7. **admin** - Administrative Commands (Admins Only)", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("general")] [Alias("1")]
      public async Task CommGeneralPage() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot General Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the General Commands you can use with PrototonBot!\nThis server's prefix is set to: `{svr.Prefix}`");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("General Commands", "•**info** (*botinfo*)\nGet some information about the bot and links!\n•**serverinfo**\nGet some information about this very server.\n•**help**\nView the categories, descriptions, and aliases of every command.\n•**simonsays**\nGet a response of what you said.\n•**ping***\nUhhh... pong?", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("fun")] [Alias("2")]
      public async Task CommFunPage() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Fun & Action Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the Fun & Action Commands you can use with PrototonBot!\nThis server's prefix is set to: `{svr.Prefix}`");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("Fun & Action Commands", "•**hug**\nHug a user of your choice, get a coin for it!\n•**nuzzle** (*boop*)\nGive a user some cute nuzzling love!\n•**ship** (*love%*)\nFind how compatible you and another user are.\n•**loveme** (*protolove*)\nGet some fuzzy lovin' from PrototonBot!\n•**noticeme** (*senpai*)\nHave PrototonBot give you some extra love!\n•**coinflip** (*flipacoin*, *flipcoin*)\nFlip a coin and get heads or tails.\n•**punish**\nPunish a user for being bad, or put your own reason!\n•**lewdpunish** (*lpunish*)\nPunish a user for being lewd.\n•**yeet**\nYeet a user off like an empty soda can.\n•**lick**\nGive a user a friendly little lick!\n•**gamble** (*bet*)\nBet money and get 0~3.75x your bet back!\n•**she**\nshe.\n•**sass**\nI cAn Be SaSsY!", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("profile")] [Alias("3")]
      public async Task CommProfilePage() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Profiling Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the Profiling Commands you can use with PrototonBot!\nThis server's prefix is set to: `{svr.Prefix}`");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("Profiling Commands", "•**profile** (*currency*, *bank*, *account*, *me*, *money*)\nView your profile or someone else's.\n•**wallet**\nView your bank in a much simpler form.\n•**inventory** (*bag*, *bags*, *items*)\nView what items you currently own.\n•**daily**\nEarn some extra Protobucks once per day!\n•**pat** (*pet*)\nGive a user a pat once per day! (Reputation)\n•**transfer** (*send*)\nSend Protobucks to another user!\n•**setprofile** (*description*, *profiledesc*, *setdesc*)\nSets the description on your profile. (240 limit)\n•**leaderboard** (*top*)\nView the top 15 users for Protobucks or Levels.\n•**partnerup** (*setpartner*, *partner*, *marry*)\nSet a partner. Mutuals get 5% boosted Chat EXP and Protobucks.\n•**partnerdown** (*removepartner*, *unpartner*, *divorce*)\nRemove your previously set partner.", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("economy")] [Alias("4")]
      public async Task CommEconomyPage() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Economy Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the Economy Commands you can use with PrototonBot!\nThis server's prefix is set to: `{svr.Prefix}`");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("Economy Commands", "•**store** (*shop*)\nView roles, effects, and items for purchase!\n•**upgrade**\nUse your coins to upgrade stats.\n•**buy** (*purchase*, *redeem*, *get*)\nBuy items listed from the store.\n•**chopdown** (*chop*)\nUse an axe to get forestry items.\n•**mine**\nUse a pick to get mineral items.\n•**salvage**\nUse a wrench to get mechanical items.", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("media")] [Alias("5")]
      public async Task CommMediaPage() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Image & Video Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the Image & Video Commands you can use with PrototonBot!\nThis server's prefix is set to: `{svr.Prefix}`");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("Media Commands", "•**hope**\nGet a hopeful sprite from the Danganronpa 1 Cast.\n•**despair**\nGet a despairful sprite from the Danganronpa 1 Cast.\n•**hope2**\nGet a hopeful sprite from the Danganronpa 2 Cast.\n•**despair2**\nGet a despairful sprite from the Danganronpa 2 Cast.\n•**yasqueen**\nGet a random image of Junko Enoshima.\n•**kamakura**\nGet a .gif related to Izuru Kamakura.\n•**explode** (*kaboom*, *boom*)\nGet a random exploding .gif.", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("scenario")] [Alias("6")]
      public async Task CommScenarioPage() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Scenario Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the Scenario Commands you can use with PrototonBot!\nThis server's prefix is set to: `{svr.Prefix}`");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("Scenario Commands", "•**8ball**\nGet a random answer to your question from an 8ball.\n•**mood**\nPrint out a completely random mood + emoji.\n•**roll**\nRoll a tabletop themed die of your choice.\n•**murdering**\nGet a scenario of the Wulfonronpa Series Cast.\n•**murdering-rp**\nGet a scenario of the Wulfonronpa RP Cast.\n•**murdering-all**\nGet a scenario of every cast listed above.", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("admin")] [Alias("7")]
      public async Task CommAdminPage() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Administrative Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"These are the Administrative you can use with PrototonBot!\n*Admins can use commands anywhere, but you must have ManageChannel permissions to use these!*\nThis server's prefix is set to: `{svr.Prefix}`");
        embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
        embed.WithFooter("Thank you for using PrototonBot!");
        embed.AddField("Administrative Commands", "•**enable** (*enablechannel*, *addchannel*)\nTag a channel to enable for commands from users.\n•**disable** (*disablechannel*, *removechannel*)\nTag a channel to disable for commands from users.\n•**logchannel** (*log*, *logging*)\nSet a channel for important bot related messages and alerts.\n•**prefix** (*setprefix*, *changeprefix*)\nChange this server's prefix for commands.\n•**privacy**\nChoose whether or not this server is considered public or private. (Features TBA)\n•**levelups** (*levelupmessages*, *levelmessages*)\nEnable or Disable level up messages on or off for this server.\n•**togglewelcomes**\nEnable or Disable welcome messages. Disabled by default.\n•**welcomechannel**\nSet a channel for welcome messages.\n•**massrolecheck**\nGet a list of the top 15 users in your server with the most roles.\n•**purge** (*prune*)\nPurge 1-100 messages in a channel.", true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("simplified")] [Alias("simple", "list")]
      public async Task SimplifiedCommands() {
        var embed = new EmbedBuilder();
        var svr = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
        embed.WithTitle("PrototonBot Commands");
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"Server Prefix: `{svr.Prefix}`");
        embed.AddField("**General Commands**", "`info` `serverinfo` `help` `simonsays`");
        embed.AddField("**Fun  Commands**", "`hug` `nuzzle` `ship` `loveme` `noticeme` `coinflip` `punish` `lewdpunish` `yeet` `lick` `gamble` `she`");
        embed.AddField("**Profile  Commands**", "`profile` `wallet` `inventory` `daily` `pat` `transfer` `setprofile` `leaderboard` `partnerup` `partnerdown`");
        embed.AddField("**Economy  Commands**", "`store` `upgrade` `buy` `chopdown` `mine` `salvage`");
        embed.AddField("**Media  Commands**", "`hope` `despair` `hope2` `despair2` `yasqueen` `yasqueen2` `kamakura` `explode`");
        embed.AddField("**Scenario  Commands**", "`8ball` `mood` `roll` `murdering` `murdering-rp` `murdering-all`");
        embed.AddField("**Admin  Commands**", "`enable` `disable` `logchannel` `prefix` `privacy` `levelups` `togglewelcomes` `welcomechannel` `massrolecheck` `purge`");
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }
    }
  }
}
