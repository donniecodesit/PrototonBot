using Discord;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace PrototonBot.MongoUtility
{
    public class MongoHandler
    {
        private static MongoClient _mongo = new MongoClient(Program.MongoURL);
        private static IMongoDatabase dbUsers = _mongo.GetDatabase("users");
        private static IMongoDatabase dbServers = _mongo.GetDatabase("servers");
        private static IMongoCollection<UserObject> userCollection = dbUsers.GetCollection<UserObject>("info");
        private static IMongoCollection<InventoryObject> userInvCollection = dbUsers.GetCollection<InventoryObject>("inventory");
        private static IMongoCollection<ServerObject> serverCollection = dbServers.GetCollection<ServerObject>("info");

        public static Task<IAggregateFluent<UserObject>> SearchUsers(FilterDefinition<UserObject> filter)
        {
            return Task.FromResult(userCollection.Aggregate().Match(filter));
        }

        public static Task<IAggregateFluent<InventoryObject>> SearchInvs(FilterDefinition<InventoryObject> filter)
        {
            return Task.FromResult(userInvCollection.Aggregate().Match(filter));
        }

        public static Task<IAggregateFluent<ServerObject>> SearchSvrs(FilterDefinition<ServerObject> filter)
        {
            return Task.FromResult(serverCollection.Aggregate().Match(filter));
        }

        public static Task<ServerObject> GetServer(string serverId)
        {
            var query =
                from obj in serverCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
                where obj.Id == serverId
                select obj;
            return query.Any() ? Task.FromResult<ServerObject>(query.First()) : Task.FromResult<ServerObject>(null);
        }

        public static Task<UserObject> GetUser(string userId)
        {
            var query =
                from obj in userCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
                where obj.Id == userId
                select obj;
            return query.Any() ? Task.FromResult<UserObject>(query.First()) : Task.FromResult<UserObject>(null);
        }

        public static Task<InventoryObject> GetInventory(string userId)
        {
            var query =
                from obj in userInvCollection.AsQueryable(new AggregateOptions { AllowDiskUse = true })
                where obj.Id == userId
                select obj;
            return query.Any() ? Task.FromResult<InventoryObject>(query.First()) : Task.FromResult<InventoryObject>(null);
        }

        public static Task<IEnumerable<UserObject>> GetRichestUsers()
        {
            var query =
                from c in userCollection.AsQueryable<UserObject>(new AggregateOptions { AllowDiskUse = true })
                orderby c.Money descending
                select c;
            var objects = query.Take(15);
            return Task.FromResult(objects.AsEnumerable());
        }

        public static Task<IEnumerable<UserObject>> GetExperiencedUsers()
        {
            var query =
                from c in userCollection.AsQueryable<UserObject>(new AggregateOptions { AllowDiskUse = true })
                orderby c.Level descending
                select c;
            var objects = query.Take(15);
            return Task.FromResult(objects.AsEnumerable());
        }

        public static Task<IEnumerable<UserObject>> GetTotalUserCount()
        {
            var query =
                from c in userCollection.AsQueryable<UserObject>(new AggregateOptions { AllowDiskUse = true })
                orderby c.Name descending
                select c;
            var objects = query;
            return Task.FromResult(objects.AsEnumerable());
        }

        public static Task<IMongoQueryable<ServerObject>> GetServerPublicity()
        {
            var query =
                from c in serverCollection.AsQueryable<ServerObject>(new AggregateOptions { AllowDiskUse = true })
                select c;
            var objects = query;
            return Task.FromResult(objects);
        }

        public static Task CreateNewUser(IUser user)
        {
            var userObj = new UserObject();
            userObj.Id = user.Id.ToString();
            userObj.Name = user.Username.ToString();
            userObj.Money = 500;
            userObj.LastDaily = 0;
            userObj.DailyStreak = 0;
            userObj.Level = 0;
            userObj.EXP = 0;
            userObj.LastMessage = 0;
            userObj.PatsReceived = 0;
            userObj.LastPat = 0;
            userObj.Purchases = 0;
            userObj.Description = "This user has not set a description.";
            userObj.Partner = "None";
            userObj.Mutuals = false;
            userObj.Boosted = false;
            userObj.LastGamble = 0;
            userObj.Gambles = 0;
            userObj.GamblesWon = 0;
            userObj.GamblesLost = 0;
            userObj.GamblesNetGain = 0;
            userObj.GamblesNetLoss = 0;
            userObj.Luck = 0;
            userObj.DailyBonus = 0;
            userObj.Donations = 0;
            userObj.TransferIn = 0;
            userObj.TransferOut = 0;
            userObj.BadgeSlots = new List<string>() { "none", "none", "none", "none", "none", "none", "none", "none", "none" };
            userObj.RedeemedCodes = new List<string>();
            userCollection.InsertOne(userObj);

            var userInv = new InventoryObject();
            userInv.Id = user.Id.ToString();
            userInv.Name = user.Username.ToString();
            userInv.DailyCoins = 0;
            userInv.DailyCoinsTotal = 0;
            userInv.PatCoins = 0;
            userInv.PatCoinsTotal = 0;
            userInv.GambleCoins = 0;
            userInv.GambleCoinsTotal = 0;
            userInv.Axes = 0;
            userInv.AxeUses = 0;
            userInv.Wrenches = 0;
            userInv.WrenchUses = 0;
            userInv.Picks = 0;
            userInv.PickUses = 0;
            userInv.Paperclips = 0;
            userInv.Bricks = 0;
            userInv.Diamonds = 0;
            userInv.Bulbs = 0;
            userInv.CDs = 0;
            userInv.Logs = 0;
            userInv.Leaves = 0;
            userInv.Bolts = 0;
            userInv.Gears = 0;
            userInv.LastChop = 0;
            userInv.LastMine = 0;
            userInv.LastSalvage = 0;
            userInv.OwnedThemes = new List<string>();
            userInv.PickedTheme = "";
            userInv.OwnedBadges = new List<string>();
            userInvCollection.InsertOne(userInv);

            return Task.CompletedTask;
        }

        public static Task CreateNewServer(IGuild guild)
        {
            if (MongoHandler.GetServer(guild.Id.ToString()).Result != null) return Task.CompletedTask;
            var data = new ServerObject();
            data.Id = guild.Id.ToString();
            data.Name = guild.Name.ToString();
            data.Public = true;
            data.LevelUpMessages = true;
            data.EnabledChannels = new List<string>();
            data.LogChannel = "";
            data.WelcomeChannel = "";
            data.WelcomeMessages = false;
            serverCollection.InsertOne(data);
            return Task.CompletedTask;
        }

        public static Task DeleteServer(IGuild guild)
        {
            var serverList = dbServers.GetCollection<ServerObject>("info");
            var query =
                from obj in serverList.AsQueryable(new AggregateOptions { AllowDiskUse = true })
                where obj.Id == guild.Id.ToString()
                select obj;
            if (!query.Any()) return Task.CompletedTask;

            query =
                from obj in serverList.AsQueryable(new AggregateOptions { AllowDiskUse = true })
                where obj.Id == guild.Id.ToString()
                select obj;
            serverList.DeleteOne(query.First().ToBsonDocument());
            return Task.CompletedTask;
        }

        public static Task UpdateUser(string userId, string key, dynamic value)
        {
            var userObj = MongoHandler.GetUser(userId);
            var filter = Builders<UserObject>.Filter.Eq("_id", userId);
            var update = Builders<UserObject>.Update.Set(key, value);
            userCollection.UpdateOne(filter, update);
            return Task.CompletedTask;
        }

        public static Task UpdateInventory(string userId, string key, dynamic value)
        {
            var inventoryObj = MongoHandler.GetInventory(userId);
            var filter = Builders<InventoryObject>.Filter.Eq("_id", userId);
            var update = Builders<InventoryObject>.Update.Set(key, value);
            userInvCollection.UpdateOne(filter, update);
            return Task.CompletedTask;
        }

        public static Task UpdateServer(string serverId, string key, dynamic value)
        {
            var serverObj = MongoHandler.GetServer(serverId);
            var filter = Builders<ServerObject>.Filter.Eq("_id", serverId);
            var update = Builders<ServerObject>.Update.Set(key, value);
            serverCollection.UpdateOne(filter, update);
            return Task.CompletedTask;
        }
    }
}
