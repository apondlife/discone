using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// a repository of characters
public sealed class Characters: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the tag for characters")]
    [SerializeField] string m_Tag;

    // -- props --
    /// the list of characters
    Lazy<DisconeCharacter[]> m_All;

    // -- lifecycle --
    void Awake() {
        m_All = new Lazy<DisconeCharacter[]>(() =>
            GameObject
                .FindGameObjectsWithTag(m_Tag)
                .Select((o) => o.GetComponent<DisconeCharacter>())
                .ToArray()
        );
    }

    // -- queries -
    /// the list of all characters
    public IEnumerable<DisconeCharacter> All {
        get => m_All.Value;
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


}
