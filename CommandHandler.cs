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
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.WireProtocol.Messages;

namespace PrototonBot
{
  public class CommandHandler
  {
    private readonly CommandService commands;
    private readonly DiscordSocketClient client;
    private readonly IServiceProvider services;
    private static AuthDiscordBotListApi dblListApi = null;
    Random random = new Random();

    public CommandHandler(IServiceProvider services)
    {
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

    public async Task InitializeAsync()
    {
      await this.commands.AddModulesAsync(Assembly.GetEntryAssembly(), this.services);
    }

    private async Task OnReady()
    {
      await client.SetStatusAsync(UserStatus.Online);
      await client.SetGameAsync("pr.help", null, ActivityType.Listening);

      if (Program.EnableBotList)
      {
        dblListApi = new AuthDiscordBotListApi(client.CurrentUser.Id, Program.BotListToken);
        var serversConnected = 0;
        foreach (var svr in client.Guilds) { serversConnected++; }
        IDblSelfBot dblStats = await dblListApi.GetMeAsync();
        await dblStats.UpdateStatsAsync(serversConnected);
      }
    }

    private Task OnDisconnect(Exception err)
    {
      return Task.CompletedTask;
    }

    public Task OnCommandAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
      if (!command.IsSpecified) return Task.CompletedTask;
      if (result.IsSuccess) return Task.CompletedTask;
      return Task.CompletedTask;
    }

    private async Task OnError(ICommandContext context, IUserMessage message, IResult result)
    {
      await context.Channel.SendMessageAsync($"Sorry, that command didn't work. Double check that you used it properly.");
      var embed = new EmbedBuilder()
        .WithTitle("Unhandled Exception")
        .WithDescription("An unhandled exception was encountered during runtime:")
        .WithColor(Color.DarkRed)
        .AddField("User", $"`{message.Author.Username}#{message.Author.Discriminator}`")
        .AddField("Guild", $"`{context.Guild.Name}`")
        .AddField("Timestamp", $"`{DateTime.Now:yyyy-MM-dd hh:mm:ss}`")
        .AddField("Command", $"`{message.Content}`")
        .AddField("Result", $"`{result.Error}: {result.ErrorReason}`");
    }

    public async Task OnMessageReceivedAsync(SocketMessage rawMessage)
    {
      if (!(rawMessage is SocketUserMessage message)
          || message.Source != MessageSource.User
          || message == null
          || message.Author.Discriminator == "0000") return;
      if (message.Channel.GetType() == typeof(SocketDMChannel)) { await OnDirectMessage(message); return; }

      await MongoHelper.CreateUser(message.Author);
      await UserUpdates.MinuteRewards(message.Author);
      await UserUpdates.LevelUpdater(message);

      var user = MongoHelper.GetUser(message.Author.Id.ToString()).Result;
      var inv = MongoHelper.GetInventory(message.Author.Id.ToString()).Result;
      var server = MongoHelper.GetServer((message.Channel as SocketGuildChannel).Guild.Id.ToString()).Result;
      var context = new SocketCommandContext(this.client, message);


      if (user.Name != message.Author.Username)
      {
        await MongoHelper.UpdateUser(user.Id, "Name", message.Author.Username.ToString());
      }
      if (server.Name != (message.Channel as SocketGuildChannel).Guild.Name)
      {
        await MongoHelper.UpdateServer(server.Id, "Name", (message.Channel as SocketGuildChannel).Guild.Name);
      }

      try
      {
        var argPosition = 0;
        var noPrefix = "";
        var cmdList = commands.Commands.ToList();

        if (message.HasStringPrefix(server.Prefix, ref argPosition))
        {
          noPrefix = message.ToString().Replace(server.Prefix, "");
        }
        else if (message.HasStringPrefix("pr.", ref argPosition))
        {
          noPrefix = message.ToString().Replace("pr.", "");
        }
        else return;

        if (!server.EnabledChannels.Contains(message.Channel.Id.ToString()) && !(message.Author as SocketGuildUser).GetPermissions(message.Channel as SocketGuildChannel).ManageChannel)
        {
          if (message.Content.StartsWith(server.Prefix) || message.Content.StartsWith("pr"))
          {
            var found = cmdList.Find(cmd => cmd.Name.ToLower().Equals(noPrefix.Split(' ')[0]));
            if (found != null)
            {
              //PrototonBot can be muted/removed from the DBL server if it responds to other bot's prefixes, so it has to be muted.
              if (server.Id.ToString() != "264445053596991498")
              {
                await context.Channel.SendMessageAsync("Commands not enabled in this channel, an admin needs to use the `enable` command.");
              }
            }
            return;
          }
        }

        IResult result;
        using (context.Channel.EnterTypingState())
        {
          result = await this.commands.ExecuteAsync(context, argPosition, this.services);
        }

        if (!result.IsSuccess)
        {
          switch (result.Error)
          {
            case CommandError.UnknownCommand:
              {
                //var NoCommand = new Emoji("❔");
                //await context.Message.AddReactionAsync(NoCommand);
                return;
              }
            case CommandError.BadArgCount:
              {
                await context.Channel.SendMessageAsync(
                    $"Sorry, but that command wasn't used correctly. Try {server.Prefix}help to learn more.");
                return;
              }
            case CommandError.ParseFailed:
              {
                await context.Channel.SendMessageAsync(
                    $"Sorry, but I wasn't able to understand that. Please try again with normal text.");
                return;
              }
            case CommandError.UnmetPrecondition:
              {
                await context.Channel.SendMessageAsync("This command only works for users with the **Administrator** permission."); return;
              }
            default:
              {
                await OnError(context, message, result);
                return;
              }
          }
        }
        Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} PrototonBot    {DateTime.Now.ToString("MM-dd")} : {(message.Channel as SocketGuildChannel).Guild.Name} : {message.Content}");
      }
      catch (Discord.Net.HttpException) { }
    }

