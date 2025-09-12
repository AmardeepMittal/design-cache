namespace Cache_LLD.Locking;

public class ReadLockHandle : IDisposable
{
    private readonly ReaderWriterLockSlim _lock;

    public ReadLockHandle(ReaderWriterLockSlim readerLock)
    {
        _lock = readerLock;
        _lock.EnterReadLock();
    }

    public void ExitLock() {
        _lock.ExitReadLock();
    }

    public void Dispose()
    {
        _lock.ExitReadLock();
        // TODO release managed resources here
    }
}