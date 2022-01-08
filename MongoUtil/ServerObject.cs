using System.Collections.Generic;

namespace PrototonBot.MongoUtil
{
  public class ServerObject
  {
    /// <summary>
    /// Server ID of the Server.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Server Name of the Server.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Whether or not the Server is public on the server list.
    /// </summary>
    public bool Public { get; set; }
    /// <summary>
    /// Whether or not the Server has Level Messages enabled.
    /// </summary>
    public bool LevelUpMessages { get; set; }
    /// <summary>
    /// The Server's custom set prefix, is "pr." by default.
    /// </summary>
    public string Prefix { get; set; }
    /// <summary>
    /// The Logging Channel for the bot in the Server.
    /// </summary>
    public string LogChannel { get; set; }
    /// <summary>
    /// A ----- of channels in the Server the bot will respond to commands in.
    /// </summary>
    public List<string> EnabledChannels { get; set; }
    /// <summary>
    /// The channel ID of the welcome/leave messages channel.
    /// </summary>
    public string WelcomeChannel { get; set; }
    /// <summary>
    /// Whether or not the Server has Welcome Messages enabled.
    /// </summary>
    public bool WelcomeMessages { get; set; }
  }
}
