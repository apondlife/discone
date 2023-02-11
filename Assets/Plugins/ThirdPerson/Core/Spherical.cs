using System;
using UnityEngine;

namespace ThirdPerson {

/// a spherical coordinate
[Serializable]
struct Spherical {
    // -- props --
    /// <summary>the radius of the coordinate arm</summary>
    public float Radius;

    /// <summary>the zenith angle in degrees; the inclination</summary>
    [Range(-90.0f, 90.0f)]
    public float Zenith;

    /// <summary>the azimuth angle in degrees; the sweep</summary>
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

}