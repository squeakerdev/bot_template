using Discord;
using Discord.WebSocket;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SampleBot.Helpers.Common;

public static class Mongo
{
    private static MongoClient? _mg;
    private static IMongoDatabase? _db;
    public static IMongoDatabase Db => _db ?? throw new NullReferenceException("_db is null.");
    
    /// <summary>
    ///   Initialize the MongoDB connection.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection string.</param>
    /// <param name="databaseName">The database to use.</param>
    /// <exception cref="Exception">Failed to connect.</exception>
    public static void InitMongo(string connectionString, string databaseName)
    {
        try
        {
            _mg = new MongoClient(connectionString);
            _db = _mg.GetDatabase(databaseName);
            
            Bot.LogAsync(new LogMessage(LogSeverity.Info, "Mongo", $"Connected to database `{databaseName}`."));
        }
        catch
        {
            throw new Exception("Failed to connect to MongoDB. Check your connection string and database name.");
        }
    }
    
    /// <summary>
    ///   Modify a user's balance.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="amount">The amount to modify the balance by.</param>
    /// <returns></returns>
    public static async Task<bool> ModifyUserBalanceAsync(SocketUser user, int amount)
    {
        try
        {
            // Filter by user ID.
            var filter = Builders<BsonDocument>.Filter.Eq("uid", user.Id);

            // Update the balance field.
            var update = Builders<BsonDocument>.Update.Inc("balance", amount);

            // Options to create the document if it doesn't exist.
            var options = new FindOneAndUpdateOptions<BsonDocument> {IsUpsert = true};

            // Modify the document.
            await Db.GetCollection<BsonDocument>("balances")
                .FindOneAndUpdateAsync(filter, update, options);
            
            // Log event.
            LogMessage msg = new(LogSeverity.Info, "Mongo", $"Modified user {user.Id}'s balance (+{amount}).");
            await Bot.LogAsync(msg);
            
            return true;
        } catch (Exception e)
        {
            LogMessage msg = new(LogSeverity.Error, "Mongo", $"Failed to modify user {user.Id}'s balance.", e);
            await Bot.LogAsync(msg);
            return false;
        }
    }

    /// <summary>
    ///   Get a user's balance.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The user's balance.</returns>
    public static async Task<int> GetUserBalanceAsync(SocketUser user)
    {
        try
        {
            // Filter by user ID.
            var filter = Builders<BsonDocument>.Filter.Eq("uid", user.Id);

            // Include only the balance field.
            var projection = Builders<BsonDocument>.Projection.Include("balance").Exclude("_id");

            // Get the document.
            var result = await Db.GetCollection<BsonDocument>("balances")
                .Find(filter)
                .Project(projection)
                .FirstOrDefaultAsync();

            int balance = result?["balance"].ToInt32() ?? 0;
            
            // Log event.
            LogMessage msg = new(LogSeverity.Info, "Mongo", $"Checked user {user.Id}'s balance ({balance}).");
            await Bot.LogAsync(msg);

            return balance;
        }
        catch (Exception e)
        {
            LogMessage msg = new(LogSeverity.Error, "Mongo", $"Failed to check user {user.Id}'s balance.", e);
            await Bot.LogAsync(msg);
            return 0;
        }
    }
}