using System.Collections.Concurrent;

namespace Engine.UtilityComponents;

public class ConcurrentQueuedDictionary<TKey, TValue> where TKey : notnull
{
    private Task _clearingTask = Task.Delay(0);
    private readonly ConcurrentDictionary<TKey, TValue> _dictionary;
    private readonly ConcurrentQueue<TKey> _keys;
    private readonly int _clearedCapacity;

    public ConcurrentQueuedDictionary(int capacity)
    {
        _keys = new ConcurrentQueue<TKey>();
        _clearedCapacity = (int)(capacity * 0.95);
        _dictionary = new ConcurrentDictionary<TKey, TValue>();
    }

    public void TryAdd(TKey key, TValue value)
    {
        if (_dictionary.TryAdd(key, value))
        {
            _keys.Enqueue(key);
        }
    }

    public bool TryGetValue(TKey key, out TValue? value) => _dictionary.TryGetValue(key, out value);

    public void ClearTrash()
    {
        _clearingTask.Wait();

        _clearingTask = Task.Run(() =>
        {
            while (_clearedCapacity < _dictionary.Count)
            {
                TryRemoveOldest();
            }
        });
    }

    private void TryRemoveOldest()
    {
        if (_keys.TryDequeue(out var oldestKey) is false)
        {
            return;
        }
        
        if (_dictionary.TryRemove(oldestKey, out _))
        {
            return;
        }
            
        _keys.Enqueue(oldestKey);
    }
}
