using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.InputSystem;
using Cinemachine;
using Soil;

namespace ThirdPerson {

/// a follow target that rotates around the player
public class Camera: MonoBehaviour, CameraContainer {
    // -- cfg --
    [Tooltip("the tuning parameters for the camera")]
    [SerializeField] CameraTuning m_Tuning;

    // -- systems --
    [Header("systems")]
    [Tooltip("the camera following system")]
    [SerializeField] CameraFollowSystem m_FollowSystem;

    [Tooltip("the camera collision system")]
    [SerializeField] CameraCollisionSystem m_CollisionSystem;

    [Tooltip("the camera zooming system")]
    [SerializeField] CameraZoomSystem m_ZoomSystem;

    [Tooltip("the camera tilting system")]
    [SerializeField] CameraTiltSystem m_TiltSystem;

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
    System<CameraContainer>[] m_Systems;

    // TODO: why do we need this here
    /// the attached character
    CharacterInputQuery m_CharacterInput;

    // -- lifecycle --
    void Start() {
        // set deps
        var c = GetComponentInParent<CharacterContainer>();
        m_State = new CameraState(
            new CameraState.Frame(),
            m_Follow.localPosition,
            c.State
        );

        m_CharacterInput = c.Inputs;

        // synchronize state
        var t = m_Camera.transform;
        m_State.Next.Forward = t.forward;
        m_State.Next.Up = t.up;

        // init systems
        m_Systems = new System<CameraContainer>[]{
            m_FollowSystem,
            m_CollisionSystem,
            m_ZoomSystem,
            m_TiltSystem,
        };

        foreach (var system in m_Systems) {
            system.Init(this);
        }

        // set initial position
        m_Destination.position = m_State.Next.Pos;

        // set camera lens props
        m_Camera.m_Lens.FieldOfView = m_State.Next.Fov;
        m_Camera.m_Lens.Dutch = m_State.Next.Dutch;

        // set camera clip shader props
        Shader.SetGlobalVector(
            ShaderProps.CameraClipPlane,
            new Plane(m_State.ClipNormal, m_State.ClipPos).AsVector4()
        );
    }

    void FixedUpdate() {
        var delta = Time.deltaTime;

        // synchronize state
        var camera = m_Camera.transform;
        m_State.Next.Forward = camera.forward;
        m_State.Next.Up = camera.up;

        // snapshot state w/ current world position (given character movement)
        m_State.Next.Pos = m_Destination.position;
        m_State.Advance();

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
            m_State.ClipPos
        );

        Shader.SetGlobalVector(
            ShaderProps.CameraClipPlane,
            new Plane(m_State.ClipNormal, m_State.ClipPos).AsVector4()
        );
    }

    // -- commands --
    /// move the camera into a new external position
    public void MoveTo(Vector3 pos) {
        var frame = m_State.Curr.Copy();
        frame.Spherical = m_State.WorldIntoSpherical(pos);
        m_State.Override(frame);
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
        get => m_State.Next.IsFreeLook;
    }

    /// the current state frame
    public CameraState.Frame Curr {
        get => m_State.Curr;
    }

    /// the cinemachine camera
    public CinemachineVirtualCamera Virtual {
        get => m_Camera;
    }

    // -- CameraContainer --
    /// the current camera state
    public CameraState State {
        get => m_State;
    }

    /// the tuning
    public CameraTuning Tuning {
        get => m_Tuning;
    }

    /// the free look camera input
    public InputAction Input {
        get => m_Input;
    }

    /// the character's input
    public CharacterInputQuery CharacterInput {
        get => m_CharacterInput;
    }

    // -- debug --
    void OnDrawGizmos() {
        var ideal = m_State.IntoIdealPosition();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(ideal, 0.3f);

        var actual = m_State.Next.Pos;
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(actual, 0.15f);

        Gizmos.color = Vector3.Distance(actual, ideal) < m_Tuning.Collision_ClipTolerance ? Color.green : Color.red;
        Gizmos.DrawLine(actual, ideal);
    }
}

}