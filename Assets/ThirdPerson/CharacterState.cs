using UnityEngine;

/// the character's shared state
public class CharacterState : ScriptableObject {
    [Header("fields")]
    [Tooltip("the velocity on the xz-plane")]
    public Vector3 PlanarVelocity;

    [Tooltip("the speed on the y-axis")]
    public float VerticalSpeed = 0;

    [Tooltip("how much the speed changed since last update")]
    public Vector3 Acceleration;

    [Tooltip("the current facing direction")]
    [SerializeField] private Vector3 _facingDirection = Vector3.zero;

    /// the current facing direction
    public Vector3 FacingDirection => _facingDirection;

    [Tooltip("if the character is grounded")]
    public bool IsGrounded = false;

    [Tooltip("if the character is in jump squat")]
    public bool IsInJumpSquat = false;

    [Tooltip("how much tilted the character is")]
    public Quaternion Tilt;

    // -- commands --
    /// updates the character state from an external velocity
    public void UpdateVelocity(Vector3 v0, Vector3 v1, Vector3? normal) {
        var v1n = v1;
        // project velocity towards upward ramps
        if (normal != null && v1.y > 0) {
            v1n = Quaternion.FromToRotation(normal.Value, Vector3.up) * v1;
        }

        SetProjectedPlanarVelocity(v1);
        VerticalSpeed = v1n.y;
        PlanarVelocity = v1.XNZ();
        Acceleration = (v1 - v0) / Time.deltaTime;
    }

    /// sets the facing direction on the xz plane
    public void SetProjectedFacingDirection(Vector3 dir) {
        var projected = Vector3.ProjectOnPlane(dir, Vector3.up);

        // if zero, use the original direction
        if (projected.sqrMagnitude > 0.0f) {
            _facingDirection = projected.normalized;
        }
    }

    /// sets the planar direction on the xz plane
    public void SetProjectedPlanarVelocity(Vector3 dir) {
        var projected = Vector3.ProjectOnPlane(dir, Vector3.up);

        // if zero, use the original direction
        if (projected.sqrMagnitude > 0.0f) {
            PlanarVelocity = projected;
        }
    }

    // -- queries --
    /// the character's calculated velocity in 3d-space
    public Vector3 Velocity {
        get => Vector3.up * VerticalSpeed + PlanarVelocity;
    }

    /// the characters look rotation (facing & tilt)
    public Quaternion LookRotation {
        get => Tilt * Quaternion.LookRotation(FacingDirection, Vector3.up);
    }
}
