using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Soil {

/// a serializable map (mostly borrowed from the internet)
[Serializable]
public sealed class Map<TKey, TValue>: IDictionary<TKey, TValue>, ISerializationCallbackReceiver {
    // -- fields --
    [Header("fields")]
    [Tooltip("the list of key-value pairs")]
    [SerializeField] List<Entry> m_Entries = new();

    #pragma warning disable CS0414
    [HideInInspector]
    [SerializeField] string m_Warning;
    #pragma warning restore CS0414

    /// -- props --
    /// the underlying dictionary
    Dictionary<TKey, TValue> m_Map = new();

    /// the key-index dictionary
    Dictionary<TKey, int> m_Indices = new();

    // -- children --
    /// a backing key-value pair
    [Serializable]
    public struct Entry {
        /// .
        public TKey Key;

        /// .
        public TValue Val;

        /// .
        public Entry(TKey key, TValue val) {
            Key = key;
            Val = val;
        }
    }

    // -- commands --
    /// sort the entries
    public void Sort(Comparison<Entry> comparison) {
        m_Entries.Sort(comparison);
    }

    // -- ISerializationCallbackReceiver --
    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        // reset state
        m_Map.Clear();
        m_Indices.Clear();

        // and regenerate values
        for (var i = 0; i < m_Entries.Count; i++) {
            var key = m_Entries[i].Key;
            if (key == null || ContainsKey(key)) {
                m_Warning = "duplicate keys will not be serialized!";
                continue;
            }

            m_Map.Add(key, m_Entries[i].Val);
            m_Indices.Add(key, i);
        }
    }

    // -- IDictionary/commands --
    public void Add(TKey key, TValue value) {
        m_Map.Add(key, value);
        m_Entries.Add(new Entry(key, value));
        m_Indices.Add(key, m_Entries.Count - 1);
    }

    public bool Remove(TKey key) {
        if (!m_Map.Remove(key)) {
            return false;
        }

        var index = m_Indices[key];
        m_Entries.RemoveAt(index);
        m_Indices.Remove(key);
        UpdateIndices(index);

        return true;
    }

    // -- IDictionary/queries --
    public TValue this[TKey key] {
        get => m_Map[key];
        set {
            m_Map[key] = value;

            if (m_Indices.TryGetValue(key, out var idx)) {
                m_Entries[idx] = new Entry(key, value);
            } else {
                m_Entries.Add(new Entry(key, value));
                m_Indices.Add(key, m_Entries.Count - 1);
            }
        }
    }

    public ICollection<TKey> Keys {
        get => m_Map.Keys;
    }

    public ICollection<TValue> Values {
        get => m_Map.Values;
    }

    public bool TryGetValue(TKey key, out TValue value) {
        return m_Map.TryGetValue(key, out value);
    }

    public bool ContainsKey(TKey key) {
        return m_Map.ContainsKey(key);
    }

    void UpdateIndices(int removed) {
        for (var i = removed; i < m_Entries.Count; i++) {
            m_Indices[m_Entries[i].Key]--;
        }
    }

    // -- ICollection/commands --
    public void Add(KeyValuePair<TKey, TValue> pair) {
        Add(pair.Key, pair.Value);
    }

    public void Clear() {
        m_Map.Clear();
        m_Entries.Clear();
        m_Indices.Clear();
    }

    // -- ICollection/queries --
    public int Count {
        get => m_Map.Count;
    }

    public bool IsReadOnly {
        get;
        set;
    }

    public bool Contains(KeyValuePair<TKey, TValue> pair) {
        if (!m_Map.TryGetValue(pair.Key, out var value)) {
            return false;
        }

        return EqualityComparer<TValue>.Default.Equals(value, pair.Value);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] arr, int i) {
        if (arr == null) {
            throw new ArgumentException("the array cannot be null.");
        }

        if (i < 0) {
           throw new ArgumentException("the starting array index cannot be negative.");
        }

        if (arr.Length - i < m_Map.Count) {
            throw new ArgumentException("the destination array has fewer elements than the collection.");
        }

        foreach (var pair in m_Map) {
            arr[i] = pair;
            i++;
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> pair) {
        if (m_Map.TryGetValue(pair.Key, out var value)) {
            if (EqualityComparer<TValue>.Default.Equals(value, pair.Value)) {
                return Remove(pair.Key);
            }
        }

        return false;
    }

    // -- IEnumerable --
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
        return m_Map.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return m_Map.GetEnumerator();
    }
}

}