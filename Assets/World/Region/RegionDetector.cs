using UnityEngine;
using UnityAtoms.Discone;

namespace Discone {

class RegionDetector: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("player entered region")]
    [SerializeField] RegionEvent m_RegionEntered;

    [Tooltip("the region object that this detector is detecting")]
    [SerializeField] RegionConstant m_Region;

    // -- events --
    // Physics settings should be set so that only things on the
    // Player layer will trigger RegionDetector layer
    void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<DisconePlayer>();
        if (player == null || player.Character == null) {
            Debug.Log($"[region] something else hit me {m_Region.Value.DisplayName} it was {other}");
            return;
        }

        Debug.Log($"[region] entered {m_Region.Value.DisplayName}");
        m_RegionEntered.Raise(m_Region.Value);
    }
}

}