using System;

#if UNITY_EDITOR
using System.Reflection;
using UnityEngine;
#endif

namespace ThirdPerson {

/// a character system; may be a state machine
[Serializable]
abstract class CharacterSystem: System {
    // -- props --
    /// the character container
    protected CharacterContainer c;

    // -- lifetime --
    /// create a new system
    public CharacterSystem() {
        // set props
        m_Name = GetType().Name;
    }

    // -- lifecycle --
    /// initialize this system with character data
    public void Init(CharacterContainer container) {
        // set props
        this.c = container;

        // run base init
        this.Init();
    }

    #if UNITY_EDITOR
    public void RestorePhase() {
        foreach (var prop in GetType().GetTypeInfo().DeclaredProperties) {
            if (prop.Name == State.PhaseName) {
                if (prop.PropertyType != typeof(Phase)) {
                    Debug.LogError($"[system] tried to restore a phase that was not a phase: {State.PhaseName}");
                    return;
                }

                SetPhase((Phase)prop.GetValue(this));
            }
        }
    }
    #endif
}

}