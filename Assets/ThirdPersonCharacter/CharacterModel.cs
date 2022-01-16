using UnityEngine;
using Cinemachine;

// needs a reference to ThirdPersonCharacter
public class CharacterModel: MonoBehaviour {
    // -- props --
    [Tooltip("the character's current state")]
    [SerializeField] private CharacterState m_State;
    [SerializeField] private CharacterState m_PreviousState;

    [Tooltip("the character's tunables/constants")]
    [SerializeField] private CharacterTunablesBase m_Tunables;

    [Tooltip("the character's animator")]
    [SerializeField] private Animator m_Animator;

    [SerializeField] private CinemachineVirtualCamera m_Camera;

    private void Awake() {
        m_PreviousState = ScriptableObject.Instantiate(m_State);
    }

    [SerializeField] private float tiltForBaseAcceleration;
    [SerializeField] private float maxTilt;
    [SerializeField] private float tiltInterpolation;
    [SerializeField] private float dutchInterpolation;

    // -- lifecycle --
    void FixedUpdate() {
        // set move animation params
        m_Animator.SetFloat(
            "MoveSpeed",
            m_State.PlanarSpeed / m_Tunables.MaxPlanarSpeed
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


        var acceleration = transform.InverseTransformVector((m_State.PlanarVelocity - m_PreviousState.PlanarVelocity) / Time.deltaTime);
        var tilt =
        Mathf.Clamp(
            (acceleration.magnitude/m_Tunables.Acceleration) * tiltForBaseAcceleration,
            0,
            maxTilt);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            Quaternion.AngleAxis(tilt, Vector3.Cross(Vector3.up, acceleration.normalized).normalized),
            tiltInterpolation
        );

        m_Camera.m_Lens.Dutch = Mathf.LerpAngle(
            m_Camera.m_Lens.Dutch,
            transform.localRotation.eulerAngles.z,
            dutchInterpolation
        );

        m_PreviousState = ScriptableObject.Instantiate(m_State);
    }
}
