using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace ThirdPerson {

/// the third person camera
public sealed class Camera: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the cinemachine camera")]
    [SerializeField] CinemachineVirtualCamera m_Camera;

    // -- hacks --
    [Header("hacks")]
    [Tooltip("the recenter action")]
    [SerializeField] InputActionReference m_Recenter;

    // -- props --
    /// the character's current state
    CharacterState m_State;

    /// the character's tunables / constants
    CharacterTunablesBase m_Tunables;

    /// the camera's transposer (the body; controls camera movement)
    CinemachineTransposer m_Transposer;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Transposer = m_Camera.GetCinemachineComponent<CinemachineTransposer>();
    }

    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = character.State;
        m_Tunables = character.Tunables;

        // set initial damping
        SetDamping(m_Tunables.Damping);
    }

    void Update() {
        var recenter = m_Recenter.action;
        if (recenter.WasPressedThisFrame()) {
            SetDamping(m_Tunables.FastDamping);
        } else if (recenter.WasReleasedThisFrame()) {
            SetDamping(m_Tunables.Damping);
        }
    }

    void FixedUpdate() {
        // get angle between tilt up and camera up
        var tilt = Vector3.SignedAngle(
            transform.up,
            Vector3.ProjectOnPlane(m_State.Tilt * Vector3.up, transform.forward),
            transform.forward
        );

        // map angle from [0, 360] to [-180, 180]
        if (tilt > 180.0f) {
            tilt -= 360.0f;
        }

        // TODO: smoothing with a finite end time (tween)
        m_Camera.m_Lens.Dutch = Mathf.LerpAngle(
            m_Camera.m_Lens.Dutch,
            tilt * m_Tunables.DutchScale,
            m_Tunables.DutchSmoothing
        );
    }

    // -- commands --
    /// set the camera's yaw damping to control recentering speed (lower is faster)
    void SetDamping(float damping) {
        if(m_Transposer) {
            m_Transposer.m_YawDamping = damping;
        }
    }
}

}