using UnityAtoms;
using UnityEngine;

/// the entities in the world
[RequireComponent(typeof(Players))]
[RequireComponent(typeof(Characters))]
[RequireComponent(typeof(EntityCulling))]
public sealed class Entities: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the entities singleton")]
    [SerializeField] EntitiesVariable m_Single;

    // -- props --
    /// the culling procedure
    EntityCulling m_Culling;

    // -- p/repos
    /// the players repo
    Players m_Players;

    /// the characters repo
    Characters m_Characters;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Culling = GetComponent<EntityCulling>();
        m_Players = GetComponent<Players>();
        m_Characters = GetComponent<Characters>();

        // set instance
        m_Single.Value = this;
    }

    void FixedUpdate() {
        var entities = this;
        m_Culling.Run(entities);
    }

    // -- queries --
    /// the players repo
    public Players Players {
        get => m_Players;
    }

    /// the characters repo
    public Characters Characters {
        get => m_Characters;
    }
}