using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache_LLD.Locking
{
    ///Overall for low contention scenarios where locked region gets executed 
    ///very quickly and there are no heavy operations during locking.
    public class DefaultLockingManager<TKey> where TKey : IEquatable<TKey>
    {
        private static readonly ConcurrentDictionary<TKey, object> _locks = new ConcurrentDictionary<TKey, object>();

        // Singleton instance
        public static DefaultLockingManager<TKey> Instance { get; } = new DefaultLockingManager<TKey>();

        private DefaultLockingManager() { }

        // Returns the lock object for a given key
        public object GetSyncObject(TKey key)
        {
            return _locks.GetOrAdd(key, _ => new object());
        }

        // Acquires a lock for the given key (blocks until acquired)
        //This can be used in low contention scenarios. For high contention
        //its better to use Timeout along with this.
        public void AcquireLock(TKey key)
        {
            var syncObj = GetSyncObject(key);
            System.Threading.Monitor.Enter(syncObj);
        }

        public bool TryAcquireLock(TKey key, int millisecondsTimeout)
        {
            var syncObj = GetSyncObject(key);
            bool isTaken = false;
            //isTaken will be true if the current thread is able to get the lock on the key.
            System.Threading.Monitor.TryEnter(syncObj, millisecondsTimeout, ref isTaken);
            return isTaken;
        }

        // Releases a lock for the given key
        public void ReleaseLock(TKey key)
        {
            var syncObj = GetSyncObject(key);
            System.Threading.Monitor.Exit(syncObj);
        }
    }
}
