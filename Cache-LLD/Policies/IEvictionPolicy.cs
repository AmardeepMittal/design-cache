namespace Cache_LLD.Policies;

public interface IEvictionPolicy<TKey>
{
    Task KeyAccessedAsync(TKey key);
    TKey? Evict();
    TKey? Evict(TKey key);
}   