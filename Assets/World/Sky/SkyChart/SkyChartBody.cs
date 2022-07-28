using System;
using UnityEngine;

/// the sky-based navigation system
[ExecuteAlways]
class SkyChartBody: MonoBehaviour {
    // -- fields --
    [Header("fields")]
    [Tooltip("the spherical coordinate")]
    [SerializeField] Spherical m_Coordinate;

    // -- props/hot --
    /// the spherical coordinate
    public Spherical Coordinate {
        get => m_Coordinate;
        set {
            m_Coordinate = value;
            SyncPosition();
        }
    }

    // -- lifecycle --
    void Awake() {
        // turn off shadows on all renderers
        var renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers) {
            r.receiveShadows = false;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    void OnValidate() {
        SyncPosition();
    }

    // -- commands --
    /// reposition the body given the player's world position
    void SyncPosition() {
        // update position from body
        transform.localPosition = m_Coordinate.IntoCartesian();
    }
}