using UnityEngine;

/// the character's shared state
public class CharacterState: ScriptableObject {
    [SerializeField] private Vector3 _planarVelocity;
    public Vector3 PlanarVelocity {
        get {
            return _planarVelocity;
        }
        set {
            _planarVelocity = Vector3.ProjectOnPlane(value, Vector3.up);
        }
    }

    public float VerticalSpeed = 0;
    public bool IsGrounded = false;

    // -- queries --
    /// the character's calculated velocity in 3d-space
    public Vector3 Velocity {
        get => Vector3.up * VerticalSpeed + PlanarVelocity;
    }
}
