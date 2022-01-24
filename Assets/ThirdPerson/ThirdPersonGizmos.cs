using UnityEngine;
using UnityEditor;

namespace ThirdPerson {

/// gizmos for vizualizing character state
sealed partial class ThirdPerson {
    // -- fields --
    [Header("state")]
    [Tooltip("if the gizmos are visible")]
    [SerializeField] private bool m_ShowGizmos;

    private float m_LabelOffset = 0;

    // -- lifecycle --
    void OnDrawGizmos() {
        m_LabelOffset = 0;
        if(!m_ShowGizmos) {
            return;
        }

        // DrawRay(Color.green, m_State.FacingDirection);
        // DrawRay(Color.cyan, m_State.Velocity);
        // DrawRay(Color.blue, m_State.Tilt * Vector3.up);

        if(m_Hit != null) {
            DrawRay(Color.red, m_Hit.normal);
        } else {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, 0.2f);
        }

        DrawLabel($"vy{m_State.VerticalSpeed}");

        if(m_State.IsInJumpSquat) {
            DrawLabel("JumpSquat");
            // DrawCube(Color.magenta, 2.0f);
        }

        if(m_State.IsGrounded) {
            DrawLabel("Grounded");
            // DrawCube(Color.red, 2.0f);
        }

        m_Controller.DrawGizmos();
    }

    // -- commands --
    /// draw a label at a vertical offset
    void DrawLabel(string text, float offset = 0.25f) {
        m_LabelOffset += offset;
        Handles.Label(transform.position + Vector3.up * m_LabelOffset + transform.right*1.0f, text);
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

}