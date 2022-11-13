using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Interactions;
using PrototonBot.Log;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrototonBot
{
    public class Program
    {
        private DiscordSocketClient? _client;
        public static List<string>? DeveloperIDs;
        public static string? MongoURL;
        public static string? GitHubRepoURL;
        public static string? LastRestartTime;
        public static string? TimeZone;
        public static string? BotAvatarUrl;
        public static string? ApplicationID;
        public static string? CacheDir;

        public static Task Main() => new Program().MainAsync();

        public async Task MainAsync()
        {
            // Configuration file, containing bot specific variables.
            var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile(Path.Combine("Storage", "config.json")).Build();
            using IHost host = Host.CreateDefaultBuilder().ConfigureServices((_, services) =>
                services
                    // Add the configuration fetched to the registered services.
                    .AddSingleton(config)
                    // Add the DiscordSocketClient with any specific intents required.
                    .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
                    {
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
                          | GatewayIntents.GuildWebhooks,
                        LogGatewayIntentWarnings = false,
                        AlwaysDownloadUsers = true,
                        LogLevel = LogSeverity.Debug
                    }))
                    // Adding Logging to Console
                    .AddTransient<ConsoleLogger>()
                    // Adding InteractionService for Discord's Slash Commands
                    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
                    // Required to subscribe to client events used with Interactions
                    .AddSingleton<InteractionHandler>()
                    // Adding the Message Handler (will not require message content)
                    .AddSingleton(x => new CommandService(new CommandServiceConfig
                    {
                        LogLevel = LogSeverity.Debug,
                        DefaultRunMode = Discord.Commands.RunMode.Async
                    }))
                    .AddSingleton<MessageHandler>())
                .Build();
            await RunAsync(host);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            _client = provider.GetRequiredService<DiscordSocketClient>();
            var config = provider.GetRequiredService<IConfigurationRoot>();

            var slashCommands = provider.GetRequiredService<InteractionService>();
            await provider.GetRequiredService<InteractionHandler>().InitializeAsync();

            // Subscribe to Client Log Events
            _client.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);
            // Subscribe to Interaction Log Events
            slashCommands.Log += _ => provider.GetRequiredService<ConsoleLogger>().Log(_);

            var messageHandler = provider.GetRequiredService<MessageHandler>();
            await messageHandler.InitializeAsync();

            _client.Ready += async () =>
            {
                // If the bot is running with the DEBUG flag, register all commands to the guild specified. Otherwise, register them globally.
                if (IsDebug())
                {
                    foreach (var server in _client.Guilds)
                    {
                        await slashCommands.RegisterCommandsToGuildAsync(server.Id, true);
                    }
                }
                else
                {
                    await slashCommands.RegisterCommandsGloballyAsync(true);

                }
            };

            await _client.LoginAsync(TokenType.Bot, config["tokens:discord"]);
            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetGameAsync("for / commands. 👀", null, ActivityType.Watching);
            await _client.StartAsync();

            MongoURL = config["MongoURL"];
            GitHubRepoURL = config["GitHubRepoURL"];
            DeveloperIDs = config.GetSection("DeveloperIDs").Get<string[]>().ToList();
            LastRestartTime = DateTime.Now.ToString("MMMM dd yyyy h:mm tt");
            TimeZone = config["Timezone"];
            CacheDir = config["CacheDir"];
            ApplicationID = config["ApplicationID"];

            await Task.Delay(-1);
        }

        static bool IsDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}