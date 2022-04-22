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

    // -- props --
    /// the character's current state
    CharacterState m_State;

    /// the character's tunables / constants
    CharacterTunablesBase m_Tunables;

    // -- lifecycle --
    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = character.State;
        m_Tunables = character.Tunables;
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
}

}