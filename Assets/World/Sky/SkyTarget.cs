using UnityEngine;
using UnityAtoms;
using UnityAtoms.BaseAtoms;

/// the sky-based navigation system
class SkyTarget: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the zenith of the object when we are Far from it")]
    [UnityEngine.Range(-90.0f, 90.0f)]
    [SerializeField] float m_FarZenith;

    [Tooltip("the zenith of the object when we are at the same position as it")]
    [UnityEngine.Range(-90.0f, 90.0f)]
    [SerializeField] float m_CloseZenith;

    [Tooltip("the max distance where the star zenith stops changing")]
    [SerializeField] FloatReference m_Far;

    [Tooltip("the speed of the target for it to lerp")]
    [SerializeField] float m_AngularSpeed;

    [Tooltip("the prefab for the body")]
    [SerializeField] GameObject m_BodyPrefab;

    [Tooltip("the custom material for the body, if any")]
    [SerializeField] Material m_BodyMaterial;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current player")]
    [SerializeField] DisconePlayerVariable m_Player;

    [Tooltip("the parent for sky chart bodies")]
    [SerializeField] GameObjectVariable m_Bodies;

    // -- props --
    /// the celestial body
    SkyBody m_Body;

    /// a set of event subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

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
        m_Body = body.GetComponent<SkyBody>();
    }

    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_Player.Value.IsReadyChanged, OnPlayerIsReady);
    }

    void OnDestroy() {
        // destroy the attached body
        if (m_Body != null) {
            Destroy(m_Body.gameObject);
        }

        // unbind events
        m_Subscriptions.Dispose();
    }

    void FixedUpdate() {
        var desire = FindDesiredPosition();

        // move the body towards its target position
        // TODO: moves not always in same speed
        var coord = m_Body.Coordinate;
        coord.Azimuth = Mathf.MoveTowardsAngle(
            coord.Azimuth,
            desire.Azimuth,
            m_AngularSpeed * Time.deltaTime
        );

        coord.Zenith = Mathf.MoveTowardsAngle(
            coord.Zenith,
            desire.Zenith,
            m_AngularSpeed * Time.deltaTime
        );

        // update body
        m_Body.Coordinate = coord;
    }

    // -- events --
    /// when the player becomes ready with a character
    void OnPlayerIsReady(bool _) {
        m_Body.Coordinate = FindDesiredPosition();
    }

    // -- queries --
    /// get the body's current target position
    Spherical FindDesiredPosition() {
        var pt = transform.position;
        var pc = m_Player.Value.transform.position;

        // get delta to the target
        var delta = Vector3.ProjectOnPlane(pt - pc, Vector3.up);

        // calculate new coordinate
        var target = m_Body.Coordinate;
        target.Azimuth = Vector3.SignedAngle(
            delta.normalized,
            Vector3.left,
            Vector3.up
        );

        target.Zenith = Mathx.Remap(
            0.0f,
            m_Far.Value,
            m_CloseZenith,
            m_FarZenith,
            delta.magnitude
        );

        return target;
    }
}