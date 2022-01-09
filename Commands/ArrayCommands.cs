using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Nett;

namespace PrototonBot.Commands
{
  public class ArrayCommands : ModuleBase<SocketCommandContext> {
    Random rand = new Random();
    TomlTable vars = Toml.ReadFile(Path.Combine("Storage", "vars.toml"));

    //Reply with a shuffled response from a list.
    [Command("8ball")]
    public async Task EightBall([Remainder] string input = null) {
      var text = vars.Get<List<string>>("fortunes");
      await Context.Channel.SendMessageAsync($"{text[rand.Next(text.Count)]} <@{Context.User.Id}>");
    }

    //Reply with a shuffled mood from a list.
    [Command("mood")]
    public async Task Mood() {
      var text = vars.Get<List<string>>("emotions");
      await Context.Channel.SendMessageAsync($"{text[rand.Next(text.Count)]} <@{Context.User.Id}>");
    }

    //Reply with a scenario murder from a list of characters.
    [Command("murdering")]
    public async Task MurderShuffle() {
      var seriesChars = vars.Get<List<string>>("series_members");
      await ScenarioShuffle(seriesChars);
    }

    //Reply with a scenario murder from a list of characters.
    [Command("murdering-rp")]
    public async Task MurderRPShuffle() {
      var seriesChars = vars.Get<List<string>>("rp_members");
      await ScenarioShuffle(seriesChars);
    }

    //Reply with a scenario murder from a list of characters.
    [Command("murdering-all")]
    public async Task MurderALLShuffle() {
      var seriesChars = vars.Get<List<string>>("all_members");
      await ScenarioShuffle(seriesChars);
    }

    //Reply with a random number depending on the type of die mentioned
    [Command("roll")]
    public async Task RollDice(string input = null) {
      if (input == null) { await Context.Channel.SendMessageAsync($"Please specify the type of die you'd like to roll!\nOptions: d4 d6 d8 d10 d12 d20"); return; }
      var result = 0;
      switch(input.ToLower()) {
        case "d4": result = rand.Next(1, 5); break;
        case "d6": result = rand.Next(1, 7); break;
        case "d8": result = rand.Next(1, 9); break;
        case "d10": result = rand.Next(1, 11); break;
        case "d12": result = rand.Next(1, 13); break;
        case "d20": result = rand.Next(1, 21); break;
        default: await Context.Channel.SendMessageAsync($"Please specify the type of die you'd like to roll!\nOptions: d4 d6 d8 d10 d12 d20"); return;
      }
      await Context.Channel.SendMessageAsync($"Oh wow, look at that die go! It landed on {result}");
    }

    //Helper function for the Murder scenarios
    private async Task ScenarioShuffle(List<string> seriesChars) {
      var killer = seriesChars[rand.Next(seriesChars.Count)];
      var victim = seriesChars[rand.Next(seriesChars.Count)];
      var helper = seriesChars[rand.Next(seriesChars.Count)];
      bool wasHelp = rand.Next(0, 101) <= 17 ? true : false;
      while (killer == victim) victim = seriesChars[rand.Next(seriesChars.Count)];
      if (!wasHelp) await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!");
      else {
        while (helper == killer || helper == victim) helper = seriesChars[rand.Next(seriesChars.Count)];
        await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!\nPuhuhu, and **{helper}** helped them commit the murder!");
      }
    }
  }
}