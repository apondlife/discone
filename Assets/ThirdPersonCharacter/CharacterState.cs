using UnityEngine;

// the character's current state
[System.Serializable]
public sealed class CharacterState {
    public Vector3 _planarVelocity;
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

    // -- Queries --
    public Vector3 Velocity => Vector3.up * VerticalSpeed + PlanarVelocity;
}
