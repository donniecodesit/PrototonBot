using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PrototonBot.MongoUtil;

namespace PrototonBot.Commands
{
  public class InfoCommands : ModuleBase<SocketCommandContext>
  {
    Random flipResult = new Random();

    [Command("she")]
    public async Task SheCommand()
    {
      await ReplyAsync("Shé. :nail_care:");
    }

    [Command("botinvite")]
    [Alias("invite", "invitebot")]
    public async Task SendBotInvite()
    {
      await ReplyAsync($"Oh, want to add me to another server? No problem, here's a link!\nhttps://top.gg/bot/{Program.UserID}");
    }

    [Command("coinflip")]
    [Alias("flipcoin", "flipacoin")]
    public async Task FlipACoin()
    {
      double coinResult = flipResult.Next(1, 3);
      if (coinResult == 1)
      {
        await ReplyAsync("Oh, oh! Look at it go! It landed on **heads**!");
        return;
      }

      await ReplyAsync("Oh, oh! Look at it go! It landed on **tails**!");
    }

    [Command("loveme")]
    [Alias("protolove")]
    public async Task ProtoLove()
    {
      await ReplyAsync($"Here, <@{Context.User.Id}>, have some adorable PrototonBot loving! :heart::hugging::heart::hugging:");
    }

    [Command("noticeme")]
    [Alias("senpai")]
    public async Task NoticeMe()
    {
      await Context.Channel.SendMessageAsync($"Awe no worries, <@{Context.User.Id}>, PrototonBot notices you! Have some lovin'! :heart::hugging::heart::hugging:");
    }

    [Command("info")]
    [Alias("botinfo")]
    public async Task BotInfo()
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var serversConnected = 0;
      foreach (var serverUpdate in Context.Client.Guilds) { serversConnected++; }
      var embed = new EmbedBuilder();
      embed.WithColor(0xFF00FF);
      embed.WithTitle("Welcome to PrototonBot!");
      embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
      embed.WithDescription($"PrototonBot is a multipurpose bot developed by Donovan Laws that is actively undergoing new additions, features, changes, and more! Please enjoy the features of this bot!");
      embed.AddField("Created By", "Donovan Laws", true);
      embed.AddField("Default Prefix", $"Default: pr.\nThis Server: {serverObj.Prefix}", true);
      embed.AddField("Servers", $"\nServers: {serversConnected}\n*Features TBA*");
      embed.WithFooter("You can report bugs/issues to the GitHub Page's Issues Tab\nBuilt with Visual Studio and Discord.NET");
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    [Command("serverinfo")]
    public async Task ServerInfo()
    {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var embed = new EmbedBuilder();
      embed.WithColor(0xFF00FF);
      embed.WithThumbnailUrl(Context.Guild.IconUrl);
      embed.WithTitle($"{Context.Guild.Name} Information");
      embed.AddField("Server Information", $"Server ID: *{Context.Guild.Id}*\nCreated At: *{Context.Guild.CreatedAt}*\nOwner: *{Context.Guild.Owner}*\nMembers: *{Context.Guild.MemberCount}*\nChannels: *{Context.Guild.Channels.Count}*\nRoles: *{Context.Guild.Roles.Count}*\nRegion: *{Context.Guild.VoiceRegionId}*\nVerification Level: *{Context.Guild.VerificationLevel}*\nLevel Messages Enabled: *{serverObj.LevelUpMessages}*\nServer is Public: *{serverObj.Public}*\nPrefix: *{serverObj.Prefix}*");
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }
  }
}
