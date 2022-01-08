using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Nett;

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
      }
      catch (FileNotFoundException) {
        config = Toml.Create();
        config.Add("DiscordToken", "InsertTokenHere");
        config.Add("EnableBotList", false);
        config.Add("BotListToken", "InsertTokenHere");
        config.Add("MongoURL", "mongodb://localhost");
        config.Add("UserID", "InsertIDHere");
        config.Add("CacheDir", "/tmp/protobot");
        config.Add("MasterSvr", "ServerIDHere");
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

        client = services.GetRequiredService<DiscordSocketClient>();
        services.GetRequiredService<CommandService>().Log += Log;
        client.Log += Log;
        Console.CancelKeyPress += delegate {
          client.LogoutAsync();
          Environment.Exit(0);
        };
        await client.LoginAsync(TokenType.Bot, DiscordToken);
        await client.StartAsync();
        await services.GetRequiredService<CommandHandler>().InitializeAsync();
        await Task.Delay(-1);
      }
    }

    private ServiceProvider ConfigureServices() {
      return new ServiceCollection()
          .AddSingleton<DiscordSocketClient>()
          .AddSingleton<CommandService>()
          .AddSingleton<CommandHandler>()
          .AddSingleton<HttpClient>()
          .BuildServiceProvider();
    }

    public static async void ShutDown() {
      await client.LogoutAsync();
      await client.StopAsync();
      Environment.Exit(0);
    }
  }
}
