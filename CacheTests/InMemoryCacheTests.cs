using Cache_LLD;
using Cache_LLD.Logger;
using Cache_LLD.Policies;
using Cache_LLD.Storage;

namespace CacheTests
{
    [TestClass]
    public class InMemoryCacheTests
    {
        Cache<int, int> cache;
        IEvictionPolicy<int> policy;
        IStorage<int,  int> storage;
        ILog logger;

        [TestInitialize]
        public void SetUp() {
            policy = new LruEvictionPolicy<int>();
            storage = new DictionaryStorage<int, int>();
            logger = new ConsoleLogger();
            cache = new Cache<int, int>(storage, policy, logger, 4);
        }


        [TestMethod]
        public void PutItemsInCache() {
            cache.Put(1, 101);
            cache.Put(2, 201);

            Assert.AreEqual(101, cache.Get(1));
            Assert.AreEqual(201, cache.Get(2));
        }

        [TestMethod]
        public void GetItemsInCache() {
            cache.Put(1, 101);
            cache.Put(2, 201);

            Assert.AreEqual(101, cache.Get(1));
            Assert.AreEqual(201, cache.Get(2));
        }

        [TestMethod]
        public void RemoveItemsFromCache() {
            cache.Put(1, 101);
            cache.Put(2, 201);
            cache.Remove(1);
            Assert.AreEqual(default(int), cache.Get(1));
        }

        [TestMethod]
        public void ItemsGetRemovedIfCacheCapacityIsReached() {
            cache.Put(1, 101);
            cache.Put(2, 201);
            cache.Put(3, 101);
            cache.Put(4, 201);
            cache.Put(5, 101);
            cache.Put(6, 201);

            Assert.AreEqual(default(int), cache.Get(1));
        }

        [TestMethod]
        public void OnlyLeastRecentlyUsedItemGetEvictedOnFullCapacity() {
            cache.Put(1, 101);
            cache.Put(2, 201);
            cache.Put(3, 101);
            cache.Put(1, 201);
            cache.Put(2, 101);
            cache.Put(4, 201);
            cache.Put(5, 201);

            Assert.AreEqual(default(int), cache.Get(3));
        }
    }
}
