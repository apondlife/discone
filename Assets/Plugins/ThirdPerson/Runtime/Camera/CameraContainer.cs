using UnityEngine.InputSystem;

namespace ThirdPerson {

/// a dependency container for the camera
public interface CameraContainer {
    /// the current camera state
    CameraState State { get; }

    /// the tuning
    CameraTuning Tuning { get; }

    /// the free look camera input
    InputAction Input { get; }

    /// the character's input
    CharacterInputQuery CharacterInput { get; }
}

}