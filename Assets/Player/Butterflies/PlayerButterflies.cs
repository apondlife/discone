using System.Collections.Generic;
using UnityAtoms;
using UnityEngine;

namespace Discone {

/// the player's butterfly collection
[RequireComponent(typeof(ParticleSystem))]
public class PlayerButterflies: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    // -- props --
    /// the current number of collected butterflies
    int m_Collected;

    /// the attached particle system
    ParticleSystem m_ParticleSystem;

    /// the current list of colliding particles
    List<ParticleSystem.Particle> m_Particles = new();

    /// the list of event subscriptions
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Awake() {
        // get refs
        m_ParticleSystem = GetComponent<ParticleSystem>();

        // bind events
        m_Subscriptions
            .Add(m_CurrentCharacter.Changed, OnCharacterChanged);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- events --
    /// when the current character changes
    void OnCharacterChanged(DisconeCharacter character) {
        // remove the previous collider
        while (m_ParticleSystem.trigger.colliderCount > 0) {
            m_ParticleSystem.trigger.RemoveCollider(0);
        }

        // add the collider for the new character
        m_ParticleSystem.trigger.AddCollider(character.Collider);
    }

    /// when the butterflies hit the character
    void OnParticleTrigger() {
        // get the colliding butterflies
        var n = m_ParticleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, m_Particles);
        if (n <= 0) {
            return;
        }

        for (var i = 0; i < n; i++) {
            // remove the butterfly from the world
            var p = m_Particles[i];
            p.remainingLifetime = -1f;
            m_Particles[i] = p;

            // and add it to your collection
            m_Collected += 1;
        }

        // sync the butterflies back to the particle system
        m_ParticleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, m_Particles);
    }
}

}