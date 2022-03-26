using UnityEngine;

/// the main third person controller
namespace ThirdPerson {

public partial class Character: MonoBehaviour {
    // -- fields --
    [Header("data")]
    [Tooltip("the tunables; for tweaking the player's attributes")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    [Header("children")]
    [Tooltip("the underlying character controller")]
    [SerializeField] CharacterController m_Controller;

    // -- props --
    /// the list of systems acting on this character
    CharacterSystem[] m_Systems;

    /// the character's state
    CharacterState m_State = new CharacterState();

    ///the input wrapper
    CharacterInput m_Input = new CharacterInput();

    // -- lifecycle --
    void Awake() {
        // init child objects
        m_State.Reset(transform.forward);

        // init data
        var data = new CharacterData(
            m_Input,
            m_State,
            m_Tunables,
            m_Controller
        );

        // init systems
        m_Systems = new CharacterSystem[] {
            new WallSystem(data),
            new GravitySystem(data),
            new MovementSystem(data),
            new JumpSystem(data),
            new TiltSystem(data),
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
        if (m_State.Velocity.sqrMagnitude > 0) {
            m_Controller.Move(m_State.Velocity * Time.deltaTime);
        }

        if(m_Controller.Collisions.Count > 0) {
            m_State.Collision = m_Controller.Collisions[m_Controller.Collisions.Count - 1];
        } else {
            m_State.Collision = null;
        }

        // sync controller state back to character state
        m_State.UpdateVelocity(v0, m_Controller.Velocity);
        var c = GetComponent<CapsuleCollider>();
        var delta = (c.height / 2.0f - c.radius) * Vector3.up;
        var p0 = c.center - delta;
        var p1 = c.center + delta;
    }

    // -- queries --
    public CharacterController Controller => m_Controller;

    public CharacterInput Input => m_Input;

    public CharacterTunablesBase Tunables => m_Tunables;

    // TODO: how should we make state immutable outside the class
    public CharacterState State => m_State;

    // -- events --
    /// when the restart button is pressed, reload the scene
    public void OnRestart() {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            scene.name,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }
}

}