namespace Cache_LLD.Locking;

public class WriteLockHandle : IDisposable
{
    private ReaderWriterLockSlim _lock;

    public WriteLockHandle(ReaderWriterLockSlim writerLock)
    {
        _lock = writerLock;
        _lock.EnterWriteLock();
    }

    public void ExitLock()
    {
        _lock.ExitWriteLock();
    }

    public void Dispose()
    {
        _lock.ExitWriteLock();
        // TODO release managed resources here
    }
}