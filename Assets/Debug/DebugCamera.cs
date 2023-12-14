using System;
using Cinemachine;
using ThirdPerson;
using UnityAtoms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

[RequireComponent(typeof(CinemachineVirtualCamera))]
sealed class DebugCamera : MonoBehaviour {
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

    CinemachineVirtualCamera m_Camera;

    void Awake() {
        m_Camera = GetComponent<CinemachineVirtualCamera>();
    }

    void Update() {
        var ct = transform;

        if (m_Toggle.action.WasPerformedThisFrame()) {
            var nextEnabled = !m_Camera.enabled;
            if (nextEnabled) {
                var targetCamera = m_Player.Value.GetComponentInChildren<UnityEngine.Camera>();

                var ot = targetCamera.transform;
                ct.position = ot.position;
                ct.rotation = ot.rotation;

                m_Camera.m_Lens.FieldOfView = targetCamera.fieldOfView;
            }

            m_Player.Value.IsInputEnabled = !nextEnabled;
            m_Camera.enabled = nextEnabled;
        }

        if (!m_Camera.enabled) {
            return;
        }

        var move = m_Move.action.ReadValue<Vector3>();
        var speed = m_Run.action.IsPressed() ? m_RunSpeed : m_MoveSpeed;
        ct.position += transform.TransformDirection(speed * Time.deltaTime * move);

        var look = m_Look.action.ReadValue<Vector2>();
        var vLook = m_LookSpeed * Time.deltaTime * look;
        var rotation = Quaternion.AngleAxis(vLook.x, Vector3.up) * Quaternion.AngleAxis(vLook.y, Vector3.right);
        ct.rotation *= rotation;
    }
}

}