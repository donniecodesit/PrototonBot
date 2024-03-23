using System.Collections.Generic;

namespace PrototonBot.MongoUtility
{
    public class UserObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long Money { get; set; }
        public long LastDaily { get; set; }
        public long DailyStreak { get; set; }
        public int Level { get; set; }
        public long EXP { get; set; }
        public long LastMessage { get; set; }
        public int PatsReceived { get; set; }
        public int LastPat { get; set; }
        public int Purchases { get; set; }
        public string Description { get; set; }
        public string Partner { get; set; }
        public bool Mutuals { get; set; }
        public bool Boosted { get; set; }
        public long LastGamble { get; set; }
        public int Gambles { get; set; }
        public int GamblesWon { get; set; }
        public int GamblesLost { get; set; }
        public int GamblesNetGain { get; set; }
        public int GamblesNetLoss { get; set; }
        public int Luck { get; set; }
        public int DailyBonus { get; set; }
        public int Donations { get; set; }
        public long TransferIn { get; set; }
        public long TransferOut { get; set; }
        public List<string> BadgeSlots { get; set; }
        public List<string> RedeemedCodes { get; set; }
    }

    public class ServerObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Public { get; set; }
        public bool LevelUpMessages { get; set; }
        public string LogChannel { get; set; }
        public List<string> EnabledChannels { get; set; }
        public string WelcomeChannel { get; set; }
        public string LeaveChannel { get; set; }
        public bool WelcomeMessages { get; set; }
        public bool LeaveMessages { get; set; }
        public bool LogDeletedMessages { get; set; }
        public bool LogUpdatedMessages { get; set; }
    }

    public class InventoryObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int DailyCoins { get; set; }
        public int DailyCoinsTotal { get; set; }
        public int PatCoins { get; set; }
        public int PatCoinsTotal { get; set; }
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
        public List<string> OwnedThemes { get; set; }
        public string PickedTheme { get; set; }
        public List<string> OwnedBadges { get; set; }
    }
}