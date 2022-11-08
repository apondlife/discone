using UnityEngine;
using UnityAtoms;

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

    // -- lifecycle --
    void Awake() {
        // store singleton
        if (s_Instance == null) {
            s_Instance = this;
        } else {
            Destroy(this);
        }
    }

    // -- commands --
    /// spawn a character at the transform position
    public void SpawnCharacterAtTransform(Transform t) {
        // find the current player
        var player = m_Entites.Value
            .Players
            .Current;

        if (player == null) {
            Debug.LogError($"[debug] no player to spawn debug character");
            return;
        }

        player.SpawnCharacterAtPoint(m_CharacterKey, t);
    }

    #if UNITY_EDITOR
    [ContextMenu("Spawn Character at Scene Camera")]
    public void SpawnCharacterAtSceneView() {
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


    #endif
}