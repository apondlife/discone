using System;
using ThirdPerson;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Discone {

[Serializable]
public sealed class InputSource: PlayerInputSource<InputFrame> {
    // -- refs/discone --
    [Tooltip("the load checkpoint input")]
    [SerializeField] InputActionReference m_Load;

    // -- PlayerInputSource --
    public override InputFrame Read() {
        return new InputFrame(
            ReadMain(),
            isLoadPressed: m_Load.action.IsPressed()
        );
    }
}

}