using Mirror;
using UnityEngine;

/// an orbit for a celestial body
[RequireComponent(typeof(SkyBody))]
class SkyBodyOrbit: MonoBehaviour {
    // -- fields --
    [Header("fields")]
    [Tooltip("the orbital period of the azimuth in seconds")]
    [SerializeField] float m_AzimuthPeriod;

    [Tooltip("the orbital period of the zenith in seconds")]
    [SerializeField] float m_ZenithPeriod;

    // -- props --
    /// the body
    SkyBody m_Body;

    /// the initial position
    Spherical m_Initial;

    /// the azimuth orbit's elapsed time
    float m_AzimuthElapsed;

    /// the zenith orbit's elapsed time
    float m_ZenithElapsed;

    // -- lifeycle --
    void Awake() {
        m_Body = GetComponent<SkyBody>();
        m_Initial = m_Body.Coordinate;
    }

    void FixedUpdate() {
        // track progress through period
        m_AzimuthElapsed = FindElapsed(m_AzimuthPeriod);
        m_ZenithElapsed = FindElapsed(m_ZenithPeriod);

        // update orbit
        var coord = m_Body.Coordinate;
        coord.Azimuth = m_Initial.Azimuth + Mathf.Lerp(
            -180.0f,
            +180.0f,
            m_AzimuthElapsed / m_AzimuthPeriod
        );

        coord.Zenith = m_Initial.Zenith + Mathf.Lerp(
            -180.0f,
            +180.0f,
            m_ZenithElapsed / m_ZenithPeriod
        );

        m_Body.Coordinate = coord;
    }

    // -- queries --
    /// find the lossless elapsed time
    float FindElapsed(float period) {
        return (float)(NetworkTime.time % (double)period);
    }
}
