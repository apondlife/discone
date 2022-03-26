#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ThirdPerson {

/// gizmos for vizualizing character state
sealed partial class Character {
    // -- fields --
    [Header("config")]
    [Tooltip("if the gizmos are visible")]
    [SerializeField] private bool m_ShowGizmos;

    // -- props --
    /// the aggregate vertical label offset
    private Vector3 m_LabelOffset;

    // -- lifecycle --
    void OnDrawGizmos() {
        if(m_ShowGizmos) {
            DrawGizmos();
        }
    }

    // -- commands --
    /// draw all the gizmos
    void DrawGizmos() {
        m_LabelOffset = Vector3.zero;

        // draw state labels
        DrawLabel($"vy{m_State.VerticalSpeed}");

        if(m_State.IsInJumpSquat) {
            DrawLabel("JumpSquat");
        }

        if(m_State.IsGrounded) {
            DrawLabel("Grounded");
        }

        // draw controller gizmos
        m_Controller.DrawGizmos();
    }

    /// draw a label offset from the previous label
    void DrawLabel(string text) {
        DrawLabel(text, Vector3.up * 0.25f);
    }

    /// draw a label offset from the previous label
    void DrawLabel(string text, Vector3 offset) {
        m_LabelOffset += offset;
        Handles.Label(transform.position + transform.right * 1.0f + offset, text);
    }

    /// draw a ray in a direction from center
    void DrawRay(Color color, Vector3 dir) {
        Gizmos.color = color;
        Gizmos.DrawRay(transform.position, dir);
    }

    /// draw a cube at center
    void DrawCube(Color color, float size) {
        DrawCube(color, size, Vector3.zero);
    }

    /// draw a cube offset from center
    void DrawCube(Color color, float size, Vector3 offset) {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position + offset, Vector3.one * size);
    }

    /// draw a sphere at center
    void DrawSphere(Color color, float radius)  {
        DrawSphere(color, radius, Vector3.zero);
    }

    /// draw a sphere offset from center
    void DrawSphere(Color color, float radius, Vector3 offset)  {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + offset, 0.2f);
    }
}

}
#endif