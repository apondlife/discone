using UnityEngine;

/// the sky-based navigation system
class SkyChartBody: MonoBehaviour {
    // -- fields --
    [Header("fields")]
    [Tooltip("the zenith angle in degrees")]
    [SerializeField] bool m_IsVisible;

    [Tooltip("the zenith angle in degrees")]
    [SerializeField] float m_Zenith;

    [Tooltip("the azimuth angle in degrees")]
    [SerializeField] float m_Azimuth;

    [Tooltip("the radial offset in world units")]
    [SerializeField] float m_Offset;

    // -- config --
    [Header("config")]
    [Tooltip("the alpha when fully visible")]
    [SerializeField] float m_VisibleAlpha;

    [Tooltip("the distance at which the body is fully hidden")]
    [SerializeField] float m_HiddenDistance;

    [Tooltip("the distance at which the body is fully visible")]
    [SerializeField] float m_VisibleDistance;

    // -- refs --
    [Header("refs")]
    [Tooltip("the target object for this body")]
    [SerializeField] Transform m_Target;

    // -- props --
    /// the body's material
    Material m_Material;

    // -- lifecycle --
    void Awake() {
        // get props
        // m_Material = GetComponent<Renderer>().material;
    }

    // -- commands --
    /// reposition the body around the center position
    public void Reposition(Vector3 center, float radius) {
        // get delta to the target
        var delta = m_Target.position - center;

        // update visibility
        var dist = delta.magnitude;
        m_IsVisible = dist > m_HiddenDistance;
        if (!m_IsVisible) {
            return;
        }

        // fade between hidden & visible distance
        // var color = m_Material.color;
        // color.a = Mathf.InverseLerp(m_HiddenDistance, m_VisibleDistance, dist) * m_VisibleAlpha;
        // m_Material.color = color;

        // update azimuth based on center position
        var dir = Vector3.ProjectOnPlane(delta, Vector3.up).normalized;
        m_Azimuth = Vector3.SignedAngle(dir, Vector3.left, Vector3.up);

        // calculate new position from polar coordinate
        var pos = transform.position;
        var r = radius - m_Offset;
        var z = (m_Zenith - 90.0f) * Mathf.Deg2Rad;
        var a = (m_Azimuth) * Mathf.Deg2Rad;

        pos.x = r * Mathf.Sin(z) * Mathf.Cos(a);
        pos.y = r * Mathf.Cos(z);
        pos.z = r * Mathf.Sin(z) * Mathf.Sin(a);

        // update position from body
        transform.position = pos;
    }
}