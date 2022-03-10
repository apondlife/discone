using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleMenu : MonoBehaviour
{
    [SerializeField] private InputActionReference m_ToggleButton;
    [SerializeField] private GameObject m_ObjectToToggle;

    // Start is called before the first frame update
    void Start()
    {
        m_ToggleButton.action.performed += _ => Toggle();
    }

    void Toggle() {
        m_ObjectToToggle.SetActive(!m_ObjectToToggle.activeSelf);
    }
}
