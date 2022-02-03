using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PrototonBot.MongoUtil;
using Nett;
using System.IO;
using System.Collections.Generic;

namespace PrototonBot.Commands
{
  public class InfoCommands : ModuleBase<SocketCommandContext>
  {
    Random flipResult = new Random();

    //Replies with a specific string.
    [Command("she")]
    public async Task SheCommand() {
      await ReplyAsync("Shé. :nail_care:");
    }

    //Reply with a flipped coin
    [Command("coinflip")] [Alias("flipcoin", "flipacoin")]
    public async Task FlipACoin() {
      double coinResult = flipResult.Next(1, 3);
      if (coinResult == 1) {
        await ReplyAsync("Oh, oh! Look at it go! It landed on **heads**!");
        return;
      }
      await ReplyAsync("Oh, oh! Look at it go! It landed on **tails**!");
    }

    //Pings the user and gives them some attention
    [Command("loveme")] [Alias("protolove")]
    public async Task ProtoLove() {
      await ReplyAsync($"Here, <@{Context.User.Id}>, have some adorable PrototonBot loving! :heart::hugging::heart::hugging:");
    }

    //Pings the user and makes them feel noticed
    [Command("noticeme")] [Alias("senpai")]
    public async Task NoticeMe() {
      await Context.Channel.SendMessageAsync($"Awe no worries, <@{Context.User.Id}>, PrototonBot notices you! Have some lovin'! :heart::hugging::heart::hugging:");
    }

    //Reply with information about the bot
    [Command("info")] [Alias("botinfo")]
    public async Task BotInfo() {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var repo = Program.GitHubRepoURL;
      var gitValid = (repo != "RepoURLHere" && repo != "" );

      var embed = new EmbedBuilder();
      embed.WithColor(0xB2A2F1);
      embed.WithTitle("Welcome to PrototonBot!");
      embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
      embed.WithDescription($"The successor to ShepherdBot Classic, a Discord Bot written for entertainment purposes with interactive commands. Mother language was JavaScript, rewritten in C#.\nIt is receiving occasional updates, features, and ideas. Please enjoy the bot!");
      embed.AddField("Created By", "Donovan Laws", true);
      embed.AddField("Default Prefix", $"Global: pr.\nHere: {serverObj.Prefix}", true);
      embed.AddField("Servers", $"\nServers: {Context.Client.Guilds.Count}", true);
      if (gitValid) embed.AddField("Git Repo", $"[Git Repo Page]({repo})", true);
      embed.AddField("Invite Bot", $"[Invite Link](https://discord.com/oauth2/authorize?client_id={Program.UserID}&permissions=8&scope=bot)", true);
      embed.AddField("Last Restart", $"{Program.LastRestartTime} {Program.TimeZone}", true);
      embed.WithFooter($"{((gitValid) ? "You can report bugs/issues to the GitHub Page's Issues Tab" : "")}\nBuilt with Visual Studio Code and Discord.Net");
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    //Reply with information about the server
    [Command("serverinfo")]
    public async Task ServerInfo() {
      var serverObj = MongoHelper.GetServer(Context.Guild.Id.ToString()).Result;
      var embed = new EmbedBuilder();
      var privacy = serverObj.Public ? "Public" : "Private";
      embed.WithColor(0xB2A2F1);
      embed.WithThumbnailUrl(Context.Guild.IconUrl);
      embed.WithTitle($"{Context.Guild.Name} Information");
      embed.AddField("Server Information", $"Server ID: `{Context.Guild.Id}`\nCreated At: `{Context.Guild.CreatedAt}`\nOwner: `{Context.Guild.Owner}`\nMembers: `{Context.Guild.MemberCount}`\nChannels: `{Context.Guild.Channels.Count}`\nRoles: `{Context.Guild.Roles.Count}`\nVerification Level: `{Context.Guild.VerificationLevel}`\nLevel Messages Enabled: `{serverObj.LevelUpMessages}`\nServer Privacy: `{privacy}`\nPrefix: `{serverObj.Prefix}`");
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    //Reply with embedded invite
    [Command("botinvite")]
    public async Task BotInvite() {
      var embed = new EmbedBuilder();
      embed.WithTitle("Want to invite the bot?");
      embed.WithColor(0xB2A2F1);
      embed.WithDescription("Hey, appreciated! All shares and invites of the bot are encouraged, as it helps the bot grow and also increases possibilities for us to find bugs and problems! :)\nClick the blue hyperlink above to invite!");
      embed.WithThumbnailUrl(Context.Guild.GetUser(Program.UserID).GetAvatarUrl());
      embed.WithUrl($"https://discord.com/oauth2/authorize?client_id={Program.UserID}&permissions=8&scope=bot");
      await Context.Channel.SendMessageAsync("", false, embed.Build());
    }

    //Reply with the user's input
    [Command("simonsays")]
    public async Task SimonSays([Remainder] string input) {
      if (input != null) await Context.Channel.SendMessageAsync($"{input}");
      return;
    }
  }
}
