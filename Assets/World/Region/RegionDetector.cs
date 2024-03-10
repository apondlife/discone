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
    // physics settings should be set so that only things on the player layer
    // will trigger RegionDetector layer
    void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<Player>();
        if (player == null || player.Character == null) {
            return;
        }

        Log.Region.I($"entered {m_Region.Value.Name}");
        m_RegionEntered.Raise(m_Region.Value);
    }
}

}