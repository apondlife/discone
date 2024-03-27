using UnityEngine;

namespace ThirdPerson {

/// an ik limb for the character model
public interface CharacterPart {
    // -- commands --
    /// initialize this limb w/ an animator
    void Init(Animator animator);

    /// applies the limb ik
    void ApplyIk();

    // -- queries --
    /// the attached game object
    GameObject gameObject { get; }
}

}