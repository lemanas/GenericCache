using System.Collections.Concurrent;

namespace GenericCache;
public class LimitedConcurrentDictionary<TKey, TValue>
{
    private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
    private ConcurrentQueue<TKey> _keys;
    private readonly int? _capacity;

    public LimitedConcurrentDictionary(int? capacity = null, int concurrencyLevel = 50)
    {
        _keys = new ConcurrentQueue<TKey>();
        _capacity = capacity;

        if (capacity.HasValue)
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, _capacity.Value);
        }
        else
        {
            _dictionary = new ConcurrentDictionary<TKey, TValue>();
        }
    }

    public void TryAdd(TKey key, TValue value)
    {
        DequeueIfFull(key);

        _dictionary.TryAdd(key, value);
        if (_capacity.HasValue && !_keys.Contains(key))
            _keys.Enqueue(key);
    }

    public void AddOrUpdate(TKey key, TValue value)
    {
        DequeueIfFull(key);
        _dictionary.AddOrUpdate(key, value, (k, v) => value);
        if (_capacity.HasValue && !_keys.Contains(key))
            _keys.Enqueue(key);
    }

    public TValue TryGetValue(TKey key)
    {
        bool isSuccess = _dictionary.TryGetValue(key, out TValue result);
        if (isSuccess)
        {
            return result;
        }
        return default;
    }

    public void TryRemove(TKey key)
    {
        _dictionary.TryRemove(key, out TValue _);
        RecreateQueue(key);
    }

    public void Clear()
    {
        _dictionary.Clear();
        _keys = new ConcurrentQueue<TKey>();
    }

    public long Count => _dictionary.Count;

    private void RecreateQueue(TKey keyToRemove)
    {
        List<TKey> queueList = _keys.ToList();
        queueList.RemoveAll(k => EqualityComparer<TKey>.Default.Equals(k, keyToRemove));
        _keys = new ConcurrentQueue<TKey>(queueList);
    }

    private void DequeueIfFull(TKey key)
    {
        if (_capacity.HasValue && _dictionary.Count >= _capacity)
        {
            var isSuccess = _keys.TryDequeue(out TKey oldestKey);
            bool isNew = key.Equals(oldestKey);
            if (isSuccess && !isNew)
                _dictionary.TryRemove(oldestKey, out TValue _);
            if (isSuccess && isNew)
                _keys.Enqueue(oldestKey);
            DequeueIfFull(key);
        }
    }
}