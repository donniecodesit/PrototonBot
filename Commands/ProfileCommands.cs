using Discord;
using Discord.Commands;
using ImageMagick;
using MongoDB.Driver;
using PrototonBot.MongoUtil;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PrototonBot.Commands
{

  public class ProfileCommands : ModuleBase<SocketCommandContext>
  {
    [Command("profile-embed")]
    public async Task ProfileEmbed(string userCalled = null)
    {
      UserObject user;
      InventoryObject inv;
      if (userCalled == null)
      {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      }
      else
      {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
      }
      if (user == null || inv == null)
      {
        await Context.Channel.SendMessageAsync($"<@{Context.User.Id}> sorry about this, but that user has no data yet. They need to have talked in a server I'm present in first.");
        return;
      }

      double multiplier = 1;
      if (user.Boosted || user.Mutuals) multiplier = 1.05;
      if (user.Boosted && user.Mutuals) multiplier = 1.1;

      var embed = new EmbedBuilder();
      if (UtilityHelper.IsUserDeveloper(user.Id))
      {
        embed.WithTitle($":star2: {user.Name}'s PrototonBot Profile :star2:");
        embed.WithColor(0xFFFF00);
        embed.WithDescription($"This user is a developer for PrototonBot!\n{user.Description}");
      }
      else
      {
        embed.WithTitle(user.Name);
        embed.WithColor(0xFF00FF);
        embed.WithDescription($"{user.Description}");
      }

      embed.WithThumbnailUrl(Context.Guild.GetUser(Convert.ToUInt64(user.Id.ToString())).GetAvatarUrl());
      embed.WithFooter("Boosted & Mutual Partners both provide a 5% boost to Chat EXP and Protobucks.");
      embed.AddField(":moneybag: Protobucks", user.Money, true);
      embed.AddField(":game_die: Level & EXP", $"**{user.Level}** ({user.EXP})", true);
      embed.AddField(":speak_no_evil: Pats Received", user.PatsReceived, true);
      embed.AddField(":money_with_wings: Items Bought", user.Purchases, true);

      if (user.Partner != "None")
      {
        var partner = MongoHelper.GetUser(user.Partner).Result;
        embed.AddField(":knife: Crime Partner", partner.Name, true);
      }
      else
      {
        embed.AddField(":knife: Crime Partner", "None", true);
      }

      embed.AddField(":busts_in_silhouette: Mutual Partners", user.Mutuals, true);
      embed.AddField("Coins", $"Daily Coins: **{inv.DailyCoins}**/{inv.DailyCoinsTotal}\nPat Coins: **{inv.PatCoins}**/{inv.PatCoinsTotal}\nGamble Coins: **{inv.GambleCoins}**/{inv.GambleCoinsTotal}\nHug Coins: **{inv.HugCoins}**/{inv.HugCoinsTotal}", true);
      embed.AddField(":money_mouth: Gambles", $"Times: {user.Gambles}\nWins: {user.GamblesWon} *({user.GamblesNetGain})*\nLosses: {user.GamblesLost} *({user.GamblesNetLoss})*", true);
      embed.AddField(":arrow_double_up: Stats", $"Luck: {user.Luck}\nDaily Bonus: {user.DailyBonus}", true);
      embed.AddField("Transfers", $"Received: {user.TransferIn}\nSent: {user.TransferOut}", true);
      embed.AddField(":chart_with_upwards_trend: PrototonBot Boosted", $"{user.Boosted} ({multiplier}x)", true);

      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    // Image read settings (and images) are never modified, unlike text settings.
    private static readonly MagickReadSettings ImageReadSettings = new MagickReadSettings
    {
      BackgroundColor = MagickColors.Transparent
    };

    // We also do not need to load most of these composing images every time, so they are declared here. Make sure you .Clone() before use!
    private static MagickImage ProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "background.png"), ImageReadSettings);
    private static MagickImage ProfileBackgroundHole = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "background_hole.png"), ImageReadSettings);
    private static MagickImage ProfileMask = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "profile_mask.png"), ImageReadSettings);
    private static MagickImage DeveloperOverlay = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "developer_overlay.png"), ImageReadSettings);
    private static MagickImage NormalOverlay = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "normal_overlay.png"), ImageReadSettings);
    private static MagickImage ExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "experience_bar.png"), ImageReadSettings);
    private static MagickImage ExperienceBarOutline = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "experience_bar_outline.png"), ImageReadSettings);

    [Command("profile")]
    [Alias("currency", "bank", "account", "me", "money")]
    public async Task ProfileNew(string userCalled = null)
    {
      UserObject user;
      InventoryObject inv;
      Directory.CreateDirectory(Program.CacheDir);
      var webClient = new WebClient();

      if (userCalled == null)
      {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      }
      else
      {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
      }
      if (user == null || inv == null)
      {
        await Context.Channel.SendMessageAsync($"<@{Context.User.Id}> sorry about this, but that user has no data yet. They need to have talked in a server I'm present in first.");
        return;
      }

      // Text read settings are modified, so they must be re-initialized every time.
      var textSettings = new MagickReadSettings
      {
        BackgroundColor = MagickColors.Transparent,
        FillColor = MagickColors.White,
        FontFamily = "Yeon Sung",
        FontStyle = FontStyleType.Normal,
        TextGravity = Gravity.Center,
        Width = 618,
        Height = 83,
        TextInterwordSpacing = 8D
      };

      // Profile photo is the only user-specific variable we use. Delete the old one, and get fresh ones.
      var discordUser = Context.Guild.GetUser(Convert.ToUInt64(user.Id));
      webClient.DownloadFile($"https://cdn.discordapp.com/avatars/{discordUser.Id}/{discordUser.AvatarId}.png?size=512", Path.Combine(Program.CacheDir, $"{user.Id}.png"));
      var userPhoto = new MagickImage(Path.Combine(Program.CacheDir, $"{user.Id}.png"), ImageReadSettings);

      // Our canvas starts with a clone of the basic background.
      var canvas = ProfileBackground.Clone();
      // We modify the text settings, so we copy our defaults before modifications.

      userPhoto.Resize(317, 317);
      userPhoto.Composite(ProfileMask, Channels.Alpha);

      // Composite the user's icon onto the canvas.
      canvas.Composite(userPhoto, 16, 16, CompositeOperator.Over);

      // Composite the background over the current canvas to temporarily fix lower-right PFP masking failure.
      canvas.Composite(ProfileBackgroundHole, 0, 0, CompositeOperator.Over);

      // Composite the appropriate overlay.
      canvas.Composite(UtilityHelper.IsUserDeveloper(user.Id) ? DeveloperOverlay : NormalOverlay, CompositeOperator.Over);

      // Composite the user's name and discriminator.
      canvas.Composite(new MagickImage($"caption:{discordUser.Username}#{discordUser.Discriminator}", textSettings), 341, 19, CompositeOperator.Over);

      // Calculate experience stuff.
      var expForCurrent = ((20d * user.Level) * ((31d * user.Level) - 17d)) / 3d;
      var expForNext = ((20d * (user.Level + 1d)) * ((31d * (user.Level + 1d)) - 17d)) / 3d;
      var expToNext = expForNext - user.EXP;
      var expOffset = user.EXP - expForCurrent;
      var expPercent = expOffset / (expForNext - expForCurrent);

      // Clone, crop, and composite the experience bar on.
      var userExperience = ExperienceBar.Clone();
      userExperience.Crop((int) (ExperienceBar.Width * expPercent), ExperienceBar.Height);
      canvas.Composite(userExperience, 234, 286, CompositeOperator.Over);

      // Write text for the experience bars.
      textSettings.Width = 159;
      textSettings.Height = 43;
      canvas.Composite(new MagickImage($"caption:Level: {user.Level}", textSettings), 275, 287, CompositeOperator.Over);
      textSettings.Width = 199;
      textSettings.Height = 27;
      textSettings.TextGravity = Gravity.East;
      canvas.Composite(new MagickImage($"caption:To Level {user.Level + 1}: {(int) expToNext}xp", textSettings), 727, 304, CompositeOperator.Over);

      // Composite the experience bar overlay.
      canvas.Composite(ExperienceBarOutline, CompositeOperator.Over);

      // Adjust text size based on description length.
      if (user.Description.Length >= 0 && user.Description.Length < 121) textSettings.FontPointsize = 36;
      else textSettings.FontPointsize = 18;
      textSettings.TextGravity = Gravity.Center;
      textSettings.Width = 655;
      textSettings.Height = 154;

      // Composite description text onto the profile.
      canvas.Composite(new MagickImage($"caption:{user.Description}", textSettings), 317, 111, CompositeOperator.Over);

      // Adjust text for left side entries.
      textSettings.TextGravity = Gravity.West;
      textSettings.Width = 477;
      textSettings.Height = 39;

      // Composite text for left side entries.
      canvas.Composite(new MagickImage($"caption:{user.Money}", textSettings), 99, 389, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:{user.PatsReceived}", textSettings), 99, 483, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:{user.Purchases}", textSettings), 99, 572, CompositeOperator.Over);

      // Check if user has a partner, and composite the appropriate text.
      canvas.Composite(user.Partner != "None"
          ? new MagickImage($"caption:{MongoHelper.GetUser(user.Partner).Result.Name}", textSettings)
          : new MagickImage($"caption:Nobody!", textSettings)
          , 99, 660, CompositeOperator.Over);

      // Check if user has a mutual, and composite the appropriate text.
      canvas.Composite(user.Mutuals
          ? new MagickImage($"caption:Yes! (+5% $+XP)", textSettings)
          : new MagickImage($"caption:No!", textSettings)
          , 99, 751, CompositeOperator.Over);

      // Continue left side entries.
      canvas.Composite(new MagickImage($"caption:Luck: {user.Luck}", textSettings), 99, 848, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:DailyBonus: {user.DailyBonus}", textSettings), 99, 891, CompositeOperator.Over);

      // Adjust text for right side entries.
      textSettings.TextGravity = Gravity.Northeast;

      // Composite text for right side entries.
      canvas.Composite(new MagickImage($"caption:Times: {user.Gambles}", textSettings), 426, 389, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Wins: {user.GamblesWon} ({user.GamblesNetGain})", textSettings), 426, 437, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Losses: {user.GamblesLost} ({user.GamblesNetLoss})", textSettings), 426, 485, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:DC: {inv.DailyCoins} / {inv.DailyCoinsTotal}", textSettings), 426, 572, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:PC: {inv.PatCoins} / {inv.PatCoinsTotal}", textSettings), 426, 620, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:GC: {inv.GambleCoins} / {inv.GambleCoinsTotal}", textSettings), 426, 668, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:HC: {inv.HugCoins} / {inv.HugCoinsTotal}", textSettings), 426, 716, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Received: {user.TransferIn}", textSettings), 426, 797, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Sent: {user.TransferOut}", textSettings), 426, 845, CompositeOperator.Over);

      canvas.Composite(user.Boosted
          ? new MagickImage($"caption:Boosted: Yes! (+5% $+XP)", textSettings)
          : new MagickImage($"caption:Boosted: No!", textSettings)
          , 426, 893, CompositeOperator.Over);

      canvas.Write(Path.Combine(Program.CacheDir, $"{user.Id}_out.png"));
      await Context.Channel.SendFileAsync(Path.Combine(Program.CacheDir, $"{user.Id}_out.png"));
    }

    [Command("bag")]
    [Alias("inventory", "bags", "items")]
    public async Task BagEmbed(string userCalled = null)
    {
      UserObject user;
      InventoryObject inv;
      if (userCalled == null)
      {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      }
      else
      {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
      }
      if (user == null || inv == null)
      {
        await Context.Channel.SendMessageAsync($"<@{Context.User.Id}> sorry about this, but that user has no data yet. They need to have talked in a server I'm present in first.");
        return;
      }
      var embed = new EmbedBuilder();
      if (UtilityHelper.IsUserDeveloper(user.Id))
      {
        embed.WithTitle($":star2: {user.Name}'s PrototonBot Bag :star2:");
        embed.WithColor(0xFFFF00);
      }
      else
      {
        embed.WithTitle($"{user.Name}'s PrototonBot Bag");
        embed.WithColor(0xFF00FF);
      }
      
      embed.WithThumbnailUrl(Context.Guild.GetUser(Convert.ToUInt64(user.Id.ToString())).GetAvatarUrl());
      embed.WithFooter("Numbers in parenthesis are how many uses that item has left.");
      embed.AddField(":pick:", $"{inv.Picks} ({inv.PickUses})", true);
      embed.AddField(":gem:", $"{inv.Diamonds}", true);
      embed.AddField(":bricks:", $"{inv.Bricks}", true);
      embed.AddField(":wrench:", $"{inv.Wrenches} ({inv.WrenchUses})", true);
      embed.AddField(":nut_and_bolt:", $"{inv.Bolts}", true);
      embed.AddField(":gear:", $"{inv.Gears}", true);
      embed.AddField(":axe:", $"{inv.Axes} ({inv.AxeUses})", true);
      embed.AddField(":evergreen_tree:", $"{inv.Logs}", true);
      embed.AddField(":fallen_leaf:", $"{inv.Leaves}", true);
      embed.AddField(":paperclip:", $"{inv.Paperclips}", true);
      embed.AddField(":bulb:", $"{inv.Bulbs}", true);
      embed.AddField(":cd:", $"{inv.CDs}", true);
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    [Command("daily")]
    public async Task DailyRedemption()
    {
      UserObject user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      InventoryObject inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

      if (user.LastDaily > (currentTime - 86400))
      {
        long secsRemaining = ((user.LastDaily + 86400) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Hours + " hour(s), " + spanOfTime.Minutes + " minute(s), and " + spanOfTime.Seconds + " second(s)!";


        var dailyRejected = new EmbedBuilder();
        dailyRejected.WithColor(0xFF00FF);
        dailyRejected.AddField("Daily Already Claimed!", $"<@{user.Id}>, You've already claimed your daily today!\nGet your next one in {str}");
        dailyRejected.WithThumbnailUrl(Context.User.GetAvatarUrl());

        await Context.Channel.SendMessageAsync("", false, dailyRejected.Build());
        return;
      }

      //Add 500 with the User's Daily Bonus, and Last Dailies
      await MongoHelper.UpdateUser(user.Id, "Money", (user.Money + 500 + user.DailyBonus));
      await MongoHelper.UpdateUser(user.Id, "LastDaily", currentTime);
      await MongoHelper.UpdateInventory(user.Id, "DailyCoins", (inv.DailyCoins + 1));
      await MongoHelper.UpdateInventory(user.Id, "DailyCoinsTotal", (inv.DailyCoinsTotal + 1));

      var dailySuccess = new EmbedBuilder();
      dailySuccess.WithColor(0xFF00FF);
      dailySuccess.AddField("Daily Claimed!", $"<@{user.Id}>, you have received **{500 + user.DailyBonus}** Protobucks as a daily reward!\nYou now have **{user.Money + 500 + user.DailyBonus}** Protobucks, and received 1 Daily Coin.");
      dailySuccess.WithThumbnailUrl(Context.User.GetAvatarUrl());

      await Context.Channel.SendMessageAsync("", false, dailySuccess.Build());
    }

    [Command("pat")]
    [Alias("pet")]
    public async Task PatRedemption(string userCalled = null)
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
        await Context.Channel.SendMessageAsync($"Don't you think it'd be unfair to give yourself a pat? You have arms after all, just pat yourself on the back! <@{Context.User.Id}>");
        return;
      }

      var taggedUsr = MongoHelper.GetUser(filteredId).Result;
      var authorUsr = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var authorInv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      if (authorUsr == null || taggedUsr == null || authorInv == null)
      {
        await Context.Channel.SendMessageAsync($"<@{Context.User.Id}> sorry about this, but that user has no data yet. They need to have talked in a server I'm present in first.");
        return;
      }
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      if (authorUsr.LastPat > (currentTime - 86400))
      {
        long secsRemaining = ((authorUsr.LastPat + 86400) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Hours + " hours, " + spanOfTime.Minutes + " minutes, and " + spanOfTime.Seconds + " seconds!";

        var dailyPatRejected = new EmbedBuilder();
        dailyPatRejected.WithColor(0xFF00FF);
        dailyPatRejected.AddField("Pat Already Claimed!", $"<@{authorUsr.Id}>, you've already given a pat today!\nYou can give another one in {str}");
        await Context.Channel.SendMessageAsync("", false, dailyPatRejected.Build());
        return;
      }

      //Add 1PC, 1PCT, and lastdaily to author
      await MongoHelper.UpdateUser(authorUsr.Id, "LastPat", currentTime);
      await MongoHelper.UpdateInventory(authorUsr.Id, "PatCoins", (authorInv.PatCoins + 1));
      await MongoHelper.UpdateInventory(authorUsr.Id, "PatCoinsTotal", (authorInv.PatCoinsTotal + 1));
      //Add 1 pat received to tagged user
      await MongoHelper.UpdateUser(taggedUsr.Id, "PatsReceived", (taggedUsr.PatsReceived + 1));

      var PatSuccess = new EmbedBuilder();
      PatSuccess.WithColor(0xFF00FF);
      PatSuccess.AddField("Pat Success!", $"{taggedUsr.Name} has received a pat! <3\n{authorUsr.Name} also received 1 Pat Coin!");
      await Context.Channel.SendMessageAsync("", false, PatSuccess.Build());
    }

    [Command("wallet")]
    public async Task WalletEmbed(string userCalled = null)
    {
      UserObject user;
      InventoryObject inv;
      if (userCalled == null)
      {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      }
      else
      {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
      }
      if (user == null || inv == null)
      {
        await Context.Channel.SendMessageAsync($"<@{Context.User.Id}> sorry about this, but that user has no data yet. They need to have talked in a server I'm present in first.");
        return;
      }
      var embed = new EmbedBuilder();
      if (UtilityHelper.IsUserDeveloper(user.Id))
      {
        embed.WithTitle($":star2: {user.Name}'s Wallet :star2:");
        embed.WithColor(0xFFFF00);
      }
      else
      {
        embed.WithTitle($"{user.Name}'s Wallet");
        embed.WithColor(0xFF00FF);
      }

      embed.AddField("Account Holder", user.Name, true);
      embed.AddField("Account Balance", user.Money, true);
      embed.AddField("Pats Received", user.PatsReceived, true);
      embed.AddField("Level", user.Level, true);
      embed.AddField("Total Purchases", user.Purchases, true);
      embed.WithFooter("Use profile for much more detailed information.");
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    [Command("transfer")]
    [Alias("send")]
    public async Task TransferProtobucks(string userCalled = null, string moneyToSend = null)
    {
      if (userCalled == null || moneyToSend == null)
      {
        await Context.Channel.SendMessageAsync("Make sure to tag the recipient and then specify the amount to send!");
        return;
      }

      UserObject user;
      UserObject author;
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      user = MongoHelper.GetUser(filteredId).Result;
      author = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      if (user == null)
      {
        await Context.Channel.SendMessageAsync($"<@{Context.User.Id}> sorry about this, but that user has no data yet. They need to have talked in a server I'm present in first.");
        return;
      }

      long moneySending = 0;

      try
      {
        moneySending = Convert.ToInt64(moneyToSend);
      }
      catch (FormatException)
      {
        await Context.Channel.SendMessageAsync("Please make sure the money you're sending is a valid number!");
        return;
      }
      if (moneySending <= 0)
      {
        await Context.Channel.SendMessageAsync("Please make sure the money you're sending is a valid number!");
        return;
      }

      if (user.Id == Context.Message.Author.Id.ToString())
      {
        await Context.Channel.SendMessageAsync("You can't send money to yourself, silly!");
        return;
      }

      if (moneySending > author.Money)
      {
        await Context.Channel.SendMessageAsync("Baka! You don't even have that much money to send!");
        return;
      }

      //author loses money and gains transfer out
      await MongoHelper.UpdateUser(author.Id, "Money", (author.Money - moneySending));
      await MongoHelper.UpdateUser(author.Id, "TransferOut", (author.TransferOut + moneySending));
      //recipient receives the money and gains transfer in
      await MongoHelper.UpdateUser(user.Id, "Money", (user.Money + moneySending));
      await MongoHelper.UpdateUser(user.Id, "TransferIn", (user.TransferIn + moneySending));

      await Context.Channel.SendMessageAsync($">>> Beep boop, boop bop! *Success!*\n{Context.User.Username}'s Protobucks: {author.Money} --> {author.Money - moneySending}\n{user.Name}'s Protobucks: {user.Money} --> {user.Money + moneySending}");
      return;
    }

    [Command("partnerup")]
    [Alias("setpartner", "partner", "marry")]
    public async Task SetPartner(string userCalled = null)
    {
      UserObject author;
      UserObject partner;
      UserObject currentPartner;
      if (userCalled == null)
      {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;


      author = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      partner = MongoHelper.GetUser(filteredId).Result;

      if (partner == null)
      {
        await Context.Channel.SendMessageAsync($"<@{author.Id}> sorry about this, but that user has no data yet. They need to have talked in a server I'm present in first.");
        return;
      }

      if (author.Partner == filteredId)
      {
        await Context.Channel.SendMessageAsync($"<@{author.Id}>, hey hey! {partner.Name} is already your partner!");
        return;
      }

      if (filteredId == Context.Message.Author.Id.ToString())
      {
        await Context.Channel.SendMessageAsync("How can you be your ***own partner*** in crime?");
        return;
      }

      if (author.Partner == "None")
      {
        await MongoHelper.UpdateUser(author.Id, "Partner", filteredId);
        if (partner.Partner == Context.User.Id.ToString())
        {
          await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
          await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);
          await Context.Channel.SendMessageAsync($"Awe, nice! You and {partner.Name} are now mutual partners!");
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Awe, nice! {partner.Name} is now your partner!");
        }
      }

      if (author.Partner != "None")
      {
        currentPartner = MongoHelper.GetUser(author.Partner).Result;
        await MongoHelper.UpdateUser(author.Id, "Partner", filteredId);


        if (partner.Partner == Context.User.Id.ToString())
        {
          await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
          await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);
          await Context.Channel.SendMessageAsync($"Awe, nice! You and {partner.Name} are now mutual partners!");
        }

        if (currentPartner.Partner == Context.User.Id.ToString())
        {
          await MongoHelper.UpdateUser(currentPartner.Id, "Partner", "None");
          await MongoHelper.UpdateUser(currentPartner.Id, "Mutuals", false);
          await MongoHelper.UpdateUser(author.Id, "Mutuals", false);
          if (partner.Partner == author.Id)
          {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your mutual partner!");
            await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
            await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);

          }
          else
          {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your partner!");
          }
        }

        if (partner.Partner != Context.User.Id.ToString() && currentPartner.Partner != Context.User.Id.ToString())
        {
          await Context.Channel.SendMessageAsync($"Sad to see em go... {currentPartner.Name} is no longer your partner, but {partner.Name} is now your partner!");
          if (partner.Partner == author.Id)
          {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your mutual partner!");
            await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
            await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);
          }
          else
          {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your partner!");
          }
        }
        return;
      }
      return;
    }

    [Command("removepartner")]
    [Alias("partnerdown", "divorce", "unpartner")]
    public async Task RemovePartner()
    {
      UserObject author = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      if (author.Partner == "None")
      {
        await Context.Channel.SendMessageAsync("You don't have a partner to remove!");
        return;
      }
      UserObject partner = MongoHelper.GetUser(author.Partner).Result;
      await MongoHelper.UpdateUser(author.Id, "Partner", "None");
      await MongoHelper.UpdateUser(author.Id, "Mutuals", false);
      if (partner.Partner == author.Id)
      {
        await MongoHelper.UpdateUser(partner.Id, "Partner", "None");
        await MongoHelper.UpdateUser(partner.Id, "Mutuals", false);
        await Context.Channel.SendMessageAsync($"Awe, you and {partner.Name} are no longer partners.");
      }
      else
      {
        await Context.Channel.SendMessageAsync($"Sad to see em go... {partner.Name} is no longer your partner.");
      }
      return;
    }

    [Command("setprofile")]
    [Alias("description", "profiledesc", "setdesc")]
    public async Task SetDescription([Remainder] string input = null)
    {
      UserObject user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      InventoryObject inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      if (input == null)
      {
        await Context.Channel.SendMessageAsync($"<@{user.Id}> you may type a message 240 characters or less to use as a description, or type ``remove``, ``none`` or ``clear`` after the command to clear your description.");
        return;
      }
      if (input == "remove" || input == "none" || input == "clear")
      {
        if (user.Description == "This user has not set a description.")
        {
          await Context.Channel.SendMessageAsync($"You haven't set a description to be removed, <@{user.Id}>!");
          return;
        }
        await Context.Channel.SendMessageAsync($"Oh, you wanted your description removed? Okay, sure thing, your previous description below will be removed. <@{user.Id}>\n> {user.Description}");
        await MongoHelper.UpdateUser(user.Id, "Description", "This user has not set a description.");
        return;
      }
      else
      {
        if (input.Count() > 240)
        {
          await Context.Channel.SendMessageAsync($"Your description is too long! <@{user.Id}>, the maximum amount of characters is 240.\nYour character count was: {input.Count()}/240");
          return;
        }
        else
        {
          await Context.Channel.SendMessageAsync($"Alright, done! Your description has been updated, <@{user.Id}>!\n> {input}");
          await MongoHelper.UpdateUser(user.Id, "Description", input);
          return;
        }

      }
    }

    [Group("leaderboard")]
    [Alias("top")]
    [RequireContext(ContextType.Guild)]
    public class LeaderBoard : ModuleBase<SocketCommandContext>
    {
      [Command]
      public async Task Default()
      {
        await Context.Channel.SendMessageAsync("Please specify either ``Money``/``Protobucks`` or ``Level``/``Levels``.");
      }

      [Command("money")]
      [Alias("Protobucks")]
      public async Task MoneyBoard()
      {
        var embed = new EmbedBuilder();
        string userlist = "";
        var index = 0;
        var TotalUsers = MongoHelper.GetTotalUserCount().Result.Count();

        embed.WithTitle("PrototonBot Protobucks Leaderboard");
        embed.WithColor(0xFF00FF);
        foreach (var obj in MongoHelper.GetLeaderboardTopMoney().Result)
        {
          index++;
          userlist += $"\n{index} - **{obj.Name}** with **{obj.Money}** Protobucks.";
        }
        embed.AddField("Richest Users", userlist, true);
        embed.AddField("Total Users", TotalUsers, true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("level")]
      [Alias("levels")]
      public async Task LevelBoard()
      {
        var embed = new EmbedBuilder();
        string userlist = "";
        var index = 0;
        var TotalUsers = MongoHelper.GetTotalUserCount().Result.Count();

        embed.WithTitle("PrototonBot Level Leaderboard");
        embed.WithColor(0xFF00FF);
        foreach (var obj in MongoHelper.GetLeaderboardTopLevels().Result)
        {
          index++;
          userlist += $"\n{index} - **{obj.Name}** who is level **{obj.Level}**.";
        }
        embed.AddField("Richest Users", userlist, true);
        embed.AddField("Total Users", TotalUsers, true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }
    }

  }
}
