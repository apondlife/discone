using UnityEngine;
using Cinemachine;

// needs a reference to ThirdPersonCharacter
public class CharacterModel: MonoBehaviour {
    // -- props --
    [Tooltip("the character's current state")]
    [SerializeField] private CharacterState m_State;
    private CharacterState m_PreviousState;

    [Tooltip("the character's tunables/constants")]
    [SerializeField] private CharacterTunablesBase m_Tunables;

    [Tooltip("the character's animator")]
    [SerializeField] private Animator m_Animator;

    [SerializeField] private CinemachineVirtualCamera m_Camera;

    private void Awake() {
        m_PreviousState = ScriptableObject.Instantiate(m_State);
    }

    // -- lifecycle --
    void FixedUpdate() {
        // update animator & model
        SyncAnimator();
        Tilt();

        // capture current state
        m_PreviousState = ScriptableObject.Instantiate(m_State);
    }

    // -- commands --
    /// sync the animator's params
    void SyncAnimator() {
        // set move animation params
        m_Animator.SetFloat(
            "MoveSpeed",
            Mathf.InverseLerp(
                m_Tunables.MinPlanarSpeed,
                m_Tunables.MaxPlanarSpeed,
                m_State.PlanarVelocity.magnitude
            )
        );

        // set jump animation params
        m_Animator.SetBool(
            "JumpSquat",
            m_State.IsInJumpSquat
        );

        m_Animator.SetBool(
            "Airborne",
            !m_State.IsGrounded
        );

        m_Animator.SetFloat(
            "VerticalSpeed",
            m_State.VerticalSpeed
        );

    }

    /// tilt the model as a fn of character acceleration
    void Tilt() {
        // this is a fundamental misunderstanding of quaternions
        transform.rotation = m_State.LookRotation;
    }
}
