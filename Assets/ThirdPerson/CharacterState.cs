using UnityEngine;

/// the character's shared state
public class CharacterState : ScriptableObject {
    [Header("fields")]
    [Tooltip("the speed on the xz-plane")]
    public float PlanarSpeed;

    [Tooltip("the speed on the y-axis")]
    public float VerticalSpeed = 0;

    [Tooltip("how much the speed changed since last update")]
    public Vector3 Acceleration;

    [Tooltip("the current facing direction")]
    [SerializeField] private Vector3 _facingDirection = Vector3.zero;

    /// the current facing direction
    public Vector3 FacingDirection => _facingDirection;

    [Tooltip("the current planar direction")]
    [SerializeField] private Vector3 _planarDirection = Vector3.zero;

    /// the current planar direction
    public Vector3 PlanarDirection => _planarDirection;

    [Tooltip("if the character is grounded")]
    public bool IsGrounded = false;

    [Tooltip("if the character is in jump squat")]
    public bool IsInJumpSquat = false;

    [Tooltip("how much tilted the character is")]
    public Quaternion Tilt;

    // -- commands --
    /// updates the character state from an external velocity
    public void UpdateVelocity(Vector3 v0, Vector3 v1) {
        SetProjectedPlanarDirection(v1);
        VerticalSpeed = v1.y;
        PlanarSpeed = v1.XNZ().magnitude;
        Acceleration = (v1 - v0) / Time.deltaTime;
    }

    /// sets the facing direction on the xz plane
    public void SetProjectedFacingDirection(Vector3 direction) {
        var newDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

        // don't set direction to zero vector. if that happens, keep previous direction
        if(newDirection.sqrMagnitude > 0 ) {
            _facingDirection = newDirection;
        }
    }

    /// sets the planar direction on the xz plane
    public void SetProjectedPlanarDirection(Vector3 direction) {
        var newDirection = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

        // don't set direction to zero vector. if that happens, keep previous direction
        if(newDirection.sqrMagnitude > 0 ) {
            _planarDirection = newDirection;
        }
    }

    // -- queries --
    /// the character's calculated velocity on the xz-plane
    public Vector3 PlanarVelocity => PlanarSpeed * PlanarDirection;

    /// the character's calculated velocity in 3d-space
    public Vector3 Velocity {
        get => Vector3.up * VerticalSpeed + PlanarVelocity;
    }

    public Quaternion LookRotation => Tilt * Quaternion.LookRotation(FacingDirection, Vector3.up);
}
