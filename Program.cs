using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nett;
using System.Collections.Generic;

namespace PrototonBot {
  public class Program {
    private static DiscordSocketClient client;
    public static bool EnableBotList;
    public static string DiscordToken;
    public static string BotListToken;
    public static string MongoURL;
    public static ulong UserID;
    public static string CacheDir;
    public static string MasterSvr;
    public static string GitHubRepoURL;


    static void Main(string[] args) {
      new Program().AsyncStart().GetAwaiter().GetResult();
    }

    private Task Log(LogMessage message) {
      Console.WriteLine(message.ToString());
      return Task.CompletedTask;
    }

    public async Task AsyncStart() {
      TomlTable config = null;
      try {
        config = Toml.ReadFile(Path.Combine("Storage", "config.toml"));
      } catch (FileNotFoundException) {
        config = Toml.Create();
        config.Add("DiscordToken", "InsertTokenHere");
        config.Add("UserID", "InsertIDHere");
        config.Add("MasterSvr", "ServerIDHere");
        config.Add("GitHubRepoURL", "RepoURLHere");
        config.Add("MongoURL", "mongodb://localhost");
        config.Add("DeveloperIDs", new List<string>());
        config.Add("EnableBotList", false);
        config.Add("BotListToken", "InsertTokenHere");
        config.Add("CacheDir", "/tmp/protobot");
        Toml.WriteFile(config, Path.Combine("Storage", "config.toml"));
        Console.WriteLine("Add your token to Storage/config.toml and restart the bot.");
        return;
      }
      using (var services = ConfigureServices()) {
        EnableBotList = config.Get<bool>("EnableBotList");
        DiscordToken = config.Get<string>("DiscordToken");
        BotListToken = config.Get<string>("BotListToken");
        MongoURL = config.Get<string>("MongoURL");
        UserID = ulong.Parse(config.Get<string>("UserID"));
        CacheDir = config.Get<string>("CacheDir");
        MasterSvr = config.Get<string>("MasterSvr");
        GitHubRepoURL = config.Get<string>("GitHubRepoURL");

        client = services.GetRequiredService<DiscordSocketClient>();
        services.GetRequiredService<CommandService>().Log += Log;
        client.Log += Log;
        await client.LoginAsync(TokenType.Bot, DiscordToken);
        await client.StartAsync();
        await services.GetRequiredService<CommandHandler>().InitializeAsync();
        await Task.Delay(-1);
      }
    }

    private ServiceProvider ConfigureServices() {
      var socketConfig = new DiscordSocketConfig {
          GatewayIntents = 
          GatewayIntents.DirectMessageReactions
          | GatewayIntents.DirectMessages
          | GatewayIntents.DirectMessageTyping
          | GatewayIntents.GuildBans
          | GatewayIntents.GuildEmojis
          | GatewayIntents.GuildIntegrations
          | GatewayIntents.GuildMembers
          | GatewayIntents.GuildMessageReactions
          | GatewayIntents.GuildMessages
          | GatewayIntents.GuildMessageTyping
          | GatewayIntents.Guilds
          | GatewayIntents.GuildVoiceStates
          | GatewayIntents.GuildWebhooks
      };
      return new ServiceCollection()
          .AddSingleton<DiscordSocketClient>(new DiscordSocketClient(socketConfig))
          .AddSingleton<CommandService>()
          .AddSingleton<CommandHandler>()
          .AddSingleton<HttpClient>()
          .BuildServiceProvider();
    }
  }
}
