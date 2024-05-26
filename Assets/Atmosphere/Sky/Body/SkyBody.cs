using Soil;
using UnityEngine;

/// a celestial body positioned spherically in the sky
[ExecuteAlways]
class SkyBody: MonoBehaviour {
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
        var position = m_Coordinate.IntoCartesian();

        var t = transform;
        if (!Mathx.IsZero(position - t.localPosition)) {
            t.localPosition = position;
        }
    }
}