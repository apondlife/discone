using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

/// the debug input
sealed class DebugInput: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the reset action")]
    public InputActionReference Reset;

    [Tooltip("spawn character")]
    public InputActionReference SpawnCharacter;

    // -- refs --
    [Header("refs")]
    [Tooltip("the debug input actions")]
    [SerializeField] InputActionAsset m_Actions;

    // -- lifecycle --
    void Awake() {
        m_Actions.Enable();
    }

    void OnDestroy() {
        m_Actions.Disable();
    }
}

}