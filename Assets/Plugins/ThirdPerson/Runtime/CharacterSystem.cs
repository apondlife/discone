using System;
using Soil;

#if UNITY_EDITOR
using System.Reflection;
using UnityEngine;
#endif

namespace ThirdPerson {

/// a character system; may be a state machine
[Serializable]
abstract class CharacterSystem: System<CharacterContainer> {
    // -- lifecycle --
    #if UNITY_EDITOR
    public void RestorePhase() {
        foreach (var prop in GetType().GetTypeInfo().DeclaredProperties) {
            if (prop.Name == State.PhaseName) {
                if (prop.PropertyType != typeof(Phase<CharacterContainer>)) {
                    Log.Character.E($"tried to restore a phase that was not a phase: {State.PhaseName}");
                    return;
                }

                SetPhase((Phase<CharacterContainer>)prop.GetValue(this));
            }
        }
    }
    #endif
}

}