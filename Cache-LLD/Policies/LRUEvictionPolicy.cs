using Cache_LLD.Entites;
using Cache_LLD.Locking;
using Cache_LLD.Logger;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cache_LLD.Policies;

public class LruEvictionPolicy<TKey> : IEvictionPolicy<TKey> where TKey : notnull
{
    private readonly DoublyLinkedList<TKey> _nodeList;
    private readonly ConcurrentDictionary<TKey, DoublyLinkedListNode<TKey>> _dictionary;

    //Its a shared locking manager b/w Cache and LruEvictionPolicy
    private readonly ReaderWriterLockingManager<TKey> _keyLockingManager;

    private readonly object sync = new object();
    private readonly ILog log = new ConsoleLogger();
    
    public LruEvictionPolicy()
    {
        _nodeList = new DoublyLinkedList<TKey>();
        _dictionary = new ConcurrentDictionary<TKey, DoublyLinkedListNode<TKey>>();
        _keyLockingManager = new ReaderWriterLockingManager<TKey>();
    }

    public async Task KeyAccessedAsync(TKey key)
    {
        //This lock is needed, so that the node does not gets updated if some other thread
        //is trying to evict it.
        // Update: Since we are sharing the lock manager with Cache, so we have already put a lock
        // on this key, no need to take another lock.
        //using (_keyLockingManager.AcquireReadLock(key))
        {
            await Task.Run(() =>
            {
                var node = _dictionary.GetOrAdd(key, _ => new DoublyLinkedListNode<TKey>(key));
                _nodeList.Remove(node);
                _nodeList.AddFirst(node);

                IList<TKey> keys = new List<TKey>();
                node = _nodeList.First;
                while (node != null && node.Next != null)
                {
                    keys.Add(node.Key);
                    node = node.Next;
                }
                if (keys.Count > 0)
                {
                    log.Log($"Key:{key} => Order of keys: {(string.Join(",", keys))} ");
                }
            });
        }
    }

    public TKey? Evict()
    {
        if (_dictionary.Count == 0)
            return  default(TKey?);

        TKey? key = default;

        //This lock will serialize the access to the last node that needs to be evicted.
        //Otherwise if 3 requests come to Evict() they all will get the same key
        //so the nodelist will become in-consistent.
        lock(sync)
        {
            //This lock will synchronize the concurrent access on a key by `Evict` and `KeyAccessed`
            //method at a given moment. 
            //It will help us in scenarios where:
            // Evict is trying to remove key:1 
            // KeyAccessed is trying to update for key:1
            //This scenario can leave the _dictionary in inconsistent state. So we will need Read and Write
            //locks.
            using (GetLockOnLastNode())
            {
                var last = _nodeList.Last;
                if (last == null)
                    return default(TKey?);

                key = last.Key;
                log.Log($"Evicting key: {key} ");
                //as we are not removing the locks from lock manager, there could be more than
                //1 thread that are trying to remove the same key. always check if key exists.
                if (_dictionary.ContainsKey(key))
                {
                    IList<TKey> keys = new List<TKey>();
                    var node = _nodeList.First;
                    while(node != null && node.Next != null)
                    {
                        keys.Add(node.Key);
                        node = node.Next;
                    }
                    if(keys.Count > 0)
                    {
                        keys.RemoveAt(keys.Count - 1);
                        log.Log($"Order of keys: {(string.Join(",", keys))} ");
                    }
                    
                    _nodeList.Remove(last);
                    _dictionary.Remove(key, out _);
                }
            }
        }
        return key;
    }

    private IDisposable? GetLockOnLastNode() {
        var last = _nodeList.Last;
        if (last == null)
            return null;
        int count = 0;
        while (last != null && count < 3) {
            //We should get the write lock first, as it will ensure that there are no more threads
            //reading this node.
            var nodeLock = _keyLockingManager.AcquireWriteLock(last.Key);
            if(_nodeList.Last == last) return nodeLock;
            nodeLock.ExitLock();
            last = _nodeList.Last;
            count++;
        }
        return null;
    }

    public TKey? Evict(TKey key)
    {
        if (_dictionary.Count == 0)
            return default(TKey?);

        DoublyLinkedListNode<TKey>? node = null;

        //This lock will synchronize the concurrent access on a key by `Evict` and `KeyAccessed`
        //method at a given moment. 
        //Here we do not need to use the lock keyword as keyManager is helping to serialize the access
        //since we already have the key in hand.
        if(_dictionary.ContainsKey(key) && _dictionary[key] != null)
        {
            using (_keyLockingManager.AcquireWriteLock(key))
            {
                if (!_dictionary.ContainsKey(key))
                    throw new Exception($"Key:{key} no longer exists..");

                node = _dictionary[key];
                _nodeList.Remove(node);
                _dictionary.Remove(key, out _);
            }
        }
        return node == null ? default : node.Key;
    }

   
}