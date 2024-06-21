using System;
using ThirdPerson;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

[Serializable]
public sealed class InputSource: PlayerInputSource<InputFrame>, PlayerInputActions {
    public string m_JumpName;
    public string m_MoveName;
    public string m_LoadName;

    InputAction m_JumpAction;
    InputAction m_MoveAction;
    InputAction m_LoadAction;

    Transform m_Look;

    // -- commands --
    /// bind the input to an input system action asset
    public void Init(InputActionAsset asset, Transform look) {
        m_JumpAction = asset.FindAction(m_JumpName);
        m_MoveAction = asset.FindAction(m_MoveName);
        m_LoadAction = asset.FindAction(m_LoadName);

        m_Look = look;
    }

    // -- PlayerInputSource --
    public override InputFrame Read() {
        return new(
            ReadMain(),
            isLoadPressed: m_LoadAction.IsPressed()
        );
    }

    protected override Transform Look {
        get => m_Look;
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