namespace ThirdPerson {

/// a controlled child component of a `Character`. the character determines the order in
/// which to call the step updates. implementers should not implement Update/FixedUpdate
/// from `MonoBehaviour`.
public interface CharacterComponent {
    // -- contract --
    /// Awake. initialize the component with the dependency container.
    void Init(CharacterContainer c);

    /// Update. step the update loop by `delta`
    void Step_I(float delta);

    /// FixedUpdate. step the fixed update loop by `delta`
    void Step_Fixed_I(float delta);
}

}