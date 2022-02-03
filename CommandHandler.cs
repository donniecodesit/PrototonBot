using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PrototonBot.MongoUtil;
using System.Linq;
using DiscordBotsList.Api.Objects;
using DiscordBotsList.Api;

namespace PrototonBot {
  public class CommandHandler {
    private readonly CommandService commands;
    private readonly DiscordSocketClient client;
    private readonly IServiceProvider services;
    private static AuthDiscordBotListApi dblListApi = null;
    Random random = new Random();

    //Initialize the state of the CommandHandler
    public CommandHandler(IServiceProvider services) {
      this.services = services;
      this.client = services.GetRequiredService<DiscordSocketClient>();
      this.commands = services.GetRequiredService<CommandService>();
      this.commands.CommandExecuted += OnCommandAsync;
      this.client.MessageReceived += OnMessageReceivedAsync;
      this.client.JoinedGuild += OnNewServer;
      this.client.LeftGuild += OnLeaveServer;
      this.client.Ready += OnReady;
      this.client.Disconnected += OnDisconnect;
      this.client.UserJoined += OnUserJoin;
      this.client.UserLeft += OnUserLeave;
      this.client.ChannelDestroyed += OnChannelDeleted;
    }

    //Runs when the bot starts
    public async Task InitializeAsync() {
      await this.commands.AddModulesAsync(Assembly.GetEntryAssembly(), this.services);
    }

    //Runs when the bot has finished starting up
    private async Task OnReady() {
      await client.SetStatusAsync(UserStatus.Online);
      await client.SetGameAsync("pr.help", null, ActivityType.Listening);
      Program.LastRestartTime = DateTime.Now.ToString("MMMM dd yyyy h:mm tt");

      //If you're using TOP.GG, this will update the bot page's server count.
      if (Program.EnableBotList) {
        dblListApi = new AuthDiscordBotListApi(client.CurrentUser.Id, Program.BotListToken);
        await UpdateDBL();
      }
    }

    //Runs when the bot's been disconnected.
    private Task OnDisconnect(Exception err) {
      return Task.CompletedTask;
    }

    //Runs any time a command is received.
    public Task OnCommandAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) {
      if (!command.IsSpecified) return Task.CompletedTask;
      if (result.IsSuccess) return Task.CompletedTask;
      return Task.CompletedTask;
    }

    //Runs whenever an unhandled error occurs.
    private async Task OnError(ICommandContext context, IUserMessage message, IResult result) {
      var embed = new EmbedBuilder()
        .WithTitle("Unhandled Exception")
        .WithDescription("An unhandled exception was encountered during runtime:")
        .WithColor(Color.DarkRed)
        .AddField("User", $"`{message.Author.Username}#{message.Author.Discriminator}`")
        .AddField("Guild", $"`{context.Guild.Name}`")
        .AddField("Timestamp", $"`{DateTime.Now:yyyy-MM-dd hh:mm:ss}`")
        .AddField("Command", $"`{message.Content}`")
        .AddField("Result", $"`{result.Error}: {result.ErrorReason}`")
        .WithFooter("Please send a screenshot of this to the developers");
        try { 
          await client.GetUserAsync(message.Author.Id).Result.SendMessageAsync(null, false, embed.Build());
          await context.Channel.SendMessageAsync("Something critical occured. I DMed you the error.");
        } catch {
          await context.Channel.SendMessageAsync("Something critical occured.. I am unable to reach your DMs.");
        }
    }

