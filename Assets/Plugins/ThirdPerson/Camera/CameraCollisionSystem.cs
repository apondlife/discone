using UnityEngine;
using UnityEngine.InputSystem;

namespace ThirdPerson {

sealed class CameraCollisionSystem: System {
    // -- deps --
    /// the current camera state
    CameraState m_State;

    /// .
    CameraTuning m_Tuning;

    /// the free look camera input
    InputAction m_Input;

    /// .
    CharacterState m_CharacterState;

    // -- props --
    /// the state-machine's state
    SystemState m_SystemState;

    // -- lifetime --
    public CameraCollisionSystem(
        CameraState state,
        CameraTuning tuning,
        InputAction input,
        CharacterState characterState
    ) {
        // set deps
        m_State = state;
        m_Tuning = tuning;
        m_Input = input;
        m_CharacterState = characterState;
    }

    // -- System --
    protected override SystemState State {
        get => m_SystemState;
        set => m_SystemState = value;
    }

    protected override Phase InitInitialPhase() {
        return Tracking;
    }

    // -- Tracking --
    Phase Tracking => new Phase(
        name: "Tracking",
        update: Tracking_Update
    );

    void Tracking_Update(float delta) {
    }

    // -- FreeLook --
    Phase FreeLook => new Phase(
        name: "FreeLook",
        update: FreeLook_Update
    );

    void FreeLook_Update(float delta) {
    }
}

}