using System.Collections.Concurrent;

namespace Cache_LLD.Locking;

public class ReaderWriterLockingManager<TKey> where TKey : notnull
{
    static ConcurrentDictionary<TKey, ReaderWriterLockSlim> _locks = new ConcurrentDictionary<TKey, ReaderWriterLockSlim>();

    private ReaderWriterLockSlim GetLock(TKey key)
    {
        return _locks.GetOrAdd(key, k => new ReaderWriterLockSlim());
    }

    // get the read lock
    public ReadLockHandle AcquireReadLock(TKey key) => new ReadLockHandle(GetLock(key));

    // get the write lock
    public WriteLockHandle AcquireWriteLock(TKey key) => new WriteLockHandle(GetLock(key));
}