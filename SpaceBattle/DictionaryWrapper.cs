using System.Collections;

public class DictionaryWrapper : IDictionary<string, object>
{
    private readonly IDictionary<string, object> _dictionary;
    private readonly IDictionary<string, Func<object>> _behavior;

    public DictionaryWrapper(
        IDictionary<string, object> dictionary,
        IDictionary<string, Func<object>> behavior)
    {
        _dictionary = dictionary;
        _behavior = behavior;
    }

    public object this[string key]
    {
        get
        {
            if (_behavior.TryGetValue(key, out var func))
            {
                return func();
            }

            return _dictionary[key];
        }
        set
        {
            if (_behavior.ContainsKey(key))
            {
                throw new InvalidOperationException($"Key '{key}' is reserved for behavior and cannot be modified.");
            }

            _dictionary[key] = value;
        }
    }

    public ICollection<string> Keys => [.. _dictionary.Keys.Union(_behavior.Keys)];

    public ICollection<object> Values =>
        [.. Keys.Select(k => this[k])];

    public int Count => Keys.Count;

    public bool IsReadOnly => _dictionary.IsReadOnly;

    public void Add(string key, object value)
    {
        if (_behavior.ContainsKey(key))
        {
            throw new ArgumentException($"Key '{key}' is reserved for behavior.", nameof(key));
        }

        _dictionary.Add(key, value);
    }

    public void Add(KeyValuePair<string, object> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _dictionary.Clear();
    }

    public bool Contains(KeyValuePair<string, object> item)
    {
        return _dictionary.Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return _dictionary.ContainsKey(key) || _behavior.ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        foreach (var kvp in this)
        {
            array[arrayIndex++] = kvp;
        }
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        foreach (var key in Keys)
        {
            yield return new KeyValuePair<string, object>(key, this[key]);
        }
    }

    public bool Remove(string key)
    {
        if (_behavior.ContainsKey(key))
        {
            throw new InvalidOperationException($"Key '{key}' is reserved for behavior and cannot be removed.");
        }

        return _dictionary.Remove(key);
    }

    public bool Remove(KeyValuePair<string, object> item)
    {
        return Remove(item.Key);
    }

    public bool TryGetValue(string key, out object value)
    {
        if (_behavior.TryGetValue(key, out var func))
        {
            value = func();
            return true;
        }

        return _dictionary.TryGetValue(key, out value!);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
