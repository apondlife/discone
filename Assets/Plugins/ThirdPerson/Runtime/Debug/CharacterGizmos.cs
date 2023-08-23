#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace ThirdPerson {

/// gizmos for vizualizing character state
sealed partial class Character {
    // -- fields --
    [Header("config")]
    [Tooltip("if the gizmos are visible")]
    [SerializeField] bool m_ShowGizmos;

    [Tooltip("the line magnitude scale")]
    [SerializeField] float m_LineMagScale = 1f;

    [Tooltip("the line thickness")]
    [SerializeField] float m_LineThickness = 3f;

    // -- props --
    /// the aggregate vertical label offset
    Vector3 m_LabelOffset;

    // -- lifecycle --
    void OnDrawGizmos() {
        if (!m_ShowGizmos) {
            return;
        }

        DrawRay(Color.magenta, m_State.Next.Inertia);

        // draw controller gizmos
        m_Controller.OnDrawGizmos();
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
    void DrawRay(Color color, Vector3 ray) {
        var p0 = transform.position;
        Gizmos.color = color;
        Gizmos.DrawLine(p0, p0 + ray * m_LineMagScale);
    }

    /// draw a cube at center
    void DrawCube(Color color, float size) {
        DrawCube(color, size, Vector3.zero);
    }

    /// draw a cube offset from center
    void DrawCube(Color color, float size, Vector3 offset) {
        Handles.color = color;
        Handles.DrawWireCube(transform.position + offset, Vector3.one * size);
    }
}

}
#endif