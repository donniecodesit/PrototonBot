using Discord.Interactions;
using Discord.WebSocket;
using System.Text;

namespace PrototonBot.Interactions
{
    public class ActionCommands : InteractionModuleBase<SocketInteractionContext>
    {
        Random RNG = new Random();

        [SlashCommand("ship", "[fun] Get a rating between you and another user")]
        public async Task LoveCalculation([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user == Context.User) await RespondAsync("That's quite the self appreciation you have there! :purple_heart:");
            else await RespondAsync($"Consulting with the goddess of love..\nShe says {Context.User.Username} and <@{user.Id}> are {RNG.Next(0, 101)}% compatible!");
        }

        [SlashCommand("hug", "[fun] Tag a user and just give em a good ol hug")]
        public async Task HugCommand([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user == Context.User) await RespondAsync("You can't give yourself a hug..\nBut I can! Come here, you! :purple_heart:");
            else await RespondAsync($"Awe, {Context.User.Username} gave <@{user.Id}> a big ol' hug! So sweet! :pleading_face:");
        }

        [SlashCommand("boop", "[fun] Tag a user and just boop em")]
        public async Task BoopCommand([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user == Context.User) await RespondAsync($"You can't boop yourself. Well I mean you can, but..\n<@{Context.User.Id}>, boop! Heh! :purple_heart:");
            else await RespondAsync($"Omgosh, {Context.User} gave <@{user.Id}> a cute lil' boop. Awe. :purple_heart:");
        }

        [SlashCommand("punish", "[fun] Punish a user for your reason. 'You have been punished for (REASON)'")]
        public async Task PunishCommand([Summary(description: "A tagged user (@)")] SocketUser user, [Summary(description: "The reason for punishment")] string message)
        {
            if (user == Context.User) await RespondAsync($"Wait, you want to punish yourself? Weird, but okay!\n<@{Context.User.Id}>, I PUNISH YOU FOR {message.ToUpper()}!");
            else await RespondAsync($"<@{user.Id}>, YOU HAVE BEEN PUNISHED FOR {message.ToUpper()}!");
        }

        [SlashCommand("lick", "[fun] Tag a user and just lick em")]
        public async Task LickCommand([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user == Context.User) await RespondAsync($"You can't lick yourself, so..\n:tongue: Here ya go, bleeehp!");
            else await RespondAsync($"<@{user.Id}> was given a slobber to the face from {Context.User.Username}! :tongue:");
        }

        [SlashCommand("yeet", "[fun] Tag a user and just yeet em")]
        public async Task YeetCommand([Summary(description: "A tagged user (@)")] SocketUser user)
        {
            if (user == Context.User) await RespondAsync($"Are.. you feeling okay? You can't yeet yourself!");
            else await RespondAsync($"<@{user.Id}> was yeeted into the oblivion by {Context.User.Username}! :wastebasket:");
        }

        [SlashCommand("sass", "[fun] sAsS uP yOuR mEsSaGe!")]
        public async Task SassCommand([Summary(description: "A message to be sassed")] string message)
        {
            int capitalFirst = RNG.Next(1, 3);
            var Builder = new StringBuilder();
            var index = 0;
            foreach (char c in message)
            {
                if (capitalFirst == 1)
                {
                    if (index % 2 == 0) Builder.Append(char.ToUpper(c));
                    else Builder.Append(char.ToLower(c));
                }
                else
                {
                    if (index % 2 == 0) Builder.Append(char.ToLower(c));
                    else Builder.Append(char.ToUpper(c));
                }
                index++;
            }

            await RespondAsync(Builder.ToString());
        }
    }
}