    public async Task OnDirectMessage(SocketMessage rawMessage)
    {
      await rawMessage.Channel.SendMessageAsync("PrototonBot is not used via DMs. Please use PrototonBot commands in a server.");
    }

    private async Task OnNewServer(IGuild server)
    {
      if (Program.EnableBotList)
      {
        var serversConnected = 0;
        foreach (var serverUpdate in client.Guilds) { serversConnected++; }
        IDblSelfBot dblStats = await dblListApi.GetMeAsync();
        await dblStats.UpdateStatsAsync(serversConnected);
      }
      await MongoHelper.CreateServer(server);
      SocketGuild newServer = server as SocketGuild;
      var svrDB = MongoHelper.GetServer(newServer.Id.ToString()).Result;
      await newServer.SystemChannel.SendMessageAsync($">>> Thank you for inviting me to this server! :purple_heart:\nAn administrator can learn what they can set up for the server, and need to enable channels before users can use commands. Please check out ``{svrDB.Prefix}help`` or ``pr.help`` to learn more about commands and configuring this bot! :)\nAdmins can use commands anywhere as long as they have the ManageChannel permission.");
    }

    private async Task OnLeaveServer(IGuild server)
    {
      if (Program.EnableBotList)
      {
        var serversConnected = 0;
        foreach (var serverUpdate in client.Guilds) { serversConnected++; }
        IDblSelfBot dblStats = await dblListApi.GetMeAsync();
        await dblStats.UpdateStatsAsync(serversConnected);
      }
      await MongoHelper.DeleteServer(server);
    }

    private Task OnUserJoin(SocketGuildUser user)
    {
      var server = MongoHelper.GetServer(user.Guild.Id.ToString()).Result;

      switch (user.Guild.Id.ToString())
      {
        case "264445053596991498": //TOP.GG
          {
            break;
          }
        default: //Any Other Server
          {
            if (!server.WelcomeMessages || server.WelcomeChannel == "") { break; }
            var welcomeChannel = user.Guild.GetTextChannel(Convert.ToUInt64(server.WelcomeChannel));
            welcomeChannel.SendMessageAsync($":sparkling_heart: Welcome <@{user.Id}>, to **{server.Name}**! Have a wonderful time here! :sparkling_heart:");
            break;
          }
      }
      return Task.CompletedTask;
    }

    private Task OnUserLeave(SocketGuildUser user)
    {
      var server = MongoHelper.GetServer(user.Guild.Id.ToString()).Result;

      switch (user.Guild.Id.ToString())
      {
        case "264445053596991498": //TOP.GG
          {
            break;
          }
        default: //Any Other Server
          {
            if (!server.WelcomeMessages || server.WelcomeChannel == "") { return Task.CompletedTask; }
            var welcomeChannel = user.Guild.GetTextChannel(Convert.ToUInt64(server.WelcomeChannel));
            welcomeChannel.SendMessageAsync($"{user.Username} has departed from this server.. We wish them a friendly farewell! :broken_heart:");
            break;
          }
      }
      return Task.CompletedTask;
    }

    private Task OnChannelDeleted(SocketChannel channel)
    {
      var server = MongoHelper.GetServer((channel as SocketGuildChannel).Guild.Id.ToString()).Result;
      if (server.WelcomeChannel == channel.Id.ToString())
      {
        MongoHelper.UpdateServer(server.Id.ToString(), "WelcomeChannel", "");
      }
      if (server.LogChannel == channel.Id.ToString())
      {
        MongoHelper.UpdateServer(server.Id.ToString(), "LogChannel", "");
      }
      if (server.EnabledChannels.Contains(channel.Id.ToString()))
      {
        var currentlyEnabled = server.EnabledChannels;
        currentlyEnabled.Remove(channel.Id.ToString());
        MongoHelper.UpdateServer(server.Id.ToString(), "EnabledChannels", currentlyEnabled);
      }
      return Task.CompletedTask;
    }
  }
}
