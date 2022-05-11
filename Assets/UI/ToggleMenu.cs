using UnityEngine;
using UnityEngine.InputSystem;

/// toggles the menus
class ToggleMenu: MonoBehaviour {
    // -- fields --
    [Header("fields")]
    [Tooltip("the action to toggle the menu")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_ToggleButton")]
    [SerializeField] InputActionReference m_Toggle;

    [Tooltip("the menu panel to toggle")]
    [UnityEngine.Serialization.FormerlySerializedAs("m_ObjectToToggle")]
    [SerializeField] GameObject m_MenuPanel;

    // -- lifecycle --
    void Start() {
        m_Toggle.action.performed += OnToggle;
    }

    void OnDestroy() {
        m_Toggle.action.performed -= OnToggle;
    }

    // -- events --
    /// when the toggle input is performed
    void OnToggle(InputAction.CallbackContext _) {
        m_MenuPanel.SetActive(!m_MenuPanel.activeSelf);
    }
}
