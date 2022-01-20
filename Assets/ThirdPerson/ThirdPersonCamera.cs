using UnityEngine;
using Cinemachine;

namespace ThirdPerson {

[RequireComponent(typeof(CinemachineVirtualCamera))]
sealed class ThirdPersonCamera: MonoBehaviour {
    // -- fields --
    [Header("references")]

    [Tooltip("the cinemachine camera")]
    CinemachineVirtualCamera m_Camera;

    [Tooltip("the character's current state")]
    [SerializeField] CharacterState m_State;

    [Tooltip("the character's tunables/constants")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    // -- lifecycle --
    private void Awake() {
        m_Camera = GetComponent<CinemachineVirtualCamera>();
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

}

}