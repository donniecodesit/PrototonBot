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
using Discord.WebSocket;

namespace PrototonBot.Commands {
  public class ProfileCommands : ModuleBase<SocketCommandContext> {
    [Command("profile-embed")]
    public async Task ProfileEmbed(string userCalled = null) {
      UserObject user;
      InventoryObject inv;
      if (userCalled == null) {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      } else {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
        if (user == null || inv == null) {
          var userPost = Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result;
          await MongoHelper.CreateUser(userPost);
          user = MongoHelper.GetUser(filteredId).Result;
          inv = MongoHelper.GetInventory(filteredId).Result;
        }
      }

      double multiplier = 1;
      if (user.Boosted || user.Mutuals) multiplier = 1.05;
      if (user.Boosted && user.Mutuals) multiplier = 1.1;
      var embed = new EmbedBuilder();
      if (UtilityHelper.IsUserDeveloper(user.Id)) {
        embed.WithTitle($":star2: {user.Name}'s PrototonBot Profile :star2:");
        embed.WithColor(0xB285FE);
        embed.WithDescription($"This user is a developer for PrototonBot!\n{user.Description}");
      } else {
        embed.WithTitle(user.Name);
        embed.WithColor(0xB2A2F1);
        embed.WithDescription($"{user.Description}");
      }
      embed.WithThumbnailUrl(Context.Client.GetUserAsync(Convert.ToUInt64(user.Id.ToString())).Result.GetAvatarUrl());
      embed.WithFooter("Boosted & Mutual Partners both provide a 5% boost to Chat EXP and Protobucks.");
      embed.AddField(":moneybag: Protobucks", user.Money, true);
      embed.AddField(":game_die: Level & EXP", $"**{user.Level}** ({user.EXP})", true);
      embed.AddField(":speak_no_evil: Pats Received", user.PatsReceived, true);
      embed.AddField(":money_with_wings: Items Bought", user.Purchases, true);

      if (user.Partner != "None") {
        var partner = MongoHelper.GetUser(user.Partner).Result;
        embed.AddField(":knife: Crime Partner", partner.Name, true);
      } else {
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
    private static readonly MagickReadSettings ImageReadSettings = new MagickReadSettings {
      BackgroundColor = MagickColors.Transparent
    };

    // We also do not need to load most of these composing images every time, so they are declared here. Make sure you .Clone() before use!
    private static MagickImage DeveloperBox = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "DEVBOX.png"), ImageReadSettings);

