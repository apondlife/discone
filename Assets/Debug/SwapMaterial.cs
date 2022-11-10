#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// swap materials for all children between two channels
public class SwapMaterial: MonoBehaviour {
    // -- types --
    /// the channel
    enum Channel {
        One,
        Two
    }

    /// a pair of materials for two channels
    [Serializable]
    class Swap {
        /// the first channel
        public Material ChannelOne;

        /// the second channel
        public Material ChannelTwo;
    }

    // -- config --
    [Header("config")]
    [Tooltip("the target object")]
    [SerializeField] GameObject m_Target;

    [Tooltip("the list of swap pairs")]
    [SerializeField] List<Swap> m_Swaps;

    // -- commands --
    [ContextMenu("swap to channel one")]
    public void SwapToChannelOne() {
        SwapTo(Channel.One);
    }

    [ContextMenu("swap to channel two")]
    public void SwapToChannelTwo() {
        SwapTo(Channel.Two);
    }

    /// swap all materials to the channel
    void SwapTo(Channel channel) {
        // build material -> material map
        var map = channel == Channel.One
            ? m_Swaps.ToDictionary(a => a.ChannelTwo, a => a.ChannelOne)
            : m_Swaps.ToDictionary(a => a.ChannelOne, a => a.ChannelTwo);

        // swap all materials
        foreach (var renderer in m_Target.GetComponentsInChildren<Renderer>()) {
            if (map.TryGetValue(renderer.sharedMaterial, out var other)) {
                renderer.sharedMaterial = other;
            }
        }
    }
}
#endif