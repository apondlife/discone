using System;
using UnityEngine;

/// the serialized flower state
[Serializable]
public record FlowerRec {
    // -- props --
    /// the flower's character
    public CharacterKey K;

    /// the flower's world position
    public Vector3 P;

    /// the world rotation in degrees
    public float A;

    // -- lifetime --
    [Obsolete("use the paramterized constructor")]
    public FlowerRec() {
    }

    /// create a flower from a position and rotation
    public FlowerRec(
        CharacterKey key,
        Vector3 pos,
        Quaternion rot
    ) {
        // get the forward vector in the xz-plane
        var fwd = rot * Vector3.forward;

        #if UNITY_EDITOR
        if (fwd != Vector3.ProjectOnPlane(fwd, Vector3.up)) {
            Debug.LogWarning($"[flower] constructed a flower w/ a rotation not in the xz-plane!");
        }
        #endif

        // set props
        K = key;
        P = pos;
        A = Vector3.Angle(fwd, Vector3.forward);
    }

    // -- queries --
    /// the flower's character
    public CharacterKey Key => K;

    /// the flower's world position
    public Vector3 Pos => P;

    /// the world rotation
    public Quaternion Rot => Quaternion.AngleAxis(A, Vector3.up);

    /// the forward direction
    public Vector3 Fwd => Rot * Vector3.forward;
}