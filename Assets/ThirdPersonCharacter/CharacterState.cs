using UnityEngine;

/// the character's shared state
public class CharacterState: ScriptableObject {
    [Header("fields")]
    [Tooltip("the speed on the xz-plane")]
    public float PlanarSpeed;

    [Tooltip("the speed on the y-axis")]
    public float VerticalSpeed = 0;

    [Tooltip("if the character is grounded")]
    public bool IsGrounded = false;

    [Tooltip("the current facing direction")]
    public Vector3 _facingDirection = Vector3.zero;

    /// the current facing direction
    public Vector3 FacingDirection => _facingDirection;

    // -- commands --
    /// sets the facing direction on the xz plane
    public void SetProjectedFacingDirection(Vector3 velocity) {
        _facingDirection = Vector3.ProjectOnPlane(velocity, Vector3.up).normalized;
    }

    /// updates the character state from an external velocity
    public void SyncExternalVelocity(Vector3 velocity) {
        SetProjectedFacingDirection(velocity);
        VerticalSpeed = velocity.y;
        PlanarSpeed = velocity.XNZ().magnitude;
    }

    // -- queries --
    /// the character's calculated velocity on the xz-plane
    public Vector3 PlanarVelocity => PlanarSpeed * FacingDirection;

    /// the character's calculated velocity in 3d-space
    public Vector3 Velocity {
        get => Vector3.up * VerticalSpeed + PlanarVelocity;
    }
}
