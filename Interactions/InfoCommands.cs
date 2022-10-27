using Discord;
using Discord.Interactions;
using PrototonBot.MongoUtility;

namespace PrototonBot.Interactions
{
    public class InfoCommands : InteractionModuleBase<SocketInteractionContext>
    {
        Random RNG = new Random();

        [SlashCommand("she", "[fun] Shé.")]
        public async Task SheCommand()
        {
            await RespondAsync("Shé. :nail_care: :eye::lips::eye:");
        }

        [SlashCommand("simonsays", "[fun] Play Simon Says")]
        public async Task SimonSaysCommand([Summary(description: "Your message input")] string message)
        {
            await RespondAsync($"{message}");
        }

        [SlashCommand("flipacoin", "[fun] Get a random coinflip of heads or tails")]
        public async Task FlipCoin()
        {
            var coinResult = RNG.Next(0, 2);
            if (coinResult == 0) await RespondAsync("Oh, oh! Look at it go! It landed on **heads**! :coin:");
            else await RespondAsync("Oh, oh! Look at it go! It landed on **tails**! :sewing_needle:");
        }

        [SlashCommand("loveme", "[fun] Receive some love from PrototonBot")]
        public async Task LoveMeCommand()
        {
            await RespondAsync("Ahem.. Well, kind user, I do love you, but you do not need my love!\nLOVE YOURSELF, YOU ARE AMAZING.");
        }

        [SlashCommand("noticeme", "[fun] Be noticed by PrototonBot")]
        public async Task NoticeMeCommand()
        {
            await RespondAsync($"Awe, no worries <@{Context.User.Id}>. PrototonBot notices you!\nHave some good ol' lovin'! :heart::hugging::heart::hugging:");
        }

        [SlashCommand("botinvite", "[info] Receive an invite for the bot")]
        public async Task BotInvite()
        {
            var _embed = new EmbedBuilder();
            _embed.WithTitle("Want to invite the bot?");
            _embed.WithColor(0xB2A2F1);
            _embed.WithDescription("Appeciated! All shares and invites of the bot are encouraged, as it helps the bot grow, and increases the network of users to think of possibilities, ideas, or help find bugs or problems.\nClick the blue hyperlink above to invite, or click the bot's profile.");
            _embed.WithThumbnailUrl(Context.Guild.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).GetAvatarUrl());
            _embed.WithUrl($"https://discord.com/oauth2/authorize?client_id={Context.Client.GetApplicationInfoAsync().Result.Id}&scope=bot&permissions=3198016");

            await RespondAsync("", embed: _embed.Build());
        }

        [SlashCommand("serverinfo", "[info] Display information about the current server")]
        public async Task ServerInfo()
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            var _embed = new EmbedBuilder();
            var guild = Context.Guild;
            _embed.WithColor(0xB2A2F1);
            _embed.WithThumbnailUrl(guild.IconUrl);
            _embed.WithTitle($"{guild.Name} Information");
            _embed.AddField("Server Information", $"Server ID: `{guild.Id}`\nCreated At: `{guild.CreatedAt}`\nOwner: `{guild.Owner}`\nMembers: `{guild.MemberCount}`\nRoles: `{guild.Roles.Count}`\nVerification Level: `{guild.VerificationLevel}`\n Level Messages: `{mongoSvr.LevelUpMessages}`\nServer Public: `{mongoSvr.Public}` (TBD)`");

            await RespondAsync("", embed: _embed.Build());
        }

        [SlashCommand("botinfo", "[info] Displays information about the bot")]
        public async Task BotInfo()
        {
            var mongoSvr = MongoHandler.GetServer(Context.Guild.Id.ToString()).Result;
            var githubRepo = Program.GitHubRepoURL;
            var gitValid = githubRepo != "RepoURLHere" && githubRepo != "";

            var _embed = new EmbedBuilder();
            _embed.WithColor(0xB2A2F1);
            _embed.WithTitle("Welcome to PrototonBot!");
            _embed.WithThumbnailUrl(Context.Client.GetUser(Context.Client.GetApplicationInfoAsync().Result.Id).GetAvatarUrl());
            _embed.WithDescription($"PrototonBot is the successor to ShepherdBot Classic, a Discord Bot written for entertainment purposes with interactive commands. Mother language was JavaScript (Discord.js), later rewritten in C# (Discord.NET).\nIt is receiving occasional updates and cleanup. Features and ideas are brainstormed and only implemented when possible.");
            _embed.AddField("Invite Bot", $"[Invite Link](https://discord.com/oauth2/authorize?client_id={Context.Client.GetApplicationInfoAsync().Result.Id}&scope=bot&permissions=3198016)", true);
            _embed.AddField("Last Restart", $"{Program.LastRestartTime} {Program.TimeZone}", true);
            _embed.WithFooter($"{(gitValid ? "You can report bugs/issues to the GitHub Page's Issues Tab" : "")}\nBuilt with Visual Studio and Discord.NET");

            await RespondAsync("", embed: _embed.Build());
        }

        [SlashCommand("themeinfo", "[info] Displays every profile theme background.")]
        public async Task ThemeInfo()
        {
            await RespondWithFileAsync(Path.Combine("Storage", "ProfileAssets", "AllThemesPreview.png"), text: "Purchase profile themes with `/buy profiletheme THEME`, equip them with `/settheme THEME`.");
        }
    }
}