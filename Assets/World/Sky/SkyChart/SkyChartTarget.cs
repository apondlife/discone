using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

/// the sky-based navigation system
[ExecuteAlways]
[RequireComponent(typeof(SkyChartBody))]
class SkyChartTarget: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the target object")]
    [SerializeField] Transform m_Target;

    [Tooltip("the zenith of the object when we are far from it")]
    [UnityEngine.Range(-90.0f, 90.0f)]
    [SerializeField] float m_FarZenith;

    [Tooltip("the zenith of the object when we are close to it")]
    [UnityEngine.Range(-90.0f, 90.0f)]
    [SerializeField] float m_CloseZenith;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player")]
    [SerializeField] DisconePlayerVariable m_Player;


    // -- props --
    /// the celestial body
    SkyChartBody m_Body;
    void Awake() {
      m_Body = GetComponent<SkyChartBody>();
    }

    // -- lifeycle --
    void FixedUpdate() {
        var pt = m_Target.transform.position;
        var pc = m_Player.Value.transform.position;

        // get delta to the target
        var delta = pt - pc;
        var dist = delta.magnitude;

        // update azimuth based on center position
        var dir = Vector3.ProjectOnPlane(delta, Vector3.up).normalized;

        // calculate new coordinate
        var coord = m_Body.Coordinate;
        coord.Azimuth = Vector3.SignedAngle(dir, Vector3.left, Vector3.up);
        coord.Zenith = ZenithCurve(dist);

        // update body
        m_Body.Coordinate = coord;
    }

    // zenith angle given the distance
    float ZenithCurve(float x) {
      var k =  2* Mathf.Atan(x) / Mathf.PI;
      return Mathf.Lerp(m_CloseZenith, m_FarZenith, k);
    }
}