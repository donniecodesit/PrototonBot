using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Nett;

namespace PrototonBot.Commands
{
  public class ArrayCommands : ModuleBase<SocketCommandContext>
  {
    Random rand = new Random();
    TomlTable vars = Toml.ReadFile(Path.Combine("Storage", "vars.toml"));

    [Command("8ball")]
    public async Task EightBall([Remainder] string input = null)
    {
      var text = vars.Get<List<string>>("fortunes");
      await Context.Channel.SendMessageAsync($"{text[rand.Next(text.Count)]} <@{Context.User.Id}>");
    }

    [Command("mood")]
    public async Task Mood()
    {
      var text = vars.Get<List<string>>("emotions");
      await Context.Channel.SendMessageAsync($"{text[rand.Next(text.Count)]} <@{Context.User.Id}>");
    }

    [Command("killing")]
    public async Task KillingShuffle()
    {
      var seriesChars = vars.Get<List<string>>("series_members");
      var killer = seriesChars[rand.Next(seriesChars.Count)];
      var victim = seriesChars[rand.Next(seriesChars.Count)];
      var helper = seriesChars[rand.Next(seriesChars.Count)];
      bool wasHelp = rand.Next(0, 101) <= 17 ? true : false;
      while (killer == victim)
      {
        victim = seriesChars[rand.Next(seriesChars.Count)];
      }
      if (!wasHelp)
      {
        await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!");
      }
      else
      {
        while (helper == killer || helper == victim) helper = seriesChars[rand.Next(seriesChars.Count)];
        await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!\nPuhuhu, and **{helper}** helped them commit the murder!");
      }
    }

    [Command("killingRP")]
    public async Task KillingRPShuffle()
    {
      var seriesChars = vars.Get<List<string>>("rp_members");
      var killer = seriesChars[rand.Next(seriesChars.Count)];
      var victim = seriesChars[rand.Next(seriesChars.Count)];
      var helper = seriesChars[rand.Next(seriesChars.Count)];
      bool wasHelp = rand.Next(0, 101) <= 17 ? true : false;
      while (killer == victim)
      {
        victim = seriesChars[rand.Next(seriesChars.Count)];
      }
      if (!wasHelp)
      {
        await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!");
      }
      else
      {
        while (helper == killer || helper == victim) helper = seriesChars[rand.Next(seriesChars.Count)];
        await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!\nPuhuhu, and **{helper}** helped them commit the murder!");
      }
    }

    [Command("killingALL")]
    public async Task KillingALLShuffle()
    {
      var seriesChars = vars.Get<List<string>>("all_members");
      var killer = seriesChars[rand.Next(seriesChars.Count)];
      var victim = seriesChars[rand.Next(seriesChars.Count)];
      var helper = seriesChars[rand.Next(seriesChars.Count)];
      bool wasHelp = rand.Next(0, 101) <= 17 ? true : false;
      while (killer == victim)
      {
        victim = seriesChars[rand.Next(seriesChars.Count)];
      }
      if (!wasHelp)
      {
        await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!");
      }
      else
      {
        while (helper == killer || helper == victim) helper = seriesChars[rand.Next(seriesChars.Count)];
        await Context.Channel.SendMessageAsync($"Oh no, a murder has occured!\n**{killer}** was found guilty of murdering **{victim}**!\nPuhuhu, and **{helper}** helped them commit the murder!");
      }
    }

    [Command("roll")]
    public async Task RollDice(string input = null)
    {
      if (input == null) { await Context.Channel.SendMessageAsync($"Please specify the type of die you'd like to roll!\nOptions: d4 d6 d8 d10 d12 d20"); return; }
      if (input.ToLower() == "d4") { await Context.Channel.SendMessageAsync($"Oh wow, look at that die go! It landed on {rand.Next(1, 5)}"); return; }
      if (input.ToLower() == "d6") { await Context.Channel.SendMessageAsync($"Oh wow, look at that die go! It landed on {rand.Next(1, 7)}"); return; }
      if (input.ToLower() == "d8") { await Context.Channel.SendMessageAsync($"Oh wow, look at that die go! It landed on {rand.Next(1, 9)}"); return; }
      if (input.ToLower() == "d10") { await Context.Channel.SendMessageAsync($"Oh wow, look at that die go! It landed on {rand.Next(1, 11)}"); return; }
      if (input.ToLower() == "d12") { await Context.Channel.SendMessageAsync($"Oh wow, look at that die go! It landed on {rand.Next(1, 13)}"); return; }
      if (input.ToLower() == "d20") { await Context.Channel.SendMessageAsync($"Oh wow, look at that die go! It landed on {rand.Next(1, 21)}"); return; }
      else await Context.Channel.SendMessageAsync($"Please specify the type of die you'd like to roll!\nOptions: d4 d6 d8 d10 d12 d20");
    }
  }
}
