using System;
using System.Collections.Generic;
using ThirdPerson;
using UnityAtoms;
using UnityEngine;

namespace Discone {

/// the player's butterfly collection
[RequireComponent(typeof(ParticleSystem))]
public class PlayerButterflies: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the y-speed to release butterflies")]
    [SerializeField] float m_ReleaseSpeed;

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

    /// a subscription to the character's landing event
    IDisposable m_OnLand;

    /// the list of event subscriptions
    DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Awake() {
        // get refs
        m_ParticleSystem = GetComponent<ParticleSystem>();

        // bind events
        m_Subscriptions
            .Add(m_CurrentCharacter.ChangedWithHistory, OnCharacterChanged);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// collect a butterfly
    void Collect() {
        m_Collected += 1;
    }

    void Release() {
        Debug.Log($"released {m_Collected} butterflies");
        m_Collected = 0;
    }

    // -- events --
    /// when the current character changes
    void OnCharacterChanged(DisconeCharacterPair characters) {
        var curr = characters.Item1;
        var prev = characters.Item2;

        // clean up after the previous character
        if (prev) {
            m_ParticleSystem.trigger.RemoveCollider(prev.Collider);
            m_OnLand?.Dispose();
        }

        // and add the next character's collider / land event subscription
        if (curr) {
            m_ParticleSystem.trigger.AddCollider(curr.Collider);
            m_OnLand = curr.Character.Events.Subscribe(CharacterEvent.Land, OnCharacterLand);
        }
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
            Collect();
        }

        // sync the butterflies back to the particle system
        m_ParticleSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, m_Particles);
    }

    void OnCharacterLand() {
        var chr = m_CurrentCharacter.Value;
        if (!chr) {
            return;
        }

        // get state
        var prev = chr.Character.State.Prev;
        var curr = chr.Character.State.Curr;

        // if the character is moving fast enough, release the butterflies
        // IDEA: this could happen on any air -> surface transition, not just landing on a "ground"
        var speed = Mathf.Abs(Vector3.Dot(prev.Velocity, curr.GroundSurface.Normal));
        if (speed >= m_ReleaseSpeed) {
            Release();
        }
    }
}

}