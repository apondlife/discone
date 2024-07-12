using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public class CharacterRig: MonoBehaviour, CharacterAnimatorProxy.Target {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the rotation speed in degrees towards look direction")]
    [SerializeField] float m_LookRotation_Speed;

    // -- refs --
    [Header("refs")]
    [Tooltip("the character's head")]
    [SerializeField] CharacterHead m_Head;

    [Tooltip("the character's legs")]
    [SerializeField] CharacterLegs m_Legs;

    [Tooltip("the character's arms")]
    [SerializeField] CharacterArms m_Arms;

    [Tooltip("the character's animator")]
    [SerializeField] Animator m_Animator;

    // -- props --
    /// the containing character
    CharacterContainer c;

    /// the list of ik limbs
    CharacterPart[] m_Limbs;

    /// the stored look rotation
    Quaternion m_LookRotation = Quaternion.identity;

    /// the current move tilt rotation
    Quaternion m_MoveTilt = Quaternion.identity;

    /// the current wall tilt rotation
    Quaternion m_SurfaceTilt = Quaternion.identity;

    /// the interpolated state frame
    CharacterState.Frame m_Frame = new();

    // TODO: don't use total time
    /// the stored last time of fixed update (for interpolation)
    float m_LastFixedUpdate = 0.0f;

    // -- lifecycle --
    void Awake() {
        // set dependencies
        c = GetComponentInParent<CharacterContainer>();

        // if this character is not animated, destroy all limbs and procedural animations
        if (!m_Animator) {
            Log.Model.E($"character {c.Name} has no animator, destroying limbs");
            Destroy(m_Head.gameObject);
            Destroy(m_Legs.gameObject);
            Destroy(m_Arms.gameObject);
            return;
        }

        // set props
        m_Limbs = GetComponentsInChildren<CharacterPart>();

        // init ik limbs
        foreach (var limb in m_Limbs) {
            limb.Init(m_Animator);
        }

        // proxy animator callbacks
        var proxy = m_Animator.gameObject.GetComponent<CharacterAnimatorProxy>();
        if (proxy == null) {
            proxy = m_Animator.gameObject.AddComponent<CharacterAnimatorProxy>();
        }

        proxy.Bind(this);
    }

    void FixedUpdate() {
        m_LastFixedUpdate = Time.time;
    }

    void Update() {
        var delta = Time.time - m_LastFixedUpdate;
        m_Frame.Interpolate(
            c.State.Curr,
            c.State.Next,
            delta / Time.fixedDeltaTime
        );

        Tilt(m_Frame, Time.deltaTime);

        m_LookRotation = Quaternion.RotateTowards(
            m_LookRotation,
            c.State.Curr.LookRotation,
            m_LookRotation_Speed * Time.deltaTime
        );

        var tilt = m_MoveTilt * m_SurfaceTilt;
        transform.localRotation =  tilt * m_LookRotation;
    }

    // -- CharacterAnimatorProxy.Target --
    public void OnAnimatorIk(int layer) {
        foreach (var limb in m_Limbs) {
            limb.ApplyIk();
        }
    }

    // -- commands --
    /// tilt the model as a fn of character acceleration
    void Tilt(CharacterState.Frame state, float delta) {
        var surface = state.MainSurface;

        // get tilt against acceleration
        var acceleration = c.State.Curr.PlanarAcceleration;
        var accelerationTiltAxis = Vector3.Cross(
            Vector3.up,
            acceleration.normalized
        );

        var accelerationTilt = Quaternion.AngleAxis(
            c.Tuning.Model.Tilt_AccelerationAngle.Evaluate(acceleration.magnitude),
            accelerationTiltAxis
        );

        // get tilt against input
        var input = c.Inputs.Move;

        var inputTiltAxis = Vector3.Cross(
            Vector3.up,
            input.normalized
        );

        var inputTilt = Quaternion.AngleAxis(
            c.Tuning.Model.Tilt_InputAngle.Evaluate(input.magnitude),
            inputTiltAxis
        );

        // interpolate move tilt
        var nextMoveTilt = Quaternion.RotateTowards(
            m_MoveTilt,
            accelerationTilt * inputTilt,
            c.Tuning.Model.Tilt_MoveSpeed * delta
        );

        // get tilt against surface
        var surfaceTiltAxis = Vector3.Cross(
            Vector3.up,
            surface.Normal
        );

        var surfaceTilt = Quaternion.AngleAxis(
            c.Tuning.Model.Tilt_SurfaceAngle.Evaluate(surface.Angle),
            surfaceTiltAxis
        );

        var nextSurfaceTilt = Quaternion.RotateTowards(
            m_SurfaceTilt,
            surfaceTilt,
            c.Tuning.Model.Tilt_SurfaceSpeed * delta
        );

        // update state
        m_MoveTilt = nextMoveTilt;
        m_SurfaceTilt = nextSurfaceTilt;
    }
}

}