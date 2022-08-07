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
    DisconeCharacter[] m_All;

    // -- lifecycle --
    void Awake() {
        m_All = GameObject
            .FindGameObjectsWithTag(m_Tag)
            .Select((o) => o.GetComponent<DisconeCharacter>())
            .ToArray();
    }

    // -- queries -
    /// the list of all characters
    public IEnumerable<DisconeCharacter> All {
        get => m_All;
    }
}
