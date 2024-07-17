using System;
using UnityEngine;

namespace ThirdPerson {

public abstract class CharacterBehaviour: MonoBehaviour, CharacterComponent {
    // -- props --
    /// the character container
    protected CharacterContainer c;

    // -- commands --
    /// Update. step the update loop by `delta` if enabled
    public void Step(float delta) {
        if (enabled && gameObject.activeInHierarchy) {
            Step_I(delta);
        }
    }

    /// FixedUpdate. step the fixed update loop by `delta` if enabled
    public void Step_Fixed(float delta) {
        if (enabled && gameObject.activeInHierarchy) {
            Step_Fixed_I(delta);
        }
    }

    // -- CharacterComponent --
    public virtual void Init(CharacterContainer c) {
        // set dependencies
        this.c = c;
    }

    public virtual void Step_I(float delta) {
        // do not implement
    }

    public virtual void Step_Fixed_I(float delta) {
        // do not implement
    }
}

}