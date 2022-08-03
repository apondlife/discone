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
        // set initial state
        Init();
    }

    void OnValidate() {
        // re-run initializers
        Init();

        // turn off shadows on all renderers
        var renderers = GetComponentsInChildren<Renderer>();

        foreach (var r in renderers) {
            r.receiveShadows = false;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    // -- commands --
    /// set initial state
    void Init() {
        SyncPosition();
    }

    /// reposition the body given the player's world position
    void SyncPosition() {
        transform.localPosition = m_Coordinate.IntoCartesian();
    }
}