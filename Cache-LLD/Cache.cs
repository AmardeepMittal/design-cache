using Cache_LLD.Logger;
using Cache_LLD.Policies;
using Cache_LLD.Storage;
using System.Threading;
using Cache_LLD.Locking;

namespace Cache_LLD;

public class Cache<TKey, TValue> where TKey : notnull 
{
    private readonly IEvictionPolicy<TKey> _policy;
    private readonly IStorage<TKey, TValue> _storage;
    private readonly int _capacity;
    private bool _disposed;
    private readonly ILog _log;

    private readonly ReaderWriterLockingManager<TKey> _keyLockingManager;

    /// <summary>
    /// Uses <see cref="Cache_LLD.Storage.IStorage{TKey, TValue}"/> to abstract how/where data is stored,
    /// allowing the cache to depend only on required operations and swap implementations without changes.
    /// </summary>
    /// <param name="storage">Backing <see cref="Cache_LLD.Storage.IStorage{TKey, TValue}"/>.</param>
    /// <param name="evictionPolicy">Eviction policy used when capacity is reached.</param>
    /// <param name="log">Logger.</param>
    /// <param name="capacity">Maximum number of items to keep.</param>
    /// <seealso cref="Cache_LLD.Storage.IStorage{TKey, TValue}"/>
    public Cache(IStorage<TKey, TValue> storage, IEvictionPolicy<TKey> evictionPolicy, ILog log, int capacity)
    {
        _storage = storage;
        _policy = evictionPolicy;
        _log = log;
        _capacity = capacity;
        _keyLockingManager = new ReaderWriterLockingManager<TKey>();
    }

    public TValue? Get(TKey key)
    {
        if (!_storage.ContainsKey(key))
        {
            _log.Log($"Get: {key} Key not found");
           return default(TValue?);
        }

        using (_keyLockingManager.AcquireReadLock(key))
        {
            _log.Log($"Get: {key} recieved");
            _policy.KeyAccessedAsync(key);
            var value = _storage.Get(key);
            _log.Log($"Get: {key}  Value:{value} successful");
            return value;
        }
    }

    public void Put(TKey key, TValue value)
    {
        using (_keyLockingManager.AcquireWriteLock(key))
        {
            _log.Log($"Put: {key}  Value:{value} recieved");
            if (!_storage.ContainsKey(key) && _storage.Count() >= _capacity)
            {
                TKey? evictedKey = _policy.Evict();
                if (evictedKey == null)
                {
                    throw new Exception("Evicted key is null!. Key cannot be null.");
                }
                _log.Log($"Put: {key} . Evicted: {evictedKey}");
                _storage.Remove(evictedKey);
            }
            _storage.Put(key, value);
            _policy.KeyAccessedAsync(key);
            _log.Log($"Put: {key} successful");
        }
        
    }

    public void Remove(TKey key)
    {
        if (!_storage.ContainsKey(key)) {
            throw new Exception("Key not found");
        }

        using (_keyLockingManager.AcquireWriteLock(key))
        {
            if (_storage.ContainsKey(key))
            {
                _log.Log($"Remove: Key removed: {key}");
                _storage.Remove(key);
                _policy.Evict(key);
            }
        }
        
    }
}