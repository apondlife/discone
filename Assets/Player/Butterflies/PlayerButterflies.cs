using System;
using System.Collections.Generic;
using ThirdPerson;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace Discone {

/// the player's butterfly collection
sealed class PlayerButterflies: MonoBehaviour {
    // -- tuning --
    [Header("tuning")]
    [Tooltip("the y-speed to release butterflies")]
    [SerializeField] MapInCurve m_ReleaseSpeed;

    // -- state --
    [Header("state")]
    [Tooltip("the number of butterflies the player is currently holding")]
    [SerializeField] IntVariable m_Collected;

    // -- refs --
    [Header("refs")]
    [Tooltip("the current character")]
    [SerializeField] DisconeCharacterVariable m_CurrentCharacter;

    [Tooltip("the ambient butterfly system")]
    [SerializeField] ParticleSystem m_AmbientSystem;

    [Tooltip("the butterfly release system")]
    [SerializeField] ParticleSystem m_ReleaseSystem;

    // -- props --
    /// the current list of colliding particles
    readonly List<ParticleSystem.Particle> m_Particles = new();

    /// a subscription to the character's landing event
    IDisposable m_OnLand;

    /// the list of event subscriptions
    readonly DisposeBag m_Subscriptions = new();

    // -- lifecycle --
    void Awake() {
        // find children
        var ambient = m_AmbientSystem.GetComponent<PlayerButterflies_Ambient>();

        // bind events
        m_Subscriptions
            .Add(m_CurrentCharacter.ChangedWithHistory, OnCharacterChanged)
            .Add(ambient.OnCollectTrigger, OnCollectTrigger);
    }

    void OnDestroy() {
        m_Subscriptions.Dispose();
    }

    // -- commands --
    /// collect a butterfly
    void Collect() {
        m_Collected.Value += 1;
    }

    /// release the butterflies
    void Release(float pct) {
        if (m_Collected.Value == 0) {
            return;
        }

        var released = Mathf.FloorToInt(m_Collected.Value * pct);
        m_ReleaseSystem.Emit(released);
        m_Collected.Value = released;
    }

    // -- events --
    /// when the current character changes
    void OnCharacterChanged(DisconeCharacterPair characters) {
        var curr = characters.Item1;
        var prev = characters.Item2;

        // clean up after the previous character
        if (prev) {
            m_AmbientSystem.trigger.RemoveCollider(prev.Collider);
            m_OnLand?.Dispose();
        }

        // and add the next character's collider / land event subscription
        if (curr) {
            m_AmbientSystem.trigger.AddCollider(curr.Collider);
            m_OnLand = curr.Character.Events.Subscribe(CharacterEvent.Land, OnCharacterLand);
        }
    }

    /// when ambient butterflies hit the character
    void OnCollectTrigger() {
        // get the colliding butterflies
        var n = m_AmbientSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, m_Particles);
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
        m_AmbientSystem.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, m_Particles);
    }

    /// when the character lands
    void OnCharacterLand() {
        var chr = m_CurrentCharacter.Value;
        if (!chr) {
            return;
        }

        // get state
        var prev = chr.Character.State.Prev;
        var curr = chr.Character.State.Curr;

        // if the character is moving fast enough, release the butterflies
        // THOUGHT: this could happen on any air -> surface transition, not just landing on a "ground"
        var speed = Mathf.Abs(Vector3.Dot(prev.Velocity, curr.GroundSurface.Normal));

        var pct = m_ReleaseSpeed.Evaluate(speed);
        if (pct > 0f) {
            Release(pct);
        }
    }
}

}