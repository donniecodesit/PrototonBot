using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using PrototonBot.MongoUtility;

namespace PrototonBot
{
    internal class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the payloads to execute interaction commands
            _client.InteractionCreated += HandleInteraction;

            // Process the command results
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task SlashCommandExecuted(SlashCommandInfo arg1, IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                // Create an execution context that matches the type parameter of your modules.
                var context = new SocketInteractionContext(_client, arg);

                // create the user if they don't exist. XP, Money, and Level do not apply to slash commands.
                if (MongoHandler.GetUser(context.Interaction.User.Id.ToString()).Result == null) await MongoHandler.CreateNewUser(context.Interaction.User);

                // Fetch the user information and update the name value if it has changed.
                var user = MongoHandler.GetUser(context.Interaction.User.Id.ToString()).Result;
                if (user.Name != context.Interaction.User.Username) await MongoHandler.UpdateUser(user.Id, "Name", context.Interaction.User.Username);

                // Fetch the server information and update the name value if it has changed.
                if (context.Guild != null)
                {
                    var server = MongoHandler.GetServer(context.Guild.Id.ToString()).Result;
                    if (server == null)
                    {
                        await MongoHandler.CreateNewServer(context.Guild);
                        server = MongoHandler.GetServer(context.Guild.Id.ToString()).Result;
                    }
                    if (server.Name != context.Guild.Name) await MongoHandler.UpdateServer(context.Guild.Id.ToString(), "Name", context.Guild.Name);

                }

                // Only accept slash commands if the channel is enabled or if the user is an admin.
                var mongoSvr = MongoHandler.GetServer(context.Guild.Id.ToString()).Result;

                if (!mongoSvr.EnabledChannels.Contains(context.Channel.Id.ToString()) && !(context.Interaction.User as SocketGuildUser).GetPermissions(context.Channel as SocketGuildChannel).ManageChannel)
                {
                    await context.Interaction.RespondAsync("Commands aren't on in this channel. Admins can enable channels using `/admin channelenable`.");
                    await Task.Delay(6000);
                    await context.Interaction.DeleteOriginalResponseAsync();
                    return;
                } else
                {
                    // Finally, execute the requested command.
                    await _commands.ExecuteCommandAsync(context, _services);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a slash command fails, it is most likely that the origianl acknowledgement will persist.
                // It's good to delete the response, or let the user know something went wrong.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }
}
