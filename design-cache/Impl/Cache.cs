using design_cache.Entities;
using design_cache.Interfaces;

namespace design_cache.Impl;

public class Cache<TValue> : ICache<TValue>
{
    private readonly Dictionary<string, Entities.LinkedListNode<TValue>> _cache = new Dictionary<string, Entities.LinkedListNode<TValue>>();
    private readonly Dictionary<string, ReaderWriterLock> _locks = new Dictionary<string, ReaderWriterLock>();
    private readonly Entities.LinkedList<TValue> _list = new Entities.LinkedList<TValue>();
    private static readonly object Sync = new object();
    
    
    public CacheEntry<string, TValue>? Get(string key)
    {
        //get the reader writer lock on a specific key
        var rwLock = GetLock(key);
        
        //acquires the read lock on the key, the lock is a shared lock, so multiple threads can read simultaneously.
        rwLock.AcquireReaderLock(TimeSpan.FromSeconds(10));
        try
        {
            return _cache.TryGetValue(key, out var node) ? node.Value : null;
        }
        finally
        {
            //release the reader lock
            rwLock.ReleaseReaderLock();
        }
    }

    public void Put(string key, TValue value)
    {
        //get the reader writer lock on a specific key
        var rwLock = GetLock(key);
        var cacheEntry = new CacheEntry<string, TValue>()
        {
            Key = key,
            Value = value
        };
        //acquires the write lock on the key, the lock is a not a shared lock.
        rwLock.AcquireWriterLock(TimeSpan.FromSeconds(10));
        try
        {
            if (_cache.ContainsKey(key))
            {
                var node = _cache[key];
                node.Value = cacheEntry;
            }
            else
            {
                var node = new Entities.LinkedListNode<TValue>(cacheEntry);
                _list.Add(cacheEntry);
                _cache.Add(key, node);
            }
        }
        finally
        {
            //release the reader lock
            rwLock.ReleaseWriterLock();
        }
    }

    public void Remove(string key)
    {
        // get a write lock and then remove the item.
    }

    private ReaderWriterLock GetLock(string key)
    {
        lock (Sync)
        {
            if (_locks.TryGetValue(key, out var keyLock))
            {
                return keyLock;
            }
            else return _locks[key] = new ReaderWriterLock();
        }
    }
}