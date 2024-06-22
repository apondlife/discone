using UnityAtoms;
using UnityEngine;

namespace Discone {

public sealed class PlayerCamera: MonoBehaviour {
    // -- state --
    [Header("state")]
    [Tooltip("the current camera")]
    [SerializeField] PlayerCameraVariable m_Current;

    // -- refs --
    [Header("refs")]
    [Tooltip("the actual camera")]
    [SerializeField] Camera m_Camera;

    // -- lifecycle --
    void Awake() {
        m_Current.Value = this;
    }

    // -- queries --
    /// the look transform
    public Transform Look {
        get => m_Camera.transform;
    }
}

}