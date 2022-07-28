using System;
using UnityEngine;

/// a spherical coordinate
[Serializable]
struct Spherical {
    /// the radius of the coordinate arm
    public float Radius;

    /// the zenith angle in degrees; the inclination
    [Range(-90.0f, 90.0f)]
    public float Zenith;

    /// the azimuth angle in degrees; the sweep
    [Range(-180.0f, 180.0f)]
    public float Azimuth;

    // -- queries --
    /// convert the spherical coordinate into a cartesian coordinate
    public Vector3 IntoCartesian() {
        var r = (Radius);
        var z = (Zenith - 90.0f) * Mathf.Deg2Rad;
        var a = (Azimuth) * Mathf.Deg2Rad;

        var pt = Vector3.zero;
        pt.x = r * Mathf.Sin(z) * Mathf.Cos(a);
        pt.y = r * Mathf.Cos(z);
        pt.z = r * Mathf.Sin(z) * Mathf.Sin(a);

        return pt;
    }
}
