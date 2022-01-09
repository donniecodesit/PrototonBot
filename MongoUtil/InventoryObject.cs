namespace PrototonBot.MongoUtil
{
  public class InventoryObject
  {
    /// <summary>
    /// Discord ID of the User.
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// Discord Username of the User.
    /// </summary>
    public string Name { get; set; }
    public int DailyCoins { get; set; }
    public int DailyCoinsTotal { get; set; }
    public int PatCoins { get; set; }
    public int PatCoinsTotal { get; set; }
    public int HugCoins { get; set; }
    public int HugCoinsTotal { get; set; }
    public int GambleCoins { get; set; }
    public int GambleCoinsTotal { get; set; }
    public int Axes { get; set; }
    public int AxeUses { get; set; }
    public int Wrenches { get; set; }
    public int WrenchUses { get; set; }
    public int Picks { get; set; }
    public int PickUses { get; set; }
    public int Paperclips { get; set; }
    public int Bricks { get; set; }
    public int Diamonds { get; set; }
    public int Bulbs { get; set; }
    public int CDs { get; set; }
    public int Logs { get; set; }
    public int Leaves { get; set; }
    public int Bolts { get; set; }
    public int Gears { get; set; }
    public int LastChop { get; set; }
    public int LastMine { get; set; }
    public int LastSalvage { get; set; }
  }
}
