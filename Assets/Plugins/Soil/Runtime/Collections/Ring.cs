using System;
using System.Collections;
using System.Collections.Generic;

namespace Soil {

/// a circular buffer of n data elements
public sealed class Ring<T>: IEnumerable<T> {
    // -- constants --
    /// a null index
    const int k_None = -1;

    // -- properties --
    /// the current position in the buffer
    int m_Head = k_None;

    /// the queue of elements in the buffer
    readonly T[] m_Queue;

    // -- lifetime --
    /// create a queue with a fixed size
    public Ring(uint size) {
        m_Queue = new T[size];
    }

    /// create a queue from a list of items
    public Ring(T[] items) {
        m_Queue = items;
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

    /// move the head of the queue by the offset
    public void Offset(int offset = -1) {
        m_Head = GetIndex(offset);
    }

    /// move the head to an index
    public void Move(int index) {
        m_Head = GetIndex(m_Head + index);
    }

    /// clear the values in the buffer
    public void Clear() {
        m_Head = k_None;

        for (var i = 0; i < m_Queue.Length; i++) {
            m_Queue[i] = default;
        }
    }

    // -- queries --
    /// if the queue is empty
    public bool IsEmpty {
        get => m_Head == k_None;
    }

    /// the length of the queue
    public int Length {
        get => m_Queue.Length;
    }

    /// gets the snapshot nth-newest item.
    public T this[int offset] {
        get {
            if (offset >= m_Queue.Length) {
                throw new IndexOutOfRangeException($"offset: {offset} > {m_Queue.Length}");
            }

            return m_Queue[GetIndex(offset)];
        }

        set {
            m_Queue[GetIndex(offset)] = value;
        }
    }

    /// the head of the ring
    public T Head {
        get => this[0];
    }

    /// gets the circular index given an offset from the start index
    int GetIndex(int offset) {
        return ((m_Head - offset) + m_Queue.Length) % m_Queue.Length;
    }

    // -- IEnumerable --
    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator() {
        var n = m_Queue.Length;
        for (var i = 0; i < n; i++) {
            yield return m_Queue[i];
        }
    }
}

}