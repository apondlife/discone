using Cinemachine;
using Soil;
using UnityAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

/// the debug flying camera
[RequireComponent(typeof(CinemachineVirtualCamera))]
sealed class DebugCamera: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the base move speed")]
    [SerializeField] float m_MoveSpeed;

    [Tooltip("the fast move speed")]
    [SerializeField] float m_RunSpeed;

    [Tooltip("the look speed")]
    [SerializeField] float m_LookSpeed;

    // -- cfg --
    [Header("cfg")]
    [Tooltip("the toggle action")]
    [SerializeField] InputActionReference m_Toggle;

    [Tooltip("the move action")]
    [SerializeField] InputActionReference m_Move;

    [Tooltip("the look action")]
    [SerializeField] InputActionReference m_Look;

    [Tooltip("the run action")]
    [SerializeField] InputActionReference m_Run;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current character")]
    [SerializeField] DisconePlayerVariable m_Player;

    // -- props --
    /// the debug flying camera
    CinemachineVirtualCamera m_Camera;

    /// the current rotation in euler angles (to avoid flip-flopping)
    Vector3 m_RotationEuler;

    /// the subscriptions
    readonly DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Awake() {
        // get dependencies
        m_Camera = GetComponent<CinemachineVirtualCamera>();

        // set initial state
        m_Camera.enabled = false;

        // bind events
        m_Subscriptions
            .Add(m_Toggle, OnTogglePressed);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    void Update() {
        if (!IsNoClip) {
            return;
        }
        var delta = Time.deltaTime;
        var ct = transform;

        // move the camera
        var move = m_Move.action.ReadValue<Vector3>();
        var speed = m_Run.action.IsPressed() ? m_RunSpeed : m_MoveSpeed;
        ct.position += transform.TransformDirection(speed * delta * move);

        // rotate the camera
        var look = m_Look.action.ReadValue<Vector2>().YXN();
        var lookVelocity = m_LookSpeed * delta * look;
        lookVelocity.x = -lookVelocity.x;
        m_RotationEuler += lookVelocity;
        ct.rotation = Quaternion.Euler(m_RotationEuler);
    }

    // -- commands --
    /// toggle between the debug & character cameras
    void ToggleDebugCamera() {
        var ct = transform;

        var nextEnabled = !m_Camera.enabled;
        if (nextEnabled) {
            var targetCamera = m_Player.Value.GetComponentInChildren<Camera>();
            var ot = targetCamera.transform;
            ct.position = ot.position;
            ct.rotation = ot.rotation;
            m_Camera.m_Lens.FieldOfView = targetCamera.fieldOfView;

            m_RotationEuler = ct.rotation.eulerAngles;
        }

        m_Player.Value.IsInputEnabled = !nextEnabled;
        m_Camera.enabled = nextEnabled;
    }

    // -- queries --
    public bool IsNoClip {
        get => m_Camera.enabled;
    }

    // -- events --
    void OnTogglePressed(InputAction.CallbackContext _) {
        ToggleDebugCamera();
    }
}

}