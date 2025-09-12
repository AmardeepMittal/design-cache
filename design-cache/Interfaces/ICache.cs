using design_cache.Entities;

namespace design_cache.Interfaces;

public interface ICache<TValue>
{
    CacheEntry<string, TValue>? Get(string key);
    void Put(string key, TValue value);
    void Remove(string key);
}