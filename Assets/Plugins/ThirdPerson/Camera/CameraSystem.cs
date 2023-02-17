using UnityEngine.InputSystem;

namespace ThirdPerson {

abstract class CameraSystem: System {
    // -- deps --
    /// the current camera state
    protected CameraState m_State;

    /// .
    protected CameraTuning m_Tuning;

    /// the free look camera input
    protected InputAction m_Input;

    // -- props --
    /// the state-machine's state
    SystemState m_SystemState;

    // -- System --
    protected override SystemState State {
        get => m_SystemState;
        set => m_SystemState = value;
    }

    public void Init(
        CameraState state,
        CameraTuning tuning,
        InputAction input
    ) {
        // set deps
        m_State = state;
        m_Tuning = tuning;
        m_Input = input;

        this.Init();
    }
}

}