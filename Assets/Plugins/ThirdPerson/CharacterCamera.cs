using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace ThirdPerson {

[RequireComponent(typeof(CinemachineVirtualCamera))]
public sealed class CharacterCamera: MonoBehaviour {
    // -- fields --
    [Header("references")]
    [Tooltip("the character's tunables/constants")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    // -- hacks --
    [Header("hacks")]
    [Tooltip("the recenter action")]
    [SerializeField] InputActionReference m_Recenter;

    // -- props --
    /// the cinemachine camera
    CinemachineVirtualCamera m_Camera;

    /// the camera's transposer (the body; controls camera movement)
    CinemachineTransposer m_Transposer;

    /// the character's current state
    CharacterState m_State;

    // -- lifecycle --
    void Awake() {
        m_Camera = GetComponent<CinemachineVirtualCamera>();
        m_Transposer = m_Camera.GetCinemachineComponent<CinemachineTransposer>();
        m_State = GetComponentInParent<Character>().State;
    }

    void Start() {
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
        m_Transposer.m_YawDamping = damping;
    }
}

}