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
    void OnValidate() {
        SyncPosition();
    }

    // -- commands --
    /// reposition the body given the player's world position
    void SyncPosition() {
        // calculate new position from polar coordinate
        var s = m_Coordinate;
        var r = (s.Radius);
        var z = (s.Zenith - 90.0f) * Mathf.Deg2Rad;
        var a = (s.Azimuth) * Mathf.Deg2Rad;

        var pos = Vector3.zero;
        pos.x = r * Mathf.Sin(z) * Mathf.Cos(a);
        pos.y = r * Mathf.Cos(z);
        pos.z = r * Mathf.Sin(z) * Mathf.Sin(a);

        // update position from body
        transform.localPosition = pos;
    }
}