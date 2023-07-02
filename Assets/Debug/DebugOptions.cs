using UnityEngine;
using UnityEngine.SceneManagement;
using UnityAtoms;
using UnityEngine.InputSystem;
using System.Reflection;
using System.Collections;
using System.Linq;

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
        m_Subscriptions
            .Add(m_Input.Reset, OnResetPressed)
            .Add(m_Input.SpawnCharacter, OnSpawnCharacterPressed);
    }

    // -- commands --
    void Reset() {
        StartCoroutine(ResetAsync());
    }

    /// reset the game to its initial state
    IEnumerator ResetAsync() {
        Debug.Log("[de-bug] restarting game");

        var variables = Resources.FindObjectsOfTypeAll(typeof(AtomBaseVariable));
        foreach (AtomBaseVariable variable in variables) {
            var typeName = variable.GetType().Name;
            if (typeName.Contains("Constant")) {
                continue;
            }

            variable.Reset();
        }

        var events = Resources.FindObjectsOfTypeAll(typeof(AtomEventBase));
        foreach (AtomEventBase evt in events) {
            var evtType = evt.GetType().BaseType;
            var replayBufferField = evtType.GetField("_replayBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
            var replayBuffer = replayBufferField.GetValue(evt);
            var replayBufferType = replayBuffer.GetType();
            var replayBufferClear = replayBufferType.GetMethod("Clear");
            replayBufferClear.Invoke(replayBuffer, new object[]{});
        }

        var mainScene = SceneManager.GetSceneAt(0);
        var mainSceneName = mainScene.name;

        var unload = SceneManager.UnloadSceneAsync(mainScene);
        while (!unload.isDone) {
            yield return unload;
        }

        var load = SceneManager.LoadSceneAsync(mainSceneName, LoadSceneMode.Additive);
        while (!load.isDone) {
            yield return load;
        }

        // var online = GameObject.FindObjectOfType<Online>();
        // online.StopHost();
        // this.DoAfterTime(1, () => {
            // online.ServerChangeScene(currScene.name);
        // });
    }

    /// spawn the character at the scene camera's position
    void SpawnCharacterAtSceneView() {
        #if UNITY_EDITOR
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
        #endif
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

    // -- events --
    /// .
    void OnResetPressed(InputAction.CallbackContext _) {
        Reset();
    }

    /// .
    void OnSpawnCharacterPressed(InputAction.CallbackContext _) {
        SpawnCharacterAtSceneView();
    }
}

}