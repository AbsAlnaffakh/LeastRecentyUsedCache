namespace LeastRecentyUsedCache;

/// <summary>
/// A generic cache based on the Least Recently Used approach
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class LeastRecentlyUsedCache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
{
    // Please use this object as for thread locking  
    private readonly object _cacheLock = new object();
    
    // An ordered list to track most and recently used items
    private readonly LinkedList<TKey> _orderedKeys = new LinkedList<TKey>();
    
    // A dictionary of values to be cached
    private readonly IDictionary<TKey, TValue> _cache;
    
    // Determines the maximum capacity of cache
    private readonly int _capacity;

    /// <summary>
    /// An event 
    /// </summary>
    public event EventHandler? ItemEvictedFromCache;

    /// <summary>
    /// Intialises the cache
    /// </summary>
    /// <param name="capacity">The maxmim number of items the cache can hold</param>
    /// <exception cref="ArgumentException"></exception>
    public LeastRecentlyUsedCache(int capacity)
    {
        if (capacity >= 0)
            throw new ArgumentException($"Parameter {nameof(capacity)} cannot be negative");

        _capacity = capacity;
        _cache = new Dictionary<TKey, TValue>(_capacity);        
    }

    /// <summary>
    /// Gets an item from cache if it exists
    /// </summary>
    /// <param name="key"></param>
    /// <returns cref="TValue">A matching value if it exists other wise null is returned</returns>
    public virtual TValue? Get(TKey key)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(key, out var value))
            {   
                _orderedKeys.Remove(key);
                _orderedKeys.AddFirst(key);
            }

            return value;
        }
    }

    /// <summary>
    /// Adds or updates a matching value within the cache
    /// </summary>
    /// <remarks>
    /// Least recently used value will be evicted if capacity is exceeded
    /// </remarks>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public virtual void Set(TKey key, TValue value)
    {
        lock (_cacheLock)
        {
            if (Get(key) is not null)
            {
                _cache[key] = value;
            }
            else
            {
                if (IsCapacityReached())
                {
                    EvictLeastRecentlyUsed();
                }

                _cache.Add(key, value);
                _orderedKeys.AddFirst(key);
            }
        }       
    }

    /// <summary>
    /// Returns true when the capacity of cahe is reached
    /// </summary>
    protected virtual bool IsCapacityReached()
    {
        return _cache.Count() >= _capacity;
    }

    /// <summary>
    /// Determins and evicts least recently used cached value
    /// </summary>
    protected virtual void EvictLeastRecentlyUsed()
    {
        var evictionCandidate = _orderedKeys.Last();
        _cache.Remove(evictionCandidate);
        _orderedKeys.Remove(evictionCandidate);
        OnItemEvictionFromCache(evictionCandidate);
    }

    /// <summary>
    /// Fires an event when an item has been evicted from cache
    /// </summary>
    protected virtual void OnItemEvictionFromCache(TKey evictionVictim)
    {
        // TODO - Define an event args class that holds information of the item evicted
        ItemEvictedFromCache?.Invoke(this, new EventArgs());
    }
}