    // Red Theme
    private static MagickImage RedProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "RedTheme", "BACKGROUND.png"), ImageReadSettings);
    private static MagickImage RedProfileBoxes = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "RedTheme", "BOXES.png"), ImageReadSettings);
    private static MagickImage RedExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "RedTheme", "EXPBAR.png"), ImageReadSettings);

    // Yellow Theme
    private static MagickImage YellowProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "YellowTheme", "BACKGROUND.png"), ImageReadSettings);
    private static MagickImage YellowProfileBoxes = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "YellowTheme", "BOXES.png"), ImageReadSettings);
    private static MagickImage YellowExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "YellowTheme", "EXPBAR.png"), ImageReadSettings);

    // Green Theme
    private static MagickImage GreenProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "GreenTheme", "BACKGROUND.png"), ImageReadSettings);
    private static MagickImage GreenProfileBoxes = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "GreenTheme", "BOXES.png"), ImageReadSettings);
    private static MagickImage GreenExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "GreenTheme", "EXPBAR.png"), ImageReadSettings);

    // Blue Theme
    private static MagickImage BlueProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "BlueTheme", "BACKGROUND.png"), ImageReadSettings);
    private static MagickImage BlueProfileBoxes = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "BlueTheme", "BOXES.png"), ImageReadSettings);
    private static MagickImage BlueExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "BlueTheme", "EXPBAR.png"), ImageReadSettings);

    // Default/Purple Theme
    private static MagickImage DefaultProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "PurpleTheme", "BACKGROUND.png"), ImageReadSettings);
    private static MagickImage DefaultProfileBoxes = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "PurpleTheme", "BOXES.png"), ImageReadSettings);
    private static MagickImage DefaultExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "PurpleTheme", "EXPBAR.png"), ImageReadSettings);

    // Pink Theme
    private static MagickImage PinkProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "PinkTheme", "BACKGROUND.png"), ImageReadSettings);
    private static MagickImage PinkProfileBoxes = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "PinkTheme", "BOXES.png"), ImageReadSettings);
    private static MagickImage PinkExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "PinkTheme", "EXPBAR.png"), ImageReadSettings);
    
    // Black Theme
    private static MagickImage BlackProfileBackground = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "BlackTheme", "BACKGROUND.png"), ImageReadSettings);
    private static MagickImage BlackProfileBoxes = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "BlackTheme", "BOXES.png"), ImageReadSettings);
    private static MagickImage BlackExperienceBar = new MagickImage(Path.Combine("Storage", "ProfileImageAssets", "BlackTheme", "EXPBAR.png"), ImageReadSettings);

    [Command("profile")] [Alias("currency", "bank", "account", "me", "money")]
    public async Task ProfileNew(string userCalled = null) {
      UserObject user;
      InventoryObject inv;
      Directory.CreateDirectory(Program.CacheDir);
      var webClient = new WebClient();

      // Pull information about the author, or the user tagged.
      if (userCalled == null) {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      } else {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
        if (user == null || inv == null) {
          var userPost = Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result;
          await MongoHelper.CreateUser(userPost);
          user = MongoHelper.GetUser(filteredId).Result;
          inv = MongoHelper.GetInventory(filteredId).Result;
        }
      }

      // Text read settings are modified, so they must be re-initialized every time.
      var textSettings = new MagickReadSettings {
        BackgroundColor = MagickColors.Transparent,
        FillColor = MagickColors.White,
        FontFamily = "Yeon Sung",
        FontStyle = FontStyleType.Normal,
        TextGravity = Gravity.Center,
        Width = 618,
        Height = 83,
        TextInterwordSpacing = 8D
      };

      // Fetch the user's profile photo.
      var discordUser = Context.Client.GetUserAsync(Convert.ToUInt64(user.Id)).Result;
      webClient.DownloadFile($"https://cdn.discordapp.com/avatars/{discordUser.Id}/{discordUser.AvatarId}.png?size=512", Path.Combine(Program.CacheDir, $"{user.Id}.png"));
      var userPhoto = new MagickImage(Path.Combine(Program.CacheDir, $"{user.Id}.png"), ImageReadSettings);
      userPhoto.Resize(185, 185);

      // LAYER 1: Background
      var canvas = inv.PickedTheme switch {
        "red" => RedProfileBackground.Clone(),
        "yellow" => YellowProfileBackground.Clone(),
        "green" => GreenProfileBackground.Clone(),
        "blue" => BlueProfileBackground.Clone(),
        "pink" => PinkProfileBackground.Clone(),
        "black" => BlackProfileBackground.Clone(),
        _ => DefaultProfileBackground.Clone()
      };

      // Calculate User Experience for the bar
      var expForCurrent = (user.Level == 0) ? 0 : ((20d * user.Level) * ((31d * user.Level) - 17d)) / 3d;
      var expForNext = ((20d * (user.Level + 1d)) * ((31d * (user.Level + 1d)) - 17d)) / 3d;
      var expToNext = expForNext - user.EXP;
      var expOffset = user.EXP - expForCurrent;
      var expPercent = expOffset / (expForNext - expForCurrent);
      if (expPercent == 0) expPercent = 0.01;

      // LAYER 2: Experience Bar
      var experienceBar = inv.PickedTheme switch {
        "red" => RedExperienceBar.Clone(),
        "yellow" => YellowExperienceBar.Clone(),
        "green" => GreenExperienceBar.Clone(),
        "blue" => BlueExperienceBar.Clone(),
        "pink" => PinkExperienceBar.Clone(),
        "black" => BlackExperienceBar.Clone(),
        _ => DefaultExperienceBar.Clone()
      };
      experienceBar.Crop((int) (experienceBar.Width * expPercent), experienceBar.Height);
      canvas.Composite(experienceBar, 217, 292, CompositeOperator.Over);

      // LAYER 3: User Icon
      canvas.Composite(userPhoto, 52, 40, CompositeOperator.Over);

      // LAYER 4: Developer Box if user is developer
      if (UtilityHelper.IsUserDeveloper(user.Id)) canvas.Composite(DeveloperBox, CompositeOperator.Over);

      // LAYER 5: Container Boxes
      var boxesOverlay = inv.PickedTheme switch {
        "red" => RedProfileBoxes.Clone(),
        "yellow" => YellowProfileBoxes.Clone(),
        "green" => GreenProfileBoxes.Clone(),
        "blue" => BlueProfileBoxes.Clone(),
        "pink" => PinkProfileBoxes.Clone(),
        "black" => BlackProfileBoxes.Clone(),
        _ => DefaultProfileBoxes.Clone()
      };
      canvas.Composite(boxesOverlay, CompositeOperator.Over);

      // LAYER 6: Username and Description
      textSettings.Width = 680;
      textSettings.Height = 96;
      textSettings.TextGravity = Gravity.Center;
      textSettings.FontPointsize = (user.Description.Length < 121) ? 32 : 16;
      canvas.Composite(new MagickImage($"caption:{user.Description}", textSettings), 275, 115, CompositeOperator.Over);

      textSettings.Height = 47;
      textSettings.FontPointsize = (user.Name.Length < 12) ? 40 : 30;
      canvas.Composite(new MagickImage($"caption:{discordUser.Username}#{discordUser.Discriminator}", textSettings), 275, 55, CompositeOperator.Over);

      // LAYER 7: Level Area
      textSettings.Width = 75;
      textSettings.Height = 48;
      textSettings.FontPointsize = 34;
      textSettings.TextGravity = Gravity.Southwest;
      canvas.Composite(new MagickImage($"caption:Level", textSettings), 46, 293, CompositeOperator.Over);

      textSettings.Width = 88;
      textSettings.Height = 60;
      textSettings.FontPointsize = 56;
      textSettings.TextGravity = Gravity.Southeast;
      canvas.Composite(new MagickImage($"caption:{user.Level}", textSettings), 112, 283, CompositeOperator.Over);

      // LAYER 8: Experience Area
      textSettings.Width = 267;
      textSettings.Height = 24;
      textSettings.FontPointsize = 20;
      canvas.Composite(new MagickImage($"caption:{(long) expToNext} exp to next", textSettings), 694, 314, CompositeOperator.Over);

      // Adjust text for left side entries.
      textSettings.TextGravity = Gravity.West;
      textSettings.Width = 420;
      textSettings.Height = 40;
      textSettings.FontPointsize = 36;

      // EDIT THE POSITION OF LEFT SIDE ENTRIES >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
      // LAYER 9: Left Side Entries
      canvas.Composite(new MagickImage($"caption:Dogbucks: {user.Money}", textSettings), 85, 375, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Pats: {user.PatsReceived}", textSettings), 85, 424, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Purchases: {user.Purchases}", textSettings), 85, 473, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Stats:", textSettings), 85, 522, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Luck: {user.Luck}", textSettings), 105, 557, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Daily Bonus: {user.DailyBonus}", textSettings), 105, 587, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Mutual: {(user.Mutuals ? "Yes! ( +5% $/XP )" : "No!" )}", textSettings), 85, 669, CompositeOperator.Over);

      var userPartner = MongoHelper.GetUser(user.Partner).Result;
      textSettings.FontPointsize = (userPartner == null) ? 36 : (userPartner.Name.Length < 12 ? 36 : 26);
      canvas.Composite(new MagickImage($"caption:Partner: {(userPartner != null ? userPartner.Name : "Nobody!")}", textSettings), 85, 629, CompositeOperator.Over);

      // Adjust text for right side entries.
      textSettings.TextGravity = Gravity.East;

      //LAYER 10: Right Side Entries
      canvas.Composite(new MagickImage($"caption:Gambles, Coins, Other", textSettings), 500, 375, CompositeOperator.Over);

      textSettings.Width = 420;
      textSettings.Height = 30;
      textSettings.FontPointsize = 26;

      canvas.Composite(new MagickImage($"caption:Times: {user.Gambles}", textSettings), 500, 410, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Wins: {user.GamblesWon} ({user.GamblesNetGain})", textSettings), 500, 435, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Losses: {user.GamblesLost} ({user.GamblesNetLoss})", textSettings), 500, 460, CompositeOperator.Over);

      canvas.Composite(new MagickImage($"caption:DC: {inv.DailyCoins} ({inv.DailyCoinsTotal})", textSettings), 500, 500, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:PC: {inv.PatCoins} ({inv.PatCoinsTotal})", textSettings), 500, 525, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:GC: {inv.GambleCoins} ({inv.GambleCoinsTotal})", textSettings), 500, 550, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:HC: {inv.HugCoins} ({inv.HugCoinsTotal})", textSettings), 500, 575, CompositeOperator.Over);

      canvas.Composite(new MagickImage($"caption:Received: {user.TransferIn}", textSettings), 500, 615, CompositeOperator.Over);
      canvas.Composite(new MagickImage($"caption:Sent: {user.TransferOut}", textSettings), 500, 645, CompositeOperator.Over);

      canvas.Composite(new MagickImage($"caption:Mutual: {(user.Boosted ? "Yes! ( +5% $/XP )" : "No!" )}", textSettings), 500, 675, CompositeOperator.Over);

      canvas.Write(Path.Combine(Program.CacheDir, $"{user.Id}_out.png"));
      await Context.Channel.SendFileAsync(Path.Combine(Program.CacheDir, $"{user.Id}_out.png"));
    }

    [Command("settheme")] [Alias("changetheme", "theme")]
    public async Task SetProfileTheme(string theme = null) {
      UserObject user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      InventoryObject inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      var webClient = new WebClient();

      if (theme == null) {
        await Context.Channel.SendMessageAsync($"Please specify the theme you want to set!\nYou own: *{String.Join(", ", inv.OwnedThemes)}, and default/purple.*");
        return;
      }
      if (!inv.OwnedThemes.Contains(theme.ToLower()) && theme.ToLower() != "default" && theme.ToLower() != "purple") {
        await Context.Channel.SendMessageAsync($"Sorry, but you don't own the {theme.ToLower()} theme!");
        return;
      }

      switch (theme.ToLower()) {
        case "red": {
          await MongoHelper.UpdateInventory(user.Id, "PickedTheme", "red");
          break;
        }
        case "yellow": {
          await MongoHelper.UpdateInventory(user.Id, "PickedTheme", "yellow");
          break;
        }
        case "green": {
          await MongoHelper.UpdateInventory(user.Id, "PickedTheme", "green");
          break;
        }
        case "blue": {
          await MongoHelper.UpdateInventory(user.Id, "PickedTheme", "blue");
          break;
        }
        case "purple":
        case "default": {
          await MongoHelper.UpdateInventory(user.Id, "PickedTheme", "default");
          break;
        }
        case "pink": {
          await MongoHelper.UpdateInventory(user.Id, "PickedTheme", "pink");
          break;
        }
        case "black": {
          await MongoHelper.UpdateInventory(user.Id, "PickedTheme", "black");
          break;
        }
        default: {
          await Context.Channel.SendMessageAsync($"Please specify the theme you want to set!\nYou own: *{String.Join(", ", inv.OwnedThemes)}, and default/purple.*");
          return;
        }
      }

      await Context.Channel.SendMessageAsync($"Sounds good, your profile theme is now set to {theme.ToLower()}!");
    }

    [Command("bag")] [Alias("inventory", "bags", "items")]
    public async Task BagEmbed(string userCalled = null) {
      UserObject user;
      InventoryObject inv;
      if (userCalled == null) {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      } else {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
        if (user == null || inv == null) {
          var userPost = Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result;
          await MongoHelper.CreateUser(userPost);
          user = MongoHelper.GetUser(filteredId).Result;
          inv = MongoHelper.GetInventory(filteredId).Result;
        }
      }

      var embed = new EmbedBuilder();
      if (UtilityHelper.IsUserDeveloper(user.Id)) {
        embed.WithTitle($":star2: {user.Name}'s PrototonBot Bag :star2:");
        embed.WithColor(0xB285FE);
      } else {
        embed.WithTitle($"{user.Name}'s PrototonBot Bag");
        embed.WithColor(0xB2A2F1);
      }
      embed.WithThumbnailUrl(Context.Client.GetUserAsync(Convert.ToUInt64(user.Id.ToString())).Result.GetAvatarUrl());
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
    public async Task DailyRedemption() {
      UserObject user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      InventoryObject inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

      if (user.LastDaily > (currentTime - 86400)) {
        long secsRemaining = ((user.LastDaily + 86400) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Hours + " hour(s), " + spanOfTime.Minutes + " minute(s), and " + spanOfTime.Seconds + " second(s)!";


        var dailyRejected = new EmbedBuilder();
        dailyRejected.WithColor(0xB2A2F1);
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
      dailySuccess.WithColor(0xB2A2F1);
      dailySuccess.AddField("Daily Claimed!", $"<@{user.Id}>, you have received **{500 + user.DailyBonus}** Protobucks as a daily reward!\nYou now have **{user.Money + 500 + user.DailyBonus}** Protobucks, and received 1 Daily Coin.");
      dailySuccess.WithThumbnailUrl(Context.User.GetAvatarUrl());

      await Context.Channel.SendMessageAsync("", false, dailySuccess.Build());
    }

    [Command("pat")] [Alias("pet")]
    public async Task PatRedemption(string userCalled = null) {
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      if (filteredId == Context.Message.Author.Id.ToString()) {
        await Context.Channel.SendMessageAsync($"Don't you think it'd be unfair to give yourself a pat? You have arms after all, just pat yourself on the back! <@{Context.User.Id}>");
        return;
      }

      var taggedUsr = MongoHelper.GetUser(filteredId).Result;
      var authorUsr = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      var authorInv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      if (taggedUsr == null) {
        var userPost = Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result;
        await MongoHelper.CreateUser(userPost);
        taggedUsr = MongoHelper.GetUser(filteredId).Result;
      }
      
      var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      if (authorUsr.LastPat > (currentTime - 86400)) {
        long secsRemaining = ((authorUsr.LastPat + 86400) - currentTime);
        var spanOfTime = TimeSpan.FromSeconds(secsRemaining);
        string str = spanOfTime.Hours + " hours, " + spanOfTime.Minutes + " minutes, and " + spanOfTime.Seconds + " seconds!";

        var dailyPatRejected = new EmbedBuilder();
        dailyPatRejected.WithColor(0xB2A2F1);
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
      PatSuccess.WithColor(0xB2A2F1);
      PatSuccess.AddField("Pat Success!", $"{taggedUsr.Name} has received a pat! <3\n{authorUsr.Name} also received 1 Pat Coin!");
      await Context.Channel.SendMessageAsync("", false, PatSuccess.Build());
    }

    [Command("wallet")]
    public async Task WalletEmbed(string userCalled = null) {
      UserObject user;
      InventoryObject inv;
      if (userCalled == null) {
        user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
        inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      } else {
        var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
        if (filteredId == null) return;
        user = MongoHelper.GetUser(filteredId).Result;
        inv = MongoHelper.GetInventory(filteredId).Result;
        if (user == null || inv == null) {
          var userPost = Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result;
          await MongoHelper.CreateUser(userPost);
          user = MongoHelper.GetUser(filteredId).Result;
          inv = MongoHelper.GetInventory(filteredId).Result;
        }
      }

      var embed = new EmbedBuilder();
      if (UtilityHelper.IsUserDeveloper(user.Id)) {
        embed.WithTitle($":star2: {user.Name}'s Wallet :star2:");
        embed.WithColor(0xB285FE);
      } else {
        embed.WithTitle($"{user.Name}'s Wallet");
        embed.WithColor(0xB2A2F1);
      }

      embed.AddField("Account Holder", user.Name, true);
      embed.AddField("Account Balance", user.Money, true);
      embed.AddField("Pats Received", user.PatsReceived, true);
      embed.AddField("Level", user.Level, true);
      embed.AddField("Total Purchases", user.Purchases, true);
      embed.WithFooter("Use profile for much more detailed information.");
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    [Command("transfer")] [Alias("send")]
    public async Task TransferProtobucks(string userCalled = null, string moneyToSend = null) {
      if (userCalled == null || moneyToSend == null) {
        await Context.Channel.SendMessageAsync("Make sure to tag the recipient and then specify the amount to send!");
        return;
      }

      UserObject user;
      UserObject author;
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;
      user = MongoHelper.GetUser(filteredId).Result;
      author = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      if (user == null) {
        var userPost = Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result;
        await MongoHelper.CreateUser(userPost);
        user = MongoHelper.GetUser(filteredId).Result;
      }

      long moneySending = 0;

      try {
        moneySending = Convert.ToInt64(moneyToSend);
      }
      catch (FormatException) {
        await Context.Channel.SendMessageAsync("Please make sure the money you're sending is a valid number!");
        return;
      }
      if (moneySending <= 0) {
        await Context.Channel.SendMessageAsync("Please make sure the money you're sending is a valid number!");
        return;
      }

      if (user.Id == Context.Message.Author.Id.ToString()) {
        await Context.Channel.SendMessageAsync("You can't send money to yourself, silly!");
        return;
      }

      if (moneySending > author.Money) {
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

    [Command("partnerup")] [Alias("setpartner", "partner", "marry")]
    public async Task SetPartner(string userCalled = null) {
      UserObject author;
      UserObject partner;
      UserObject currentPartner;
      if (userCalled == null) {
        await Context.Channel.SendMessageAsync("Sorry, but you need to tag someone for this command to work!");
        return;
      }
      var filteredId = UtilityHelper.FilterUserIdInput(Context, userCalled);
      if (filteredId == null) return;


      author = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      partner = MongoHelper.GetUser(filteredId).Result;
      if (partner == null) {
        var userPost = Context.Client.GetUserAsync(Convert.ToUInt64(filteredId)).Result;
        await MongoHelper.CreateUser(userPost);
        partner = MongoHelper.GetUser(filteredId).Result;
      }

      if (author.Partner == filteredId) {
        await Context.Channel.SendMessageAsync($"<@{author.Id}>, hey hey! {partner.Name} is already your partner!");
        return;
      }

      if (filteredId == Context.Message.Author.Id.ToString()) {
        await Context.Channel.SendMessageAsync("How can you be your ***own partner*** in crime?");
        return;
      }

      if (author.Partner == "None") {
        await MongoHelper.UpdateUser(author.Id, "Partner", filteredId);
        if (partner.Partner == Context.User.Id.ToString()) {
          await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
          await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);
          await Context.Channel.SendMessageAsync($"Awe, nice! You and {partner.Name} are now mutual partners!");
        } else {
          await Context.Channel.SendMessageAsync($"Awe, nice! {partner.Name} is now your partner!");
        }
      }

      if (author.Partner != "None") {
        currentPartner = MongoHelper.GetUser(author.Partner).Result;
        await MongoHelper.UpdateUser(author.Id, "Partner", filteredId);


        if (partner.Partner == Context.User.Id.ToString()) {
          await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
          await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);
          await Context.Channel.SendMessageAsync($"Awe, nice! You and {partner.Name} are now mutual partners!");
        }

        if (currentPartner.Partner == Context.User.Id.ToString()) {
          await MongoHelper.UpdateUser(currentPartner.Id, "Partner", "None");
          await MongoHelper.UpdateUser(currentPartner.Id, "Mutuals", false);
          await MongoHelper.UpdateUser(author.Id, "Mutuals", false);
          if (partner.Partner == author.Id) {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your mutual partner!");
            await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
            await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);
          } else {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your partner!");
          }
        }

        if (partner.Partner != Context.User.Id.ToString() && currentPartner.Partner != Context.User.Id.ToString()) {
          await Context.Channel.SendMessageAsync($"Sad to see em go... {currentPartner.Name} is no longer your partner, but {partner.Name} is now your partner!");
          if (partner.Partner == author.Id) {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your mutual partner!");
            await MongoHelper.UpdateUser(author.Id, "Mutuals", true);
            await MongoHelper.UpdateUser(partner.Id, "Mutuals", true);
          } else {
            await Context.Channel.SendMessageAsync($"Awe, you and {currentPartner.Name} are no longer partners, but {partner.Name} is now your partner!");
          }
        }
      }
      return;
    }

    [Command("removepartner")] [Alias("partnerdown", "divorce", "unpartner")]
    public async Task RemovePartner() {
      UserObject author = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      if (author.Partner == "None") {
        await Context.Channel.SendMessageAsync("You don't have a partner to remove!");
        return;
      }
      UserObject partner = MongoHelper.GetUser(author.Partner).Result;
      await MongoHelper.UpdateUser(author.Id, "Partner", "None");
      await MongoHelper.UpdateUser(author.Id, "Mutuals", false);
      if (partner.Partner == author.Id) {
        await MongoHelper.UpdateUser(partner.Id, "Partner", "None");
        await MongoHelper.UpdateUser(partner.Id, "Mutuals", false);
        await Context.Channel.SendMessageAsync($"Awe, you and {partner.Name} are no longer partners.");
      } else {
        await Context.Channel.SendMessageAsync($"Sad to see em go... {partner.Name} is no longer your partner.");
      }
      return;
    }

    [Command("setprofile")] [Alias("description", "profiledesc", "setdesc")]
    public async Task SetDescription([Remainder] string input = null) {
      UserObject user = MongoHelper.GetUser(Context.User.Id.ToString()).Result;
      InventoryObject inv = MongoHelper.GetInventory(Context.User.Id.ToString()).Result;
      if (input == null) {
        await Context.Channel.SendMessageAsync($"<@{user.Id}> you may type a message 240 characters or less to use as a description, or type ``remove``, ``none`` or ``clear`` after the command to clear your description.");
        return;
      }
      if (input == "remove" || input == "none" || input == "clear") {
        if (user.Description == "This user has not set a description.") {
          await Context.Channel.SendMessageAsync($"You haven't set a description to be removed, <@{user.Id}>!");
          return;
        }
        await Context.Channel.SendMessageAsync($"Oh, you wanted your description removed? Okay, sure thing, your previous description below will be removed. <@{user.Id}>\n> {user.Description}");
        await MongoHelper.UpdateUser(user.Id, "Description", "This user has not set a description.");
        return;
      } else {
        if (input.Count() > 240) {
          await Context.Channel.SendMessageAsync($"Your description is too long! <@{user.Id}>, the maximum amount of characters is 240.\nYour character count was: {input.Count()}/240");
        } else {
          await Context.Channel.SendMessageAsync($"Alright, done! Your description has been updated, <@{user.Id}>!\n> {input}");
          await MongoHelper.UpdateUser(user.Id, "Description", input);
        }
        return;
      }
    }

    [Group("leaderboard")] [Alias("top")] [RequireContext(ContextType.Guild)]
    public class LeaderBoard : ModuleBase<SocketCommandContext> {
      [Command]
      public async Task Default() {
        await Context.Channel.SendMessageAsync("Please specify either ``Money``/``Protobucks`` or ``Level``/``Levels``.");
      }

      [Command("money")] [Alias("Protobucks")]
      public async Task MoneyBoard() {
        var embed = new EmbedBuilder();
        string userlist = "";
        var index = 0;
        var TotalUsers = MongoHelper.GetTotalUserCount().Result.Count();

        embed.WithTitle("PrototonBot Protobucks Leaderboard");
        embed.WithColor(0xB2A2F1);
        foreach (var obj in MongoHelper.GetLeaderboardTopMoney().Result) {
          index++;
          userlist += $"\n{index} - **{obj.Name}** with **{obj.Money}** Protobucks.";
        }
        embed.AddField("Richest Users", userlist, true);
        embed.AddField("Total Users", TotalUsers, true);
        await Context.Channel.SendMessageAsync("", false, embed.Build());
      }

      [Command("level")] [Alias("levels")]
      public async Task LevelBoard() {
        var embed = new EmbedBuilder();
        string userlist = "";
        var index = 0;
        var TotalUsers = MongoHelper.GetTotalUserCount().Result.Count();

        embed.WithTitle("PrototonBot Level Leaderboard");
        embed.WithColor(0xB2A2F1);
        foreach (var obj in MongoHelper.GetLeaderboardTopLevels().Result) {
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
