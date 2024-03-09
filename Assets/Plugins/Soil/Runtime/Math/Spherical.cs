using System;
using UnityEngine;

namespace Soil {

/// a spherical coordinate
[Serializable]
public struct Spherical: IEquatable<Spherical> {
    // -- props --
    /// <summary>the radius of the coordinate arm</summary>
    public float Radius;

    /// <summary>the azimuth angle in degrees; the sweep</summary>
    [Range(-180.0f, 180.0f)]
    public float Azimuth;

    /// <summary>the zenith angle in degrees; the inclination</summary>
    [Range(-90.0f, 90.0f)]
    public float Zenith;

    // -- init --
    public Spherical(
        float radius,
        float azimuth,
        float zenith
    ) {
        Radius = radius;
        Azimuth = azimuth;
        Zenith = zenith;
    }

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

    // -- IEquatable --
    // TODO: code generation
    public bool Equals(Spherical other) {
        return (
            Radius == other.Radius &&
            Azimuth == other.Azimuth &&
            Zenith == other.Zenith
        );
    }

    // -- == --
    public override bool Equals(object obj) {
        if (obj is Spherical pos) {
            return Equals(pos);
        }

        return false;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Radius, Azimuth, Zenith);
    }

    public static bool operator ==(Spherical pos1, Spherical pos2) {
        return pos1.Equals(pos2);
    }

    public static bool operator !=(Spherical pos1, Spherical pos2) {
        return !(pos1 == pos2);
    }

    public static Spherical operator *(Spherical pos, float scale) {
        return new Spherical(
            pos.Radius * scale,
            pos.Azimuth * scale,
            pos.Zenith * scale
        );
    }

    // -- debug --
    public override string ToString() {
        return $"<Spherical r={Radius} a={Azimuth} z={Zenith}";
    }
}

}