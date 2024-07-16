using System;
using UnityEngine;

namespace ThirdPerson {

public abstract class CharacterBehaviour: MonoBehaviour, CharacterComponent {
    // -- props --
    /// the character container
    protected CharacterContainer c;

    /// the list of children; set this in init before calling `base.Init`.
    protected CharacterComponent[] m_Children;

    // -- CharacterComponent --
    protected virtual CharacterComponent[] InitChildren() {
        return null;
    }

    public virtual void Init(CharacterContainer c) {
        m_Children = InitChildren();

        // set dependencies
        this.c = c;

        // initialize children
        if (m_Children != null) {
            foreach (var child in m_Children) {
                child.Init(c);
            }
        }
    }

    public virtual void Step_I(float delta) {
        // step through children
        if (m_Children != null) {
            foreach (var child in m_Children) {
                child.Step(delta);
            }
        }
    }

    public virtual void Step_Fixed_I(float delta) {
        // step through children
        if (m_Children != null) {
            foreach (var child in m_Children) {
                child.Step_Fixed(delta);
            }
        }
    }

    public void Step(float delta) {
        if (enabled && gameObject.activeInHierarchy) {
            Step_I(delta);
        }
    }

    public void Step_Fixed(float delta) {
        if (enabled && gameObject.activeInHierarchy) {
            Step_Fixed_I(delta);
        }
    }
}

}