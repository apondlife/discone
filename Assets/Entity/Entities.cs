using UnityAtoms;
using UnityEngine;
using Discone;

/// the entities in the world
[RequireComponent(typeof(OnlinePlayers))]
[RequireComponent(typeof(Characters))]
[RequireComponent(typeof(Flowers))]
[RequireComponent(typeof(EntityPerception))]
public sealed class Entities: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the entities singleton")]
    [SerializeField] EntitiesVariable m_Single;

    // -- props --
    /// the perceiption checks
    EntityPerception m_Perception;

    // -- p/repos
    /// the players repo
    OnlinePlayers m_OnlinePlayers;

    /// the characters repo
    Characters m_Characters;

    /// the flowers repo
    Flowers m_Flowers;

    // -- lifecycle --
    void Awake() {
        // set props
        m_Perception = GetComponent<EntityPerception>();
        m_OnlinePlayers = GetComponent<OnlinePlayers>();
        m_Characters = GetComponent<Characters>();
        m_Flowers = GetComponent<Flowers>();

        // set instance
        m_Single.Value = this;
    }

    void FixedUpdate() {
        var entities = this;
        m_Perception.Run(entities);
    }

    // -- queries --
    /// the players repo
    public OnlinePlayers OnlinePlayers {
        get => m_OnlinePlayers;
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