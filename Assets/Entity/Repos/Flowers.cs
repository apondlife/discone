using System.Linq;
using System.Collections.Generic;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

/// a repository of flowers
public sealed class Flowers: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the chunk size for flower position hashing")]
    [SerializeField] FloatReference m_ChunkSize;

    // -- subscribed --
    [Header("subscribed")]
    [Tooltip("when a flower is planted")]
    [SerializeField] CharacterFlowerEvent m_FlowerPlanted;

    // -- props --
    /// the list of all flowers
    List<CharacterFlower> m_All = new List<CharacterFlower>();

    /// a bag of subscriptions
    Subscriptions m_Subscriptions = new Subscriptions();

    // -- lifecycle --
    void Start() {
        // bind events
        m_Subscriptions
            .Add(m_FlowerPlanted, OnFlowerPlanted);
    }

    void OnDestroy() {
        // release events
        m_Subscriptions.Dispose();
    }

    // -- queries --
    public CharacterFlower FindClosest(Vector3 position) {
        var distCache = new Dictionary<CharacterFlower, float>();

        float Distance(CharacterFlower f) {
            if(!distCache.TryGetValue(f, out var dist)) {
                dist = Vector3.Distance(f.transform.position, position);
                distCache.Add(f, dist);
            } else {
                Debug.Log($"cache miss");
            }

            return dist;
        }

        var closest = m_All
            .OrderBy(Distance)
            .FirstOrDefault();

        return closest;
    }

    // -- events --
    void OnFlowerPlanted(CharacterFlower flower) {
        m_All.Add(flower);
    }
}
