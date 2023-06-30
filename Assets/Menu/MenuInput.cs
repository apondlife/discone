using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone.Ui {

/// the in-game menu input
sealed class MenuInput: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the toggle action")]
    [SerializeField] InputActionReference m_Toggle;

    [Tooltip("the connect action")]
    [SerializeField] InputActionReference m_Connect;

    // -- refs --
    [Header("refs")]
    [Tooltip("the menu input actions")]
    [SerializeField] InputActionAsset m_Actions;

    // -- lifecycle --
    void Awake() {
        m_Actions.Enable();
    }

    void OnDestroy() {
        m_Actions.Disable();
    }

    // TODO: update these to mirror DebugInput (just make input public)
    // -- queries --
    /// the toggle action
    public InputAction Toggle {
        get => m_Toggle;
    }

    /// the connect action
    public InputAction Connect {
        get => m_Connect;
    }
}

}