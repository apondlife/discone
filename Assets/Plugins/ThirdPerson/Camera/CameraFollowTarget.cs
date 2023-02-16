using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class CameraFollowTarget: MonoBehaviour {
    // -- cfg --
    [Tooltip("the tuning parameters for the camera")]
    [SerializeField] CameraTuning m_Tuning;

    // -- refs --
    [Header("refs")]
    [Tooltip("the character model position we are trying to follow")]
    [SerializeField] Transform m_Model;

    [Tooltip("the target we are trying to look at")]
    [SerializeField] CameraLookAtTarget m_LookAtTarget;

    [Tooltip("the output follow destination for cinemachine")]
    [SerializeField] Transform m_Destination;

    [Tooltip("the free look camera input")]
    [FormerlySerializedAs("m_FreeLook")]
    [SerializeField] InputActionReference m_Input;

    // -- props --
    /// the current camera state
    CameraState m_State;

    /// the current character state
    CharacterState m_CharacterState;

    /// target damping velocity
    Vector3 m_CorrectionVel;

    /// the camera following sytem (state machine)
    CameraFollowSystem m_FollowSystem;

    /// the camera following sytem (state machine)
    CameraCollisionSystem m_CollisionSystem;

    // -- lifecycle --
    void Awake() {
        // set props
        m_State = new CameraState(new CameraState.Frame(), m_CharacterState, transform.localPosition);
    }

    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_CharacterState = character.State;

        // init systems
        m_FollowSystem = new CameraFollowSystem(
            m_State,
            m_Tuning,
            m_Input.action
        );

        m_CollisionSystem = new CameraCollisionSystem(
            m_State,
            m_Tuning
        );

        m_FollowSystem.Init();
        m_CollisionSystem.Init();

        // set initial position
        m_Destination.position = m_State.Next.Pos;
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // snapshot state w/ current world position (given character movement)
        m_State.Next.Pos = m_Destination.position;
        m_State.Snapshot();

        // run systems
        m_FollowSystem.Update(delta);
        m_CollisionSystem.Update(delta);

        // run collision system
        // m_State.Next.Pos = Vector3.SmoothDamp(
        //     m_State.Next.Pos,
        //     GetCorrectedPos(m_State.Next.Pos),
        //     ref m_CorrectionVel,
        //     m_Tuning.CorrectionSmoothTime,
        //     m_Tuning.CorrectionSpeed
        // );


        // update camera pos
        m_Destination.position = m_State.Next.Pos;
    }

    // -- queries --
    public void SetInvertX(bool value) {
        m_Tuning.IsInvertedX = value;
    }

    public void SetInvertY(bool value) {
        m_Tuning.IsInvertedY = value;
    }

    /// the target's position
    public Vector3 TargetPosition {
        get => m_Destination.position;
    }

    /// the base follow distance
    public float BaseDistance {
        get => m_Tuning.MinRadius;
    }

    /// the minimum follow distance
    public float MinDistance {
        get => m_Tuning.MinRadius * Mathf.Cos(Mathf.Deg2Rad * m_Tuning.FreeLook_MinPitch);
    }

    /// if free look is enabled
    public bool IsFreeLookEnabled {
        get => !m_State.IsTracking;
    }
}

}