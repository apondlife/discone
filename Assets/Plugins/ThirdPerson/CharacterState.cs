using UnityEngine;

namespace ThirdPerson {

/// the character's shared state
/// TODO: feel like this should just be a (readonly?!) struct, draft the next frame, store
/// a buffer of readonly frames. CONTEXT: i really need a state-based jump
public sealed class CharacterState {
    [Header("fields")]
    [Tooltip("the velocity on the xz-plane")]
    public Vector3 PlanarVelocity;

    [Tooltip("the previous planar velocity")]
    public Vector3 PrevPlanarVelocity;

    [Tooltip("the speed on the y-axis")]
    public float VerticalSpeed = 0;

    [Tooltip("the previous vertical speed")]
    public float PrevVerticalSpeed;

    [Tooltip("how much velocity changed since last update")]
    public Vector3 Acceleration;

    [Tooltip("the current facing direction")]
    [SerializeField] private Vector3 m_FacingDirection = Vector3.zero;

    /// the current facing direction
    public Vector3 FacingDirection => m_FacingDirection;

    [Tooltip("if the character is grounded")]
    public bool IsGrounded = false;

    [Tooltip("if the character is in jump squat")]
    public bool IsInJumpSquat = false;

    [Tooltip("the current frame of jump squat")]
    public int JumpSquatFrame = -1;

    [Tooltip("if the character is in its first jump frame")]
    public bool IsInJumpStart = false;

    [Tooltip("how much tilted the character is")]
    public Quaternion Tilt;

    [Tooltip("the last collided surface")]
    public CharacterCollision? Collision;

    [Tooltip("whether or not the characer is on the wall")]
    public bool IsOnWall;

    // -- commands --
    /// reset to initial state
    public void Reset() {
        PlanarVelocity = Vector3.zero;
        PrevPlanarVelocity = Vector3.zero;
        VerticalSpeed = 0.0f;
        PrevVerticalSpeed = 0.0f;
        Acceleration = Vector3.zero;
        m_FacingDirection = Vector3.forward; // TODO: should be pulled from transform
        IsGrounded = false;
        IsInJumpSquat = false;
        IsInJumpStart = false;
        Tilt = Quaternion.identity;
    }

    /// updates the character state from an external velocity
    public void UpdateVelocity(Vector3 v0, Vector3 v1) {
        // capture intended velocity
        // TODO: buffer of character state
        PrevPlanarVelocity = PlanarVelocity;
        PrevVerticalSpeed = VerticalSpeed;

        // update state
        SetProjectedPlanarVelocity(v1);
        VerticalSpeed = v1.y;
        PlanarVelocity = v1.XNZ();
        Acceleration = (v1 - v0) / Time.deltaTime;
    }

    /// sets the facing direction on the xz plane
    public void SetProjectedFacingDirection(Vector3 dir) {
        var projected = Vector3.ProjectOnPlane(dir, Vector3.up);

        // if zero, use the original direction
        if (projected.sqrMagnitude > 0.0f) {
            m_FacingDirection = projected.normalized;
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
        get {
            var look = FacingDirection;

            // if airborne, look in direction of velocity
            // TODO: we don't want to calculate a look direction from a zero velocity,
            // but is using FacingDirection in this situation correct?
            if (!IsGrounded && PlanarVelocity != Vector3.zero) {
                look = PlanarVelocity.normalized;
            }

            return Tilt * Quaternion.LookRotation(look, Vector3.up);
        }
    }
}

}