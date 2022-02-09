using UnityEngine;

/// the main third person controller
namespace ThirdPerson {

public partial class ThirdPerson: MonoBehaviour {
    // -- fields --
    [Header("data")]
    [Tooltip("the character's state")]
    [SerializeField] CharacterState m_State;

    [Tooltip("the tunables; for tweaking the player's attributes")]
    [SerializeField] CharacterTunablesBase m_Tunables;

    [Header("children")]
    [Tooltip("the input wrapper")]
    [SerializeField] CharacterInput m_Input;

    [Tooltip("the underlying character controller")]
    [SerializeField] CharacterController m_Controller;

    [SerializeField] private Log.Level logLevel;

    // -- props --
    /// the list of systems acting on this character
    private CharacterSystem[] m_Systems;

    // -- lifecycle --
    private void Awake() {
        // set log level
        // TODO: do this at game startup
        Log.Init(logLevel);

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
            new WallSystem(character),
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