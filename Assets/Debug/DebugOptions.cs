using UnityEngine;
using UnityAtoms;
using UnityEngine.InputSystem;
using System.Reflection;

namespace Discone {

/// debug utilties
public class DebugOptions: MonoBehaviour {
    // -- module --
    /// the shared instance
    public static DebugOptions s_Instance;

    /// resolve the module
    public static DebugOptions Get {
        get => s_Instance;
    }

    // -- character --
    [Header("character")]
    [Tooltip("the character to spawn at the editor camera position")]
    [SerializeField] CharacterKey m_CharacterKey;

    // -- refs --
    [Header("refs")]
    [Tooltip("the entity repos")]
    [SerializeField] EntitiesVariable m_Entites;

    // -- props --
    /// the menu input
    DebugInput m_Input;

    /// the subscriptions
    DisposeBag m_Subscriptions = new DisposeBag();

    // -- lifecycle --
    void Awake() {
        // store singleton
        if (s_Instance == null) {
            s_Instance = this;
        } else {
            Destroy(this);
        }

        // set props
        m_Input = GetComponent<DebugInput>();

        // bind events
        m_Subscriptions.Add(m_Input.Reset, OnResetPressed);
        #if UNITY_EDITOR
        m_Subscriptions.Add(m_Input.SpawnCharacter, OnSpawnCharacterPressed);
        #endif
    }

    // -- commands --
    /// reset the game to its initial state
    void Reset() {
        Debug.Log("[de-bug] restarting game");

        // reset atom variables
        var variables = Resources.FindObjectsOfTypeAll(typeof(AtomBaseVariable));
        foreach (AtomBaseVariable variable in variables) {
            var typeName = variable.GetType().Name;
            if (typeName.Contains("Constant")) {
                continue;
            }

            variable.Reset();
        }

        // clear atom event replay buffers
        var events = Resources.FindObjectsOfTypeAll(typeof(AtomEventBase));
        foreach (AtomEventBase evt in events) {
            var replayBufferField = evt.GetType().BaseType.GetField("_replayBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
            var replayBuffer = replayBufferField.GetValue(evt);
            var replayBufferType = replayBuffer.GetType();
            var replayBufferClear = replayBufferType.GetMethod("Clear");
            replayBufferClear.Invoke(replayBuffer, new object[]{});
        }

        // restart online, which will restart the game
        var online = GameObject.FindObjectOfType<Online>();
        online.Restart();
    }

    #if UNITY_EDITOR
    /// spawn the character at the scene camera's position
    void SpawnCharacterAtSceneView() {
        // get the editor camera
        var scene = UnityEditor.SceneView.lastActiveSceneView;
        if (scene == null) {
            return;
        }

        var camera = scene.camera;
        if (camera == null) {
            return;
        }

        var t = camera.transform;

        SpawnCharacterAtTransform(t);
    }

    /// spawn a character at the transform position
    void SpawnCharacterAtTransform(Transform t) {
        // find the current player
        var player = m_Entites.Value
            .Players
            .Current;

        if (player == null) {
            Debug.LogError($"[de-bug] no player to spawn debug character");
            return;
        }

        player.SpawnCharacterAtPoint(m_CharacterKey, t);
    }
    #endif

    // -- events --
    /// .
    void OnResetPressed(InputAction.CallbackContext _) {
        Reset();
    }

    #if UNITY_EDITOR
    /// .
    void OnSpawnCharacterPressed(InputAction.CallbackContext _) {
        SpawnCharacterAtSceneView();
    }
    #endif
}

}