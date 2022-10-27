using Discord.Interactions;
using Nett;

namespace PrototonBot.Interactions
{
    public class GenerateCommands : InteractionModuleBase<SocketInteractionContext>
    {
        readonly Random RNG = new Random();
        readonly TomlTable vars = Toml.ReadFile(Path.Combine("Storage", "vars.toml"));

        [SlashCommand("rolldie", "[fun] Roll an X sided die depending on your choice.")]
        public async Task RollDice(
            [Summary(description: "Type of die: d4, d6, d8, d10, d12, d20")]
            [Choice("d4", "d4")]
            [Choice("d6", "d6")]
            [Choice("d8", "d8")]
            [Choice("d10", "d10")]
            [Choice("d12", "d12")]
            [Choice("d20", "d20")] String message)
        {
            var result = 0;
            switch (message.ToLower())
            {
                case "d4": result = RNG.Next(1, 5); break;
                case "d6": result = RNG.Next(1, 7); break;
                case "d8": result = RNG.Next(1, 9); break;
                case "d10": result = RNG.Next(1, 11); break;
                case "d12": result = RNG.Next(1, 13); break;
                case "d20": result = RNG.Next(1, 21); break;
                default: await RespondAsync($"Please specify the type of die you'd like to roll (read parameter tip)."); return;
            }
            await RespondAsync($"Oh wow look at that die spin! Alright, it landed on {result}!");
        }

        [SlashCommand("mood", "[fun] Get a random mood returned to you.")]
        public async Task RollMood()
        {
            var emotions = vars.Get<List<string>>("emotions");
            await RespondAsync($"{emotions[RNG.Next(emotions.Count)]}");
        }

        [SlashCommand("8ball", "[fun] Get a random 8ball fortune.")]
        public async Task EightBall([Summary(description: "Your question.")] String message)
        {
            if (!message.Contains('?'))
            {
                await RespondAsync("Hmmm... For some reason, that didn't seem like a question, right__**?**__");
                return;
            }

            var fortunes = vars.Get<List<string>>("fortunes");
            await RespondAsync($"Hmm.. {fortunes[RNG.Next(fortunes.Count)]}");
        }

        [SlashCommand("danganronpa", "[fun] Get a random scenario/murder between the cast of DR1, DR2, DRV3, and DR3")]
        public async Task DanganronpaScenario()
        {
            var danganChars = vars.Get<List<string>>("danganronpa_chars");
            var killer = danganChars[RNG.Next(danganChars.Count)];
            var victim = danganChars[RNG.Next(danganChars.Count)];
            var helper = danganChars[RNG.Next(danganChars.Count)];
            bool wasHelped = RNG.Next(0, 101) <= 17;

            while (killer == victim) victim = danganChars[RNG.Next(danganChars.Count)];

            if (wasHelped)
            {
                while (helper == killer || helper == victim) helper = danganChars[RNG.Next(danganChars.Count)];
                await RespondAsync($"__Oh no, a body has been discovered!__\n**{killer}** was revealed as the blackened who killed **{victim}**!\nPuhu... oh, and **{helper}** helped them out with it, too!");
            }
            else
            {
                await RespondAsync($"__Oh no, a body has been discovered!__\n**{killer}** was found as the blackened who killed **{victim}**!");
            }
        }
    }
}
