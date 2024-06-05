using Integration.Backend;
using Integration.Common;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Integration.Service
{
    public sealed class ItemIntegrationService
    {
        private ItemOperationBackend ItemIntegrationBackend { get; set; } = new();

        // Change to whatever the link is
        private string ConnectionString = "localhost:6379";

        private readonly ConnectionMultiplexer redis;
        private readonly IDatabase database;
        private readonly RedLockFactory redLockFactory;

        // Use SetScan to retrieve all keys from the Redis cache
        IEnumerable<RedisKey> ItemKeys {get =>redis.GetServer("localhost:6379").Keys();  }
    public ItemIntegrationService()
        {
            // Initialize Redis connection
            redis = ConnectionMultiplexer.Connect(ConnectionString);
            database = redis.GetDatabase(0);

            var multiplexers = new List<RedLockMultiplexer> { redis };
            redLockFactory = RedLockFactory.Create(multiplexers);
        }

        public Result SaveItem(string itemContent)
        {
            var lockKey = $"lock:item:{itemContent}";
            var lockExpiry = TimeSpan.FromSeconds(30);
  

          

            using (var redLock = redLockFactory.CreateLock(lockKey, lockExpiry))
            {
                if (redLock.IsAcquired)
                {
                    var keys = ItemKeys;
                    // Check if an item with the same content already exists
                    var existingItemKey = keys.FirstOrDefault(key => database.StringGet(key) == itemContent);


                   

                    if (!string.IsNullOrEmpty(existingItemKey))
                    {
                        // Item with the same content already exists, return an error
                        if(ItemIntegrationBackend.FindItemsWithContent(itemContent).Count==0)
                        ItemIntegrationBackend.SaveItem(itemContent);

                        Console.WriteLine($"Item with content {itemContent} already exists with id {int.Parse(existingItemKey)}");
                        return new Result(false, $"Item with content {itemContent} already exists with id {int.Parse(existingItemKey)}");
                    }

                    // Save item to backend

                    var item = ItemIntegrationBackend.SaveItem(itemContent);
                    // Save item data to Redis cache
                    database.StringSet(item.Id.ToString(), item.Content);

                    return new Result(true, $"Item with content {itemContent} saved with id {item.Id}");
                }
                else
                {
                    // Failed to acquire lock
                    return new Result(false, "Could not acquire lock, please try again.");
                }
            }
        }

        public List<Item> GetDatabase()
        {
            var database = redis.GetDatabase();

            // Fetch items from cache or backend
            var items = new List<Item>();
            foreach (var itemKey in ItemKeys)
            {
                var content = database.StringGet(itemKey);
                if (!content.IsNull)
                {
                    if (int.TryParse(itemKey, out int id))
                    {
                        var item = new Item { Id = int.Parse(itemKey), Content = content };
                        items.Add(item);
                    }

                }

            }

            //just sorting to get a better look
            items = items.OrderBy(x => x.Id).ToList();
            return items;
        }

        public List<Item> GetAllItems()
        {
            return ItemIntegrationBackend.GetAllItems();
        }
    }
}
