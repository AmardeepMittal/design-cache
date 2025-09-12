namespace design_cache.Entities;

public class CacheEntry<TKey, TValue>
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
}