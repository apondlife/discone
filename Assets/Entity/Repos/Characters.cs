using System;
using System.Collections.Generic;
using System.Linq;
using UnityAtoms;
using UnityEngine;

namespace Discone {

/// a repository of characters
public sealed class Characters: MonoBehaviour {
    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a character spawns")]
    [SerializeField] DisconeCharacterEvent m_SpawnedCharacter;

    [Tooltip("when a character is destroyed")]
    [SerializeField] DisconeCharacterEvent m_DestroyedCharacter;

    [Tooltip("when a player switches character")]
    [SerializeField] DisconeCharacterPairEvent m_SwitchedCharacter;

    // -- props --
    /// the list of characters
    HashSet<Character> m_All = new HashSet<Character>();

    /// the set of driven characters (hash codes)
    HashSet<int> m_Driven = new HashSet<int>();

    /// a bag of subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_SpawnedCharacter, OnSpawnedCharacter)
            .Add(m_DestroyedCharacter, OnDestroyedCharacter)
            .Add(m_SwitchedCharacter, OnSwitchedCharacter);
    }

    void OnDestroy() {
        // release events
        m_Subscriptions.Dispose();
    }

    // -- queries -
    /// the list of all characters
    public IEnumerable<Character> All {
        get => m_All;
    }

    /// the list of simulating characters
    public IEnumerable<Character> Simulating {
        get => m_All.Where((c) => c.Online.IsSimulating);
    }

    /// find an available character to play
    public Character FindInitialCharacter() {
        var all = m_All;

        // use debug characters if available, otherwise the first initial character
        var sets = new[] {
            #if UNITY_EDITOR
            all.Where(c => c.Online.IsDebug),
            #endif
            all.Where(c => c.Online.IsAvailable && c.Online.IsInitial)
        };

        var available = sets
            .First(cs => cs.Any())
            .ToArray();

        // pick a random character from the list
        var character = available[UnityEngine.Random.Range(0, available.Length)];

        return character;
    }

    /// if the character is driven
    public bool IsDriven(Character character) {
        return m_Driven.Contains(character.GetHashCode());
    }

    // -- events --
    /// when a character spawns
    void OnSpawnedCharacter(Character character) {
        m_All.Add(character);
    }

    /// when a character is destroyed
    void OnDestroyedCharacter(Character character) {
        m_All.Remove(character);
    }

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

}