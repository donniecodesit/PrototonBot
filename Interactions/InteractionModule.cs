/*

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototonBot.Interactions
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ulong roleID = 998862499199844403;

        // Slash Commands, used by beginnig a / command. <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        [SlashCommand("pinguser", "Ping someone else")]
        public async Task HandlePingCommand([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            await RespondAsync($"Okay, pinging <@{user.Id}>");
        }

        [SlashCommand("components", "Demonstrate buttons and select menus.")]
        public async Task HandleComponentCommand()
        {
            var button = new ButtonBuilder()
            {
                Label = "Simon Says",
                CustomId = "simonsays",
                Style = ButtonStyle.Danger
            };

            var menu = new SelectMenuBuilder()
            {
                CustomId = "menu",
                Placeholder = "Sample Menu"
            };

            menu.AddOption("First Option", "first");
            menu.AddOption("Second Option", "second");

            var component = new ComponentBuilder();
            component.WithButton(button);
            component.WithSelectMenu(menu);

            await RespondAsync("testing", components: component.Build());
        }

        // User Commands, only seen by right clicking on a user. <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        [UserCommand("give-role")]
        public async Task HandleUserCommand(IUser user)
        {
            await (user as SocketGuildUser).AddRoleAsync(roleID);
            var roles = (user as SocketGuildUser).Roles;
            string rolesList = string.Empty;
            foreach (var role in roles)
            {
                if (role.Name != "@everyone") rolesList += role.Name + "\n";
            }

            await RespondAsync($"User {user.Mention} has the following roles:\n" + rolesList);
        }

        // Message Commands, only seen by right clicking on a message. <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        [MessageCommand("msg-command")]
        public async Task HandleMessageCommand(IMessage message)
        {
            await RespondAsync($"Message author is: {message.Author.Username}");
        }

        // Component Interactions, functions used by custom components (like the buttons and menus above. CustomId must match name). <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        [ComponentInteraction("simonsays")]
        public async Task HandleButtonInput()
        {
            await RespondWithModalAsync<DemoModal>("demo_modal");
        }

        [ComponentInteraction("menu")]
        public async Task HandleMenuSelection(string[] inputs)
        {
            await RespondAsync(inputs[0]);
        }

        [ModalInteraction("demo_modal")]
        public async Task HandleModalInput(DemoModal modal)
        {
            string input = modal.Greeting;
            await RespondAsync(input);
        }
    }

    public class DemoModal : IModal
    {
        public string Title => "Demo Modal";
        [InputLabel("Send a greeting!")]
        [ModalTextInput("greeting_input", TextInputStyle.Short, placeholder: "Be nice...", maxLength: 100)]
        public string Greeting { get; set; }

    }
}
*/