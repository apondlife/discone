using UnityEngine;

namespace ThirdPerson {

/// the character's shared state
sealed class CharacterState: ScriptableObject {
    [Header("fields")]
    [Tooltip("the velocity on the xz-plane")]
    public Vector3 PlanarVelocity;

    [Tooltip("the previous planar velocity")]
    public Vector3 PrevPlanarVelocity;

    [Tooltip("the previous vertical speed")]
    public float PrevVerticalSpeed;

    [Tooltip("the speed on the y-axis")]
    public float VerticalSpeed = 0;

    [Tooltip("how much velocity changed since last update")]
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

    [Tooltip("the most recent hit")]
    public Collision Collision;

    public ContactPoint? Hit => Collision?.contactCount > 0 ? Collision?.GetContact(0) : null;

    // -- commands --
    /// updates the character state from an external velocity
    public void UpdateVelocity(Vector3 v0, Vector3 v1) {
        // capture intended velocity
        // TODO: buffer of character state
        PrevPlanarVelocity = PlanarVelocity;
        PrevVerticalSpeed = VerticalSpeed;

        // project velocity towards upward ramps
        var v1n = v1;
        var normal = Hit?.normal;
        if (normal != null && v1.y > 0) {
            v1n = Quaternion.FromToRotation(normal.Value, Vector3.up) * v1;
        }

        // update state
        SetProjectedPlanarVelocity(v1);
        VerticalSpeed = v1n.y;
        PlanarVelocity = v1.XNZ();
        Acceleration = (v1 - v0) / Time.deltaTime;

        if (Hit != null && Mathf.Abs(Vector3.Dot(Hit.Value.normal, Vector3.up)) < 0.5f) {
            Debug.Log($"hit wall: v={v1} n={Hit.Value.normal} v•up={Vector3.Dot(Hit.Value.normal, Vector3.up)}");
            Debug.Break();
        }
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

}