using System;
using ThirdPerson;
using UnityAtoms;
using UnityEngine;

namespace Discone {

[Serializable]
public sealed class InputSource: PlayerInputSource<InputFrame> {
    // -- cfg --
    [Header("cfg")]
    [Tooltip("the player's main camera")]
    [SerializeField] PlayerCameraVariable m_PlayerCamera;

    // -- props --
    /// the player input
    Input m_Input;

    // -- commands --
    /// bind the input to an input system action asset
    public void Bind(Input input) {
        m_Input = input;
    }

    // -- PlayerInputSource --
    protected override PlayerInputActions Actions {
        get => m_Input;
    }

    protected override Transform Look {
        get => m_PlayerCamera.Value.Look;
    }

    protected override void ReadNext(ref InputFrame frame) {
        frame.IsLoadPressed = m_Input.IsLoadPressed;
        ReadMain(ref frame);
    }

}

}