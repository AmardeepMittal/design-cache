namespace Cache_LLD.Storage;

public interface IStorage<in TKey, TValue>
{
    TValue? Get(TKey key);
    void Put(TKey key, TValue value);
    void Remove(TKey key);
    bool ContainsKey(TKey key); 
    int Count();
    void Clear();
}