using System;
using System.Collections.Generic;
using System.Linq;
using UnityAtoms;
using UnityEngine;

// TODO: root collection should probably be a dictionary keyed by
// by object id?

/// a repository of characters
public sealed class Characters: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the tag for characters")]
    [SerializeField] string m_Tag;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a player switches character")]
    [SerializeField] DisconeCharacterPairEvent m_SwitchedCharacter;

    // -- props --
    /// the list of characters
    Lazy<DisconeCharacter[]> m_All;

    /// the set of driven characters (hash codes)
    HashSet<int> m_Driven = new HashSet<int>();

    /// a bag of subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Awake() {
        m_All = new Lazy<DisconeCharacter[]>(() =>
            GameObject
                .FindGameObjectsWithTag(m_Tag)
                .Select((o) => o.GetComponent<DisconeCharacter>())
                .ToArray()
        );
    }

    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_SwitchedCharacter, OnSwitchedCharacter);
    }

    void OnDestroy() {
        // release events
        m_Subscriptions.Dispose();
    }

    // -- queries -
    /// the list of all characters
    public IEnumerable<DisconeCharacter> All {
        get => m_All.Value;
    }

    /// the list of simulating characters
    public IEnumerable<DisconeCharacter> Simulating {
        get => m_All.Value.Where((c) => c.IsSimulating);
    }

    /// find an available character to play
    public DisconeCharacter FindInitialCharacter() {
        var all = m_All.Value;

        // use debug characters if available, otherwise the first initial character
        var sets = new[] {
            #if UNITY_EDITOR
            all.Where(c => c.IsDebug),
            #endif
            all.Where(c => c.IsAvailable && c.IsInitial)
        };

        var available = sets
            .Where((cs) => cs.Any())
            .First()
            .ToArray();

        // pick a random character from the list
        var character = available[UnityEngine.Random.Range(0, available.Length)];

        return character;
    }

    /// if the character is driven
    public bool IsDriven(DisconeCharacter character) {
        return m_Driven.Contains(character.GetHashCode());
    }

    // -- events --
    /// when a player switches character
    void OnSwitchedCharacter(DisconeCharacterPair characters) {
        var curr = characters.Item1;
        var prev = characters.Item2;

        // update player driven characters; we only care about membership
        // so just store the hash code
        if (prev != null) {
            m_Driven.Remove(prev.GetHashCode());
        }

        if (curr != null) {
            m_Driven.Add(curr.GetHashCode());
        }
    }
}
