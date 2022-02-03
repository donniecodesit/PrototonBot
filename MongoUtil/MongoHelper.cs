using Discord;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver.Linq;
using System.Net.NetworkInformation;

namespace PrototonBot.MongoUtil
{
  public class MongoHelper
  {
    private static MongoClient mongoClient = new MongoClient(Program.MongoURL);
    private static IMongoDatabase usersDatabase = mongoClient.GetDatabase("users");
    private static IMongoDatabase serversDatabase = mongoClient.GetDatabase("servers");
    private static IMongoCollection<UserObject> userInfoCollection = usersDatabase.GetCollection<UserObject>("info");
    private static IMongoCollection<InventoryObject> userInventoryCollection = usersDatabase.GetCollection<InventoryObject>("inventory");
    private static IMongoCollection<ServerObject> serverDataCollection = serversDatabase.GetCollection<ServerObject>("info");

    public static Task<IAggregateFluent<UserObject>> SearchUsers(FilterDefinition<UserObject> filter)
    {
      return Task.FromResult(userInfoCollection.Aggregate().Match(filter));
    }

    public static Task<IAggregateFluent<InventoryObject>> SearchInvs(FilterDefinition<InventoryObject> filter)
    {
      return Task.FromResult(userInventoryCollection.Aggregate().Match(filter));
    }

    public static Task<IAggregateFluent<ServerObject>> SearchSvrs(FilterDefinition<ServerObject> filter)
    {
      return Task.FromResult(serverDataCollection.Aggregate().Match(filter));
    }

    public static Task<ServerObject> GetServer(string serverId)
    {
      var query =
          from obj in serverDataCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
          where obj.Id == serverId
          select obj;
      if (!query.Any()) return Task.FromResult<ServerObject>(null);
      return Task.FromResult<ServerObject>(query.First());
    }

    public static Task<UserObject> GetUser(string userId)
    {
      var query =
          from obj in userInfoCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
          where obj.Id == userId
          select obj;
      if (!query.Any()) return Task.FromResult<UserObject>(null);
      return Task.FromResult<UserObject>(query.First());
    }

    public static Task<InventoryObject> GetInventory(string userId)
    {
      var query =
          from obj in userInventoryCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
          where obj.Id == userId
          select obj;
      if (!query.Any()) return Task.FromResult<InventoryObject>(null);
      return Task.FromResult<InventoryObject>(query.First());
    }

    public static Task<IEnumerable<UserObject>> GetLeaderboardTopMoney()
    {
      var query =
          from c in userInfoCollection.AsQueryable<UserObject>(new AggregateOptions { AllowDiskUse = true })
          orderby c.Money descending
          select c;
      var objects = query.Take(15);
      return Task.FromResult(objects.AsEnumerable());
    }

    public static Task<IEnumerable<UserObject>> GetLeaderboardTopLevels()
    {
      var query =
          from c in userInfoCollection.AsQueryable<UserObject>(new AggregateOptions { AllowDiskUse = true })
          orderby c.Level descending
          select c;
      var objects = query.Take(15);
      return Task.FromResult(objects.AsEnumerable());
    }

    public static Task<IEnumerable<UserObject>> GetTotalUserCount()
    {
      var query =
          from c in userInfoCollection.AsQueryable<UserObject>(new AggregateOptions { AllowDiskUse = true })
          orderby c.Name descending
          select c;
      var objects = query;
      return Task.FromResult(objects.AsEnumerable());
    }

    public static Task<IMongoQueryable<ServerObject>> GetServerPublicity()
    {
      var query =
          from c in serverDataCollection.AsQueryable<ServerObject>(new AggregateOptions { AllowDiskUse = true })
          select c;
      var objects = query;
      return Task.FromResult(objects);
    }