    //Runs on every single message seen.
    public async Task OnMessageReceivedAsync(SocketMessage rawMessage) {
      //If the message source is from a bot, null/empty, or the user's discrim is #0000, return.
      if (!(rawMessage is SocketUserMessage message)
          || message.Source != MessageSource.User
          || message == null
          || message.Author.Discriminator == "0000") return;
      //If the bot was DMed, run OnDirectMessage.
      if (message.Channel.GetType() == typeof(SocketDMChannel)) { await OnDirectMessage(message); return; }

      //Set up context, perform per-message functions, create user and server objects.
      var context = new SocketCommandContext(this.client, message);
      await MongoHelper.CreateUser(message.Author);
      await UserUpdates.MinuteRewards(message.Author);
      await UserUpdates.LevelUpdater(message);

      var user = MongoHelper.GetUser(message.Author.Id.ToString()).Result;
      var server = MongoHelper.GetServer((message.Channel as SocketGuildChannel).Guild.Id.ToString()).Result;
      
      if (user.Name != message.Author.Username) {
        await MongoHelper.UpdateUser(user.Id, "Name", message.Author.Username.ToString());
      }
      if (server.Name != (message.Channel as SocketGuildChannel).Guild.Name) {
        await MongoHelper.UpdateServer(server.Id, "Name", (message.Channel as SocketGuildChannel).Guild.Name);
      }

      try {
        //Check the prefix of the message and assign it to a variable with the prefix split off.
        var atIndex = 0;
        var msgSliced = "";
        var cmdList = commands.Commands.ToList();

        if (message.HasStringPrefix(server.Prefix, ref atIndex)) {
          msgSliced = message.ToString().Replace(server.Prefix, "");
        }
        else if (message.HasStringPrefix("pr.", ref atIndex)) {
          msgSliced = message.ToString().Replace("pr.", "");
        }
        else return;

        //Check if the command exists, if it doesn't, return early.
        var command = msgSliced.Split(' ')[0];
        var found = cmdList.Find(cmd => (cmd.Name.ToLower().Equals(command) || cmd.Aliases.Contains(command)));
        if (found == null) {
          return;
        }

        //If the channel is not enabled for commands, or the user is not an admin, inform them.
        var hasPermissions = (message.Author as SocketGuildUser).GetPermissions(message.Channel as SocketGuildChannel).ManageChannel;
        if (!server.EnabledChannels.Contains(message.Channel.Id.ToString()) && !hasPermissions) {
          //PrototonBot can be muted/removed from the TOP.GG server if it responds to other bot's prefixes, so it has to be muted.
          if (server.Id.ToString() != "264445053596991498") {
            await context.Channel.SendMessageAsync("Commands not enabled in this channel, an admin needs to use the `enable` command.");
          }
          return;
        }

        //The command is valid and the bot is processing now, so enter the typing state.
        IResult result;
        using (context.Channel.EnterTypingState()) {
          var msg = (message.Content.Length > 37) ? (message.Content.Substring(0, 37) + "...") : message.Content;
          Console.WriteLine($"{DateTime.Now.ToString("MM-dd HH:mm:ss")} [PrototonBot]: '{(message.Channel as SocketGuildChannel).Guild.Name}' >> '{msg}'");
          result = await this.commands.ExecuteAsync(context, atIndex, this.services);
        }

        if (!result.IsSuccess) {
          switch (result.Error) {
            case CommandError.UnknownCommand: {
              return;
            }
            case CommandError.BadArgCount: {
              await context.Channel.SendMessageAsync($"Sorry, but that command wasn't used correctly. Try {server.Prefix}help to learn more.");
              return;
            }
            case CommandError.ParseFailed: {
              await context.Channel.SendMessageAsync("Sorry, but I wasn't able to understand that. Please try again with normal text.");
              return;
            }
            case CommandError.UnmetPrecondition: {
              await context.Channel.SendMessageAsync("This command only works for users with the **Administrator** permission."); 
              return;
            }
            default: {
              await OnError(context, message, result);
              return;
            }
          }
        }
      }
      catch (Discord.Net.HttpException) { }
    }

    //Runs when a user DMs the bot.
    public async Task OnDirectMessage(SocketMessage rawMessage) {
      await rawMessage.Channel.SendMessageAsync("PrototonBot is not used via DMs. Please use PrototonBot commands in a server.");
    }

