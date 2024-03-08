using System;
using System.Collections;
using System.Collections.Generic;

namespace Soil {

/// a fixed-width buffer of n data elements
public sealed class Buffer<T>: IEnumerable<T> {
    // -- props --
    /// the current count of items
    int m_Count;

    /// the array of items in the buffer
    readonly T[] m_Buffer;

    // -- lifetime --
    public Buffer(uint size) {
        m_Count = 0;
        m_Buffer = new T[size];
    }

    // -- commands --
    /// adds a new element to the buffer, removing the oldest one.
    public void Add(T item) {
        if (m_Count >= m_Buffer.Length) {
            throw new IndexOutOfRangeException($"{m_Count} >= {m_Buffer.Length}");
        }

        m_Buffer[m_Count] = item;
        m_Count++;
    }

    /// remove all the items in the buffer
    public void Clear() {
        m_Count = 0;
    }

    // -- queries --
    /// gets the last item
    public T Last {
        get {
            if (m_Count == 0) {
                return default;
            }

            return m_Buffer[m_Count - 1];
        }
    }

    /// the current count of items
    public int Count {
        get => m_Count;
    }

    /// the length of the buffer
    public int Length {
        get => m_Buffer.Length;
    }

    /// get a readonly copy of this buffer or null if its empty
    public T[] ToArrayOrNull() {
        if (m_Count == 0) {
            return null;
        }

        var result = new T[m_Count];
        Array.Copy(m_Buffer, result, m_Count);

        return result;
    }

    // -- operators --
    /// access an item by index
    public T this[int index] {
        get {
            if (index >= m_Count) {
                throw new IndexOutOfRangeException($"{index} >= {m_Count} [{m_Buffer.Length}]");
            }

            return m_Buffer[index];
        }

        set {
            if (index >= m_Count) {
                throw new IndexOutOfRangeException($"{index} >= {m_Count} [{m_Buffer.Length}]");
            }

            m_Buffer[index] = value;
        }
    }

    // -- IEnumerable --
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator() {
        var n = m_Count;
        for (var i = 0; i < n; i++) {
            yield return m_Buffer[i];
        }
    }
}

}