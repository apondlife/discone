using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using System;

/// a sun that changes color
public class SkySun: MonoBehaviour {
    // -- statics --
    /// the cache of per-color materials
    static Dictionary<string, Material> s_MaterialCache = new Dictionary<string, Material>();

    // -- config --
    [Header("config")]
    [Tooltip("the sun color when off")]
    [ColorUsage(true, true)]
    [UnityEngine.Serialization.FormerlySerializedAs("m_HostColor")]
    [SerializeField] Color m_OffColor;

    [Tooltip("the sun color when on")]
    [ColorUsage(true, true)]
    [UnityEngine.Serialization.FormerlySerializedAs("m_ClientColor")]
    [SerializeField] Color m_OnColor;

    // -- refs --
    [Header("refs")]
    [Tooltip("the number of players")]
    [SerializeField] IntVariable m_PlayerCount;

    // -- props --
    /// the list of renderers
    Renderer[] m_Renderers;

    /// a set of event subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    private void Awake() {
        // set props
        m_Renderers = GetComponentsInChildren<Renderer>();

        // set current state
        SyncColor();

        // bind events
        m_Subscriptions
            .Add(m_PlayerCount.Changed, OnPlayerCountChanged);
    }

    void OnDestroy() {
        // unbind events
        s_MaterialCache.Clear();
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// sync color with current state
    void SyncColor() {
        // get current color
        var count = m_PlayerCount.Value;
        var color = count <= 1 ? m_OffColor : m_OnColor;

        // update materials
        foreach (var r in m_Renderers) {
            r.material = FindInstance(r.sharedMaterial, color);
        }
    }

    // -- queries --
    /// find cached material for texture and saturation
    Material FindInstance(Material mat, Color color) {
        var key = $"{mat.name}/{color.r}:{color.g}:{color.b}";

        // create instanced material for the texture if not cached
        if (!s_MaterialCache.TryGetValue(key, out var material)) {
            material = Instantiate(mat);
            material.color = color;
            s_MaterialCache.Add(key, material);
        }

        return material;
    }

    // -- events --
    /// when the player switches between host/client
    void OnPlayerCountChanged(int count) {
        SyncColor();
    }
}