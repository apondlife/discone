using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using Cinemachine;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class Camera: MonoBehaviour {
    // -- cfg --
    [Tooltip("the tuning parameters for the camera")]
    [SerializeField] CameraTuning m_Tuning;

    // -- refs --
    [Header("refs")]
    [Tooltip("the cinemachine camera")]
    [SerializeField] CinemachineVirtualCamera m_Camera;

    [Tooltip("the character model position we are trying to follow")]
    [SerializeField] Transform m_Model;

    [Tooltip("the follow offset")]
    [SerializeField] Transform m_Follow;

    [Tooltip("the output follow destination for cinemachine")]
    [SerializeField] Transform m_Destination;

    [Tooltip("the target we are trying to look at")]
    [SerializeField] CameraLookAtTarget m_LookAtTarget;

    [Tooltip("the free look camera input")]
    [FormerlySerializedAs("m_FreeLook")]
    [SerializeField] InputActionReference m_Input;

    // -- props --
    /// the current camera state
    CameraState m_State;

    /// target damping velocity
    Vector3 m_CorrectionVel;

    /// the list of systems acting on this camera
    CameraSystem[] m_Systems;

    /// the camera following sytem (state machine)
    [SerializeField] CameraFollowSystem m_FollowSystem;

    /// the camera following sytem (state machine)
    [SerializeField] CameraCollisionSystem m_CollisionSystem;

    /// the camera zooming sytem (state machine)
    [SerializeField] CameraZoomSystem m_ZoomSystem;

    /// the camera tilting sytem (state machine)
    [SerializeField] CameraTiltSystem m_TiltSystem;

    // -- lifecycle --
    void Start() {
        // set deps
        var character = GetComponentInParent<Character>();
        m_State = new CameraState(
            new CameraState.Frame(),
            m_Follow.localPosition,
            character.State
        );

        // synchronize state
        m_State.Next.Forward = m_Camera.transform.forward;
        m_State.Next.Up = m_Camera.transform.up;

        // init systems
        m_Systems = new CameraSystem[]{
            m_FollowSystem,
            m_CollisionSystem,
            m_ZoomSystem,
            m_TiltSystem,
        };

        var characterInput = character.Input;
        foreach (var system in m_Systems) {
            system.Init(
                m_State,
                m_Tuning,
                m_Input,
                characterInput
            );
        }

        // set initial position
        m_Destination.position = m_State.Next.Pos;

        // set camera lens props
        m_Camera.m_Lens.FieldOfView = m_State.Next.Fov;
        m_Camera.m_Lens.Dutch = m_State.Next.Dutch;

        // set camera clip shader props
        Shader.SetGlobalVector(
            ShaderProps.CameraClipPlane,
            new Plane(m_CollisionSystem.ClipNormal, m_CollisionSystem.ClipPos).AsVector4()
        );
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // synchronize state
        m_State.Next.Forward = m_Camera.transform.forward;
        m_State.Next.Up = m_Camera.transform.up;

        // snapshot state w/ current world position (given character movement)
        m_State.Next.Pos = m_Destination.position;
        m_State.Snapshot();

        // run systems
        foreach (var system in m_Systems) {
            system.Update(delta);
        }

        // update camera coords
        m_Destination.position = m_State.Next.Pos;

        // set camera lens properties
        m_Camera.m_Lens.FieldOfView = m_State.Next.Fov;
        m_Camera.m_Lens.Dutch = m_State.Next.Dutch;

        // set camera clip shader props
        Shader.SetGlobalVector(
            ShaderProps.CameraClipPos,
            m_CollisionSystem.ClipPos
        );

        Shader.SetGlobalVector(
            ShaderProps.CameraClipPlane,
            new Plane(m_CollisionSystem.ClipNormal, m_CollisionSystem.ClipPos).AsVector4()
        );
    }

    // -- queries --
    public void SetInvertX(bool value) {
        m_Tuning.IsInvertedX = value;
    }

    public void SetInvertY(bool value) {
        m_Tuning.IsInvertedY = value;
    }

    /// the distance between the follow's curr and dest positions
    public float FollowDistance {
        get => Vector3.Distance(
            m_Follow.position, // TODO: should this be ground target?
            m_Destination.position
        );
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
        get => m_State.IsFreeLook;
    }

    // -- debug --
    void OnDrawGizmos() {
        var ideal = m_State.IntoIdealPosition();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(ideal, 0.3f);

        var idealDest = m_State.IntoIdealDestPosition();
        Gizmos.color = Color.green + Color.yellow * 0.5f;
        Gizmos.DrawWireSphere(idealDest, 0.3f);

        var actual = m_State.Pos;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(actual, 0.15f);

        Gizmos.color = Vector3.Distance(actual, ideal) < m_Tuning.Collision_ClipTolerance ? Color.green : Color.red;
        Gizmos.DrawLine(actual, ideal);
    }
}

}