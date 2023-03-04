using System;
using System.Collections;
using System.Collections.Generic;

namespace ThirdPerson {

/// a fixed-width buffer of n data elements
public sealed class Buffer<T>: IEnumerable<T> {
    // -- props --
    /// the current count of items
    private int m_Count;

    /// the array of items in the buffer
    private readonly T[] m_Buffer;

    // -- lifetime --
    public Buffer(uint size) {
        m_Count = 0;
        m_Buffer = new T[size];
    }

    // -- commands --
    /// adds a new element to the buffer, removing the oldest one.
    public void Add(T item) {
        if (m_Count >= m_Buffer.Length) {
            throw new IndexOutOfRangeException();
        }

        m_Buffer[m_Count] = item;
        m_Count++;
    }

    /// remove all the items in the buffer
    public void Clear() {
        m_Count = 0;
    }

    // -- queries --
    /// gets the nth item
    public T this[int index] {
        get {
            if (index >= m_Count) {
                throw new IndexOutOfRangeException();
            }

            return m_Buffer[index];
        }
    }

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