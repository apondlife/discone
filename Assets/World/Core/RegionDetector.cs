using UnityEngine;
using UnityAtoms.Discone;

public class RegionDetector: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("player entered region")]
    [SerializeField] private RegionEvent m_RegionEntered;

    [Tooltip("the region object that this detector is detecting")]
    [SerializeField] private RegionConstant m_Region;

    // -- events --
    // Physics settings should be set so that only things on the
    // Player layer will trigger RegionDetector layer
    void OnTriggerEnter() {
        Debug.Log($"[region] entered {m_Region.Value.DisplayName}");
        m_RegionEntered.Raise(m_Region.Value);
    }
}
