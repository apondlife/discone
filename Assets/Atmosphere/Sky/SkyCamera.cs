using UnityAtoms;
using UnityEngine;

/// the camera that observes the sky & celestial objects
class SkyCamera: MonoBehaviour {
    // -- refs --
    [Header("refs")]
    [Tooltip("a reference to the player's main camera")]
    [SerializeField] PlayerCameraVariable m_PlayerCamera;

    // -- lifecycle --
    void LateUpdate() {
        transform.rotation = m_PlayerCamera.Value.Look.rotation;
    }
}