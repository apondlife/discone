using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace ThirdPerson {

[RequireComponent(typeof(CinemachineVirtualCamera))]
public sealed class ThirdPersonCamera: MonoBehaviour {
    // -- fields --
    [Header("references")]
    [Tooltip("the cinemachine camera")]
    CinemachineVirtualCamera m_Camera;


    [Tooltip("the character's tunables/constants")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    // -- props --
    /// the camera's transposer (the body; controls camera movement)
    CinemachineTransposer m_Transposer;

    /// the character's current state
    CharacterState m_State;

    // -- lifecycle --
    private void Awake() {
        m_Camera = GetComponent<CinemachineVirtualCamera>();
        m_Transposer = m_Camera.GetCinemachineComponent<CinemachineTransposer>();
        m_State = GetComponentInParent<ThirdPerson>().State;
    }

    private void Start() {
        SetDamping(m_Tunables.Damping);
    }

    private void FixedUpdate() {
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

    // -- events --
    /// recenter the camera on the player
    public void OnRecenter(InputAction.CallbackContext ctx) {
        var pressed = ctx.ReadValueAsButton();
        SetDamping(pressed ? m_Tunables.FastDamping : m_Tunables.Damping);
    }
}

}