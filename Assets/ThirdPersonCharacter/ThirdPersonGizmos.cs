using UnityEngine;
using UnityEditor;

/// gizsmos for vizualizing character state
public partial class ThirdPerson {
    // -- fields --
    [Header("state")]
    [Tooltip("if the gizmos are visible")]
    [SerializeField] private bool m_ShowGizmos;

    // -- lifecycle --
    void OnDrawGizmos() {
        if(!m_ShowGizmos) {
            return;
        }

        DrawRay(Color.green, m_State.FacingDirection);

        if(m_State.IsInJumpSquat) {
            DrawLabel("JumpSquat", 2.0f);
            DrawCube(Color.magenta, 2.0f);
        }

        if(m_State.IsGrounded) {
            DrawLabel("Grounded", 2.0f);
            DrawCube(Color.red, 2.0f);
        }
    }

    // -- commands --
    /// draw a label at a vertical offset
    void DrawLabel(string text, float offset) {
        Handles.Label(transform.position + Vector3.up * offset, text);
    }

    /// draw a ray in a direction
    void DrawRay(Color color, Vector3 dir) {
        Gizmos.color = color;
        Gizmos.DrawRay(transform.position, dir);
    }

    /// draw a cube at a vertical offset
    void DrawCube(Color color, float offset, float size = 2.0f) {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position + Vector3.up * offset, Vector3.one * size);
    }
}