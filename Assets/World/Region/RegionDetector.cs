using UnityEngine;
using UnityAtoms.Discone;

namespace Discone {

/// a trigger for characters entering a region
sealed class RegionDetector: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("the region to emit")]
    [SerializeField] RegionConstant m_Region;

    [Tooltip("when the player enters a region")]
    [SerializeField] RegionEvent m_RegionEntered;

    // -- events --
    // Physics settings should be set so that only things on the
    // Player layer will trigger RegionDetector layer
    void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<DisconePlayer>();
        if (player == null || player.Character == null) {
            return;
        }

        Debug.Log($"[region] entered {m_Region.Value.DisplayName}");
        m_RegionEntered.Raise(m_Region.Value);
    }
}

}