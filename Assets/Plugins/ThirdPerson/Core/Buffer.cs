using System;

namespace ThirdPerson {

/// a fixed-width buffer of n data elements
sealed class Buffer<T> {
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
    /// gets the snapshot nth-newest snapshot.
    public T this[int index] {
        get => m_Buffer[index];
    }

    /// the current count of items
    public int Count {
        get => m_Count;
    }
}

}