using System.Collections.Concurrent;

namespace Cache_LLD.Storage;

public class DictionaryStorage<TKey, TValue> : IStorage<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _dictionary;

    public DictionaryStorage()
    {
        _dictionary = new ConcurrentDictionary<TKey, TValue>();
    }

    public TValue? Get(TKey key)
    {
        if(key.Equals(default(TKey))) throw new ArgumentNullException(nameof(key));
        if (_dictionary.TryGetValue(key, out var value)) ;
        return value;
    }

    public void Put(TKey key, TValue value)
    {
        if(key.Equals(default(TKey))) throw new ArgumentNullException(nameof(key));
        _dictionary.AddOrUpdate(key, value, (key, oldValue) => value);
    }

    public void Remove(TKey key)
    {
        if(key.Equals(default(TKey))) throw new ArgumentNullException(nameof(key));
        if(_dictionary.ContainsKey(key)) _dictionary.Remove(key, out var _);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public int Count()
    {
        return _dictionary.Count;
    }

    public bool ContainsKey(TKey key)
    {
        return (_dictionary.ContainsKey(key));
    }
}