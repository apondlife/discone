using System;
using ThirdPerson;
using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Discone {

[Serializable]
public sealed class InputSource: PlayerInputSource<InputFrame>, PlayerInputActions {
    [Tooltip("the player's main camera")]
    [SerializeField] PlayerCameraVariable m_PlayerCamera;

    /// AAA
    public string m_JumpName;
    public string m_MoveName;
    public string m_LoadName;

    InputAction m_JumpAction;
    InputAction m_MoveAction;
    InputAction m_LoadAction;

    // -- commands --
    /// bind the input to an input system action asset
    public void Init(InputActionAsset asset) {
        m_JumpAction = asset.FindAction(m_JumpName);
        m_MoveAction = asset.FindAction(m_MoveName);
        m_LoadAction = asset.FindAction(m_LoadName);
    }

    // -- PlayerInputSource --
    public override InputFrame Read() {
        return new(
            ReadMain(),
            isLoadPressed: m_LoadAction.IsPressed()
        );
    }

    protected override Transform Look {
        get => m_PlayerCamera.Value.Look;
    }

    // -- PlayerInputActions --
    public Vector2 Move {
        get => m_MoveAction.ReadValue<Vector2>();
    }

    public bool IsJumpPressed {
        get => m_JumpAction.IsPressed();
    }
}

}