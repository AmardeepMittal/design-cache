using Cache_LLD.Policies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheTests
{
    [TestClass]
    public class LruEvictiontionPolicyTests
    {
        public LruEvictionPolicy<int> evictionPolicy = null;

        [TestInitialize]
        public void SetUp() {
            evictionPolicy = new LruEvictionPolicy<int>();
        }

        [TestMethod]
        public void NoKeyToEvict() {
            var result = evictionPolicy.Evict();
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void EvictionHappensByLeastRecentlyUsedKey() {
            var task1 = evictionPolicy.KeyAccessedAsync(1);
            var task2 = evictionPolicy.KeyAccessedAsync(2);
            var task3 = evictionPolicy.KeyAccessedAsync(3);
            var task4 = evictionPolicy.KeyAccessedAsync(4);
            Task.WaitAll(new[] { task2, task3, task4, task1 });

            Assert.AreEqual(1, evictionPolicy.Evict());
            Assert.AreEqual(2, evictionPolicy.Evict());
            Assert.AreEqual(3, evictionPolicy.Evict());
            Assert.AreEqual(4, evictionPolicy.Evict());
        }

        [TestMethod]
        public void AccessingKeyPutsItAsFirstKeyAccessed() {
            var tas1 = evictionPolicy.KeyAccessedAsync(1);
            var task2 = evictionPolicy.KeyAccessedAsync(2);
            var task3 = evictionPolicy.KeyAccessedAsync(3);
            var task4 = evictionPolicy.KeyAccessedAsync(1);
            Task.WaitAll(new[] { task2, task3, task4, tas1 });
            Assert.AreEqual(2, evictionPolicy.Evict());
            Assert.AreEqual(3, evictionPolicy.Evict());
            Assert.AreEqual(1, evictionPolicy.Evict());
        }

        [TestMethod]
        public void RemovingInvalidKey() {
            var result = evictionPolicy.Evict(1);
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void RemovingValidKey() {
            var task1 = evictionPolicy.KeyAccessedAsync(1);
            var task2 = evictionPolicy.KeyAccessedAsync(2);
            Task.WaitAll(new[] { task1, task2});
            Assert.AreEqual(1, evictionPolicy.Evict(1));
            Assert.AreEqual(2, evictionPolicy.Evict(2));
        }
    }
}
