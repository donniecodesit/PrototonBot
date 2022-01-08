using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using PrototonBot.MongoUtil;

namespace PrototonBot.Commands
{
  public class ImageCommands : ModuleBase<SocketCommandContext>
  {
    Random rand = new Random();

    [Command("despair")]
    public async Task DespairSprite()
    {
      var images = Directory.GetFiles(Path.Combine("Storage", "IMAGES", "DANGANRONPA", "DGR1", "Despair"), "*.png");
      await Context.Channel.SendFileAsync(images[rand.Next(images.Length)]);
    }

    [Command("despair2")]
    public async Task DespairSprite2()
    {
      var images = Directory.GetFiles(Path.Combine("Storage", "IMAGES", "DANGANRONPA", "DGR2", "Despair"), "*.png");
      await Context.Channel.SendFileAsync(images[rand.Next(images.Length)]);
    }

    [Command("hope")]
    public async Task HopeSprite()
    {
      var images = Directory.GetFiles(Path.Combine("Storage", "IMAGES", "DANGANRONPA", "DGR1", "Hope"), "*.png");
      await Context.Channel.SendFileAsync(images[rand.Next(images.Length)]);
    }

    [Command("hope2")]
    public async Task HopeSprite2()
    {
      var images = Directory.GetFiles(Path.Combine("Storage", "IMAGES", "DANGANRONPA", "DGR2", "Hope"), "*.png");
      await Context.Channel.SendFileAsync(images[rand.Next(images.Length)]);
    }

    [Command("yasqueen")]
    public async Task Kamakura()
    {
      await Context.Channel.SendFileAsync(Path.Combine("Storage", "IMAGES", "DANGANRONPA", "JunkoSmile.jpg"));
    }

    [Command("kamakura")]
    public async Task Kamakura2()
    {
      await Context.Channel.SendFileAsync(Path.Combine("Storage", "IMAGES", "DANGANRONPA", "Kamakura.gif"));
    }

    [Command("explode")]
    [Alias("kaboom", "boom")]
    public async Task ExplosionCommand()
    {
      var images = Directory.GetFiles(Path.Combine("Storage", "IMAGES", "EXPLOSIONS"), "*.gif");
      await Context.Channel.SendFileAsync(images[rand.Next(images.Length)]);
    }
  }
}
