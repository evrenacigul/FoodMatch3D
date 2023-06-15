using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<KeyValueEntry> entries;
    private List<TKey> keys = new List<TKey>();

    [Serializable]
    class KeyValueEntry
    {
        public TKey key;
        public TValue value;
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            Add(entries[i].key, entries[i].value);
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (entries == null)
        {
            return;
        }

        keys.Clear();

        for (int i = 0; i < entries.Count; i++)
        {
            keys.Add(entries[i].key);
        }

        var result = keys.GroupBy(x => x)
                         .Where(g => g.Count() > 1)
                         .Select(x => new { Element = x.Key, Count = x.Count() })
                         .ToList();

        if (result.Count > 0)
        {
            var duplicates = string.Join(", ", result);
            Debug.LogError($"Warning {GetType().FullName} keys has duplicates {duplicates}");
        }
    }
}