    //Runs when the bot joins a server.
    private async Task OnNewServer(IGuild server) {
      //If you're using TOP.GG, this will update the bot page's server count.
      await UpdateDBL();
      //Create the server info in the database.
      await MongoHelper.CreateServer(server);
      var svrDB = MongoHelper.GetServer(server.Id.ToString()).Result;

      var embed = new EmbedBuilder();
      embed.WithTitle("Thank you for inviting me!");
      embed.WithColor(0xB2A2F1);
      embed.WithThumbnailUrl(client.GetUserAsync(Program.UserID).Result.GetAvatarUrl());
      embed.AddField("Quick Tip:", $"An admin will need to enable a channel before users can use commands anywhere. Check out `{svrDB.Prefix}help` to learn more about commands and configuring this bot!\nAdmins: Enable a channel using `{svrDB.Prefix}enable #channel`. (or the channel ID.)", true);
      embed.WithFooter("Admins in this case are considered anyone with the ManageChannel permission.");

      SocketGuild newServer = server as SocketGuild;
      try {
        await newServer.SystemChannel.SendMessageAsync("", false, embed.Build());
      } catch {
        return;
      }
    }

    //Runs when the bot leaves a server.
    private async Task OnLeaveServer(IGuild server) {
      //If you're using TOP.GG, this will update the bot page's server count.
      await UpdateDBL();
      await MongoHelper.DeleteServer(server);
    }

    //Runs when a user joins a server.
    private Task OnUserJoin(SocketGuildUser user) {
      var server = MongoHelper.GetServer(user.Guild.Id.ToString()).Result;
      if (user.IsBot) return Task.CompletedTask;

      //If TOP.GG, ignore, otherwise send a welcome message if they're enabled.
      switch (user.Guild.Id.ToString()) {
        case "264445053596991498": {
            break;
          }
        default: {
            if (!server.WelcomeMessages || server.WelcomeChannel == "") { break; }
            var welcomeChannel = user.Guild.GetTextChannel(Convert.ToUInt64(server.WelcomeChannel));
            welcomeChannel.SendMessageAsync($":sparkling_heart: Welcome <@{user.Id}>, to **{server.Name}**! Have a wonderful time here! :sparkling_heart:");
            break;
          }
      }
      return Task.CompletedTask;
    }

    //Runs when a user leaves a server.
    private Task OnUserLeave(SocketGuild guild, SocketUser user) {
      var server = MongoHelper.GetServer(guild.Id.ToString()).Result;
      //If TOP.GG, ignore, otherwise send a leaving message if they're enabled.
      switch (guild.Id.ToString()) {
        case "264445053596991498": {
            break;
          }
        default: {
            if (!server.WelcomeMessages || server.WelcomeChannel == "") { return Task.CompletedTask; }
            var welcomeChannel = guild.GetTextChannel(Convert.ToUInt64(server.WelcomeChannel));
            welcomeChannel.SendMessageAsync($"{user.Username} has departed from this server.. We wish them a friendly farewell! :broken_heart:");
            break;
          }
      }
      return Task.CompletedTask;
    }

    //Runs when a channel is deleted.
    private Task OnChannelDeleted(SocketChannel channel) {
      //If the channel deleted matched any config it may have been set to, clear or remove it from there.
      var server = MongoHelper.GetServer((channel as SocketGuildChannel).Guild.Id.ToString()).Result;
      if (server.WelcomeChannel == channel.Id.ToString()) {
        MongoHelper.UpdateServer(server.Id.ToString(), "WelcomeChannel", "");
      }
      if (server.LogChannel == channel.Id.ToString()) {
        MongoHelper.UpdateServer(server.Id.ToString(), "LogChannel", "");
      }
      if (server.EnabledChannels.Contains(channel.Id.ToString())) {
        var currentlyEnabled = server.EnabledChannels;
        currentlyEnabled.Remove(channel.Id.ToString());
        MongoHelper.UpdateServer(server.Id.ToString(), "EnabledChannels", currentlyEnabled);
      }
      return Task.CompletedTask;
    }

    //Helper function to update server count on DBL/TOP.GG
    private async Task UpdateDBL() {
      if (Program.EnableBotList) {
        IDblSelfBot dblStats = await dblListApi.GetMeAsync();
        await dblStats.UpdateStatsAsync(client.Guilds.Count());
      }
    }
  }
}