using UnityEngine;

// the character's current state
[System.Serializable]
public sealed class CharacterState {
    public Vector3 Velocity = Vector3.zero;
    public float LastTimeGrounded = 0;
}
