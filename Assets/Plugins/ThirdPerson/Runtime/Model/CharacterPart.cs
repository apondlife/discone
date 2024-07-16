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
    /// if the part matches a certain step event
    bool MatchesStep(CharacterEvent mask);

    /// the current placement of the limb (must match step first!)
    LimbPlacement Placement { get; }
}

}