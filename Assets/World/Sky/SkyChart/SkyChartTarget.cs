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

    [Tooltip("the speed of the target for it to lerp")]
    [SerializeField] float m_AngularSpeed;

    [Tooltip("the prefab for the body")]
    [SerializeField] GameObject m_BodyPrefab;

    [Tooltip("the custom material for the body, if any")]
    [SerializeField] Material m_BodyMaterial;

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

        body.name = $"Sky_{m_BodyPrefab.name}-{name}";

        // if we have a custom body material, set it
        if (m_BodyMaterial != null) {
            var renderers = body.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) {
                r.material = m_BodyMaterial;
            }
        }

        // store it
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
        var target = m_Body.Coordinate;
        target.Azimuth = Vector3.SignedAngle(
            dir,
            Vector3.left,
            Vector3.up
        );

        target.Zenith = Mathf.Lerp(
            m_CloseZenith,
            m_FarZenith,
            2.0f * Mathf.Atan(dist) / Mathf.PI
        );

        // lerp towards target
        // TODO: moves not always in same speed
        var coord = m_Body.Coordinate;
        coord.Azimuth = Mathf.MoveTowards(
            coord.Azimuth,
            target.Azimuth,
            m_AngularSpeed * Time.deltaTime
        );

        coord.Zenith = Mathf.MoveTowards(
            coord.Zenith,
            target.Zenith,
            m_AngularSpeed * Time.deltaTime
        );

        // update body
        m_Body.Coordinate = coord;
    }
}