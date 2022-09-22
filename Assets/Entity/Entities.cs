using UnityAtoms;
using UnityEngine;

/// the entities in the world
[RequireComponent(typeof(Players))]
[RequireComponent(typeof(Characters))]
[RequireComponent(typeof(Flowers))]
[RequireComponent(typeof(EntityCulling))]
[RequireComponent(typeof(EntityPerception))]
public sealed class Entities: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the entities singleton")]
    [SerializeField] EntitiesVariable m_Single;

    // -- props --
    /// the culling procedure
    EntityCulling m_Culling;

    /// the culling procedure
    EntityPerception m_Perception;

    // -- p/repos
    /// the players repo
    Players m_Players;

    /// the characters repo
    Characters m_Characters;

    /// the flowers repo
    Flowers m_Flowers;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Culling = GetComponent<EntityCulling>();
        m_Perception = GetComponent<EntityPerception>();
        m_Players = GetComponent<Players>();
        m_Characters = GetComponent<Characters>();
        m_Flowers = GetComponent<Flowers>();

        // set instance
        m_Single.Value = this;
    }

    void FixedUpdate() {
        var entities = this;
        m_Culling.Run(entities);
        m_Perception.Run(entities);
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

    /// the flowers repo
    public Flowers Flowers {
        get => m_Flowers;
    }
}