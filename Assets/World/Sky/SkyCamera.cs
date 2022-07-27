using UnityEngine;
using UnityAtoms.BaseAtoms;

/// the camera that observes the sky & celestial objects
class SkyCamera: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("a reference to the player's main camera")]
    [SerializeField] GameObjectVariable m_PlayerCamera;

    // -- lifecycle --
    void LateUpdate() {
        if (m_PlayerCamera?.Value != null) {
            transform.rotation = m_PlayerCamera.Value.transform.rotation;
        }
    }
}