    public static Task CreateUser(IUser user)
    {
      if (MongoHelper.GetUser(user.Id.ToString()).Result != null) return Task.CompletedTask;
      var userData = new UserObject();
      userData.Id = user.Id.ToString();
      userData.Name = user.Username.ToString();
      userData.Money = 500;
      userData.LastDaily = 0;
      userData.DailyStreak = 0;
      userData.Level = 0;
      userData.EXP = 0;
      userData.LastMessage = 0;
      userData.PatsReceived = 0;
      userData.LastPat = 0;
      userData.Purchases = 0;
      userData.Description = "This user has not set a description.";
      userData.Partner = "None";
      userData.Mutuals = false;
      userData.Boosted = false;
      userData.LastGamble = 0;
      userData.Gambles = 0;
      userData.GamblesWon = 0;
      userData.GamblesLost = 0;
      userData.GamblesNetGain = 0;
      userData.GamblesNetLoss = 0;
      userData.Luck = 0;
      userData.DailyBonus = 0;
      userData.Donations = 0;
      userData.TransferIn = 0;
      userData.TransferOut = 0;

      userInfoCollection.InsertOne(userData);

      var userInventory = new InventoryObject();
      userInventory.Id = user.Id.ToString();
      userInventory.Name = user.Username.ToString();
      userInventory.DailyCoins = 0;
      userInventory.DailyCoinsTotal = 0;
      userInventory.PatCoins = 0;
      userInventory.PatCoinsTotal = 0;
      userInventory.HugCoins = 0;
      userInventory.HugCoinsTotal = 0;
      userInventory.GambleCoins = 0;
      userInventory.GambleCoinsTotal = 0;
      userInventory.Axes = 0;
      userInventory.AxeUses = 0;
      userInventory.Wrenches = 0;
      userInventory.WrenchUses = 0;
      userInventory.Picks = 0;
      userInventory.PickUses = 0;
      userInventory.Paperclips = 0;
      userInventory.Bricks = 0;
      userInventory.Diamonds = 0;
      userInventory.Bulbs = 0;
      userInventory.CDs = 0;
      userInventory.Logs = 0;
      userInventory.Leaves = 0;
      userInventory.Bolts = 0;
      userInventory.Gears = 0;
      userInventory.LastChop = 0;
      userInventory.LastMine = 0;
      userInventory.LastSalvage = 0;
      userInventory.OwnedThemes = new List<string>();
      userInventory.PickedTheme = "";

      userInventoryCollection.InsertOne(userInventory);

      return Task.CompletedTask;
    }

    public static Task CreateServer(IGuild server)
    {
      if (MongoHelper.GetServer(server.Id.ToString()).Result != null) return Task.CompletedTask;
      var serverData = new ServerObject();
      serverData.Id = server.Id.ToString();
      serverData.Name = server.Name.ToString();
      serverData.Public = true;
      serverData.LevelUpMessages = true;
      serverData.Prefix = "pr.";
      serverData.EnabledChannels = new List<string>();
      serverData.LogChannel = "";
      serverData.WelcomeChannel = "";
      serverData.WelcomeMessages = false;

      serverDataCollection.InsertOne(serverData);
      return Task.CompletedTask;
    }

    public static Task DeleteServer(IGuild server)
    {
      var serverDataCollection = serversDatabase.GetCollection<ServerObject>("info");
      var query =
          from obj in serverDataCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
          where obj.Id == server.Id.ToString()
          select obj;
      if (!query.Any()) return Task.CompletedTask;

      query =
          from obj in serverDataCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
          where obj.Id == server.Id.ToString()
          select obj;
      serverDataCollection.DeleteOne(query.First().ToBsonDocument());
      return Task.CompletedTask;
    }

    public static Task UpdateUser(string userId, string field, dynamic value)
    {
      var userObj = MongoHelper.GetUser(userId);
      var filter = Builders<UserObject>.Filter.Eq("_id", userId);
      var update = Builders<UserObject>.Update.Set(field, value);
      userInfoCollection.UpdateOne(filter, update);

      return Task.CompletedTask;
    }

    public static Task UpdateInventory(string userId, string field, dynamic value)
    {
      var inventoryObj = MongoHelper.GetInventory(userId);
      var filter = Builders<InventoryObject>.Filter.Eq("_id", userId);
      var update = Builders<InventoryObject>.Update.Set(field, value);
      userInventoryCollection.UpdateOne(filter, update);
      return Task.CompletedTask;
    }

    public static Task UpdateServer(string serverId, string field, dynamic value)
    {
      var serverObj = MongoHelper.GetServer(serverId);
      var filter = Builders<ServerObject>.Filter.Eq("_id", serverId);
      var update = Builders<ServerObject>.Update.Set(field, value);
      serverDataCollection.UpdateOne(filter, update);
      return Task.CompletedTask;
    }
  }
}
