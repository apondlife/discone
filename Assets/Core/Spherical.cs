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
}
