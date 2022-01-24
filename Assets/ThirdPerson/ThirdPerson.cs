using UnityEngine;

/// the main third person controller
namespace ThirdPerson {

sealed partial class ThirdPerson: MonoBehaviour {
    // -- fields --
    [Header("data")]
    [Tooltip("the character's state")]
    [SerializeField] CharacterState m_State;

    [Tooltip("the tunables; for tweaking the player's attributes")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    [Header("children")]
    [Tooltip("the input wrapper")]
    // TODO: this should probably be outside of the character, since it needs a reference to a camera.
    [SerializeField] CharacterInput m_Input;

    [Tooltip("the underlying character controller")]
    [SerializeField] CharacterController m_Controller;

    // -- props --
    /// the list of systems acting on this character
    private CharacterSystem[] m_Systems;

    /// the last collision between the character and ground
    private ControllerColliderHit m_Hit;

    // -- lifecycle --
    private void Awake() {
        // init child objects
        m_Input.Init();
        m_State.Reset();

        // init character
        var character = new Character(
            m_Input,
            m_State,
            m_Tunables,
            m_Controller
        );

        // init systems
        m_Systems = new CharacterSystem[] {
            // new WallSystem(character),
            new GravitySystem(character),
            new MovementSystem(character),
            new JumpSystem(character),
            new TiltSystem(character),
        };
    }

    void FixedUpdate() {
        var v0 = m_State.Velocity;

        // camera to left/forward movement
        m_Input.Read();

        // update the character's systems
        foreach (var system in m_Systems) {
            system.Update();
        }

        // update controller state from character state
        if (m_State.Velocity.magnitude > 0) {
            m_State.Collision = null;
            m_Controller.Move(m_State.Velocity * Time.deltaTime);
        }

        // sync controller state back to character state
        m_State.UpdateVelocity(v0, m_Controller.velocity);
        frame ++;
    }

    // -- events --
    /// when the controller collider contact something
    public int frame = 0;
    void OnCollisionEnter(Collision hit) {
        Debug.Log("hit frame " + frame + " " + hit.gameObject.name + hit.GetContact(0).normal);
        m_State.Collision = hit;
    }

    /// when the restart button is pressed, reload the scene
    public void OnRestart() {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}

}