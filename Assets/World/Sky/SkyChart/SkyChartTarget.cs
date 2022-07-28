using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

/// the sky-based navigation system
class SkyChartTarget: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the zenith of the object when we are far from it")]
    [UnityEngine.Range(-90.0f, 90.0f)]
    [SerializeField] float m_FarZenith;

    [Tooltip("the zenith of the object when we are close to it")]
    [UnityEngine.Range(-90.0f, 90.0f)]
    [SerializeField] float m_CloseZenith;

    [Tooltip("the prefab for the body")]
    [SerializeField] GameObject m_BodyPrefab;

    // -- refs --
    [Header("refs")]
    [Tooltip("the player")]
    [SerializeField] DisconePlayerVariable m_Player;

    [Tooltip("the parent for sky chart bodies")]
    [SerializeField] GameObjectVariable m_Bodies;

    // -- props --
    /// the celestial body
    SkyChartBody m_Body;

    // -- lifeycle --
    void Awake() {
        // create the body in the sky
        var body = Instantiate(
            m_BodyPrefab,
            m_Bodies.GetComponent<Transform>()
        );

        m_Body = body.GetComponent<SkyChartBody>();
    }

    void FixedUpdate() {
        var pt = transform.position;
        var pc = m_Player.Value.transform.position;

        // get delta to the target
        var delta = pt - pc;
        var dist = delta.magnitude;

        // update azimuth based on center position
        var dir = Vector3.ProjectOnPlane(delta, Vector3.up).normalized;

        // calculate new coordinate
        var coord = m_Body.Coordinate;
        coord.Azimuth = Vector3.SignedAngle(
            dir,
            Vector3.left,
            Vector3.up
        );

        coord.Zenith = Mathf.Lerp(
            m_CloseZenith,
            m_FarZenith,
            2.0f * Mathf.Atan(dist) / Mathf.PI
        );
;
        // update body
        m_Body.Coordinate = coord;
    }
}