using Soil;
using UnityEngine;

namespace ThirdPerson {

/// a container for the character's model and animations
public class CharacterRig: CharacterBehaviour, CharacterAnimatorProxy.Target {
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

    [Tooltip("the character's effects")]
    [SerializeField] CharacterBehaviour[] m_Effects;

    // -- props --
    /// the stored look rotation
    Quaternion m_LookRotation = Quaternion.identity;

    /// the current move tilt rotation
    Quaternion m_MoveTilt = Quaternion.identity;

    /// the current wall tilt rotation
    Quaternion m_SurfaceTilt = Quaternion.identity;

    // -- lifecycle --
    public override void Init(CharacterContainer c) {
        base.Init(c);

        // if this character is not animated, destroy all limbs and procedural animations
        if (!m_Animator) {
            Log.Model.W($"character {c.Name} has no animator, disabling limbs");
            m_Head.gameObject.SetActive(false);
            m_Legs.gameObject.SetActive(false);
            m_Arms.gameObject.SetActive(false);
            return;
        }

        // init ik limbs
        m_Legs.Init(c);
        m_Arms.Init(c);
        m_Head.Init(c);

        // release the effects list
        // m_Effects = null;

        // proxy animator callbacks
        var proxy = m_Animator.gameObject.GetComponent<CharacterAnimatorProxy>();
        if (proxy == null) {
            proxy = m_Animator.gameObject.AddComponent<CharacterAnimatorProxy>();
        }

        proxy.Bind(this);
    }

    public override void Step_I(float delta) {
        var frame = c.State.Interpolated;

        // step ik limbs
        m_Head.Step(delta);
        m_Legs.Step(delta);
        m_Arms.Step(delta);

        Tilt(frame, delta);

        // TODO: should this be using the interpolated frame? or next?
        m_LookRotation = Quaternion.RotateTowards(
            m_LookRotation,
            c.State.Curr.LookRotation,
            m_LookRotation_Speed * Time.deltaTime
        );

        var tilt = m_MoveTilt * m_SurfaceTilt;
        transform.localRotation =  tilt * m_LookRotation;
    }

    public override void Step_Fixed_I(float delta) {
        // step ik limbs
        m_Head.Step_Fixed(delta);
        m_Legs.Step_Fixed(delta);
        m_Arms.Step_Fixed(delta);
    }

    // -- CharacterAnimatorProxy.Target --
    public void OnAnimatorIk(int layer) {
        m_Head.ApplyIk();
        m_Legs.ApplyIk();
        m_Arms.ApplyIk();
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

    // -- queries --
    /// the animator
    public Animator Animator {
        get => m_Animator;
    }

    /// finds a limb based on ik goal
    public Limb FindLimb(AvatarIKGoal goal) {
        return goal switch {
            AvatarIKGoal.LeftFoot => m_Legs.Left,
            AvatarIKGoal.RightFoot => m_Legs.Right,
            AvatarIKGoal.LeftHand => m_Arms.Left,
            _ /*AvatarIKGoal.RightHand*/ => m_Arms.Right,
        };
    }
}

}