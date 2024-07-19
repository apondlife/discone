using Soil;
using UnityEngine;

namespace Discone {

public class IntroRetrigger: MonoBehaviour {
    // -- config --
    [Header("config")]
    [Tooltip("the time it takes within the collider to retrigger the camera")]
    [SerializeField] EaseTimer m_RetriggerDelay;

    // -- refs --
    [Header("refs")]
    [Tooltip("the camera that shows the intro letter")]
    [SerializeField] GameObject m_RetriggerCamera;

    // -- props --
    /// .
    bool m_EnableCamera = false;

    // -- lifecycle --
    void Update() {
        if (m_RetriggerDelay.TryComplete()) {
            m_RetriggerCamera.SetActive(m_EnableCamera);
        }
    }

    void OnTriggerEnter(Collider other) {
        var player = other.GetComponentInParent<Player>();
        if (player == null || player.Character == null) {
            return;
        }

        // if already enabling the camera, don't trigger timer
        if (m_EnableCamera) {
            return;
        }

        m_EnableCamera = true;
        m_RetriggerDelay.Play();
    }

    void OnTriggerExit(Collider other) {
        var player = other.GetComponentInParent<Player>();
        if (player == null || player.Character == null) {
            return;
        }

        // if already disabling the camera, don't trigger timer
        if (!m_EnableCamera) {
            return;
        }

        m_EnableCamera = false;
        m_RetriggerDelay.Play();
    }
}

}