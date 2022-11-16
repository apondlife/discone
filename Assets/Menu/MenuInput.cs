using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone.Ui {

/// the in-game menu
sealed class MenuInput: MonoBehaviour {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the toggle action")]
    [SerializeField] InputActionReference m_Toggle;

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

    // -- queries --
    /// the toggle action
    public InputAction Toggle {
        get => m_Toggle;
    }
}

}