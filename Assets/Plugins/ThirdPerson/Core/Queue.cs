using System;

namespace ThirdPerson {

/// a circular buffer of n data elements
public sealed class Queue<T> {
    // -- constants --
    /// a null index
    const int k_None = -1;

    // -- properties --
    /// the current position in the buffer
    int m_Head = k_None;

    /// the queue of elements in the buffer
    readonly T[] m_Queue;

    // -- lifetime --
    public Queue(uint size) {
        m_Queue = new T[size];
    }

    // -- commands --
    /// adds a new element to the buffer, removing the oldest one.
    public void Add(T snapshot) {
        m_Head = GetIndex(-1);
        m_Queue[m_Head] = snapshot;
    }

    /// fills the buffer with a given value
    public void Fill(T snapshot) {
        for (var i = 0; i < m_Queue.Length; i++) {
            m_Queue[i] = snapshot;
        }
    }

    // -- queries --
    /// if the queue is empty
    public bool IsEmpty {
        get => m_Head == k_None;
    }

    /// gets the snapshot nth-newest snapshot.
    public T this[uint offset] {
        get {
            if (offset >= m_Queue.Length) {
                throw new IndexOutOfRangeException();
            }

            return m_Queue[GetIndex((int)offset)];
        }

        set {
            m_Queue[GetIndex((int)offset)] = value;
        }
    }

    /// gets the circular index given an offset from the start index
    private int GetIndex(int offset) {
        return ((m_Head - offset) + m_Queue.Length) % m_Queue.Length;
    }
}

}